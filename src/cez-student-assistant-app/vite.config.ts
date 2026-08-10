import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
    server: {
        host: "0.0.0.0",
        port: 5173,
        strictPort: true,
        proxy: {
            "^/(auth|cez|course($|/)|quiz|user|sync-hub)": {
                target: "https://localhost:8081",
                secure: false,
                changeOrigin: true,
                ws: true,
            },
        },
    },
    plugins: [
        react(),
        tailwindcss(),
    ],
});
