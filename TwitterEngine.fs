module TwitterEngine

//#time
//#r "nuget: Akka.FSharp"
//#r "nuget: Akka.Remote"
//#r "nuget: Akka.Serialization.Hyperion"
//#r "nuget: MathNet.Numerics"
//#load "MessageType.fs"
open System

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
//let config =  
//    Configuration.parse
//        @"akka {
//            actor.serializers{
//                wire  = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
//            }
//            actor.serialization-bindings {
//                ""System.Object"" = wire 
//            }
//            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
//            remote {
//                maximum-payload-bytes = 30000000 bytes
//                dot-netty.tcp {
//                    hostname = 127.0.0.1
//                    port = 9004
//                    message-frame-size =  30000000b
//                    send-buffer-size =  30000000b
//                    receive-buffer-size =  30000000b
//                    maximum-frame-size = 30000000b
//                    tcp-reuse-addr = off
//                }
//            }
//        }"



let mutable totalOperations=0
let mutable finalOperations=0;
let mutable maxSubs=0;
let mutable numQueries=0;
let mutable numTweets=0;
let system = System.create "system" (Configuration.defaultConfig())
//let system = System.create "RemoteFSharp" config
let dummyActor(mailbox:Actor<_>)=
    let rec loop(x) = actor {
        let! msg=mailbox.Receive()
        match msg with
            |_->printfn ""
        return! loop(x)
    }
    loop(0)
let dummyRef=spawn system "dummy" dummyActor
let mutable printerActor=dummyRef
//let system = System.create "system" (Configuration.defaultConfig())
let sampleActor(mailbox:Actor<_>)=
    let rec loop()=actor{
        let! msg=mailbox.Receive();
        printfn "%A" msg
        return! loop()
    }
    loop()
let sample=spawne system "sample" <@ sampleActor @>[]
let Engine(mailbox:Actor<_>)=
    let mutable AllTweetsSet = new Dictionary<int,Tweet>()
    let mutable users = new HashSet<string>()
    let mutable usersLoggedin = new HashSet<string>()
    let mutable TweetDictionary=new Dictionary<string,Tweet>()
    let mutable tweetMsgMap = new Dictionary<String,HashSet<Tweet>>()
    let mutable mentionsMap = new Dictionary<String,HashSet<Tweet>>()
    let mutable hashTagMap = new Dictionary<String,HashSet<Tweet>>()
    let mutable SubscriberList=new List<String>()
    let mutable subscribedTo=new Dictionary<string,List<string>>()
    let mutable Subscribers=new Dictionary<String,List<String>>()
    let rec loop() = actor {
        let! msg=mailbox.Receive()
        match msg with
            |StartTimers->
                ()
            |Sample(s)->
                printfn "Sample Message"
                //mailbox.Sender()<!"Done"
            |Register(username)->
                printfn "registering"
                //if users.Contains(username) then
                //    let response = "username is already taken"
                //    mailbox.Sender() <? response |> ignore
                //else
                //    users.Add(username)|>ignore
                //    let response = "Registration Successfull"
                //    mailbox.Sender() <? response |> ignore
                users.Add(username)|>ignore
                let response = "Registration Successfull"
                mailbox.Sender() <? response |> ignore

                
                //let byteResponse =
                //    response
                //    |> System.Text.Encoding.ASCII.GetBytes
                //    |> ByteSegment
                //do! webSocket.send Text byteResponse true 
               
            |TweetMsg(twt)->
                AllTweetsSet.Add(AllTweetsSet.Count,twt)
                let user = twt.user
                let hashTagList = twt.HashTag
                let mentionsList = twt.Mentions
                let tweetSet = new HashSet<Tweet>()
                let tagSet = new HashSet<Tweet>()
                let mentionSet = new HashSet<Tweet>()
                if tweetMsgMap.ContainsKey(user) then
                    tweetMsgMap.Item(user).Add(twt)|>ignore
                else
                    tweetMsgMap.Add(user,tweetSet)|>ignore
                    tweetMsgMap.Item(user).Add(twt)|>ignore
                for tag in hashTagList do
                    if hashTagMap.ContainsKey(tag) then
                        hashTagMap.Item(tag).Add(twt)|>ignore
                    else
                        hashTagMap.Add(tag,tagSet)
                        hashTagMap.Item(tag).Add(twt)|>ignore
                for mentions in mentionsList do
                    if mentionsMap.ContainsKey(mentions) then  
                        mentionsMap.Item(mentions).Add(twt)|>ignore
                    else
                        mentionsMap.Add(mentions,mentionSet)
                        mentionsMap.Item(mentions).Add(twt)|>ignore
                let mutable responseList = new List<string>()
                if(Subscribers.ContainsKey(user)) then
                    responseList <- Subscribers.Item(user)
                mailbox.Sender()<!responseList|>ignore
            //user1 subscribes to user2       
            |Subscribe(user1,user2)->
                let subsList = new List<string>()
                let subsToList = new List<string>()
                if subscribedTo.ContainsKey(user1) then
                    subscribedTo.Item(user1).Add(user2)
                else 
                    subscribedTo.Add(user1,subsToList)
                    subscribedTo.Item(user1).Add(user2)
                if Subscribers.ContainsKey(user2) then
                    Subscribers.Item(user2).Add(user1)
                else
                    Subscribers.Add(user2,subsList)
                    Subscribers.Item(user2).Add(user1)
                mailbox.Sender()<!(sprintf "You successfully subscribed %s" user2)
            |QuerySubs(userName)->
                let tweetList=new List<Tweet>()
                if(subscribedTo.ContainsKey(userName))then
                    let followingList=subscribedTo.Item(userName)
                    for i in followingList do
                        if(tweetMsgMap.ContainsKey(i))then
                            for j in tweetMsgMap.Item(i) do
                                tweetList.Add(j)
                mailbox.Sender()<?tweetList|>ignore
                printfn "subscribers queried by user %s" userName
            |QueryTag(hashTag)->
                let tagList=new List<Tweet>()
                if (hashTag<>" ") then
                    if(hashTagMap.ContainsKey(hashTag))then
                        for i in hashTagMap.Item(hashTag) do
                            tagList.Add(i)   
                mailbox.Sender()<?tagList|>ignore
                    //printfn "Hashtag queries by user %i %s" userName hashTag
            |QueryMentions(queriedActor)->
                let mentionList=new List<Tweet>()
                if(mentionsMap.ContainsKey(queriedActor))then
                    for i in mentionsMap.Item(queriedActor) do
                        mentionList.Add(i)
                mailbox.Sender()<?mentionList|>ignore
            |Logout(userName)->
                 if(usersLoggedin.Contains(userName))then
                     usersLoggedin.Remove(userName)
                     printfn "User %s logged out" userName
            |Login(userName)->
                if(not (usersLoggedin.Contains(userName)))then
                    usersLoggedin.Add(userName) |>ignore
                    printfn "User %s Logged In" userName 
            |AllTweets->
                mailbox.Sender()<?AllTweetsSet|>ignore
            |GetSubscriberRanksInfo->
                ()
            |_ -> printf ""
        return! loop()
    }
    loop()

//let server = spawne system "Server" <@ Server @>[]
//let serv=system.ActorSelection("akka://system/user/Server")
//server<!Sample("hello")

//printfn "%A" server

//printfn "Waiting on port 9001"
// while true do
//Console.ReadKey() |> ignore
//0
