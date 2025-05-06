[![progress-banner](https://backend.codecrafters.io/progress/http-server/3357626b-124f-4d56-9090-900013a94a0c)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

[HTTP](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) is the
protocol that powers the web. In this challenge, you'll build a HTTP/1.1 server
that is capable of serving multiple clients.

Along the way you'll learn about TCP servers,
[HTTP request syntax](https://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html),
and more.

## Resources needed for a complete understanding:

- https://www.cloudflare.com/en-ca/learning/ddos/glossary/tcp-ip/

- https://developer.mozilla.org/en-US/docs/Glossary/CRLF

- https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/User-Agent

- https://en.wikipedia.org/wiki/HTTP_compression

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

### Echo Service

```http
GET /echo/hello-world HTTP/1.1
Host: localhost:4221
Accept-Encoding: gzip

HTTP/1.1 200 OK
Content-Encoding: gzip
Content-Type: text/plain
Content-Length: 30

[compressed content]
```

Without compression:
```http
GET /echo/hello-world HTTP/1.1
Host: localhost:4221

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 11

hello-world
```

### User-Agent Information

```http
GET /user-agent HTTP/1.1
Host: localhost:4221
User-Agent: (Windows NT 10.0; Win64; x64)

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 41

 (Windows NT 10.0; Win64; x64)
```

### File Operations

Creating a file:
```http
POST /files/tmp.txt HTTP/1.1
Host: localhost:4221
Content-Length: 18

This is a test file

HTTP/1.1 201 Created
```

Retrieving a file:
```http
GET /files/tmp.txt HTTP/1.1
Host: localhost:4221

HTTP/1.1 200 OK
Content-Type: application/nest-kit
Content-Length: 18
