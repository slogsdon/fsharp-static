namespace Static

open System
open Chiron
open DotLiquid

open WordPress
open WordPress.Types

[<AutoOpen>]
module Static =
  let wpOptions : Options =
      { defaultOptions with
          apiHost = "slogsdon.azurewebsites.net";
          embedRelations = true }

  let template (name : string) =
    IO.File.ReadAllText (sprintf "./example/templates/%s.liquid" name)
    |> Templates.parseTemplate<Post>

  [<EntryPoint>]
  let main argv =
    Template.FileSystem <- LocalFileSystem "./example/templates"

    WordPress.Posts.getAllAsync wpOptions
    |> Async.RunSynchronously
    |> Seq.take 1
    |> Seq.exactlyOne
    |> template "test" "post"
    |> printf "%s"
    0 // return an integer exit code
