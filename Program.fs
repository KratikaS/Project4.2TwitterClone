// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Diagnostics


[<EntryPoint>]
let main argv =
    printfn "Do you want to register the user? write yes or no"
 
    let registerVariable = System.Console.ReadLine();
    printfn "%s" registerVariable
    match registerVariable with
       |yes -> 
           printfn "write code for register"
           printfn "user registered"
       |_-> printfn "nothing"
    
    
    let mutable whileFlg = true 
    while whileFlg do
          printfn "select the number according to the operation you want to perform"
          printfn "1. Tweet"
          printfn "2. ReTweet"
          printfn "3. Query hashtag"
          printfn "4. Query mention"
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
                 |_ ->
                     whileFlg <- false
                     printf "nothing"



    0 // return an integer exit code