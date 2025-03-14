import { fileURLToPath, URL } from 'node:url'
import { vitePlugin as remix } from '@remix-run/dev'
import { defineConfig } from 'vite'
import { flatRoutes } from 'remix-flat-routes'

export default defineConfig({
  plugins: [
    remix({
      ignoredRouteFiles: ['**/*'],
      serverModuleFormat: 'esm',
      routes: async defineRoutes => {
        return flatRoutes('routes', defineRoutes, {
          ignoredRouteFiles: [
            '.*',
            '**/*.css',
            '**/*.test.{js,jsx,ts,tsx}',
            '**/__*.*',
            // This is for server-side utilities you want to colocate
            // next to your routes without making an additional
            // directory. If you need a route that includes "server" or
            // "client" in the filename, use the escape brackets like:
            // my-route.[server].tsx
            '**/*.server.*',
            '**/*.client.*',
          ],
        })
      },
    }),
  ],
  resolve: {
    alias: {
      '~': fileURLToPath(new URL('./app', import.meta.url)),
    },
  },
  server: {
    port: 3100,
  },
})
