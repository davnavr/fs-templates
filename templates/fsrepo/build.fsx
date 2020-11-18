#if FAKE_DEPENDENCIES
#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
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

let rootDir = __SOURCE_DIRECTORY__
let outDir = rootDir </> "out"
let slnFile = rootDir </> "MyProject.sln"

[<AutoOpen>]
module Helpers =
    let handleErr msg: ProcessResult -> _ =
        function
        | { ExitCode = ecode } when ecode <> 0 ->
            failwithf "Process exited with code %i: %s" ecode msg
        | _ -> ()

    let runProj args proj =
        [
            "--configuration Release"
            "--no-build"
            "--no-restore"
        ]
        |> args
        |> String.concat " "
        |> sprintf
            "--project %s %s"
            proj
        |> DotNetCli.exec
            id
            "run"

Target.create "Clean" <| fun _ ->
    Shell.cleanDir outDir

    List.iter
        (fun cfg ->
            [
                slnFile
                sprintf "--configuration %s" cfg
                "--nologo"
            ]
            |> String.concat " "
            |> DotNetCli.exec id "clean"
            |> handleErr "Unexpected error while cleaning solution")
        [ "Debug"; "Release" ]

Target.create "Build" <| fun _ ->
    DotNetCli.build
        (fun opt ->
            { opt with
                Configuration = DotNetCli.Release
                NoRestore = true })
        slnFile

Target.create "Test" <| fun _ ->
    let projs = !!(rootDir </> "test" </> "*" </> "*.fsproj")
    Seq.iter
        (runProj id >> handleErr "One or more tests failed")
        projs

Target.create "Run Benchmarks" <| fun _ ->
    runProj
        (fun args ->
            [
                yield! args
                "--"
                "--filter *"
                "--artifacts"
                outDir </> "BenchmarkDotNet.Artifacts"
            ])
        (rootDir </> "benchmark" </> "MyProject.Benchmarks.fsproj")
    |> handleErr "One or more benchmarks could not be run"

Target.create "All" ignore

"Clean"
==> "Build"
==> "Test"
==> "Run Benchmarks"
==> "All"

Target.runOrDefault "All"
