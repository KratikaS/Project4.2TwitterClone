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


let connectionManager = new Dictionary<string,WebSocket>()
type Credentials = {
    Uid: string
    Password: string
}


let ws (webSocket : WebSocket) (context: HttpContext) =
  socket {
    // if `loop` is set to false, the server will stop receiving messages
    let mutable loop = true

    while loop do
      // the server will wait for a message to be received without blocking the thread
      let! msg = webSocket.read()
      match msg with

      
      | (Text, data, true) ->
        let str = UTF8.toString data
        let msgObject = JsonConvert.DeserializeObject<Tweet>(str)
        let username = msgObject.user
        let Type = msgObject.Type
        match Type with
        |"Connection"->
            if connectionManager.ContainsKey(msgObject.user) = false then
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


(*let loginUser (req: HttpRequest) = 
    let creds = parseCreds req
    if creds.Password = "lassan" then
        // OK "You're good!"
        handShake ws
    else
        OK "password not authenticated!"*)
(*
let group1 = {
    Name="Kratika"
    Age=25
}

let group2 = {
    Name="Shri"
    Age=25
}*)

let LogoutUser = 
    fun(user)->
        connectionManager.Remove(user)
        OK "Logout Successful"
let MentionsQuery=
    fun(mention)->
        let tsk =twitterEngine<?QueryMentions(mention)
        let response = Async.RunSynchronously(tsk,10000)
        //printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let HashTagQuery=
    fun(tag)->
        let tsk =twitterEngine<?QueryTag(tag)
        let response = Async.RunSynchronously(tsk,10000)
        //printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let QueryAllSubs = 
    fun(user)->
        let tsk =twitterEngine<?QuerySubs(user)
        let response = Async.RunSynchronously(tsk,10000)
        //printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let  GetAlltweets = 
    fun(s)->
        let tsk =twitterEngine<?AllTweets
        let response = Async.RunSynchronously(tsk,10000)
        //printfn "this is the responsesdfsdf %A" response
        let responseJson = JsonConvert.SerializeObject(response)
        //printfn "this is json string %s" responseJson 
        OK responseJson
let SubscribeApi=
    fun (user1,user2) ->
        let tsk = twitterEngine<?Subscribe(user1.ToString(),user2.ToString())
        let response = Async.RunSynchronously(tsk,10000)
        let reponseJson = {msg=response}
        let response = JsonConvert.SerializeObject(reponseJson)
        //printfn "this is json string %s" response 
        OK response
let Register =
    fun (a) ->
        let tsk = twitterEngine<?Register(a.ToString())
        let response = Async.RunSynchronously(tsk,10000)
        let reponseJson = {msg=response}
        let response = JsonConvert.SerializeObject(reponseJson)
        OK response
 

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
              pathScan "/Logout/%s" LogoutUser
            ]
        //  POST >=> choose
        //    [ 
        //        path "/hello" >=> OK "Hello POST!"
        //        path "/login" >=> request loginUser
        //         ]
                 ]

[<EntryPoint>]
let main argv =
    printfn "Server starting...."
    startWebServer defaultConfig app            
    0 // return an integer exit code