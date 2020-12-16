
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

let liveFeedActor(mailbox:Actor<_>)=
    let rec loop(x) = actor {
        let! msg=mailbox.Receive()
        match msg with
            |"live"->
                let mutable rcvTsk = webSocket.ReceiveAsync(rcvBuffer,CancellationToken.None)
                while rcvTsk.IsCompleted=false && rcvTsk.Result.EndOfMessage=false do
                    ()
                printfn "task complete"
                printfn "byte count%i" rcvTsk.Result.Count
                let response = Encoding.ASCII.GetString(rcvBuffer.Array,0,rcvTsk.Result.Count)
                System.Console.WriteLine("this is new ",response)
                mailbox.Self<!"live"
        return! loop(x)
    }
    loop(0)
let operation() = 
    let mutable whileFlg = true 
    while whileFlg do
          printfn "select the number according to the operation you want to perform"
          printfn "1. Tweet"
          printfn "2. ReTweet"
          printfn "3. Query hashtag"
          printfn "4. Query mention"
          printfn "5. Exit"
          let userInput = System.Console.ReadLine();
          let inputInt = userInput |>int
          //let x = Some(int32(userInput)) |>ignore
          printfn "%i" inputInt
          match inputInt with
                 |1 -> 
                     printfn "Enter the text you want to Tweet"
                     let TweetTxt = System.Console.ReadLine();
                     printfn "Enter the hashtag you want to add"
                     let hashTagTxt = System.Console.ReadLine();
                     printfn "Enter the user id you want to mention"
                     let userMention = System.Console.ReadLine();
                     printfn "%s %s %s" TweetTxt hashTagTxt userMention
                 |2-> 
                     printfn "Enter the user whose tweet you want to Retweet"
                     let userNum = System.Console.ReadLine(); 
                     let userNumInt = userNum |>int
                     printfn "Retweet done"
                 |3 -> 
                     printfn "Query hashtag"
                 |4 -> 
                     printfn "Query hashtag"
                 |5 ->
                     printfn "write code to exit"  
                     whileFlg <- false  
                 |_ ->
                     printfn "Not a valid input"
                     printf "nothing"

printfn "Do you want to register the user? write yes or no"

let registerVariable = System.Console.ReadLine();
let registerValue = registerVariable |>string
//printfn "%s" registerVariable 
match registerValue with
 |"yes" -> 
     printf "Enter username for registering: "
     let userInput = System.Console.ReadLine();
     //add username in map
     printfn "user registered"
     operation()
 |"no"-> 
     printf "Enter userId to Login: "
     let loginInput = System.Console.ReadLine();
     //write code to authenticate user
     operation()
 |_ -> printfn "nothing boss"



let tempObject={username="user2";val1="someVal1";val2="someVal2"}
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
let rcvBuffer =  WebSocket.CreateClientBuffer(1000000,1000000)
let rcvTsk = webSocket.ReceiveAsync(rcvBuffer,CancellationToken.None)
while rcvTsk.Result.EndOfMessage do
    ()
let response = Encoding.ASCII.GetString(rcvBuffer.Array,0,rcvTsk.Result.Count)
printf "%A" response
//    printfn "connecting"


while true do
    ()
