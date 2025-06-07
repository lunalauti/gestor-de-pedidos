# Gestor de Pedidos

Este proyecto utiliza Docker Compose para orquestar los servicios de backend (ASP.NET Core), PostgreSQL y RabbitMQ.

---

## Requisitos Previos

Asegúrate de tener instalado en tu sistema:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (incluye Docker Engine y Docker Compose)
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download)

---

## Cómo correr el proyecto

### 1. Navegar al directorio raíz del proyecto

Abre una terminal y ubícate en la carpeta donde se encuentra el archivo `docker-compose.yml`:

```bash
cd /ruta/a/tu/proyecto/gestor-de-pedidos
```

---

### 2. Generar migraciones de Entity Framework Core  
(Solo la primera vez o cuando cambie el esquema)

Requiere tener el .NET SDK 8.0 instalado.

#### a. Entrar al directorio del backend:

```bash
cd backend
```

#### b. Crear una nueva migración:

```bash
dotnet ef migrations add <NombreDeTuNuevaMigracion>
```

Ejemplo:

```bash
dotnet ef migrations add InitialCreate
```

#### c. Volver al directorio raíz del proyecto:

```bash
cd ..
```

---

### 3. Construir las imágenes de Docker

Este comando compila la aplicación y construye las imágenes necesarias:

```bash
docker-compose build
```

---

### 4. Iniciar todos los servicios

Este comando levanta el backend, PostgreSQL y RabbitMQ en segundo plano:

```bash
docker-compose up -d
```

---

## Acceso a los servicios

| Servicio            | URL / Acceso                                     |
|---------------------|--------------------------------------------------|
| Backend             | http://localhost:5001                            |
| RabbitMQ Admin      | http://localhost:15672                           |
|                     | Usuario: `guest` / Contraseña: `guest`          |
| PostgreSQL          | Puerto: `5433` (local) → `5432` (contenedor)     |
|                     | Usuario: `postgres`                              |
|                     | Contraseña: `postgres123`                        |
|                     | Base de datos: `OrderManagementDB`              |

---

## Comandos útiles

### Detener y eliminar todos los servicios

```bash
docker-compose down
```

Los volúmenes persisten, por lo que no perderás los datos.

---

### Ver logs de un servicio (por ejemplo, el backend)

```bash
docker-compose logs backend -f
```

Presiona `Ctrl + C` para salir.

---

### Ejecutar el backend localmente sin Docker

Asegúrate de tener:

- El SDK de .NET 8.0 instalado
- PostgreSQL y RabbitMQ corriendo localmente o mediante `docker-compose up -d`

Luego, navega al directorio `backend` y ejecuta:

```bash
dotnet run
```
