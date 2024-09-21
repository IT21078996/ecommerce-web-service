# Ecommerce Web API

This is an **Ecommerce Web API** built with **ASP.NET Core 8.0**, utilizing **MongoDB** as the database, and implementing **JWT-based authentication** for security. The API provides functionalities for managing products, orders, users, vendors, and inventory, along with API documentation through Swagger.

## Technologies Used

- **.NET Core 8.0**: Latest version of .NET for building scalable web APIs.
- **MongoDB.Driver 2.29.0**: MongoDB client for interacting with a MongoDB database.
- **JWT Authentication**: Token-based authentication using `Microsoft.AspNetCore.Authentication.JwtBearer`.
- **Swagger (Swashbuckle.AspNetCore)**: Automatically generates API documentation and testing UI.
- **Code Generation**: `Microsoft.VisualStudio.Web.CodeGeneration.Design` for scaffolding controllers and views.

## Features

- **JWT Authentication**: Secures API endpoints with token-based authentication.
- **MongoDB Integration**: Manages data with MongoDB's flexible, document-based database.
- **CRUD Operations**: Supports Create, Read, Update, and Delete operations for Products, Orders, Users, Vendors, and Inventory.
- **Swagger UI**: Includes auto-generated API documentation for easy testing and integration.

## Prerequisites

- **.NET 8.0 SDK** (Download from [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)).
- **MongoDB** (Download from [here](https://www.mongodb.com/try/download/community)).
- A development environment like **Visual Studio 2022** or **VS Code**.

## Setup and Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/IT21078996/ecommerce-web-service.git
   ```

2. Navigate to the project directory:
   ```bash
   cd EcommerceWebAPI
   ```

3. Build and run the project using Visual Studio or the .NET CLI:
   ```bash
   dotnet build
   dotnet run
   ```


## License

This project is licensed under the MIT License.
