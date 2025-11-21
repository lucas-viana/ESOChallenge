FROM node:20-alpine AS build
WORKDIR /app

# Build argument para a URL da API
ARG VITE_API_BASE_URL=http://localhost:8081

# Define como variável de ambiente durante o build
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

COPY package*.json ./
RUN npm install

COPY . .

RUN npm run build

FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]