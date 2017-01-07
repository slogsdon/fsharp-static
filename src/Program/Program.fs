// Learn more about F# at http://fsharp.org

open System
open Chiron

open WordPress
open WordPress.Types

let wpOptions : Options =
    { defaultOptions with
        apiHost = "slogsdon.azurewebsites.net";
        embedRelations = true }

[<EntryPoint>]
let main argv =
    WordPress.Posts.getAllAsync wpOptions
    |> Async.RunSynchronously
    |> Seq.take 1
    |> Seq.exactlyOne
    |> (fun p -> p.title)
    |> printf "%A"
    0 // return an integer exit code
