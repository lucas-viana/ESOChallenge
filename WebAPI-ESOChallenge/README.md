# WebAPI-ESOChallenge

## Overview
WebAPI-ESOChallenge is a web API project designed to manage Fortnite cosmetics and handle user authentication. The project is structured into independent modules for better organization and maintainability.

## Features

### Authentication Module
- **AuthController**: Handles authentication-related HTTP requests, such as login and registration.
- **Dtos**: Contains data transfer objects for authentication requests and responses.
  - `AuthResponse`: Represents the response structure for authentication requests.
  - `LoginRequest`: Represents the structure of the login request payload.
  - `RegisterRequest`: Represents the structure of the registration request payload.
- **Interfaces**: Defines contracts for authentication services.
  - `IAuthService`: Methods for user authentication operations.
  - `IJwtTokenGenerator`: Methods for generating JWT tokens.
- **Models**: Represents the user entity in the application.
  - `ApplicationUser`: Defines the user entity.
- **Services**: Implements authentication logic.
  - `AuthService`: Provides the logic for user authentication.
  - `JwtTokenGenerator`: Provides the logic for generating JWT tokens.

### Cosmetics Module
- **CosmeticsController**: Handles HTTP requests related to cosmetics in the application.
- **Dtos**: Contains data transfer objects for cosmetics.
  - `CosmeticsListResponse`: Represents the response structure for a list of cosmetics.
  - `FortniteApiResponse`: Represents the response structure from the Fortnite API.
- **Interfaces**: Defines contracts for cosmetic services.
  - `ICosmeticService`: Methods for managing cosmetics.
- **Models**: Represents cosmetic items in the application.
  - `Cosmetic`: Defines the cosmetic item entity.
- **Services**: Implements logic for managing cosmetics.
  - `CosmeticService`: Provides the logic for managing cosmetics.

## Data Layer
- **ApplicationDbContext**: Represents the database context for the application.
- **Migrations**: Contains migration logic for setting up the database schema.

## Services
- **HttpClientService**: Provides methods for making HTTP requests.
- **Interfaces**: Defines contracts for HTTP client services.

## Configuration
- **appsettings.json**: General configuration settings for the application.
- **appsettings.Development.json**: Configuration settings for the development environment.
- **launchSettings.json**: Settings for launching the application, including environment variables.

## Entry Point
- **Program.cs**: The entry point of the application, setting up the web host and configuring services.

## Getting Started
To run the project, ensure you have the necessary dependencies installed and follow the setup instructions in the `INTEGRATION_README.md` file.