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
    { apiClient: ApiClient;
      apiHost: string;
      apiScheme: string;
      embedRelations: bool; }

  let defaultOptions =
    { apiClient = Default;
      apiHost = "";
      apiScheme = "https";
      embedRelations = false; }

  type Link =
    { name: string;
      href: string;
      templated: bool;
      embeddable: bool;
      taxonomy: string; }

  type Tag =
    { id: int;
      count: int;
      description: string;
      link: string;
      name: string;
      slug: string;
      taxonomy: string;
      // meta
      }

    static member FromJson (_ : Tag) =
      fun i c d l n s t ->
            { id = i
              count = c
              description = d
              link = l
              name = n
              slug = s
              taxonomy = t
              // meta
              }
      <!> Json.readOrDefault "id" 0
      <*> Json.readOrDefault "count" 0
      <*> Json.readOrDefault "description" ""
      <*> Json.readOrDefault "link" ""
      <*> Json.readOrDefault "name" ""
      <*> Json.readOrDefault "slug" ""
      <*> Json.readOrDefault "taxonomy" ""
      // <*> Json.read "meta"

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
      }

    static member FromJson (_ : Category) =
      fun i c d l n s t pi ->
            { id = i
              count = c
              description = d
              link = l
              name = n
              slug = s
              taxonomy = t
              parentId = pi
              // meta
              }
      <!> Json.readOrDefault "id" 0
      <*> Json.readOrDefault "count" 0
      <*> Json.readOrDefault "description" ""
      <*> Json.readOrDefault "link" ""
      <*> Json.readOrDefault "name" ""
      <*> Json.readOrDefault "slug" ""
      <*> Json.readOrDefault "taxonomy" ""
      <*> Json.readOrDefault "parent" 0
      // <*> Json.read "meta"

  type User =
    { id: int;
      name: string;
      url: string;
      description: string;
      link: string;
      slug: string;
      avatarUrls: Map<string, string>; }

    static member FromJson (_ : User) =
      fun i n u d l s au ->
            { id = i
              name = n
              url = u
              description = d
              link = l
              slug = s
              avatarUrls = au }
      <!> Json.read "id"
      <*> Json.read "name"
      <*> Json.read "url"
      <*> Json.read "description"
      <*> Json.read "link"
      <*> Json.read "slug"
      <*> Json.read "avatar_urls"

  let categoriesFromTermSet json : JsonResult<Category list> =
    match json with
    | Array lists ->
      Value (Seq.item 0 lists |> Json.deserialize)
    | _ -> Error (sprintf "Unable to parse %A as a wp:term" json)

  let tagsFromTermSet json : JsonResult<Tag list> =
    match json with
    | Array lists ->
      Value (Seq.item 1 lists |> Json.deserialize)
    | _ -> Error (sprintf "Unable to parse %A as a wp:term" json)

  type EmbedCollection =
    { authors: User list;
      categories: Category list;
      tags: Tag list; }

    static member FromJson (_ : EmbedCollection) =
      fun a c t ->
            { authors = a
              categories = c
              tags = t }
      <!> Json.read "author"
      <*> Json.readWith categoriesFromTermSet "wp:term"
      <*> Json.readWith tagsFromTermSet "wp:term"

  type Post =
    { id: int;
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
      embeds: EmbedCollection;
    }

    static member FromJson (_ : Post) =
      fun i d dg g m mg s pt l t c e ai fmi cs ps st te f ci ti em ->
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
              embeds = em }
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
      <*> Json.read "_embedded"
