# Project4.2 - Twitter API Part 2
The aim of this project is to develop a twitter clone and demonstrate it using a client simulator. This is implemented using a server and client model. 
We have used WebSockets and JSON based API to establish server client connection. We are using Suave library to implement sockets. 
The server and client are executed on different terminals as different process. The structure is divided into three parts:
1. The Server: server mimics the functionality of a global server which servers all the requests made by the client. The server is webserver implementing sockets and APIs. The request includes functionalities like register, tweet etc.

2. The Client: The client here is a user. Multiple users can be registered at a time by executing the client from different terminal. se users are log in and log out randomly. Once they are logged in, they make requests to the server.

3. Twitter Engine: It helps in demonstrating the twitter functionality based on user inputs. The twitter engine is connected to the server here.

The twitter engine demonstrates the following functionalities – register a user, tweet, retweet, query hashtag, query subscribers, query mentions (other users). The operations are selected based on the inputs given by the user.
