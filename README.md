# Chat Server

> Note: This is a project for my own testing purposes

Server for a chat application.

### Diagram

![diagram](./docs/chat-server.png)

### Getting Started

### Run with Docker Compose

```sh
# Developemnt
$ docker-compose build
$ docker-compose up

# Production
$ docker-compose -f docker-compose-prod.yml build
$ docker-compose -f docker-compose-prod.yml up
```

### Run development environment

Install required containers:

```sh
# Create network
$ docker network create --attachable chat

# Redis
$ docker run -d --name chat-redis --network=chat -p 6379:6379 redis

# RabbitMQ
$ docker run -d --name chat-rabbit --network=chat -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Quick start/stop
$ docker start chat-redis && docker start chat-rabbit
$ docker stop chat-redis && docker stop chat-rabbit
```

Start project from _Visual Studio_.

### Run projects with Docker

Start debuggable projects in _containers_.

```sh
# Build image
$ docker build -t chatserver:dev .

# Start container
$ docker run --rm -it -e "Connections__Redis=chat-redis" -e "Connections__RabbitMQ=chat-rabbit" -e "DOTNET_RUNNING_IN_CONTAINER=true" -p 5000:5000 -p 10222:22 --name chat-server --network=chat -v C:\src\GitHub\chat-server\src\ChatServer:/app/ -w /app chatserver:dev

# Login to container
$ docker exec -it chat-server /bin/bash

# Start SSH server (TODO: should be always on)
$ service ssh start

# Debug -> Attach to process -> SSH (target: localhost:10222 root:Docker!) 
# Attach to dotnet exec process
# Select Managed (.NET Core...)
```


If you don't care about debugging from container, just start clean image with `dotnet watch`
```sh
$ docker run --rm -it -e "Connections__Redis=chat-redis" -e "Connections__RabbitMQ=chat-rabbit" -e "DOTNET_RUNNING_IN_CONTAINER=true" -p 5000:5000 --name chat-server --network=chat -v C:\src\GitHub\chat-server\src\ChatServer:/app/ -w /app microsoft/dotnet:2.1-sdk dotnet watch run
$ docker run --rm -it -e "Connections__Redis=chat-redis" -e "Connections__RabbitMQ=chat-rabbit" -e "DOTNET_RUNNING_IN_CONTAINER=true" -p 5000:5000 --name chat-server --network=chat -v C:\src\GitHub\chat-server\src\ChatBroker:/app/ -w /app microsoft/dotnet:2.1-sdk dotnet watch run
```

### Notes

#### TODO

* Production build for React app
* Authorization
* Save messages temporarily to DB
* Send not sent messages to logged in user
* Handle client on multiple servers
 
#### Redis Commander

https://www.npmjs.com/package/redis-commander

```sh
$ npm install -g redis-commander
$ redis-commander
```

Open: http://localhost:8081/

##### ChatServer

...

##### ChatBroker / RabbitMQ

...

##### ClientRegister / Redis

...

##### Authentication

...


#### Flow

```
# Client logs in

ChatServer: Login
 Authenticate
 Send update to client register
 Check if there is waiting messages for the client in the sent_queue

# Client sends a new message

ChatServer: New message /api/send
 Validate
 Send update to client register
 Send message to RabbitMQ

ChatBroker: New message from RabbitMQ
 Get receiver server from client register
 Save message to sent_queue
 Send to correct server /api/receive
  Success
   Remove message from sent_queue
  Fail
   Update message status to sent_queue_

ChatServer: New message /api/receive
 Send to receiver with WebSocket
 Send status to broker (success/fail)

 


```

#### Routing

Routes: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#id7

Route constraints: 
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#route-constraint-reference


#### Performance

https://medium.com/@tampajohn/net-core-2-and-golang-797566350095

#### Integration tests

https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#customize-webapplicationfactory

### Docker networking

https://runnable.com/docker/docker-compose-networking

https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/multi-container-applications-docker-compose

https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md#requirements