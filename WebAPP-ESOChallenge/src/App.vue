<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div id="app">
    <nav class="main-nav">
      <div class="nav-container">
        <RouterLink to="/" class="nav-logo">
          🎮 Fortnite API
        </RouterLink>
        
        <div class="nav-links">
          <RouterLink to="/">Home</RouterLink>
          <RouterLink to="/cosmetics">Cosméticos</RouterLink>
          <RouterLink to="/about">Sobre</RouterLink>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
#app {
  min-height: 100%;
  background: linear-gradient(135deg, #0f0f1e 0%, #1a1a2e 100%);
}

.main-nav {
  background: rgba(15, 15, 30, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-logo {
  font-size: 1.5rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  text-decoration: none;
  transition: transform 0.2s ease;
}

.nav-logo:hover {
  transform: scale(1.05);
}

.nav-links {
  display: flex;
  gap: 24px;
}

.nav-links a {
  color: #9ca3af;
  text-decoration: none;
  font-weight: 600;
  font-size: 1rem;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.nav-links a:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.05);
}

.nav-links a.router-link-active {
  color: #667eea;
  background: rgba(102, 126, 234, 0.1);
}

.main-content {
  min-height: calc(100vh - 70px);
}

@media (max-width: 768px) {
  .nav-container {
    flex-direction: column;
    gap: 16px;
  }

  .nav-links {
    width: 100%;
    justify-content: center;
  }

  .nav-links a {
    font-size: 0.875rem;
    padding: 6px 12px;
  }
}
</style>
