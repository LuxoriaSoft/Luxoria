# -------- Build Stage --------
# Use the official Node.js Alpine image as the base for building the app
FROM node:23.11.0-alpine3.20 as build

# Set the working directory inside the container
WORKDIR /app

# Copy only package.json and package-lock.json to install dependencies
COPY package*.json ./

# Install Node.js dependencies
RUN npm install

# Copy the rest of the application source code
COPY . .

# Build the application (e.g., Vue, React, etc.)
RUN npm run build

# -------- Production Stage --------
# Use the official Nginx Alpine image to serve the built app
FROM nginx:1.27.4-alpine3.21

# Copy built static files from the previous build stage to Nginx's default public directory
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom Docker entrypoint script
COPY docker-entrypoint.sh /

# Copy custom Nginx configuration for SPA routing support
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Make the entrypoint script executable
RUN chmod +x /docker-entrypoint.sh

# Run the custom entrypoint script
ENTRYPOINT ["/docker-entrypoint.sh"]

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]