﻿//// Learn more about F# at http://fsharp.org
//open System

//[<EntryPoint>]
//let main argv =
//    printfn "Hello World from F#!"
//    0 // return an integer exit code
// Learn more about F# at http://fsharp.org
open System

open TwitterEngine
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
let system = System.create "system" (Configuration.defaultConfig())
let twitterEngine=spawn system "TwitterEngine" Engine

type Group = {
    Name: string
    Age: int
}
let socketMap = new Dictionary<string,WebSocket>()
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
      // the message has type (Opcode * byte [] * bool)
      //
      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      //
      // byte [] contains the actual message
      //
      // the last element is the FIN byte, explained later
      
      | (Text, data, true) ->
        // the message can be converted to a string
        let str = UTF8.toString data
        
        let tempObject = JsonConvert.DeserializeObject<SampleObject>(str)
        socketMap.Add(tempObject.username,webSocket)
        let username = tempObject.username
        let response = sprintf "request from %s received" username
        printfn "%A" (tempObject)
        twitterEngine<!Sample(str)
        //printf "%s" response
        //do! socketMap.Item(username).send "messageReceived"

        // the response needs to be converted to a ByteSegment
        let byteResponse =
          response
          |> System.Text.Encoding.ASCII.GetBytes
          |> ByteSegment

        // the `send` function sends a message back to the client
        do! socketMap.Item(username).send Text byteResponse true

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

let Group = [|group1; group2|]

let app =
    choose
        [ 
        path "/sampleSocket" >=> handShake ws
        //GET >=> choose
        //    [ path "/" >=> OK "Index"
        //      path "/hello" >=> OK "Hello!"
        //      path "/londabhejo" >=> JSON londa
        //      path "/londebhejo" >=> JSON londe
        //      pathScan "/hello/%s" (fun (a) -> OK (sprintf "You passed: %s" ((a).ToString()))) ]
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