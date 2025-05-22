#!/bin/sh
# Replace placeholder with environment variable
sed -i "s|http://localhost:5269|${API_URL:-http://localhost:5269}|g" /usr/share/nginx/html/config.js
exec "$@"