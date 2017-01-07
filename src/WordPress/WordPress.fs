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
  let private getResponseBodyAsync (uri : Uri) =
    async {
      use httpClient = new HttpClient()
      use! response = httpClient.GetAsync(uri) |> Async.AwaitTask
      response.EnsureSuccessStatusCode() |> ignore
      return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    }

  /// Builds a query string for a request
  let getQuery (options : Options) =
    let embed =
      match options.embedRelations with
      | false -> ""
      | true -> "_embed"

    [| "?"; embed |]
    |> Seq.filter(String.IsNullOrEmpty >> not)
    |> String.concat("&")

  /// Builds a `System.Uri` object for a single request
  let buildUri (endpoint : string) (options : Options) =
    let uri = new UriBuilder()
    uri.Host <- options.apiHost
    uri.Scheme <- options.apiScheme
    uri.Path <- endpointBase + endpoint
    uri.Query <- getQuery options
    uri.Uri

  module Async =
    /// Convenience function to provide `map` functionality to `Async<'a>`
    let map f op =
      async {
        let! x = op
        let value = f x
        return value
      }

  /// Wraps WordPress REST API (v2) endpoints for obtaining post
  /// data from a WordPress installation
  module Posts =
    /// Gets all posts asynchronously
    let getAllAsync (options : Options) : Async<Post list> =
      buildUri "/posts" options
      |> (match options.apiClient with
          | Default -> getResponseBodyAsync
          | Func fn -> fn)
      |> Async.map(Json.parse >> Json.deserialize)

    /// Gets a single post by `postId` asynchronously
    let getSingleAsync (options : Options) (postId : int) : Async<Post> =
      buildUri (sprintf "/posts/%i" postId) options
      |> (match options.apiClient with
          | Default -> getResponseBodyAsync
          | Func fn -> fn)
      |> Async.map(Json.parse >> Json.deserialize)
