#if FAKE_DEPENDENCIES
#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
//"
#endif

#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

module DotNetCli = Fake.DotNet.DotNet
module NuGetCli = Fake.DotNet.NuGet.NuGet

let rootDir = __SOURCE_DIRECTORY__
let outDir = rootDir </> "out"

let version = Environment.environVarOrDefault "PACKAGE_VERSION" "0.0.0"

[<AutoOpen>]
module Helpers =
    let handleErr msg: ProcessResult -> _ =
        function
        | { ExitCode = ecode } when ecode <> 0 ->
            failwithf "Process exited with code %i: %s" ecode msg
        | _ -> ()

Target.create "Clean" <| fun _ -> Shell.cleanDir outDir

Target.create "Build Templates" <| fun _ ->
    let templates =
        !!(rootDir </> "templates" </> "*")
    try
        rootDir </> "templates"
        |> DirectoryInfo.ofPath
        |> DirectoryInfo.getSubDirectories
        |> Seq.iter
            (fun dir ->
                let template = dir.FullName
                Shell.chdir template

                let projs = !!(template </> "**" </> ".*proj")
                Seq.iter
                    (DotNetCli.restore id)
                    projs

                let script = template </> "build.fsx"
                [
                    "run"
                    script
                ]
                |> List.map (sprintf "\"%s\"")
                |> String.concat " "
                |> DotNetCli.exec
                    id
                    "fake"
                |> handleErr "One or more templates had a failed build")
    finally
        Shell.chdir rootDir

Target.create "Pack" <| fun _ ->
    // nuget pack -OutputDirectory ./out -NoDefaultExcludes -Properties PackageVersion=0.1.0
    // TODO: Exclude bin and obj files properly.
    NuGetCli.NuGetPackDirectly
        (fun p ->
            { p with
                NoDefaultExcludes = true
                OutputPath = outDir
                Properties = [ "PackageVersion", version ]
                Version = version
                WorkingDir = rootDir })
        (rootDir </> "davnavr.FSharpTemplates.nuspec")
    ()

"Clean" ==> "Build Templates" ==> "Pack"

Target.runOrDefault "Pack"
