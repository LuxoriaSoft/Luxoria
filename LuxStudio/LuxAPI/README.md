# LuxAPI

## Requirements
- Docker (min. 27.4.0)
- Docker Compose

## Installation
1. Clone the repository
2. Run `docker-compose up` in the `LuxStudio` directory
3. Run `dotnet tool install --global dotnet-ef` in the `LuxAPI` directory
4. Run `dotnet ef migrations add InitialCreate` in the `LuxAPI` directory
5. Run `dotnet ef database update` in the `LuxAPI` directory
6. Run `dotnet run` in the `LuxAPI` directory