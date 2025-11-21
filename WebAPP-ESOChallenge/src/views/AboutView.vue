<template>
  <div class="about-view">
    <!-- Header -->
    <header class="about-header">
      <div class="about-header-content">
        <h1 class="about-title">📖 Sobre o Projeto</h1>
        <p class="about-subtitle">Arquitetura e tecnologias utilizadas</p>
      </div>
    </header>

    <!-- Main Container -->
    <div class="about-main-container">
      <div class="about-content-wrapper">
        <div class="about-content">
          <!-- Visão Geral -->
          <section class="info-section">
            <h2 class="section-title">🎯 Visão Geral</h2>
            <p class="section-text">
              Sistema full-stack de gerenciamento de cosméticos do Fortnite, integrando APIs oficiais
              para exibir itens, gerenciar inventário e realizar compras com sistema de autenticação.
            </p>
          </section>

          <!-- Arquitetura -->
          <section class="info-section">
            <h2 class="section-title">🏗️ Arquitetura</h2>
            <div class="architecture-grid">
              <div class="arch-card">
                <div class="arch-icon">⚙️</div>
                <h3>Backend (API)</h3>
                <p>ASP.NET Core 8.0 com arquitetura baseada em Features</p>
              </div>
              <div class="arch-card">
                <div class="arch-icon">🎨</div>
                <h3>Frontend (SPA)</h3>
                <p>Vue 3 + TypeScript com Composition API</p>
              </div>
              <div class="arch-card">
                <div class="arch-icon">🗄️</div>
                <h3>Banco de Dados</h3>
                <p>PostgreSQL com Entity Framework Core</p>
              </div>
              <div class="arch-card">
                <div class="arch-icon">🔐</div>
                <h3>Autenticação</h3>
                <p>JWT (JSON Web Tokens) + ASP.NET Identity</p>
              </div>
            </div>
          </section>

          <!-- Feature Design -->
          <section class="info-section">
            <h2 class="section-title">📁 Feature Design Architecture</h2>
            <p class="section-text">
              O projeto segue a organização por features, onde cada funcionalidade possui seus próprios
              componentes agrupados (Controllers, Services, DTOs, Models) facilitando manutenção e escalabilidade.
            </p>
            <div class="features-list">
              <div class="feature-item">
                <span class="feature-icon">🔐</span>
                <span class="feature-name">Authentication</span>
                <span class="feature-desc">Login, registro e gerenciamento de usuários</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">✨</span>
                <span class="feature-name">Cosmetics</span>
                <span class="feature-desc">Catálogo, filtros e visualização de itens</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">🛒</span>
                <span class="feature-name">Purchases</span>
                <span class="feature-desc">Sistema de compras e V-Bucks</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">📰</span>
                <span class="feature-name">News</span>
                <span class="feature-desc">Integração com API de notícias</span>
              </div>
              <div class="feature-item">
                <span class="feature-icon">👑</span>
                <span class="feature-name">Admin</span>
                <span class="feature-desc">Painel administrativo e sincronização</span>
              </div>
            </div>
          </section>

          <!-- Tecnologias Backend -->
          <section class="info-section">
            <h2 class="section-title">🔧 Tecnologias Backend</h2>
            <div class="tech-grid">
              <div class="tech-card">
                <h4>ASP.NET Core 8.0</h4>
                <p>Framework web moderno e performático</p>
              </div>
              <div class="tech-card">
                <h4>Entity Framework Core</h4>
                <p>ORM para acesso ao banco de dados</p>
              </div>
              <div class="tech-card">
                <h4>PostgreSQL</h4>
                <p>Banco relacional robusto e escalável</p>
              </div>
              <div class="tech-card">
                <h4>JWT Bearer</h4>
                <p>Autenticação stateless e segura</p>
              </div>
              <div class="tech-card">
                <h4>Swagger/OpenAPI</h4>
                <p>Documentação interativa da API</p>
              </div>
              <div class="tech-card">
                <h4>HttpClient</h4>
                <p>Integração com APIs externas</p>
              </div>
            </div>
          </section>

          <!-- Tecnologias Frontend -->
          <section class="info-section">
            <h2 class="section-title">🎨 Tecnologias Frontend</h2>
            <div class="tech-grid">
              <div class="tech-card">
                <h4>Vue 3</h4>
                <p>Framework progressivo e reativo</p>
              </div>
              <div class="tech-card">
                <h4>TypeScript</h4>
                <p>JavaScript com tipagem estática</p>
              </div>
              <div class="tech-card">
                <h4>Pinia</h4>
                <p>Gerenciamento de estado global</p>
              </div>
              <div class="tech-card">
                <h4>Vue Router</h4>
                <p>Roteamento SPA com navegação</p>
              </div>
              <div class="tech-card">
                <h4>Vite</h4>
                <p>Build tool rápido e moderno</p>
              </div>
            </div>
          </section>

          <!-- Principais Bibliotecas -->
          <section class="info-section">
            <h2 class="section-title">📦 Principais Bibliotecas</h2>
            <div class="libs-list">
              <div class="lib-item">
                <strong>ASP.NET Identity</strong>
                <span>Gerenciamento completo de usuários e autenticação</span>
              </div>
              <div class="lib-item">
                <strong>Npgsql</strong>
                <span>Provider PostgreSQL para .NET</span>
              </div>
              <div class="lib-item">
                <strong>Composition API</strong>
                <span>API moderna do Vue 3 para lógica de componentes</span>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.about-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

/* Header */
.about-header {
  padding: 2rem 0;
  text-align: center;
}

.about-header-content {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 24px;
}

.about-title {
  font-size: 3rem;
  font-weight: 900;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 16px 0;
}

.about-subtitle {
  color: #cbd5e0;
  font-size: 1.125rem;
  margin: 0;
}

/* Main Container */
.about-main-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px 24px;
}

.about-content-wrapper {
  background: rgba(26, 26, 46, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  min-height: calc(100vh - 280px);
}

.about-content {
  max-width: 1000px;
  margin: 0 auto;
}

/* Sections */
.info-section {
  margin-bottom: 3rem;
}

.section-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 1.5rem;
  padding-bottom: 0.75rem;
  border-bottom: 2px solid rgba(102, 126, 234, 0.3);
}

.section-text {
  font-size: 1.125rem;
  line-height: 1.8;
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 1.5rem;
}

/* Architecture Grid */
.architecture-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.arch-card {
  background: rgba(102, 126, 234, 0.1);
  border: 1px solid rgba(102, 126, 234, 0.2);
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
  transition: all 0.3s ease;
}

.arch-card:hover {
  transform: translateY(-4px);
  border-color: rgba(102, 126, 234, 0.4);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.2);
}

.arch-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.arch-card h3 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 0.5rem;
}

.arch-card p {
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.7);
  line-height: 1.5;
}

/* Features List */
.features-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1.5rem;
}

.feature-item {
  display: grid;
  grid-template-columns: 40px 150px 1fr;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  transition: all 0.3s ease;
}

.feature-item:hover {
  background: rgba(255, 255, 255, 0.05);
  border-color: rgba(102, 126, 234, 0.3);
}

.feature-icon {
  font-size: 1.5rem;
  text-align: center;
}

.feature-name {
  font-weight: 700;
  color: #667eea;
  font-size: 1rem;
}

.feature-desc {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.95rem;
}

/* Tech Grid */
.tech-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.tech-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  padding: 1.25rem;
  transition: all 0.3s ease;
}

.tech-card:hover {
  background: rgba(102, 126, 234, 0.1);
  border-color: rgba(102, 126, 234, 0.3);
  transform: translateY(-2px);
}

.tech-card h4 {
  font-size: 1.125rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 0.5rem;
}

.tech-card p {
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.6);
  line-height: 1.5;
}

/* Libraries List */
.libs-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1.5rem;
}

.lib-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.03);
  border-left: 3px solid #667eea;
  border-radius: 4px;
}

.lib-item strong {
  font-size: 1rem;
  color: #ffffff;
  font-weight: 700;
}

.lib-item span {
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.7);
}

/* Responsive */
@media (max-width: 768px) {
  .about-title {
    font-size: 2rem;
  }

  .about-subtitle {
    font-size: 1rem;
  }

  .about-content-wrapper {
    padding: 24px;
  }

  .section-title {
    font-size: 1.5rem;
  }

  .architecture-grid {
    grid-template-columns: 1fr;
  }

  .feature-item {
    grid-template-columns: 40px 1fr;
    gap: 0.75rem;
  }

  .feature-name {
    grid-column: 2;
  }

  .feature-desc {
    grid-column: 1 / -1;
    margin-top: 0.5rem;
    padding-left: 0.5rem;
  }

  .tech-grid {
    grid-template-columns: 1fr;
  }
}
</style>
