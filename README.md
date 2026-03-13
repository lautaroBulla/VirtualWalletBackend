# 🏦 Virtual Wallet API

API RESTful que simula el núcleo transaccional de una billetera virtual. Diseñada con un fuerte enfoque en buenas prácticas de ingeniería, seguridad y consistencia de datos.

## 🏗️ Arquitectura y Tecnologías

El proyecto está estructurado utilizando **Clean Architecture**, asegurando un bajo acoplamiento y alta cohesión mediante la separación estricta de responsabilidades (Domain, Application, Infrastructure y Presentation).

* **Framework:** ASP.NET Core (C#)
* **Base de Datos:** SQL Server + Entity Framework Core (Code-First)
* **Seguridad:** Autenticación y Autorización basada en **JWT** (JSON Web Tokens).
* **Validaciones:** FluentValidation.
* **Testing:** xUnit y Moq (Suite de pruebas unitarias para casos de éxito y fallo).
* **Infraestructura:** Docker & Docker Compose.
* **Patrones de Diseño:** Repository Pattern, Unit of Work, Dependency Injection.

## ⚙️ Funcionalidades Principales

* **Auth & Seguridad:** Registro de usuarios con contraseñas encriptadas (Hash) y login con emisión de JWT.
* **Gestión de Cuentas:** Generación automática de identificadores de cuenta (CVU) y consulta segura de saldo.
* **Motor Transaccional ACID:** Operaciones de **Depósito, Retiro y Transferencia** entre cuentas, protegidas por transacciones de base de datos (`Commit`/`Rollback`) para evitar estados inconsistentes.
* **Historial:** Consulta paginada de movimientos.

---

## 🚀 Cómo ejecutar el proyecto localmente

El proyecto está dockerizado para que no necesites instalar dependencias locales (como SQL Server) en tu máquina. 

### Requisitos previos
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en ejecución.

### Pasos de instalación

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/tu-usuario/VirtualWalletBackend.git
   cd VirtualWalletBackend
   ```

2. **Levantar los contenedores:**
   Ejecuta el siguiente comando en la raíz del proyecto. Esto construirá la API y levantará una instancia de SQL Server. Las migraciones de base de datos **se aplicarán automáticamente** al iniciar.
   ```bash
   docker-compose up --build
   ```

3. **Probar la API (Swagger):**
   Una vez que la terminal indique que la aplicación está escuchando, abre tu navegador e ingresa a:
   👉 **http://localhost:8080/swagger/index.html**

*(Nota: Como la base de datos se crea desde cero en el contenedor, tu primer paso en Swagger debe ser usar el endpoint `POST /api/Auth/register` para crear un usuario y obtener tu Token JWT).*

---

## 🧪 Pruebas Unitarias

La capa de aplicación (`TransactionService`) cuenta con pruebas unitarias exhaustivas aisladas de la base de datos mediante Mocks. Para ejecutarlas:

```bash
dotnet test
```