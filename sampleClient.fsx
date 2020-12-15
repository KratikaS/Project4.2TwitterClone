#r "nuget: Akka.FSharp"
#r "nuget: Newtonsoft.Json"
#load "MessageType.fs"
open Newtonsoft.Json
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks;
open System.Text
open System
open MessageType
let tempObject={username="user1";val1="someVal1";val2="someVal2"}
let webSocket = new ClientWebSocket()
let t = webSocket.ConnectAsync(new System.Uri("ws://127.0.0.1:8080/sampleSocket"), CancellationToken.None)
while t.IsCompleted = false do
    printfn "connecting"
let data = "helloWorld"
let tempObjectBytes=JsonConvert.SerializeObject(tempObject)
printfn "%s" tempObjectBytes
let encodedData = Encoding.UTF8.GetBytes(tempObjectBytes)
let buffer =  new ArraySegment<Byte>(encodedData,0,encodedData.Length)
let xt = webSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None)
while xt.IsCompleted=false do
    printfn "connecting"
    //while t.IsCompleted=false do
let rcvBuffer =  new ArraySegment<Byte>(encodedData,0,encodedData.Length)
let rcvTsk = webSocket.ReceiveAsync(rcvBuffer,CancellationToken.None)
while rcvTsk.IsCompleted=false do
    ()
let response = Encoding.ASCII.GetString(rcvBuffer.Array,0,rcvTsk.Result.Count)
printf "%A" response
//    printfn "connecting"


//while true do
//    ()
