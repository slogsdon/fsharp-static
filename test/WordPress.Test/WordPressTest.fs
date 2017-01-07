namespace WordPress.Test

open System
open Xunit
open WordPress
open WordPress.Types

// see example explanation on xUnit.net website:
// https://xunit.github.io/docs/getting-started-dotnet-core.html
module WordPressTest =
  let readFixture file = "./fixtures/" + file |> IO.File.ReadAllText
  let apiClient response = Func (fun _ -> async { return response })

  [<Fact>]
  let ``WordPress getQuery returns empty string with defaultOptions`` () =
    let options = defaultOptions
    let actual = WordPress.getQuery (Map.ofList []) options
    let expected = ""

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress getQuery returns correctly with Options.embedRelations = true`` () =
    let options = { defaultOptions with embedRelations = true }
    let actual = WordPress.getQuery (Map.ofList []) options
    let expected = "_embed=1"

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress getQuery returns correctly with non-empty arguments`` () =
    let options = defaultOptions
    let args = Map.ofList [ "order", String "desc"
                            "order_by", String "date" ]
    let actual = WordPress.getQuery args options
    let expected = "order=desc&order_by=date"

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress buildUri outputs correctly with valid host`` () =
    let options = { defaultOptions with apiHost = "valid.host.localhost" }
    let actual = WordPress.buildUri "/posts" options
    let expected = new Uri("https://valid.host.localhost/wp-json/wp/v2/posts")

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress buildUri returns correctly with valid host and scheme`` () =
    let options =
      { defaultOptions with
          apiHost = "valid.host.localhost";
          apiScheme = "http" }
    let actual = WordPress.buildUri "/posts" options
    let expected = new Uri("http://valid.host.localhost/wp-json/wp/v2/posts")

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress buildUri returns correctly with valid host, scheme, and global param`` () =
    let options =
      { defaultOptions with
          apiHost = "valid.host.localhost";
          apiScheme = "http";
          embedRelations = true }
    let actual = WordPress.buildUri "/posts" options
    let expected = new Uri("http://valid.host.localhost/wp-json/wp/v2/posts?_embed=1")

    Assert.Equal(expected, actual)

  [<Fact>]
  let ``WordPress buildUri fails with default host ""`` () =
    let options = defaultOptions
    let ex = Assert.Throws<UriFormatException>(Action (fun () ->
      WordPress.buildUri "/posts" options |> ignore
    ))
    Assert.Equal("Invalid URI: The hostname could not be parsed.", ex.Message)

  [<Fact>]
  let ``WordPress buildUri fails with invalid scheme ""`` () =
    let options = { defaultOptions with apiScheme = "asdf" }
    let ex = Assert.Throws<ArgumentException>(Action (fun () ->
      WordPress.buildUri "/posts" options |> ignore
    ))

    Assert.Equal("Scheme is invalid.\nParameter name: Options.apiScheme", ex.Message)
