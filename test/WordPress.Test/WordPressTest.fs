namespace WordPress.Test

open System
open Xunit
open WordPress
open WordPress.Types

// see example explanation on xUnit.net website:
// https://xunit.github.io/docs/getting-started-dotnet-core.html
module WordPressTest =
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

  let apiClient response = Func (fun _ -> async { return response })

  module Posts =
    [<Fact>]
    let ``WordPress.Posts getAllAsync with empty response``  () =
      let options =
        { defaultOptions with
            apiHost = "localhost";
            apiClient = apiClient "[]"; }

      WordPress.Posts.getAllAsync options
      |> Async.map(Assert.Empty)

    [<Fact>]
    let ``WordPress.Posts getAllAsync with non-empty response``  () =
      let response = "[{\"id\":1,\"date\":\"2017-01-06T03:17:47\",\"date_gmt\":\"2017-01-06T03:17:47\",\"guid\":{\"rendered\":\"http:\\/\\/localhost\\/?p=1\"},\"modified\":\"2017-01-05T23:29:29\",\"modified_gmt\":\"2017-01-06T04:29:29\",\"slug\":\"hello-world\",\"type\":\"post\",\"link\":\"https:\\/\\/localhost\\/hello-world\\/\",\"title\":{\"rendered\":\"Hello world!\"},\"content\":{\"rendered\":\"\\r\\n\\r\\n## this is a header\\r\\n\\r\\nsome text\",\"protected\":false},\"excerpt\":{\"rendered\":\"## this is a header some text\",\"protected\":false},\"author\":1,\"featured_media\":0,\"comment_status\":\"open\",\"ping_status\":\"open\",\"sticky\":false,\"template\":\"\",\"format\":\"standard\",\"meta\":[],\"categories\":[1],\"tags\":[31,21,11],\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\\/1\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/types\\/post\"}],\"author\":[{\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/users\\/1\"}],\"replies\":[{\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/comments?post=1\"}],\"version-history\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\\/1\\/revisions\"}],\"wp:attachment\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/media?parent=1\"}],\"wp:term\":[{\"taxonomy\":\"category\",\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/categories?post=1\"},{\"taxonomy\":\"post_tag\",\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags?post=1\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]}}]"
      let options =
        { defaultOptions with
            apiHost = "localhost";
            apiClient = apiClient response; }

      WordPress.Posts.getAllAsync options
      |> Async.map(Assert.NotEmpty)

    [<Fact>]
    let ``WordPress.Posts getAllAsync with non-empty response + embeds`` () =
      let response = "[{\"id\":1,\"date\":\"2017-01-06T03:17:47\",\"date_gmt\":\"2017-01-06T03:17:47\",\"guid\":{\"rendered\":\"http:\\/\\/localhost\\/?p=1\"},\"modified\":\"2017-01-05T23:29:29\",\"modified_gmt\":\"2017-01-06T04:29:29\",\"slug\":\"hello-world\",\"type\":\"post\",\"link\":\"https:\\/\\/localhost\\/hello-world\\/\",\"title\":{\"rendered\":\"Hello world!\"},\"content\":{\"rendered\":\"\\r\\n\\r\\n## this is a header\\r\\n\\r\\nsome text\",\"protected\":false},\"excerpt\":{\"rendered\":\"## this is a header some text\",\"protected\":false},\"author\":1,\"featured_media\":0,\"comment_status\":\"open\",\"ping_status\":\"open\",\"sticky\":false,\"template\":\"\",\"format\":\"standard\",\"meta\":[],\"categories\":[1],\"tags\":[31,21,11],\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\\/1\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/types\\/post\"}],\"author\":[{\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/users\\/1\"}],\"replies\":[{\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/comments?post=1\"}],\"version-history\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts\\/1\\/revisions\"}],\"wp:attachment\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/media?parent=1\"}],\"wp:term\":[{\"taxonomy\":\"category\",\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/categories?post=1\"},{\"taxonomy\":\"post_tag\",\"embeddable\":true,\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags?post=1\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]},\"_embedded\":{\"author\":[{\"id\":1,\"name\":\"shane\",\"url\":\"\",\"description\":\"\",\"link\":\"https:\\/\\/localhost\\/author\\/shane\\/\",\"slug\":\"shane\",\"avatar_urls\":{\"24\":\"https:\\/\\/localhost\\/image?s=24&d=mm&r=g\",\"48\":\"https:\\/\\/localhost\\/image?s=48&d=mm&r=g\",\"96\":\"https:\\/\\/localhost\\/image?s=96&d=mm&r=g\"},\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/users\\/1\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/users\"}]}}],\"wp:term\":[[{\"id\":1,\"link\":\"https:\\/\\/localhost\\/category\\/uncategorized\\/\",\"name\":\"Uncategorized\",\"slug\":\"uncategorized\",\"taxonomy\":\"category\",\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/categories\\/1\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/categories\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/taxonomies\\/category\"}],\"wp:post_type\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts?categories=1\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]}}],[{\"id\":31,\"link\":\"https:\\/\\/localhost\\/tag\\/here\\/\",\"name\":\"here\",\"slug\":\"here\",\"taxonomy\":\"post_tag\",\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\\/31\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/taxonomies\\/post_tag\"}],\"wp:post_type\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts?tags=31\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]}},{\"id\":21,\"link\":\"https:\\/\\/localhost\\/tag\\/tag\\/\",\"name\":\"tag\",\"slug\":\"tag\",\"taxonomy\":\"post_tag\",\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\\/21\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/taxonomies\\/post_tag\"}],\"wp:post_type\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts?tags=21\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]}},{\"id\":11,\"link\":\"https:\\/\\/localhost\\/tag\\/test\\/\",\"name\":\"test\",\"slug\":\"test\",\"taxonomy\":\"post_tag\",\"_links\":{\"self\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\\/11\"}],\"collection\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/tags\"}],\"about\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/taxonomies\\/post_tag\"}],\"wp:post_type\":[{\"href\":\"https:\\/\\/localhost\\/wp-json\\/wp\\/v2\\/posts?tags=11\"}],\"curies\":[{\"name\":\"wp\",\"href\":\"https:\\/\\/api.w.org\\/{rel}\",\"templated\":true}]}}]]}}]"
      let options =
        { defaultOptions with
            apiHost = "localhost";
            apiClient = apiClient response; }

      WordPress.Posts.getAllAsync options
      |> Async.map(Assert.NotEmpty)
