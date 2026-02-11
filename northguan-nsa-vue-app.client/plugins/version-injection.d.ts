// Type definitions for version-injection plugin
export declare function versionInjectionPlugin(): {
  name: string;
  transformIndexHtml(html: string): string;
};