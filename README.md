# Chat Server

> Note: This project is for my own testing and a reference for Docker scripts

Server for a chat application.

## Diagram

![diagram](./docs/chat-server.png)

* ChatServer
  * _.NET Core Web API_
* ChatBroker
  * _.NET Core Web API_
  * Handles new messages from RabbitMQ
* Client
  * _React application_
  * Send and receive messages
* RabbitMQ
  * Queue for new messages 
* Redis
  * Client register
  * Has information on which server user is logged on
* TODO: Postgres
* TODO: Authentication

## Getting Started

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

# Debug -> Attach to process -> SSH (localhost:10222 root:Docker!) 
# Attach to dotnet exec process
# Select Managed Managed (.NET Core for Unix)
```


If you don't care about debugging from container, just start clean image with `dotnet watch`
```sh
$ docker run --rm -it -e "Connections__Redis=chat-redis" -e "Connections__RabbitMQ=chat-rabbit" -e "DOTNET_RUNNING_IN_CONTAINER=true" -p 5000:5000 --name chat-server --network=chat -v C:\src\GitHub\chat-server\src\ChatServer:/app/ -w /app microsoft/dotnet:2.1-sdk dotnet watch run
$ docker run --rm -it -e "Connections__Redis=chat-redis" -e "Connections__RabbitMQ=chat-rabbit" -e "DOTNET_RUNNING_IN_CONTAINER=true" -p 5000:5000 --name chat-server --network=chat -v C:\src\GitHub\chat-server\src\ChatBroker:/app/ -w /app microsoft/dotnet:2.1-sdk dotnet watch run
```

##### Debug chat-app React application with VS Code

`sourceMapPathOverrides` in `launch.json`must override folder structure in Docker container.

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "chrome",
      "request": "attach",
      "name": "Attach to Chrome",
      "port": 9222,
      "webRoot": "${workspaceFolder}/src",
      "sourceMapPathOverrides": {
        "/app/src/*": "${webRoot}/*"
      }
    }
  ]
}
```

### Production

```sh
# Build image
$ docker build -f Dockerfile-prod -t chatapp:prod .

# Start prod React app
$ docker run -it -p 800:80 --rm --name chat-app-prod chatapp:prod
```

## TODO

* Docker doesn't set ASPNETCORE_ENVIRONMENT correctly on development compose
* Authorization
* Save messages temporarily to DB
* When user logs in send not sent messages to him immediately
* Handle same user with multiple clients on multiple servers

## Notes
 
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


### Flow

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

### Links

#### Routing

* Routes: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#id7
* Route constraints: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#route-constraint-reference

#### Performance

* https://medium.com/@tampajohn/net-core-2-and-golang-797566350095

#### RabbitMQ life cycle

* https://www.rabbitmq.com/dotnet-api-guide.html#connection-and-channel-lifspan

#### Integration tests

* https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#customize-webapplicationfactory

### Docker networking

* https://runnable.com/docker/docker-compose-networking
* https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/multi-container-applications-docker-compose

### Docker

* https://mherman.org/blog/dockerizing-a-react-app/#production
* https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-dev-in-container.md#requirements

### VS Debug

* https://blog.quickbird.uk/debug-netcore-containers-remotely-9a103060b2ff
* https://stackoverflow.com/questions/48661857/how-to-debug-a-net-core-app-runnig-in-linux-docker-container-from-visual-studio