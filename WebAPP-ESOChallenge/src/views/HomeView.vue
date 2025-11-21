<script setup lang="ts">
import { ref, onMounted } from 'vue'
import NewsCard from '@/components/NewsCard.vue'
import newsService from '@/services/newsService'
import type { Motd } from '@/types/news'

const newsItems = ref<Motd[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const fetchNews = async () => {
  try {
    loading.value = true
    error.value = null
    const response = await newsService.getNews()
    
    // Get BR news (Battle Royale) and filter out hidden items
    if (response.data?.br?.motds) {
      newsItems.value = response.data.br.motds
        .filter(motd => !motd.hidden)
        .sort((a, b) => b.sortingPriority - a.sortingPriority)
    }
  } catch (err) {
    console.error('Error fetching news:', err)
    error.value = 'Erro ao carregar notícias do Fortnite'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchNews()
})
</script>

<template>
  <main class="home-view">
    <div class="hero-section">
      <div class="hero-content">
        <h1 class="hero-title">🎮 Fortnite API Challenge</h1>
        <p class="hero-subtitle">
          Explore a coleção completa de cosméticos do Fortnite
        </p>
        <div class="hero-actions">
          <router-link to="/cosmetics" class="btn btn-primary">
            📚 Ver Coleção
          </router-link>
          <router-link to="/shop" class="btn btn-secondary">
            🛒 Visitar Loja
          </router-link>
        </div>
      </div>
    </div>

    <!-- News Section -->
    <div class="news-section">
      <div class="section-header">
        <h2 class="section-title">📰 Notícias do Fortnite</h2>
        <p class="section-subtitle">Fique por dentro das últimas novidades do Battle Royale</p>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Carregando notícias...</p>
      </div>

      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
      </div>

      <div v-else-if="newsItems.length > 0" class="news-grid">
        <NewsCard
          v-for="motd in newsItems"
          :key="motd.id"
          :motd="motd"
        />
      </div>

      <div v-else class="empty-state">
        <p>Nenhuma notícia disponível no momento</p>
      </div>
    </div>

    <div class="features-section">
      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">📚</div>
          <h3>Coleção Completa</h3>
          <p>Navegue por todos os cosméticos do Fortnite com filtros avançados</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">🛒</div>
          <h3>Loja Atual</h3>
          <p>Veja os itens disponíveis para compra na loja do momento</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">✨</div>
          <h3>Itens Novos</h3>
          <p>Descubra os cosméticos mais recentes adicionados ao jogo</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">👜</div>
          <h3>Seu Inventário</h3>
          <p>Gerencie seus itens adquiridos e histórico de compras</p>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.home-view {
  min-height: 100vh;
  padding-bottom: 4rem;
}

.hero-section {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  padding: 6rem 2rem;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-title {
  font-size: 3.5rem;
  font-weight: 900;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 1rem;
}

.hero-subtitle {
  font-size: 1.5rem;
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 2rem;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.btn {
  padding: 1rem 2rem;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1.1rem;
  text-decoration: none;
  transition: all 0.3s ease;
  display: inline-block;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 32px rgba(102, 126, 234, 0.4);
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 2px solid rgba(255, 255, 255, 0.2);
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.3);
  transform: translateY(-2px);
}

.features-section {
  max-width: 1200px;
  margin: 4rem auto;
  padding: 0 2rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
}

.feature-card {
  background: rgba(26, 26, 46, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 2rem;
  text-align: center;
  transition: all 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-4px);
  border-color: rgba(102, 126, 234, 0.5);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.2);
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature-card h3 {
  font-size: 1.5rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.5rem;
}

.feature-card p {
  color: rgba(255, 255, 255, 0.7);
  line-height: 1.6;
}

/* News Section */
.news-section {
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 2rem;
}

.section-header {
  text-align: center;
  margin-bottom: 3rem;
}

.section-title {
  font-size: 2.5rem;
  font-weight: 800;
  color: white;
  margin-bottom: 0.5rem;
}

.section-subtitle {
  font-size: 1.1rem;
  color: rgba(255, 255, 255, 0.7);
}

.news-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2rem;
}

.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.7);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(255, 255, 255, 0.1);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.error-state p {
  color: #ef4444;
  font-weight: 600;
}

@media (max-width: 768px) {
  .hero-title {
    font-size: 2.5rem;
  }

  .hero-subtitle {
    font-size: 1.2rem;
  }

  .hero-section {
    padding: 4rem 1rem;
  }

  .features-grid {
    grid-template-columns: 1fr;
  }

  .news-section {
    padding: 3rem 1rem;
  }

  .section-title {
    font-size: 2rem;
  }

  .news-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
}
</style>

