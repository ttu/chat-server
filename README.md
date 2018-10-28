# Chat Server

> Note: This is a project for my own testing purposes

Server for a chat application.

### Diagram

![diagram](./docs/chat-server.png)

### Getting Started

### Run with Docker Compose

```sh
$ docker-compose build
$ docker-compose up
```

### Run development environment

Install required systems:

```sh
# Redis
$ docker run -p 6379:6379 --name chat-redis -d redis

RabbitMQ
$ docker run -d --hostname my-rabbit --name chat-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# quick start/stop
$ docker start chat-redis && docker start chat-rabbit
$ docker stop chat-redis && docker stop chat-rabbit
```

### Notes

#### TODO

*
* 

#### Redis Commander

https://www.npmjs.com/package/redis-commander

```sh
$ npm install -g redis-commander
$ redis-commander
```

Open: http://localhost:8081/

##### ChatServer

...

##### Broker / RabbitMQ

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
