# syntax=docker/dockerfile:1

FROM node:22-alpine AS build
WORKDIR /app
COPY . .
ARG VITE_API_URL
ARG VITE_HUB_URL
ENV VITE_API_URL=$VITE_API_URL VITE_HUB_URL=$VITE_HUB_URL
RUN npm install --no-audit --no-fund
RUN npm run build -w @licoreria/servicio

FROM nginx:alpine AS final
COPY apps/servicio/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/apps/servicio/dist /usr/share/nginx/html
EXPOSE 80
