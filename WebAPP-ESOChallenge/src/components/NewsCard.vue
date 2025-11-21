<template>
  <div class="news-card">
    <div v-if="motd.image || motd.tileImage" class="news-image">
      <img :src="motd.image || motd.tileImage || ''" :alt="motd.title" />
    </div>
    <div class="news-content">
      <h3 class="news-title">{{ motd.title }}</h3>
      <p v-if="motd.body" class="news-body">{{ motd.body }}</p>
      <div v-if="motd.websiteUrl" class="news-footer">
        <a :href="motd.websiteUrl" target="_blank" rel="noopener noreferrer" class="news-link">
          Saiba mais →
        </a>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Motd } from '@/types/news'

defineProps<{
  motd: Motd
}>()
</script>

<style scoped>
.news-card {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.news-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.4);
}

.news-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
  background: linear-gradient(135deg, #334155 0%, #1e293b 100%);
}

.news-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.news-card:hover .news-image img {
  transform: scale(1.05);
}

.news-content {
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.news-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #f8fafc;
  margin-bottom: 0.75rem;
  line-height: 1.4;
}

.news-body {
  color: #cbd5e1;
  font-size: 0.95rem;
  line-height: 1.6;
  margin-bottom: 1rem;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
}

.news-footer {
  margin-top: auto;
}

.news-link {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: #60a5fa;
  font-weight: 600;
  text-decoration: none;
  transition: color 0.2s ease;
}

.news-link:hover {
  color: #3b82f6;
}

@media (max-width: 768px) {
  .news-image {
    height: 150px;
  }

  .news-content {
    padding: 1rem;
  }

  .news-title {
    font-size: 1.1rem;
  }

  .news-body {
    font-size: 0.9rem;
    -webkit-line-clamp: 2;
  }
}
</style>
