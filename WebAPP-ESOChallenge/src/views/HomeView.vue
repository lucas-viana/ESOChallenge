<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import newsService from '@/services/newsService'
import type { Motd, NewsMessage } from '@/types/news'

// Unified type for displaying news items (both MOTDs and Messages)
interface NewsItem {
  id: string
  title: string
  body: string
  image: string | null
  websiteUrl?: string | null
  sortingPriority: number
}

const newsItems = ref<NewsItem[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const currentIndex = ref(0)

const currentNews = computed(() => newsItems.value[currentIndex.value])

const convertMotdToNewsItem = (motd: Motd): NewsItem => ({
  id: motd.id,
  title: motd.title,
  body: motd.body,
  image: motd.image || motd.tileImage,
  websiteUrl: motd.websiteUrl,
  sortingPriority: motd.sortingPriority
})

const convertMessageToNewsItem = (message: NewsMessage, index: number): NewsItem => ({
  id: `message-${index}`,
  title: message.title,
  body: message.body,
  image: message.image,
  websiteUrl: undefined,
  sortingPriority: 0
})

const fetchNews = async () => {
  try {
    loading.value = true
    error.value = null
    const response = await newsService.getNews()

    console.log('News API Response:', response)

    const allNewsItems: NewsItem[] = []
    
    // Try to get MOTDs from any available game mode
    if (response.data?.br?.motds && response.data.br.motds.length > 0) {
      console.log('Found BR motds:', response.data.br.motds.length)
      allNewsItems.push(...response.data.br.motds.filter(m => !m.hidden).map(convertMotdToNewsItem))
    }
    
    if (response.data?.stw?.motds && response.data.stw.motds.length > 0) {
      console.log('Found STW motds:', response.data.stw.motds.length)
      allNewsItems.push(...response.data.stw.motds.filter(m => !m.hidden).map(convertMotdToNewsItem))
    }
    
    if (response.data?.creative?.motds && response.data.creative.motds.length > 0) {
      console.log('Found Creative motds:', response.data.creative.motds.length)
      allNewsItems.push(...response.data.creative.motds.filter(m => !m.hidden).map(convertMotdToNewsItem))
    }

    // If no MOTDs found, try to get messages instead
    if (allNewsItems.length === 0) {
      console.log('No motds found, trying messages...')
      
      if (response.data?.br?.messages && response.data.br.messages.length > 0) {
        console.log('Found BR messages:', response.data.br.messages.length)
        allNewsItems.push(...response.data.br.messages.map(convertMessageToNewsItem))
      }
      
      if (response.data?.stw?.messages && response.data.stw.messages.length > 0) {
        console.log('Found STW messages:', response.data.stw.messages.length)
        allNewsItems.push(...response.data.stw.messages.map(convertMessageToNewsItem))
      }
      
      if (response.data?.creative?.messages && response.data.creative.messages.length > 0) {
        console.log('Found Creative messages:', response.data.creative.messages.length)
        allNewsItems.push(...response.data.creative.messages.map(convertMessageToNewsItem))
      }
    }

    // Sort by priority and assign to newsItems
    newsItems.value = allNewsItems.sort((a, b) => b.sortingPriority - a.sortingPriority)
    
    console.log('Total news items loaded:', newsItems.value.length)
  } catch (err) {
    console.error('Error fetching news:', err)
    error.value = 'Erro ao carregar notícias do Fortnite'
  } finally {
    loading.value = false
  }
}

const nextSlide = () => {
  if (currentIndex.value < newsItems.value.length - 1) {
    currentIndex.value++
  } else {
    currentIndex.value = 0
  }
}

const prevSlide = () => {
  if (currentIndex.value > 0) {
    currentIndex.value--
  } else {
    currentIndex.value = newsItems.value.length - 1
  }
}

const goToSlide = (index: number) => {
  currentIndex.value = index
}

onMounted(() => {
  fetchNews()
})
</script>

<template>
  <div class="home-view">
    <!-- Header -->
    <header class="home-header">
      <div class="home-header-content">
        <h1 class="home-title">🎮 Fortnite</h1>
        <p class="home-subtitle">Explore a coleção completa de cosméticos do Fortnite</p>
        <div class="hero-actions">
          <router-link to="/cosmetics" class="btn btn-primary">
            📚 Ver Coleção
          </router-link>
          <router-link to="/shop" class="btn btn-secondary">
            🛒 Visitar Loja
          </router-link>
        </div>
      </div>
    </header>

    <!-- Main Container -->
    <div class="home-main-container">
      <div class="home-content-wrapper">
        <!-- News Section -->
        <div class="news-wrapper">
          <main class="home-content">
            <!-- Loading State -->
            <div v-if="loading" class="loading-state">
              <div class="spinner"></div>
              <p>Carregando notícias...</p>
            </div>

            <!-- Error State -->
            <div v-else-if="error" class="error-state">
              <p>{{ error }}</p>
            </div>

            <!-- Empty State -->
            <div v-else-if="newsItems.length === 0" class="empty-state">
              <div class="empty-icon">📰</div>
              <h3 class="empty-title">Nenhuma notícia disponível</h3>
              <p class="empty-description">Volte mais tarde para ver as novidades do Fortnite</p>
            </div>

            <!-- News Carousel -->
            <div v-else class="news-carousel">
              <div class="carousel-container">
                <button @click="prevSlide" class="carousel-btn carousel-btn-prev" aria-label="Anterior">
                  ‹
                </button>

                <div class="carousel-track">
                  <div
                    v-for="(newsItem, index) in newsItems"
                    :key="newsItem.id"
                    class="carousel-slide"
                    :class="{ active: index === currentIndex }"
                  >
                    <img
                      v-if="newsItem.image"
                      :src="newsItem.image"
                      :alt="newsItem.title"
                      class="carousel-image"
                    />
                    <div v-else class="carousel-placeholder">
                      <div class="placeholder-icon">📰</div>
                    </div>
                  </div>
                </div>

                <button @click="nextSlide" class="carousel-btn carousel-btn-next" aria-label="Próximo">
                  ›
                </button>

                <!-- Carousel Content Overlay -->
                <div v-if="currentNews" class="carousel-overlay">
                  <div class="overlay-content">
                    <h2 class="news-title">{{ currentNews.title }}</h2>
                    <p v-if="currentNews.body" class="news-body">{{ currentNews.body }}</p>
                    <a
                      v-if="currentNews.websiteUrl"
                      :href="currentNews.websiteUrl"
                      target="_blank"
                      rel="noopener noreferrer"
                      class="news-link"
                    >
                      Saiba mais →
                    </a>
                  </div>
                </div>
              </div>

              <!-- Carousel Indicators -->
              <div class="carousel-indicators">
                <button
                  v-for="(newsItem, index) in newsItems"
                  :key="'indicator-' + newsItem.id"
                  @click="goToSlide(index)"
                  class="indicator"
                  :class="{ active: index === currentIndex }"
                  :aria-label="'Ir para slide ' + (index + 1)"
                ></button>
              </div>
            </div>
          </main>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

/* Header */
.home-header {
  padding: 0rem 0 1rem;
  text-align: center;
}

.home-header-content {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 24px;
}

.home-title {
  font-size: 3rem;
  font-weight: 900;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 16px 0;
}

.home-subtitle {
  color: #cbd5e0;
  font-size: 1.125rem;
  margin: 0 0 24px 0;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.btn {
  padding: 0.875rem 1.75rem;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  text-decoration: none;
  transition: all 0.3s ease;
  display: inline-block;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
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

/* Main Container */
.home-main-container {
  max-width: 80%;
  margin: 0 auto;
  padding: 0 24px 24px;
}

.home-content-wrapper {
  background: rgba(26, 26, 46, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  min-height: calc(100vh - 280px);
}

.news-wrapper {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* Content Area */
.home-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
}

/* Carousel */
.news-carousel {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

.carousel-container {
  position: relative;
  height: 600px;
  border-radius: 16px;
  overflow: hidden;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.carousel-track {
  position: relative;
  width: 100%;
  height: 100%;
}

.carousel-slide {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  transition: opacity 0.5s ease-in-out;
  display: flex;
  align-items: center;
  justify-content: center;
}

.carousel-slide.active {
  opacity: 1;
  z-index: 1;
}

.carousel-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.carousel-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.placeholder-icon {
  font-size: 8rem;
  opacity: 0.3;
}

/* Content Overlay */
.carousel-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background: linear-gradient(to top, rgba(0, 0, 0, 0.95) 0%, rgba(0, 0, 0, 0.7) 70%, transparent 100%);
  padding: 3rem 2rem 2rem;
  z-index: 2;
}

.overlay-content {
  max-width: 800px;
}

.news-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: white;
  margin-bottom: 1rem;
  line-height: 1.2;
  text-shadow: 2px 2px 8px rgba(0, 0, 0, 0.8);
}

.news-body {
  font-size: 1.1rem;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 1.5rem;
  line-height: 1.6;
  text-shadow: 1px 1px 4px rgba(0, 0, 0, 0.8);
}

.news-link {
  display: inline-block;
  padding: 0.75rem 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
  text-shadow: none;
}

.news-link:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
}

.carousel-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, 0.5);
  color: white;
  border: none;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  font-size: 2rem;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 3;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
}

.carousel-btn:hover {
  background: rgba(102, 126, 234, 0.8);
  transform: translateY(-50%) scale(1.1);
}

.carousel-btn-prev {
  left: 1rem;
}

.carousel-btn-next {
  right: 1rem;
}

.carousel-indicators {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 1.5rem;
}

.indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.5);
  background: transparent;
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 0;
}

.indicator:hover {
  border-color: rgba(102, 126, 234, 0.8);
  transform: scale(1.2);
}

.indicator.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  transform: scale(1.3);
}

/* Responsive Design */
@media (max-width: 768px) {
  .carousel-container {
    height: 400px;
  }

  .news-title {
    font-size: 1.75rem;
  }

  .news-body {
    font-size: 0.95rem;
  }

  .carousel-btn {
    width: 40px;
    height: 40px;
    font-size: 1.5rem;
  }

  .carousel-overlay {
    padding: 2rem 1rem 1rem;
  }

  .placeholder-icon {
    font-size: 5rem;
  }
}

/* Loading, Error, Empty States */
.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.7);
}

.empty-icon {
  font-size: 5rem;
  margin-bottom: 1.5rem;
  opacity: 0.5;
}

.empty-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 0.5rem;
}

.empty-description {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.6);
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

/* Responsive Design */
@media (max-width: 768px) {
  .home-title {
    font-size: 2rem;
  }

  .home-subtitle {
    font-size: 1rem;
  }

  .home-main-container {
    padding: 0 16px 16px;
  }

  .home-content-wrapper {
    padding: 16px;
    min-height: calc(100vh - 320px);
  }

  .carousel-container {
    height: 400px;
  }

  .news-title {
    font-size: 1.5rem;
  }

  .news-body {
    font-size: 0.9rem;
  }

  .carousel-btn {
    width: 40px;
    height: 40px;
    font-size: 1.5rem;
  }

  .carousel-overlay {
    padding: 2rem 1rem 1rem;
  }

  .placeholder-icon {
    font-size: 5rem;
  }

  .hero-actions {
    flex-direction: column;
    width: 100%;
    max-width: 300px;
    margin: 0 auto;
  }

  .btn {
    width: 100%;
  }
}
</style>

