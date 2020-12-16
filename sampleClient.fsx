#load "MessageType.fs"
#r "nuget: Akka.FSharp"
#r "nuget: Suave"
#r "nuget: Newtonsoft.Json"
#r "System.Net.Http.dll"

open Newtonsoft.Json
open Akka.FSharp
open Akka.Actor
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks;
open System.Text
open System
open System.IO
open System.Net.Http
open MessageType
open System.Collections.Generic
let system = System.create "system" (Configuration.defaultConfig())
printfn "Please enter your username for Register/Login"

let username = System.Console.ReadLine()
let httpclient = new HttpClient()
let mutable webSocket = new ClientWebSocket()
let t = webSocket.ConnectAsync(new System.Uri("ws://127.0.0.1:8080/sampleSocket"), CancellationToken.None)
while t.IsCompleted = false do
    //printfn "connecting"

let tempObject = {Type="Connection";user=username;tweetText="";HashTag=new List<string>();Mentions=new List<string>()}
let tempObjectBytes=JsonConvert.SerializeObject(tempObject)
printfn "%s" tempObjectBytes
let encodedData = Encoding.UTF8.GetBytes(tempObjectBytes)
let buffer =  new ArraySegment<Byte>(encodedData,0,encodedData.Length)
let xt = webSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None)

let responseTsk = httpclient.GetAsync("http://localhost:8080/Register/"+username)
while responseTsk.IsCompleted = false do
    ()
let responseJson = responseTsk.Result
let content = responseJson.Content
let byteArray = content.ReadAsByteArrayAsync()
while byteArray.IsCompleted=false do
    ()
let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
let responseObj = JsonConvert.DeserializeObject<SampleResponseType>(jsonStr)
printfn "response is %A" responseObj
    
let mutable rcvBuffer =  WebSocket.CreateClientBuffer(1000000,1000000)

let cancel =true
async{
    
    while cancel do
        let rcvTsk = webSocket.ReceiveAsync(rcvBuffer,CancellationToken.None)
        while rcvTsk.IsCompleted=false && rcvTsk.Result.EndOfMessage=false do
            ()
        //printfn "task complete"
        //printfn "byte count%i" rcvTsk.Result.Count
        let response = Encoding.ASCII.GetString(rcvBuffer.Array,0,rcvTsk.Result.Count)
        if(response<>"") then
            printfn "this is new %s" response
        //webSocket.ReceiveAsync(rcvBuffer,CancellationToken.None)
}|>Async.Start

//let operation() = 
let mutable whileFlg = true 
while whileFlg do

    printfn "select the number according to the operation you want to perform: "
    printfn "1. Tweet"
    printfn "2. ReTweet"
    printfn "3. Query all who you have subscribed to"
    printfn "4. Query Hashtags"
    printfn "5. Query user mentioned in Tweet"
    printfn "6. Subscribe"
    printfn "7. Exit "
    let userInput = System.Console.ReadLine();
    
    let inputInt = userInput |>int
    match inputInt with
            |1 -> 
                printfn "Enter the text you want to Tweet"
                let TweetTxt = System.Console.ReadLine();
                printfn "Enter the hashtag you want to add"
                let hashTagTxt = System.Console.ReadLine();
                let hashTagArray = hashTagTxt.Split ','
                let mutable hashTagList = new List<string>()
                for i in hashTagArray do
                    hashTagList.Add(i.Remove(0,1))
                printfn "Enter the user id you want to mention"
                let userMention = System.Console.ReadLine();
                printfn "%s %s %s" TweetTxt hashTagTxt userMention
                let mentionsArray = userMention.Split '@'
                let mutable mentionsList = new List<string>()
                for i in 1 .. (mentionsArray.Length-1) do
                    mentionsList.Add(mentionsArray.[i])
                let twtMsg = {Type="Tweet";user=username;tweetText = TweetTxt; HashTag=hashTagList;Mentions=mentionsList}
                let twtMsgJson=string(JsonConvert.SerializeObject(twtMsg))
                //printfn "%s" tempObjectBytes
                let encodedTwt = Encoding.UTF8.GetBytes(twtMsgJson)
                let buffer =  new ArraySegment<Byte>(encodedTwt,0,encodedTwt.Length)
                webSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None)|>ignore

            |2->                 
                
                //printfn "Retweet done"
                let responseTsk = httpclient.GetAsync("http://localhost:8080/getAllTweets/allusers")
                while responseTsk.IsCompleted = false do
                    ()
                let responseJson = responseTsk.Result
                let content = responseJson.Content
                let byteArray = content.ReadAsByteArrayAsync()
                while byteArray.IsCompleted=false do
                    ()
                let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
                let responseObj = JsonConvert.DeserializeObject<Dictionary<int,Tweet>>(jsonStr)
                printfn "The Tweets are  %A" responseObj
                printfn "Enter index of tweet you wanna retweet"
                let userNum = System.Console.ReadLine(); 
                let userNumInt = userNum |>int
                let mutable twt = responseObj.Item(userNumInt)
                let rTwt = {Type="Retweet";user=username;tweetText=twt.tweetText;HashTag=twt.HashTag;Mentions=twt.Mentions}
                printfn "this is the retweet %A" rTwt
                let twtMsgJson=string(JsonConvert.SerializeObject(rTwt))
                //printfn "%s" tempObjectBytes
                let encodedTwt = Encoding.UTF8.GetBytes(twtMsgJson)
                let buffer =  new ArraySegment<Byte>(encodedTwt,0,encodedTwt.Length)
                webSocket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None)|>ignore

            |3 -> 
                let responseTsk = httpclient.GetAsync("http://localhost:8080/QuerySubs/"+username)
                while responseTsk.IsCompleted = false do
                    ()
                let responseJson = responseTsk.Result
                let content = responseJson.Content
                let byteArray = content.ReadAsByteArrayAsync()
                while byteArray.IsCompleted=false do
                    ()
                let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
                let responseObj = JsonConvert.DeserializeObject<List<Tweet>>(jsonStr)
                printfn "Tweets from the queried subscribers are "
                printfn "%A" responseObj
                
            |4 -> 
                printfn "Enter the hashTag name you want to search without #"
                let tag = System.Console.ReadLine();
                let responseTsk = httpclient.GetAsync("http://localhost:8080/hashTagQuery/"+tag)
                while responseTsk.IsCompleted = false do
                    ()
                let responseJson = responseTsk.Result
                let content = responseJson.Content
                let byteArray = content.ReadAsByteArrayAsync()
                while byteArray.IsCompleted=false do
                    ()
                let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
                let responseObj = JsonConvert.DeserializeObject<List<Tweet>>(jsonStr)
                printfn "Tweet containing queried hashtag are "
                printfn "%A" responseObj
                
            |5 ->
                printfn "enter the user whose mentions you wanna check"
                let mention = System.Console.ReadLine();
                let responseTsk = httpclient.GetAsync("http://localhost:8080/mentionsQuery/"+mention)
                while responseTsk.IsCompleted = false do
                    ()
                let responseJson = responseTsk.Result
                let content = responseJson.Content
                let byteArray = content.ReadAsByteArrayAsync()
                while byteArray.IsCompleted=false do
                    ()
                let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
                let responseObj = JsonConvert.DeserializeObject<List<Tweet>>(jsonStr)
                printfn "Tweets containing mentioned user are "
                printfn "%A" responseObj
            |6 ->
                printfn "Who do you want to subscribe?"
                let user2 = System.Console.ReadLine()|>string; 
                let responseTsk = httpclient.GetAsync("http://localhost:8080/Subscribe/"+username+"/"+user2)
                while responseTsk.IsCompleted = false do
                    ()
                let responseJson = responseTsk.Result
                let content = responseJson.Content
                let byteArray = content.ReadAsByteArrayAsync()
                while byteArray.IsCompleted=false do
                    ()
                let jsonStr = System.Text.Encoding.Default.GetString(byteArray.Result);
                let responseObj = JsonConvert.DeserializeObject<SampleResponseType>(jsonStr)
                printfn "response is %A" responseObj
            |7-> 
                whileFlg <- false 
                let responseTsk = httpclient.GetAsync("http://localhost:8080/Logout/"+username)
                while responseTsk.IsCompleted do
                    ()
                let tsk = webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,"Close",CancellationToken.None)
                while tsk.IsCompleted do
                    ()
            |_ ->
                printfn "Not a valid input"
                printf "nothing"

