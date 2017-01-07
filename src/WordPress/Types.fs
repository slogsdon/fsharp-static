namespace WordPress

module Types =
  open System
  open System.Net.Http
  open Chiron
  open Chiron.Operators

  module Json =
    let stringFromRendered json =
      match json with
      | Object o ->
        match Map.tryFind "rendered" o with
        | None -> Value ""
        | Some(v) ->
          match v with
          | String value -> Value value
          | _ -> Value ""
      | _ -> Error (sprintf "Unable to parse %A as a {\"rendered\": ...}" json)

  type ApiClient =
    | Default
    | Func of (Uri -> Async<string>)

  type ApiArgument =
    | String of string
    | Strings of string list
    | Ints of int list
    | Int of int
    | Bool of bool

  type Options =
    {
      apiClient: ApiClient;
      apiHost: string;
      apiScheme: string;
      embedRelations: bool;
    }

  let defaultOptions =
    {
      apiClient = Default;
      apiHost = "";
      apiScheme = "https";
      embedRelations = false;
    }

  type Link =
    {
      name: string;
      href: string;
      templated: bool;
      embeddable: bool;
      taxonomy: string;
    }

    static member FromJson (_ : Link) =
      fun n h te e tx ->
            { name = n
              href = h
              templated = te
              embeddable = e
              taxonomy = tx }
      <!> Json.readOrDefault "name" ""
      <*> Json.readOrDefault "href" ""
      <*> Json.readOrDefault "templated" false
      <*> Json.readOrDefault "embeddable" false
      <*> Json.readOrDefault "taxonomy" ""

  type LinkCollections =
    {
      self: Link list;
      collections: Link list;
      abouts: Link list;
      authors: Link list;
      replies: Link list;
      versionHistory: Link list;
      attachments: Link list;
      terms: Link list;
    }

    static member FromJson (_ : LinkCollections) =
      fun s co ab au r v at t ->
            { self = s
              collections = co
              abouts = ab
              authors = au
              replies = r
              versionHistory = v
              attachments = at
              terms = t }
      <!> Json.read "self"
      <*> Json.read "collection"
      <*> Json.read "about"
      <*> Json.read "author"
      <*> Json.read "replies"
      <*> Json.read "version-history"
      <*> Json.read "wp:attachment"
      <*> Json.read "wp:term"

  type Post =
    {
      id: int;
      date: DateTime;
      dateGmt: DateTime;
      guid: string;
      modified: DateTime;
      modifiedGmt: DateTime;
      slug: string;
      postType: string;
      link: string;
      title: string;
      content: string;
      excerpt: string;
      authorId: int;
      featuredMediaId: int;
      commentStatus: string;
      pingStatus: string;
      sticky: bool;
      template: string;
      format: string;
      // meta: [],
      categoryIds: int list;
      tagIds: int list;
      links: LinkCollections;
      // "_embedded": {
      //     "author": [{
      //         "id": 1,
      //         "name": "shane",
      //         "url": "",
      //         "description": "",
      //         "link": "https:\/\/slogsdon.azurewebsites.net\/author\/shane\/",
      //         "slug": "shane",
      //         "avatar_urls": {
      //             "24": "https:\/\/localhost\/image?s=24&d=mm&r=g",
      //             "48": "https:\/\/localhost\/image?s=48&d=mm&r=g",
      //             "96": "https:\/\/localhost\/image?s=96&d=mm&r=g"
      //         },
      //         "_links": {
      //             "self": [{
      //                 "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/users\/1"
      //             }],
      //             "collection": [{
      //                 "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/users"
      //             }]
      //         }
      //     }],
      //     "wp:term": [
      //         [{
      //             "id": 1,
      //             "link": "https:\/\/slogsdon.azurewebsites.net\/category\/uncategorized\/",
      //             "name": "Uncategorized",
      //             "slug": "uncategorized",
      //             "taxonomy": "category",
      //             "_links": {
      //                 "self": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/categories\/1"
      //                 }],
      //                 "collection": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/categories"
      //                 }],
      //                 "about": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/taxonomies\/category"
      //                 }],
      //                 "wp:post_type": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/posts?categories=1"
      //                 }],
      //                 "curies": [{
      //                     "name": "wp",
      //                     "href": "https:\/\/api.w.org\/{rel}",
      //                     "templated": true
      //                 }]
      //             }
      //         }],
      //         [{
      //             "id": 31,
      //             "link": "https:\/\/slogsdon.azurewebsites.net\/tag\/here\/",
      //             "name": "here",
      //             "slug": "here",
      //             "taxonomy": "post_tag",
      //             "_links": {
      //                 "self": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags\/31"
      //                 }],
      //                 "collection": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags"
      //                 }],
      //                 "about": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/taxonomies\/post_tag"
      //                 }],
      //                 "wp:post_type": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/posts?tags=31"
      //                 }],
      //                 "curies": [{
      //                     "name": "wp",
      //                     "href": "https:\/\/api.w.org\/{rel}",
      //                     "templated": true
      //                 }]
      //             }
      //         }, {
      //             "id": 21,
      //             "link": "https:\/\/slogsdon.azurewebsites.net\/tag\/tag\/",
      //             "name": "tag",
      //             "slug": "tag",
      //             "taxonomy": "post_tag",
      //             "_links": {
      //                 "self": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags\/21"
      //                 }],
      //                 "collection": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags"
      //                 }],
      //                 "about": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/taxonomies\/post_tag"
      //                 }],
      //                 "wp:post_type": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/posts?tags=21"
      //                 }],
      //                 "curies": [{
      //                     "name": "wp",
      //                     "href": "https:\/\/api.w.org\/{rel}",
      //                     "templated": true
      //                 }]
      //             }
      //         }, {
      //             "id": 11,
      //             "link": "https:\/\/slogsdon.azurewebsites.net\/tag\/test\/",
      //             "name": "test",
      //             "slug": "test",
      //             "taxonomy": "post_tag",
      //             "_links": {
      //                 "self": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags\/11"
      //                 }],
      //                 "collection": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/tags"
      //                 }],
      //                 "about": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/taxonomies\/post_tag"
      //                 }],
      //                 "wp:post_type": [{
      //                     "href": "https:\/\/slogsdon.azurewebsites.net\/wp-json\/wp\/v2\/posts?tags=11"
      //                 }],
      //                 "curies": [{
      //                     "name": "wp",
      //                     "href": "https:\/\/api.w.org\/{rel}",
      //                     "templated": true
      //                 }]
      //             }
      //         }]
      //     ]
      // }
    }

    static member FromJson (_ : Post) =
      fun i d dg g m mg s pt l t c e ai fmi cs ps st te f ci ti li ->
            { id = i
              date = d
              dateGmt = dg
              guid = g
              modified = m
              modifiedGmt = mg
              slug = s
              postType = pt
              link = l
              title = t
              content = c
              excerpt = e
              authorId = ai
              featuredMediaId = fmi
              commentStatus = cs
              pingStatus = ps
              sticky = st
              template = te
              format = f
              // meta: [],
              categoryIds = ci
              tagIds = ti
              links = li }
      <!> Json.read "id"
      <*> Json.read "date"
      <*> Json.read "date_gmt"
      <*> Json.readWith Json.stringFromRendered "guid"
      <*> Json.read "modified"
      <*> Json.read "modified_gmt"
      <*> Json.read "slug"
      <*> Json.read "type"
      <*> Json.read "link"
      <*> Json.readWith Json.stringFromRendered "title"
      <*> Json.readWith Json.stringFromRendered "content"
      <*> Json.readWith Json.stringFromRendered "excerpt"
      <*> Json.read "author"
      <*> Json.read "featured_media"
      <*> Json.read "comment_status"
      <*> Json.read "ping_status"
      <*> Json.read "sticky"
      <*> Json.read "template"
      <*> Json.read "format"
      // <*> Json.read "meta"
      <*> Json.read "categories"
      <*> Json.read "tags"
      <*> Json.read "_links"

  type Tag =
    { id: int;
      count: int;
      description: string;
      link: string;
      name: string;
      slug: string;
      taxonomy: string;
      // meta
      links: LinkCollections; }

    static member FromJson (_ : Tag) =
      fun i c d l n s t ls ->
            { id = i
              count = c
              description = d
              link = l
              name = n
              slug = s
              taxonomy = t
              // meta
              links = ls
              }
      <!> Json.read "id"
      <*> Json.read "count"
      <*> Json.read "description"
      <*> Json.read "link"
      <*> Json.read "name"
      <*> Json.read "slug"
      <*> Json.read "taxonomy"
      // <*> Json.read "meta"
      <*> Json.read "_links"

  type Category =
    { id: int;
      count: int;
      description: string;
      link: string;
      name: string;
      slug: string;
      taxonomy: string;
      parentId: int;
      // meta
      links: LinkCollections; }

    static member FromJson (_ : Category) =
      fun i c d l n s t pi ls ->
            { id = i
              count = c
              description = d
              link = l
              name = n
              slug = s
              taxonomy = t
              parentId = pi
              // meta
              links = ls
              }
      <!> Json.read "id"
      <*> Json.read "count"
      <*> Json.read "description"
      <*> Json.read "link"
      <*> Json.read "name"
      <*> Json.read "slug"
      <*> Json.read "taxonomy"
      <*> Json.read "parent"
      // <*> Json.read "meta"
      <*> Json.read "_links"
