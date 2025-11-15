# 🎨 Guia: Entendendo Problemas de CSS no Vue.js

## 🐛 Onde os Erros de CSS Podem Ocorrer?

### 1️⃣ **Arquivos de Estilos Globais**

#### `src/assets/base.css`
```
└── Define estilos GLOBAIS para TODA a aplicação
    ├── Reset de CSS (* { margin: 0; padding: 0; })
    ├── Variáveis CSS (cores, espaçamentos)
    └── Estilos do <body> e <html>
```

**⚠️ Problema Comum:**
```css
/* ❌ ERRADO - Força background em todas as páginas */
body {
  background: var(--color-background); /* Sobrescreve backgrounds de views! */
}

/* ✅ CORRETO - Permite que cada view controle seu background */
body {
  margin: 0;
  padding: 0;
  /* Sem background fixo */
}
```

---

#### `src/assets/main.css`
```
└── Importa base.css e adiciona estilos ao #app
    └── Define layout geral da aplicação
```

**⚠️ Problema Comum:**
```css
/* ❌ ERRADO - Força layout grid que quebra views */
#app {
  display: grid;
  grid-template-columns: 1fr 1fr; /* Views não ficam full-width! */
  max-width: 1280px; /* Limita largura da view! */
}

/* ✅ CORRETO - Permite que cada view controle seu layout */
#app {
  width: 100%;
  min-height: 100vh;
  margin: 0;
  padding: 0;
}
```

---

### 2️⃣ **Estilos no App.vue**

#### `src/App.vue` (Componente Raiz)
```vue
<style scoped>
/* Estilos do componente raiz */
#app {
  /* Pode sobrescrever main.css */
}
</style>
```

**⚠️ Problema Comum:**
```css
/* ❌ ERRADO - Conflita com estilos globais */
#app {
  background: white; /* Sobrescreve background da view! */
  padding: 20px; /* Adiciona padding indesejado! */
}
```

---

### 3️⃣ **Estilos Scoped vs Global**

#### **Scoped (Local ao Componente)**
```vue
<style scoped>
/* Afeta APENAS este componente */
.cosmetics-view {
  background: linear-gradient(...);
}
</style>
```

#### **Global (Afeta Toda Aplicação)**
```vue
<style>
/* Afeta TODOS os componentes */
.cosmetics-view {
  background: linear-gradient(...);
}
</style>
```

**⚠️ Problema Comum:**
```vue
<!-- ❌ ERRADO - Sem scoped, afeta outros componentes -->
<style>
.title {
  color: red; /* Todos os .title ficam vermelhos! */
}
</style>

<!-- ✅ CORRETO - Com scoped, afeta só este componente -->
<style scoped>
.title {
  color: red; /* Só o .title deste componente fica vermelho */
}
</style>
```

---

## 🔍 Hierarquia de Estilos (Ordem de Aplicação)

```
1. Browser Defaults (estilos padrão do navegador)
   ↓
2. base.css (reset e variáveis CSS)
   ↓
3. main.css (estilos do #app)
   ↓
4. App.vue <style> (componente raiz)
   ↓
5. View Component <style scoped> (CosmeticsView.vue)
   ↓
6. Child Components <style scoped> (CosmeticCard.vue)
   ↓
7. Inline Styles (:style="{ color: 'red' }")
```

**Regra:** Estilos mais específicos sobrescrevem estilos mais gerais!

---

## 🎯 Como Identificar Problemas de CSS

### **1. Usar o DevTools do Navegador (F12)**

```
1. Clique com botão direito no elemento → "Inspecionar"
2. Veja a aba "Styles" (Estilos)
3. Estilos riscados = foram sobrescritos
4. Veja qual arquivo está aplicando o estilo
```

**Exemplo no DevTools:**
```css
/* Arquivo: CosmeticsView.vue */
.cosmetics-view {
  background: linear-gradient(...); ✅ Aplicado
}

/* Arquivo: base.css */
body {
  background: white; ❌ Sobrescrito
}
```

---

### **2. Verificar Conflitos de Especificidade**

```css
/* Especificidade BAIXA (10) */
.title { color: blue; }

/* Especificidade MÉDIA (20) */
.header .title { color: red; }

/* Especificidade ALTA (30) */
#app .header .title { color: green; } ✅ Este vence!
```

**Cálculo de Especificidade:**
- `elemento` = 1 ponto
- `.classe` = 10 pontos
- `#id` = 100 pontos
- `!important` = 1000 pontos (evite usar!)

---

### **3. Verificar Herança de Estilos**

```css
/* Propriedades que são HERDADAS dos pais: */
- color
- font-family
- font-size
- line-height
- text-align

/* Propriedades que NÃO são herdadas: */
- margin
- padding
- background
- border
- width/height
```

---

## 🛠️ Fluxo de Depuração de CSS

### **Passo 1: Identifique o Elemento com Problema**
```
→ Abra o DevTools (F12)
→ Clique em "Inspecionar" no elemento
→ Veja qual estilo está sendo aplicado
```

### **Passo 2: Encontre o Arquivo Responsável**
```
DevTools → Aba "Styles" → Veja o nome do arquivo
```

### **Passo 3: Verifique a Hierarquia**
```
1. É um estilo global? → base.css ou main.css
2. É do componente raiz? → App.vue
3. É da view? → CosmeticsView.vue
4. É de um componente filho? → CosmeticCard.vue
```

### **Passo 4: Corrija no Local Certo**
```
❌ ERRADO: Adicionar !important
✅ CORRETO: Corrigir a hierarquia de estilos
```

---

## 📋 Checklist de Problemas Comuns

### ✅ **Estilos Globais**
- [ ] `base.css` não força background
- [ ] `main.css` não força layout
- [ ] `body` tem `margin: 0; padding: 0;`
- [ ] `#app` tem `width: 100%; min-height: 100vh;`

### ✅ **Componentes**
- [ ] `<style scoped>` está presente (para evitar conflitos)
- [ ] Classes têm nomes únicos (evite `.title`, use `.cosmetics-title`)
- [ ] Background da view não é sobrescrito por estilos globais

### ✅ **Layout**
- [ ] View usa `min-height: 100vh` (ocupa tela toda)
- [ ] Sem `max-width` em elementos que devem ser full-width
- [ ] Grid/Flex configurado corretamente

---

## 🎨 Boas Práticas de CSS no Vue

### **1. Use Scoped Styles**
```vue
<style scoped>
/* Evita conflitos entre componentes */
</style>
```

### **2. Use Classes Descritivas**
```css
/* ❌ Muito genérico */
.title { }
.button { }

/* ✅ Específico ao componente */
.cosmetics-title { }
.cosmetics-filter-btn { }
```

### **3. Use BEM Naming Convention**
```css
/* Block__Element--Modifier */
.cosmetic-card { }
.cosmetic-card__image { }
.cosmetic-card__title { }
.cosmetic-card__title--highlighted { }
```

### **4. Evite !important**
```css
/* ❌ EVITE - Dificulta manutenção */
.title {
  color: red !important;
}

/* ✅ USE - Especificidade correta */
.cosmetics-view .cosmetics-title {
  color: red;
}
```

### **5. Use Variáveis CSS**
```css
:root {
  --primary-color: #667eea;
  --secondary-color: #764ba2;
  --spacing-lg: 24px;
}

.cosmetics-view {
  padding: var(--spacing-lg);
  background: var(--primary-color);
}
```

---

## 🔥 Problema Específico que Corrigimos

### **Antes (Quebrado):**
```css
/* main.css - PROBLEMA! */
#app {
  max-width: 1280px; /* ← Limitava largura */
  display: grid;
  grid-template-columns: 1fr 1fr; /* ← Quebrava layout */
}

/* base.css - PROBLEMA! */
body {
  background: var(--color-background); /* ← Sobrescrevia background da view */
}
```

### **Depois (Corrigido):**
```css
/* main.css - CORRETO! */
#app {
  width: 100%; /* ← Full width */
  min-height: 100vh;
  margin: 0;
  padding: 0;
}

/* base.css - CORRETO! */
body {
  margin: 0;
  padding: 0;
  /* Sem background fixo ← Permite que view controle */
}
```

---

## 🎯 Resumo: Ordem de Verificação

```
1. DevTools (F12) → Inspecionar elemento
2. Verificar qual arquivo aplica o estilo
3. Verificar hierarquia: global → component → inline
4. Corrigir no arquivo correto
5. Evitar !important
6. Usar scoped styles
7. Testar no navegador
```

---

## 📚 Recursos Úteis

- **Chrome DevTools**: F12 → Elements → Styles
- **Vue DevTools**: Extensão do navegador para Vue.js
- **CSS Specificity Calculator**: https://specificity.keegan.st/
- **BEM Naming**: https://getbem.com/

---

## 🎉 Resultado Final

Após correções:
✅ Background gradient funciona
✅ Layout full-width
✅ Componentes não conflitam
✅ Navegação visível
✅ Grid de cosméticos renderiza corretamente
