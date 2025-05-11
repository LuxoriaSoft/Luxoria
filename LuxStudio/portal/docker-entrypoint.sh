#!/bin/sh
# Replace placeholder with environment variable
sed -i "s|__API_URL__|${API_URL:-http://localhost:5269}|g" /usr/share/nginx/html/config.js
exec "$@"