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

module DotNetCli = Fake.DotNet.DotNet

let rootDir = __SOURCE_DIRECTORY__
let outDir = rootDir </> "out"
let srcDir = rootDir </> "src"

[<AutoOpen>]
module Helpers =
    let handleErr msg: ProcessResult -> _ =
        function
        | { ExitCode = ecode } when ecode <> 0 ->
            failwithf "Process exited with code %i: %s" ecode msg
        | _ -> ()

Target.create "Clean" <| fun _ ->
    Shell.cleanDir outDir

Target.create "Build" <| fun _ ->
    Trace.log "Building..."

Target.create "Test" <| fun _ ->
    Trace.log "Testing..."

"Clean" ==> "Build" ==> "Test"

Target.runOrDefault "Test"
