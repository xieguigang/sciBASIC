# IServicesSocket
_namespace: [Microsoft.VisualBasic.Net.Abstract](./index.md)_

Socket listening object which is running at the server side asynchronous able multiple threading.
 (运行于服务器端上面的Socket监听对象，多线程模型)



### Methods

#### Run
```csharp
Microsoft.VisualBasic.Net.Abstract.IServicesSocket.Run(System.Net.IPEndPoint)
```
This server waits for a connection and then uses asychronous operations to
 accept the connection, get data from the connected client,
 echo that data back to the connected client.
 It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)


### Properties

#### LocalPort
The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
