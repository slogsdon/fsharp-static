namespace WordPress

/// Wraps WordPress REST API (v2) endpoints for obtaining data
/// from a WordPress installation
module WordPress =
  open Chiron
  open System
  open System.Net.Http
  open WordPress.Types

  /// WP REST API (v2) endpoint base used for constructing
  /// request `System.Uri`s
  let private endpointBase = "/wp-json/wp/v2"

  /// Retrieves a remote resource asynchronously
  let private getResponseBodyAsync (uri : Uri) : Async<string> =
    async {
      use httpClient = new HttpClient()
      use! response = httpClient.GetAsync(uri) |> Async.AwaitTask
      response.EnsureSuccessStatusCode() |> ignore
      return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    }

  /// Builds a query string for a request
  let getQuery (args : Map<string, ApiArgument>) (options : Options) : string =
    let args =
      match options.embedRelations with
      | false -> args
      | true -> Map.add "_embed" (String "1") args

    let toQueryStringParam (k, v) =
      match v with
      | Int i -> sprintf "%s=%i" k i
      | Ints is -> ""
      | String s -> sprintf "%s=%s" k s
      | Strings ss -> ""
      | Bool b -> sprintf "%s=%s" k (if b then "true" else "false")

    args
    |> Map.toList
    |> Seq.map toQueryStringParam
    |> String.concat("&")

  /// Builds a `System.Uri` object for a single request with arguments
  let buildUriWithArgs (endpoint : string) (args : Map<string, ApiArgument>) (options : Options) : Uri =
    if [| "http"; "https" |] |> Seq.contains options.apiScheme |> not
    then invalidArg "Options.apiScheme" "Scheme is invalid."

    let uri = new UriBuilder()
    uri.Host <- options.apiHost
    uri.Scheme <- options.apiScheme
    uri.Path <- endpointBase + endpoint
    uri.Query <- getQuery args options
    uri.Uri

  /// Builds a `System.Uri` object for a single request
  let buildUri endpoint options =
    buildUriWithArgs endpoint (Map.ofList []) options

  /// Wraps WordPress REST API (v2) endpoints for obtaining post
  /// data from a WordPress installation
  module Posts =
    /// Gets all posts asynchronously with arguments
    let getAllWithArgsAsync(args : Map<string, ApiArgument>) (options : Options)  : Async<Post list> =
      buildUriWithArgs "/posts" args options
      |> (match options.apiClient with
          | Default -> getResponseBodyAsync
          | Func fn -> fn)
      |> Async.map(Json.parse >> Json.deserialize)

    /// Gets all posts asynchronously
    let getAllAsync options =
      getAllWithArgsAsync (Map.ofList []) options

    /// Gets a single post by `postId` asynchronously with arguments
    let getSingleWithArgsAsync (postId : int) (args : Map<string, ApiArgument>) (options : Options) : Async<Post> =
      buildUriWithArgs (sprintf "/posts/%i" postId) args options
      |> (match options.apiClient with
          | Default -> getResponseBodyAsync
          | Func fn -> fn)
      |> Async.map(Json.parse >> Json.deserialize)

    /// Gets a single post by `postId` asynchronously
    let getSingleAsync postId options =
      getSingleWithArgsAsync postId (Map.ofList []) options
