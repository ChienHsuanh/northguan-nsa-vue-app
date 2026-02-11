import { fileURLToPath, URL } from "node:url";

import { defineConfig, loadEnv } from "vite";
import plugin from "@vitejs/plugin-vue";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
import tailwindcss from "@tailwindcss/vite";
import removeConsole from "vite-plugin-remove-console";
import { versionInjectionPlugin } from "./plugins/version-injection.js";

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  // Load env file based on `mode` in the current working directory.
  // Set the third parameter to '' to load all env regardless of the `VITE_` prefix.
  const viteEnv = loadEnv(mode, process.cwd(), "");

  // Get HTTPS configuration from loaded environment variables
  const getHttpsEnabledFromEnv = () => {
    if (viteEnv.VITE_HTTPS_ENABLED === "false") return false;
    if (viteEnv.VITE_HTTPS_ENABLED === "true") return true;

    // Default: enable HTTPS in development, disable in build
    return mode === "development" && command === "serve";
  };

  // SSL certificate configuration (only for development server)
  let httpsConfigFromEnv = undefined;

  // Only setup HTTPS if enabled and serving (not building)
  if (getHttpsEnabledFromEnv() && command === "serve") {
    const baseFolder =
      env.APPDATA !== undefined && env.APPDATA !== ""
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

    const certificateName = "northguan-nsa-vue-app.client";
    const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
    const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

    try {
      if (!fs.existsSync(baseFolder)) {
        fs.mkdirSync(baseFolder, { recursive: true });
      }

      if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
        // Try to create certificate, but don't fail if dotnet is not available
        const result = child_process.spawnSync(
          "dotnet",
          ["dev-certs", "https", "--export-path", certFilePath, "--format", "Pem", "--no-password"],
          { stdio: "inherit" }
        );

        if (result.status !== 0) {
          console.warn("Could not create SSL certificate. HTTPS will be disabled.");
        }
      }

      // Only use HTTPS if certificates exist
      if (fs.existsSync(certFilePath) && fs.existsSync(keyFilePath)) {
        httpsConfigFromEnv = {
          key: fs.readFileSync(keyFilePath),
          cert: fs.readFileSync(certFilePath),
        };
      }
    } catch (error) {
      console.warn("SSL certificate setup failed. HTTPS will be disabled.", error instanceof Error ? error.message : String(error));
    }
  }

  // Get backend target URL based on loaded environment variables
  const getBackendTargetFromEnv = () => {
    // Check if HTTPS is enabled for backend - default to false (HTTP)
    const backendHttpsEnabled = viteEnv.VITE_BACKEND_HTTPS_ENABLED === "true";
    const httpsPort = viteEnv.VITE_BACKEND_HTTPS_PORT || env.ASPNETCORE_HTTPS_PORT || "7289";
    const httpPort = viteEnv.VITE_BACKEND_HTTP_PORT || "5114";

    // Use environment variables if available
    if (env.ASPNETCORE_URLS) {
      return env.ASPNETCORE_URLS.split(";")[0];
    }

    // Construct URL based on HTTPS setting
    if (backendHttpsEnabled) {
      return `https://localhost:${httpsPort}`;
    } else {
      return `http://localhost:${httpPort}`;
    }
  };

  const targetFromEnv = getBackendTargetFromEnv();
  const devServerPort = parseInt(viteEnv.DEV_SERVER_PORT || viteEnv.VITE_HTTPS_PORT || "13343");

  console.log(`🔧 Vite Config (${mode}):`, {
    httpsEnabled: getHttpsEnabledFromEnv(),
    backendTarget: targetFromEnv,
    devServerPort: devServerPort,
    command: command,
  });

  return {
    plugins: [plugin(), tailwindcss(), removeConsole(), versionInjectionPlugin()],
    resolve: {
      alias: {
        "@": fileURLToPath(new URL("./src", import.meta.url)),
      },
    },
    build: {
      rollupOptions: {
        output: {
          manualChunks: {
            // Safer static chunking to avoid dependency issues
            "vendor-vue": ["vue", "vue-router", "pinia"],
            "vendor-charts": ["echarts", "vue-echarts"],
            "vendor-maps": ["leaflet"],
            "vendor-ui": ["lucide-vue-next", "@vueuse/core"],
            "vendor-utils": ["lodash", "moment", "axios"],
          },
          // Ensure all files have proper hash for cache busting
          entryFileNames: `assets/[name]-[hash].js`,
          chunkFileNames: `assets/[name]-[hash].js`,
          assetFileNames: (assetInfo) => {
            if (!assetInfo.name) {
              return `assets/[name]-[hash][extname]`;
            }
            const info = assetInfo.name.split(".");
            const extType = info[info.length - 1];
            // Group assets by type with hash for cache busting
            if (/\.(css)$/i.test(assetInfo.name)) {
              return `assets/css/[name]-[hash][extname]`;
            }
            if (/\.(png|jpe?g|gif|svg|ico|webp)$/i.test(assetInfo.name)) {
              return `assets/images/[name]-[hash][extname]`;
            }
            if (/\.(woff2?|eot|ttf|otf)$/i.test(assetInfo.name)) {
              return `assets/fonts/[name]-[hash][extname]`;
            }
            return `assets/[name]-[hash][extname]`;
          },
        },
        // Prevent problematic external dependencies
        external: [],
        // Ensure proper module resolution
        treeshake: {
          preset: "recommended",
        },
      },
      // Increase chunk size warning limit since we're intentionally creating larger chunks
      chunkSizeWarningLimit: 1500,
      // Ensure proper source map generation for debugging
      sourcemap: mode === "development",
      // Generate manifest for cache validation
      manifest: true,
      // Ensure proper cache busting
      assetsInlineLimit: 0,
    },
    server: {
      proxy: {
        "^/api/": {
          target: targetFromEnv,
          secure: false,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api\//, "/api/"),
        },
        "^/uploads/": {
          target: targetFromEnv,
          secure: false,
          changeOrigin: true,
        },
      },
      port: devServerPort,
      https: httpsConfigFromEnv,
      allowedHosts: viteEnv.VITE_DEV_SERVER_ALLOWED_HOSTS?.toString().split(",") || null,
    },
    // Define global constants for the app
    define: {
      __VITE_ENV__: JSON.stringify(viteEnv),
    },
  };
});
