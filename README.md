# Chat Server

> Note: This is a project for my own learning purposes

Minimal implementation of server for a chat application.

### Docker

```sh
$ docker-compose build
```

```sh
$ docker-compose up
```

### Notes

##### ChatServer

##### Broker / RabbitMQ

##### ClientRegister / Redis

##### Authentication


```
New client online:
 Authenticate
 Check if there is waiting messages for the client in the sent_queu

Server: New Message
 Validate
 Send update to client register
 Send message to broker

Broker: New message
 Get receiver server from client register
 Send to correct server
 Save message to sent_queue

Server: New receiver message
 Send to reveiver with WebSocket
 Send status to broker (success/fail)

Broker: Received message
 Can't use RabbitMQ ack as this would block handler completely
 Remove message from sent_queue


```

#### Routing

Routes: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#id7

Route constraints: 
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.1#route-constraint-reference


#### Performance

https://medium.com/@tampajohn/net-core-2-and-golang-797566350095

#### Integration tests

https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.1#customize-webapplicationfactory