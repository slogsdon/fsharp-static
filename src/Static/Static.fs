namespace Static

open System
open Chiron

open WordPress
open WordPress.Types

[<AutoOpen>]
module Static =
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
