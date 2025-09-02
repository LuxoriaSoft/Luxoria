FROM node:24.3.0-alpine3.22

WORKDIR /app

COPY package.json .
COPY package-lock.json .

RUN npm i

COPY . .

ENTRYPOINT [ "npm", "run", "dev" ]