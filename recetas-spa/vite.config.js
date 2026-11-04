import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// En desarrollo (npm run dev) el proxy manda /api y /uploads a la API que corre en Docker (puerto 5000).
// En Docker, nginx hace lo mismo (ver nginx.conf).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: { '/api': 'http://localhost:5000', '/uploads': 'http://localhost:5000' },
  },
})
