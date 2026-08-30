FROM nginx:1.27-alpine

COPY nginx.conf /etc/nginx/nginx.conf
# Содержимое билда (папка build/) монтируется через volume в docker-compose
# для локальной разработки; для прод-образа можно раскомментировать строку
# ниже и собирать билд прямо в образ:
# COPY build/ /usr/share/nginx/html/

EXPOSE 8080
