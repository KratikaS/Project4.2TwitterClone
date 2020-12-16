module TwitterEngine

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

(*let mutable totalOperations=0
let mutable finalOperations=0;
let mutable maxSubs=0;
let mutable numQueries=0;
let mutable numTweets=0;
let system = System.create "system" (Configuration.defaultConfig())
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
let sampleActor(mailbox:Actor<_>)=
    let rec loop()=actor{
        let! msg=mailbox.Receive();
        printfn "%A" msg
        return! loop()
    }
    loop()
let sample=spawne system "sample" <@ sampleActor @>[]*)

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
            |Register(username)->
                printfn "User Registered"
                users.Add(username)|>ignore
                let response = "Registration Successfull"
                mailbox.Sender() <? response |> ignore
               
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
