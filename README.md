🎮 Fortnite Shop Challenge - Sistema ESO
Este projeto é uma solução Full Stack desenvolvida como parte do desafio técnico para o Sistema ESO. A aplicação simula uma loja virtual de cosméticos do jogo Fortnite, integrando-se a uma API externa para sincronização de dados e oferecendo um sistema completo de gestão de usuários, compras, devoluções e histórico de transações.

🚀 Demonstração Online (Deploy)
A aplicação está funcional e hospedada na plataforma Render.

🔗 Acesse aqui: https://eso-frontend.onrender.com

Por que o Render?
A escolha do Render para a infraestrutura de produção baseou-se em três pilares:

Suporte Nativo a Docker: Como a aplicação foi containerizada desde o início, o Render permitiu o deploy direto dos Dockerfiles do Frontend e Backend sem configurações complexas de ambiente.

PostgreSQL Gerenciado: A plataforma oferece uma instância de banco de dados PostgreSQL robusta e de fácil integração, eliminando a necessidade de manutenção de servidor de banco de dados.

CI/CD Integrado: O deploy é realizado automaticamente a partir da branch production do GitHub, garantindo que a versão online esteja sempre sincronizada com o código estável.

🛠️ Arquitetura e Tecnologias
O projeto foi construído visando escalabilidade, manutenibilidade e as melhores práticas de desenvolvimento moderno.

Backend: .NET 8 Web API
Utilizamos a arquitetura Feature-Driven Architecture (FDA) (Arquitetura Orientada a Funcionalidades).

Por que FDA? Ao contrário da arquitetura tradicional em camadas (Controllers, Services, Repositories), a FDA agrupa o código por contexto de negócio (ex: Features/Authentication, Features/Cosmetics, Features/Purchases). Isso aumenta a coesão, facilita a manutenção e permite que novas funcionalidades sejam adicionadas sem "quebrar" domínios não relacionados.

Destaques:

Entity Framework Core: ORM para manipulação de dados com PostgreSQL.

Auto-Migration: O sistema detecta e aplica migrações de banco de dados automaticamente ao iniciar, facilitando o deploy.

Background Services: Um serviço em segundo plano (HostedService) mantém os dados sincronizados com a API oficial do Fortnite.

JWT Authentication: Segurança robusta para proteção de rotas.

Frontend: Vue.js 3 + Vite
Composition API: Utilizada para uma lógica mais reutilizável e organizada.

TypeScript: Garante tipagem estática, reduzindo erros em tempo de desenvolvimento.

Pinia: Gerenciamento de estado moderno e leve, substituindo o Vuex.

Docker & Nginx: O frontend é buildado e servido através de um container Nginx otimizado para produção.

Banco de Dados
PostgreSQL: Escolhido pela sua robustez, conformidade ACID e excelente suporte no ecossistema .NET com Npgsql.

🐳 Como Rodar com Docker (Para Avaliadores)
O projeto está totalmente configurado com Docker Compose, permitindo que você suba todo o ambiente (Banco de Dados, Backend API e Frontend) com um único comando, sem a necessidade de instalar SDKs do .NET ou Node.js localmente.

## Pré-requisitos
- **Docker Desktop** instalado e rodando ([Download aqui](https://www.docker.com/products/docker-desktop))
- **Git** instalado ([Download aqui](https://git-scm.com/))

## Passo a Passo Completo

### 1️⃣ Clone o Repositório
Abra o terminal (PowerShell, CMD ou Git Bash) e execute:

```bash
git clone https://github.com/lucas-viana/ESOChallenge.git
cd ESOChallenge
```

### 2️⃣ Inicie os Containers
Na raiz do projeto (onde está o arquivo `docker-compose.yml`), execute:

```bash
docker-compose up --build
```

> **O que acontece neste comando?**
> - `docker-compose up`: Inicia todos os serviços definidos no docker-compose.yml
> - `--build`: Reconstrói as imagens Docker para garantir que as alterações mais recentes sejam aplicadas

### 3️⃣ Acompanhe a Inicialização
Você verá os logs dos 3 containers sendo iniciados:

1. **PostgreSQL** (`eso-postgres-db`): Banco de dados iniciando na porta 5433
2. **Backend API** (`eso-backend-api`): Aguardando o banco ficar pronto → Aplicando migrações → Sincronizando dados do Fortnite
3. **Frontend** (`eso-frontend-web`): Build do Vue.js → Servidor Nginx rodando

**Aguarde até ver estas mensagens importantes:**
- ✅ `Banco de dados migrado com sucesso!`
- 🔄 `Iniciando sincronização completa do Fortnite...`
- ✅ `Sincronização completa finalizada com sucesso!`

> ⏱️ **Tempo estimado**: 2-5 minutos na primeira execução (dependendo da conexão para baixar imagens Docker e sincronizar dados)

### 4️⃣ Acesse a Aplicação
Quando os logs mostrarem que tudo está pronto, acesse:

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Frontend (Aplicação Web)** | http://localhost:8080 | Interface principal da loja |
| **Backend (Swagger UI)** | http://localhost:8081/swagger | Documentação interativa da API |
| **Banco de Dados** | `localhost:5433` | PostgreSQL (use um cliente como DBeaver ou pgAdmin) |

### 5️⃣ Credenciais de Teste
O sistema inicia sem usuários pré-cadastrados. Para testar:

1. Acesse http://localhost:8080
2. Clique em **"Registrar"**
3. Crie uma conta (qualquer email/senha)
4. Você receberá automaticamente **10.000 V-Bucks** para testar as compras!

### 6️⃣ Para Parar os Containers
Pressione `Ctrl + C` no terminal onde o docker-compose está rodando, ou execute:

```bash
docker-compose down
```

### 7️⃣ Para Recomeçar do Zero (Resetar Banco de Dados)
Se quiser apagar todos os dados e começar novamente:

```bash
docker-compose down -v
docker-compose up --build
```

> **Atenção**: O parâmetro `-v` remove os volumes (incluindo dados do banco de dados)

Credenciais de Teste (Opcional)
Você pode registrar um novo usuário livremente na aplicação, mas caso queira testar rapidamente:

O sistema inicia limpo. Navegue até "Registrar", crie uma conta e você receberá automaticamente um saldo inicial de V-Bucks para testar as compras.

📋 Funcionalidades Implementadas
[x] Listagem de Cosméticos com paginação e filtros (Raridade, Tipo, Preço).

[x] Detalhes do Cosmético (Modal).

[x] Sistema de Autenticação (Login/Registro).

[x] Carteira de Usuário (Saldo em V-Bucks).

[x] Compra de Itens (Validação de saldo e duplicidade).

[x] Devolução de Itens (Reembolso de saldo).

[x] Histórico de Transações (Compras e Reembolsos).

[x] Página de Usuários (Lista pública de perfis).

[x] Sincronização automática com API externa.

Desenvolvido por Lucas Viana
