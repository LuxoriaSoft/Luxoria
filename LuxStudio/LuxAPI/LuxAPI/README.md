# LuxAPI

## Requirements

- .NET 8
- PostgreSQL
- Docker (min. 27.4.0) *(optionnel pour déploiement)*
- Docker Compose *(optionnel pour déploiement)*

---

## Installation

### 1. Cloner le dépôt

```sh
git clone https://github.com/ton-repo/LuxAPI.git
cd LuxAPI
```

### 2. Configurer les variables d'environnement

Crée un fichier `.env` à la racine du projet et ajoute :

```ini
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=LuxDB;Username=postgres;Password=yourpassword
Jwt__Key=your-secret-key
Jwt__Issuer=https://your-api.com
Jwt__Audience=https://your-api.com
Minio__Endpoint=http://localhost:9000
Minio__AccessKey=your-access-key
Minio__SecretKey=your-secret-key
```

### 3. Restaurer les dépendances

```sh
dotnet restore
```

### 4. Appliquer les migrations

```sh
dotnet ef database update
```

### 5. Lancer l'API

```sh
dotnet run
```

---

## API Endpoints

### Authentication

| Méthode | Endpoint             | Description                                      |
|---------|----------------------|--------------------------------------------------|
| `POST`  | `/api/auth/login`    | Authentifie un utilisateur et retourne un JWT   |
| `POST`  | `/api/auth/register` | Crée un nouvel utilisateur                      |

### Collections

| Méthode | Endpoint                                         | Description                                        |
|---------|--------------------------------------------------|----------------------------------------------------|
| `GET`   | `/api/collection`                                | Récupère toutes les collections                    |
| `GET`   | `/api/collection/{id}`                           | Récupère une collection par ID                     |
| `POST`  | `/api/collection`                                | Crée une nouvelle collection                       |
| `PUT`   | `/api/collection/{id}`                           | Met à jour une collection par ID                   |
| `PATCH` | `/api/collection/{collectionId}/allowedEmails`   | Met à jour les emails autorisés dans la collection |
| `DELETE`| `/api/collection/{id}`                           | Supprime une collection par ID                     |
| `POST`  | `/api/collection/{collectionId}/upload`          | Télécharge des fichiers pour une collection        |
| `POST`  | `/api/collection/{collectionId}/chat`            | Ajoute un message au chat d'une collection         |

### Chat (SignalR)

**Hub Address**: `/hubs/chat`

#### Méthodes disponibles :

- `SendMessage(collectionId, senderEmail, message)` → Envoie un message à tous les clients du groupe
- `JoinCollection(collectionId)` → Rejoint un groupe SignalR pour une collection
- `LeaveCollection(collectionId)` → Quitte un groupe SignalR

---

## Running with Docker

### 1. Build and run the Docker containers

```sh
docker-compose up --build
```

### 2. Accéder à l'API

📌 [http://localhost:5269](http://localhost:5269)

### 3. Accéder à MinIO

📌 [http://localhost:9000](http://localhost:9000)

---

## Swagger Documentation

Swagger UI est disponible à l'adresse :
📌 [http://localhost:5269/swagger](http://localhost:5269/swagger)

---

