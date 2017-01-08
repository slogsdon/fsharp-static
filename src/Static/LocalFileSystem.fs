namespace Static

open System
open System.Text.RegularExpressions
open DotLiquid
open DotLiquid.Exceptions
open DotLiquid.FileSystems

type LocalFileSystem(root : string) =
  member this.Root = root

  interface IFileSystem with
    member this.ReadTemplateFile (context : Context, templateName : string) =
      let templatePath = string (context.Item templateName)
      let fullPath = this.FullPath(templatePath)
      if IO.File.Exists(fullPath) |> not
      then new FileSystemException("Template not found: {0}", templatePath) |> raise
      IO.File.ReadAllText fullPath

  member this.FullPath (templatePath : string) =
    if isNull templatePath || Regex.IsMatch(templatePath, @"^[^.\/][a-zA-Z0-9_\/]+$") |> not
    then FileSystemException("Illegal path: {0}", templatePath) |> raise

    let fullPath =
      if templatePath.Contains("/")
      then IO.Path.Combine(IO.Path.Combine(this.Root, IO.Path.GetDirectoryName(templatePath)), (sprintf "_%s.liquid" (IO.Path.GetFileName(templatePath))))
      else IO.Path.Combine(this.Root, (sprintf "_%s.liquid" templatePath))

    fullPath
