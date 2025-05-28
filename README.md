# Build it using C#

[![progress-banner](https://backend.codecrafters.io/progress/http-server/3357626b-124f-4d56-9090-900013a94a0c)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

---

This repository contains a C# implementation of an HTTP/1.1 server. It is capable of handling multiple client connections concurrently and supports various HTTP features, including request parsing, header processing, file serving (from a specified directory), and Gzip content compression. This project was developed based on the CodeCrafters "Build your own HTTP server" challenge.

## Implemented Features

- [x] Bind to a port
- [x] Respond with 200
- [x] Extract with 200
- [x] Extract URL path
- [x] Respond with body
- [x] Read header
- [x] Concurrent connections
- [x] Return a file
- [x] Read request body
- [x] Compression headers

## Usage Examples

### Root Path

Responds with a simple 200 OK for the root path (`/`).

```http
GET / HTTP/1.1
Host: localhost:4221

HTTP/1.1 200 OK
Connection: keep-alive
```

### Echo Service

The `/echo/{message}` endpoint returns the `{message}` part of the URL path in the response body. It supports Gzip compression if the client includes `Accept-Encoding: gzip` in the request headers.

**Without Gzip compression:**
```http
GET /echo/hello-world HTTP/1.1
Host: localhost:4221

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 11
Connection: keep-alive

hello-world
```

**With Gzip compression:**
```http
GET /echo/hello-world HTTP/1.1
Host: localhost:4221
Accept-Encoding: gzip

HTTP/1.1 200 OK
Content-Encoding: gzip
Content-Type: text/plain
Content-Length: 30
Connection: keep-alive

[compressed content of "hello-world"]
```

### User-Agent Information

The `/user-agent` endpoint returns the value of the `User-Agent` header from the request body.

```http
GET /user-agent HTTP/1.1
Host: localhost:4221
User-Agent: (Windows NT 10.0; Win64; x64)

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 41
Connection: keep-alive

 (Windows NT 10.0; Win64; x64)
```

### File Operations

The server can serve files from a directory specified using the `--directory <path>` command-line argument. The `/files/{filename}` endpoint allows for creating files (via POST) and retrieving files (via GET).

**Creating a file (POST):**
The content of the request body will be written to the specified file.
```http
POST /files/tmp.txt HTTP/1.1
Host: localhost:4221
Content-Length: 18

This is a test file

HTTP/1.1 201 Created
Connection: keep-alive
```

**Retrieving a file (GET):**
The content of the specified file will be returned in the response body.
```http
GET /files/tmp.txt HTTP/1.1
Host: localhost:4221

HTTP/1.1 200 OK
Content-Type: application/nest-kit
Content-Length: 18
Connection: keep-alive

This is a test file
```

**Attempting to retrieve a non-existent file (GET):**
```http
GET /files/nonexistent.txt HTTP/1.1
Host: localhost:4221

HTTP/1.1 404 Not Found
Connection: keep-alive
```

## Building and Running

### Prerequisites

- .NET SDK (Version 9.0 or compatible, as specified in `codecrafters-http-server.csproj` and `codecrafters.yml`)

### Building

To build the project, navigate to the repository root and run:
```bash
dotnet build --configuration Release --output /tmp/codecrafters-build-http-server-csharp codecrafters-http-server.csproj
```
This command compiles the server and places the output in the `/tmp/codecrafters-build-http-server-csharp` directory (the exact output path might vary slightly based on your OS and .NET setup).

### Running

The server can be run using the provided shell script `your_program.sh`, which handles the build process and then executes the server:
```bash
./your_program.sh
```

Alternatively, you can run the compiled executable directly after building:
```bash
/tmp/codecrafters-build-http-server-csharp/codecrafters-http-server
```

By default, the server listens on port 4221.

**Serving files from a directory:**

To enable file serving capabilities (for the `/files/` endpoint), start the server with the `--directory` argument, specifying the path to the directory from which files should be served:
```bash
./your_program.sh --directory /path/to/your/files
```
Or, if running the executable directly:
```bash
/tmp/codecrafters-build-http-server-csharp/codecrafters-http-server --directory /path/to/your/files
```

## Project Structure

-   `codecrafters-http-server.csproj`: The C# project file defining dependencies, target framework (.NET 9.0), and build settings.
-   `codecrafters-http-server.sln`: Visual Studio solution file for managing the project.
-   `src/`: This directory contains all the C# source code for the HTTP server.
    -   `Program.cs`: The main entry point for the application. It parses command-line arguments (e.g., `--directory`) and initializes and starts the `HttpServer`.
    -   `HttpServer.cs`: Implements the TCP listener that accepts incoming client connections. It handles each client connection in a separate task to support concurrency.
    -   `HttpRequest.cs`: Defines a class responsible for parsing raw HTTP request strings from a client into a structured `HttpRequest` object. This includes extracting the HTTP method, path, headers (like `User-Agent`, `Accept-Encoding`, `Connection`), and the request body.
    -   `HttpResponse.cs`: Defines a class for constructing HTTP responses. It allows setting the status code, status message, headers, and body (either as a string or raw bytes). It also includes a method to serialize the response into a byte array suitable for sending over a socket.
    -   `IRequestHandler.cs`: An interface defining the contract for request handling logic. This promotes separation of concerns for processing different types of requests.
    -   `RequestHandler.cs`: The primary implementation of `IRequestHandler`. It contains the core logic to route incoming `HttpRequest` objects based on their path and method to appropriate handlers (e.g., handling requests for `/`, `/echo/*`, `/user-agent`, and `/files/*`). It is responsible for generating the corresponding `HttpResponse`.
-   `your_program.sh`: A utility shell script provided for easily building and running the server locally.
-   `.codecrafters/`: This directory contains scripts specifically used by the CodeCrafters platform for automated testing and execution of the project.
    -   `compile.sh`: Script used by CodeCrafters to compile the C# project.
    -   `run.sh`: Script used by CodeCrafters to execute the compiled server.
-   `codecrafters.yml`: Configuration file for the CodeCrafters platform, specifying settings like the C# language version (`dotnet-9.0`) and debug log preferences.
