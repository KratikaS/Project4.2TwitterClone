
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks;
open System.Text
open System
//load 
//open MessageType
let webSocket = new ClientWebSocket()
let t = webSocket.ConnectAsync(new System.Uri("ws://127.0.0.1:8080/sampleSocket"), CancellationToken.None)
while t.IsCompleted = false do
    printfn "connecting"
let data = "helloWorld"
let encodedData = Encoding.UTF8.GetBytes(data)
let buffer =  new ArraySegment<Byte>(encodedData,0,encodedData.Length)
let xt = webSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None)
while xt.IsCompleted=false do
    printfn "connecting"
    //while t.IsCompleted=false do


//    printfn "connecting"



