//// Learn more about F# at http://fsharp.org
//open System

//[<EntryPoint>]
//let main argv =
//    printfn "Hello World from F#!"
//    0 // return an integer exit code
// Learn more about F# at http://fsharp.org
open System

open MessageType
open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System
open System.Net
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Akka.FSharp
open MessageType
open System.Collections.Generic
open TwitterEngine
let system = System.create "system" (Configuration.defaultConfig())
let twitterEngine=spawn system "TwitterEngine" Engine

type Group = {
    Name: string
    Age: int
}
let connectionManager = new Dictionary<string,WebSocket>()
type Credentials = {
    Uid: string
    Password: string
}
//let responseObject1={response1="response1";response2="response2"}
//let responseObject2={response1="response1";response2="response2"}
//let responseObject3={response1="response1";response2="response2"}
//let responseList = new List<SampleResponseType>()
//responseList.Add(responseObject1)
//responseList.Add(responseObject2)
//responseList.Add(responseObject3)

let ws (webSocket : WebSocket) (context: HttpContext) =
  socket {
    // if `loop` is set to false, the server will stop receiving messages
    let mutable loop = true

    while loop do
      // the server will wait for a message to be received without blocking the thread
      let! msg = webSocket.read()
      match msg with
      // the message has type (Opcode * byte [] * bool)
      //
      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      //
      // byte [] contains the actual message
      //
      // the last element is the FIN byte, explained later
      
      | (Text, data, true) ->
        printfn "jdfvbjdsbsjdhvbjdsbvjdsbshdbvjshbvjdb"
        // the message can be converted to a string
        let str = UTF8.toString data
        let msgObject = JsonConvert.DeserializeObject<Tweet>(str)
        let username = msgObject.user
        let Type = msgObject.Type
        match Type with
        |"Connection"->
            connectionManager.Add(msgObject.user,webSocket)
        |"Tweet"->
            let subsListTsk = twitterEngine<?TweetMsg(msgObject)
            let responseList = Async.RunSynchronously(subsListTsk,10000)
            for subs in responseList do
                if(connectionManager.ContainsKey(subs)) then
                    let twt =JsonConvert.SerializeObject(msgObject)
                    printfn "tweet is %s" twt
                    let byteResponse =
                        twt
                        |> System.Text.Encoding.ASCII.GetBytes
                        |> ByteSegment
                    do! connectionManager.Item(subs).send Text byteResponse true
        |"Retweet" ->
            let subsListTsk = twitterEngine<?TweetMsg(msgObject)
            let responseList = Async.RunSynchronously(subsListTsk,10000)
            for subs in responseList do
                if(connectionManager.ContainsKey(subs)) then
                    let twt =JsonConvert.SerializeObject(msgObject)
                    printfn "tweet is %s" twt
                    let byteResponse =
                        twt
                        |> System.Text.Encoding.ASCII.GetBytes
                        |> ByteSegment
                    do! connectionManager.Item(subs).send Text byteResponse true
                
        //let response = sprintf "request from %s received" username
        //printfn "%A" (msgObject)
        ////let tsk = twitterEngine<?Register(username)
        ////let response = Async.RunSynchronously (tsk, 10000)
        //////printfn "[command]%s" serverOp
        ////printfn "[Reply]%s" (string(response))
        ////printf "%s" response
        ////do! socketMap.Item(username).send "messageReceived"
        //let response =JsonConvert.SerializeObject(responseList)
        //printfn " response is%s" response
        //// the response needs to be converted to a ByteSegment
        //let byteResponse =
        //  response
        //  |> System.Text.Encoding.ASCII.GetBytes
        //  |> ByteSegment

        //// the `send` function sends a message back to the client
        //do! connectionManager.Item(username).send Text byteResponse true

      | (Close, _, _) ->
        let emptyResponse = [||] |> ByteSegment
        do! webSocket.send Close emptyResponse true

        // after sending a Close message, stop the loop
        loop <- false

      | _ -> ()
    }

let JSON v =
    let jsonSerializerSettings = JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

let fromJson<'a> json =
  JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getCredsFromJsonString json =
  JsonConvert.DeserializeObject(json, typeof<Credentials>) :?> Credentials


let getResourceFromReq<'a> (req : HttpRequest) =
    let getString (rawForm: byte[]) = System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>

let parseCreds (req : HttpRequest) =
    let getString (rawForm: byte[]) = System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> getCredsFromJsonString


let loginUser (req: HttpRequest) = 
    let creds = parseCreds req
    if creds.Password = "lassan" then
        // OK "You're good!"
        handShake ws
    else
        OK "password not authenticated!"

// let rest resourceName (resource: 'a) =
//   let resourcePath = "/" + resourceName
//   let gellAll = resource.GetAll () |> JSON
//   path resourcePath >=> GET >=> getAll


let group1 = {
    Name="Kratika"
    Age=25
}

let group2 = {
    Name="Shri"
    Age=25
}
let MentionsQuery=
    fun(mention)->
        let tsk =twitterEngine<?QueryMentions(mention)
        let response = Async.RunSynchronously(tsk,10000)
        printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let HashTagQuery=
    fun(tag)->
        let tsk =twitterEngine<?QueryTag(tag)
        let response = Async.RunSynchronously(tsk,10000)
        printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let QueryAllSubs = 
    fun(user)->
        let tsk =twitterEngine<?QuerySubs(user)
        let response = Async.RunSynchronously(tsk,10000)
        printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let  GetAlltweets = 
    fun(s)->
        let tsk =twitterEngine<?AllTweets
        let response = Async.RunSynchronously(tsk,10000)
        printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let SubscribeApi=
    fun (user1,user2) ->
        let tsk = twitterEngine<?Subscribe(user1.ToString(),user2.ToString())
        let response = Async.RunSynchronously(tsk,10000)
        let reponseJson = {msg=response}
        let response = JsonConvert.SerializeObject(reponseJson)
        printfn "this is json string %s" response 
        OK response
let Register =
    fun (a) ->
        printfn "%s" (a.ToString())
        let tsk = twitterEngine<?Register(a.ToString())
        let response = Async.RunSynchronously(tsk,10000)
        let reponseJson = {msg=response}
        let response = JsonConvert.SerializeObject(reponseJson)
        printfn "this is json string %s" response 
        OK response
        //if(response = "Registration Successfull") then
        //    OK response
        //else
        //    BAD_REQUEST (sprintf "Username %s already taken" ((a).ToString()))
    //printfn "register request for %s received" username
let Group = [|group1; group2|]

let app =
    choose
        [ 
        path "/sampleSocket" >=> handShake ws
        GET >=> choose
            [ path "/" >=> OK "Index"
              path "/hello" >=> OK "Hello!"
              pathScan "/getAllTweets/%s" GetAlltweets
              pathScan "/Register/%s" Register
              pathScan "/Subscribe/%s/%s" SubscribeApi
              pathScan "/QuerySubs/%s" QueryAllSubs
              pathScan "/hashTagQuery/%s" HashTagQuery
              pathScan "/mentionsQuery/%s" MentionsQuery
            ]
        //  POST >=> choose
        //    [ 
        //        path "/hello" >=> OK "Hello POST!"
        //        path "/login" >=> request loginUser
        //         ]
                 ]

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    startWebServer defaultConfig app            
    0 // return an integer exit code
