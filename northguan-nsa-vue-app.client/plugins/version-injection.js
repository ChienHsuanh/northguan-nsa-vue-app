// Vite plugin for injecting build version and timestamp into HTML
export function versionInjectionPlugin() {
  return {
    name: "version-injection",
    transformIndexHtml(html) {
      const buildVersion =
        process.env.npm_package_version ||
        new Date().toISOString().slice(0, 19).replace(/[:-]/g, "");
      const buildTime = new Date().toISOString();

      return html.replace("{{BUILD_VERSION}}", buildVersion).replace("{{BUILD_TIME}}", buildTime);
    },
  };
}
