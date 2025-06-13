export MINIO_ACCESS_KEY=$(openssl rand -hex 8)   # 16-char hex string
export MINIO_SECRET_KEY=$(openssl rand -hex 16)  # 32-char hex string

kubectl create secret generic minio-secret \
  --from-literal=MINIO_ROOT_USER=$MINIO_ACCESS_KEY \
  --from-literal=MINIO_ROOT_PASSWORD=$MINIO_SECRET_KEY
