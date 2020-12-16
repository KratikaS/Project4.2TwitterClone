module MessageType

open System
//open Akka.Actor
//open Akka.FSharp
open System.Collections.Generic
type Tweet=
    {
        Type:string
        user:string
        tweetText : String;
        HashTag : List<String>;
        Mentions : List<String>;
    }
type SocketConnectionObject=
    {
        Type:string
        user:string
    }
type SampleObject=
    {
        username:string
        val1:String;
        val2:String;
    }
type SampleResponseType=
    {
        msg:string
    }
type TweeterEngine =
    |StartTimers
    |Sample of String
    |Register of String
    |TweetMsg of Tweet
    |QuerySubs of string
    |QueryTag of string
    |QueryMentions of string
    |Login of string
    |Logout of string
    |Simulate
    |GetSubscriberRanksInfo
    |Retweet of string * string 
    |PrintRetweet of Tweet * int * int
    |PrintLive of string 
    |PrintQueryTag of string
    |PrintQueryMention of  string
    |PrintQuerySubs 
    |Subscribe of string * string
    |AllTweets
    |Done