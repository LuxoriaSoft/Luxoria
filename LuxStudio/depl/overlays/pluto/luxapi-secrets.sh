kubectl create secret generic luxapi-secrets \
  --from-literal=smtp-user='<your-smtp-username>' \
  --from-literal=smtp-password='<your-smtp-password>' \
  --namespace=luxstudio-pluto
