# 📚 Guia de Entendimento - Frontend Vue.js

## 🎯 Arquitetura e Organização

Este projeto segue os princípios de **Clean Code** e **SOLID**, organizando o código em camadas bem definidas.

### 📁 Estrutura de Pastas

```
src/
├── config/           # Configurações centralizadas
├── types/            # Definições de tipos TypeScript
├── services/         # Lógica de comunicação com APIs
├── stores/           # Gerenciamento de estado (Pinia)
├── components/       # Componentes reutilizáveis
├── views/            # Páginas/Telas da aplicação
└── router/           # Configuração de rotas
```

---

## 🔄 Fluxo de Execução (Data Flow)

### 1️⃣ Usuário Acessa a Página

```
Usuário → Router → CosmeticsView.vue (montado)
```

**O que acontece:**
- Vue Router identifica a rota `/cosmetics`
- Carrega o componente `CosmeticsView.vue`
- Executa o lifecycle hook `onMounted()`

### 2️⃣ Componente Solicita Dados

```
CosmeticsView.vue → cosmeticStore.fetchNewCosmetics()
```

**Código:**
```typescript
onMounted(async () => {
  await cosmeticStore.fetchNewCosmetics()
})
```

**O que acontece:**
- Componente é montado na tela
- Automaticamente chama o método da store
- Inicia o processo de busca de dados

### 3️⃣ Store Processa a Requisição

```
Store → Service → HttpClient → Backend API
```

**Fluxo detalhado:**

```typescript
// 1. Store (cosmetic.store.ts)
async function fetchNewCosmetics() {
  loading.value = true  // ← Ativa spinner na UI
  error.value = null
  
  // 2. Chama o Service
  const data = await cosmeticService.getNewCosmetics()
  
  // 3. Atualiza o estado
  cosmetics.value = data
  loading.value = false  // ← Desativa spinner
}
```

```typescript
// 2. Service (cosmetic.service.ts)
async getNewCosmetics(): Promise<Cosmetic[]> {
  // 3. Chama o HttpClient genérico
  const response = await httpClient.get<ApiResponse<Cosmetic[]>>(
    '/api/cosmetics/new'
  )
  return response.data || []
}
```

```typescript
// 3. HttpClient (httpClient.service.ts)
async get<T>(endpoint: string): Promise<T> {
  // 4. Faz requisição HTTP para o backend
  const response = await fetch(
    'https://localhost:7001/api/cosmetics/new'
  )
  return response.json()
}
```

### 4️⃣ Backend Retorna os Dados

```
Backend API → HttpClient → Service → Store → View
```

**O que acontece:**
- Backend processa e retorna JSON
- HttpClient converte para objeto TypeScript
- Service valida e retorna array de Cosmetic
- Store atualiza o estado reativo
- Vue detecta mudança e re-renderiza automaticamente

### 5️⃣ Vue Re-renderiza a Interface

```typescript
// A View detecta mudanças no estado
<div v-if="cosmeticStore.loading">
  <LoadingSpinner />  // ← Enquanto carrega
</div>

<div v-else>
  <CosmeticCard         // ← Quando terminar
    v-for="cosmetic in cosmeticStore.cosmetics"
    :cosmetic="cosmetic"
  />
</div>
```

---

## 🧩 Camadas da Aplicação (SOLID)

### **Layer 1: Types** (Contratos de Dados)
```typescript
// types/cosmetic.types.ts
export interface Cosmetic {
  id: string
  name: string
  // ...
}
```
**Responsabilidade:** Define a estrutura dos dados (Type Safety)

---

### **Layer 2: Config** (Configurações)
```typescript
// config/api.config.ts
export const API_CONFIG = {
  BASE_URL: 'https://localhost:7001',
  ENDPOINTS: { /* ... */ }
}
```
**Responsabilidade:** Centraliza configurações (DRY Principle)

---

### **Layer 3: HttpClient** (Comunicação HTTP)
```typescript
// services/httpClient.service.ts
export class HttpClientService {
  async get<T>(endpoint: string): Promise<T>
  async post<T, B>(endpoint: string, body: B): Promise<T>
}
```
**Responsabilidade:** 
- Gerenciar requisições HTTP
- Tratar erros de rede
- Reutilizável para qualquer API (Open/Closed Principle)

---

### **Layer 4: Services** (Lógica de Negócio)
```typescript
// services/cosmetic.service.ts
export class CosmeticService {
  async getAllCosmetics(): Promise<Cosmetic[]>
  async getNewCosmetics(): Promise<Cosmetic[]>
}
```
**Responsabilidade:**
- Implementar lógica específica de cosméticos
- Transformar dados da API
- Validar business rules

---

### **Layer 5: Store** (Estado Global)
```typescript
// stores/cosmetic.store.ts
export const useCosmeticStore = defineStore('cosmetic', () => {
  const cosmetics = ref<Cosmetic[]>([])
  const loading = ref(false)
  
  async function fetchNewCosmetics() { /* ... */ }
  
  return { cosmetics, loading, fetchNewCosmetics }
})
```
**Responsabilidade:**
- Gerenciar estado da aplicação
- Coordenar chamadas aos services
- Prover dados reativos para componentes

---

### **Layer 6: Components** (Apresentação)

#### 📦 Dumb Components (Apresentação Pura)
```vue
<!-- components/CosmeticCard.vue -->
<template>
  <article class="cosmetic-card">
    <img :src="cosmetic.images.icon" />
    <h3>{{ cosmetic.name }}</h3>
  </article>
</template>
```
**Responsabilidade:**
- Apenas exibir dados recebidos via props
- Sem lógica de negócio
- Altamente reutilizável

#### 🧠 Smart Components (Container)
```vue
<!-- views/CosmeticsView.vue -->
<script setup>
const cosmeticStore = useCosmeticStore()

onMounted(async () => {
  await cosmeticStore.fetchNewCosmetics()
})
</script>
```
**Responsabilidade:**
- Orquestrar lógica
- Buscar dados
- Passar props para componentes "burros"

---

## 🔁 Reatividade do Vue 3

### Como Funciona?

```typescript
// Store cria referências reativas
const cosmetics = ref<Cosmetic[]>([])  // ← Reativo!

// Quando atualiza...
cosmetics.value = [/* novos dados */]  // ← Vue detecta mudança

// Vue automaticamente re-renderiza todos os componentes que usam
<div v-for="cosmetic in cosmeticStore.cosmetics">
  <!-- Atualiza automaticamente! -->
</div>
```

### Fluxo de Reatividade

```
State Muda → Vue Detecta → Virtual DOM Diff → Re-render Otimizado
```

---

## 🎨 Padrões de Design Utilizados

### 1. **Singleton Pattern**
```typescript
export const httpClient = new HttpClientService()
export const cosmeticService = new CosmeticService()
```
**Por quê?** Uma única instância compartilhada por toda aplicação

### 2. **Repository Pattern**
```typescript
class CosmeticService {
  async getAllCosmetics() { /* busca dados */ }
}
```
**Por quê?** Abstrai a origem dos dados (pode trocar API sem quebrar)

### 3. **Container/Presentational Pattern**
```
CosmeticsView (Smart) → CosmeticCard (Dumb)
```
**Por quê?** Separação entre lógica e apresentação

### 4. **Observer Pattern** (Vue Reatividade)
```typescript
const cosmetics = ref([])  // Observable
// Componentes "observam" e reagem às mudanças
```

---

## 🚀 Executando o Projeto

### 1. Instalar Dependências
```bash
cd WebAPP-ESOChallenge
npm install
```

### 2. Configurar Variáveis de Ambiente
Já criado: `.env` e `.env.development`

### 3. Executar em Desenvolvimento
```bash
npm run dev
```

### 4. Acessar
```
http://localhost:5173/cosmetics
```

---

## 🧪 Testando o Fluxo

### Abra o DevTools do navegador:

1. **Network Tab**: Veja as requisições HTTP
2. **Vue DevTools**: Inspecione o estado da Pinia Store
3. **Console**: Veja logs de debug

### Fluxo Esperado:
```
1. Página carrega → Loading aparece
2. Requisição para backend → Network mostra chamada
3. Dados retornam → Store atualiza
4. Loading some → Cards aparecem
```

---

## 📖 Conceitos Vue 3 Importantes

### `ref()` - Cria dado reativo
```typescript
const loading = ref(false)
loading.value = true  // Atualiza e re-renderiza
```

### `computed()` - Valor derivado
```typescript
const count = computed(() => cosmetics.value.length)
// Recalcula automaticamente quando cosmetics muda
```

### `onMounted()` - Lifecycle Hook
```typescript
onMounted(() => {
  // Executa quando componente é montado
})
```

### `v-if` / `v-else` - Renderização Condicional
```vue
<div v-if="loading">Carregando...</div>
<div v-else>Dados carregados!</div>
```

### `v-for` - Loop
```vue
<div v-for="item in items" :key="item.id">
  {{ item.name }}
</div>
```

---

## 🎯 Principais Benefícios desta Arquitetura

✅ **Testável**: Cada camada pode ser testada isoladamente  
✅ **Escalável**: Fácil adicionar novas features  
✅ **Manutenível**: Código organizado e bem documentado  
✅ **Reutilizável**: Componentes e services podem ser usados em outros lugares  
✅ **Type-Safe**: TypeScript previne erros em tempo de desenvolvimento  

---

## 🔧 Próximos Passos (Melhorias)

1. ✅ Adicionar testes unitários (Vitest)
2. ✅ Implementar cache local (localStorage)
3. ✅ Adicionar paginação
4. ✅ Criar filtros avançados
5. ✅ Adicionar animações de transição

---

## 📚 Recursos para Aprender Mais

- [Vue 3 Documentation](https://vuejs.org/)
- [Pinia Documentation](https://pinia.vuejs.org/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Clean Code Principles](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
