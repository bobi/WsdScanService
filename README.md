# WsdScanService

## Project Overview

This project is a .NET-based implementation of a WSD (Web Services for Devices) Scan Service. It allows clients to discover and interact with scanners over the network using the WSD protocol. The solution is divided into several projects:

*   **WsdScanService.Host:** The main application that hosts the WCF services and manages the scanner devices.
*   **WsdScanService.Scanner:** Implements the core WSD Scan protocol using CoreWCF.
*   **WsdScanService.Contracts:** Contains the data contracts and service interfaces for the WSD Scan protocol.
*   **WsdScanService.Discovery:** Implements the WS-Discovery protocol for device discovery.
*   **WsdScanService.Common:** Contains common utilities and extension methods used across the solution.

The application is designed to be run as a containerized service using Docker.

## Building and Running

The project can be built and run using the .NET SDK and Docker.

### Building the Docker Image

To build the Docker image, run the following command from the root of the project:

```bash
docker build -t wsdscanservice .
```

### Running the Service

The service can be run using Docker Compose:

```bash
docker-compose up
```

The service will be available at the IP address and port specified in the `appsettings.json` file.

### Development

To run the project in a development environment, you can use the .NET SDK:

```bash
dotnet run --project WsdScanService.Host/WsdScanService.Host.csproj
```

## Development Conventions

*   **Dependency Injection:** The project uses the built-in dependency injection container in .NET. Services are registered in the `Program.cs` file and in extension methods in each project.
*   **Configuration:** The application is configured using `appsettings.json` files and environment variables.
*   **Logging:** The project uses the standard .NET logging framework.
*   **WCF:** The project uses CoreWCF for implementing the WSD Scan protocol.
*   **WS-Discovery:** The project implements the WS-Discovery protocol for device discovery.
