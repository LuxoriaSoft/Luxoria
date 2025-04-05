# build stage
FROM node:23.11.0-alpine3.20 as build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# production stage
FROM nginx:1.27.4-alpine3.21
COPY --from=build /app/dist /usr/share/nginx/html
COPY docker-entrypoint.sh /
COPY nginx.conf /etc/nginx/conf.d/default.conf
RUN chmod +x /docker-entrypoint.sh
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]