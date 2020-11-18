module MyProject.Benchmarks.Benchmarks

open System

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<MemoryDiagnoser>]
[<MinColumn; MaxColumn>]
type Benchmarks() =
    let data = [ 1..100 ]

    [<Benchmark>]
    member _.MyBenchmark() = List.sum data

[<EntryPoint>]
let main _ =
    let assm = Reflection.Assembly.GetExecutingAssembly()
    BenchmarkSwitcher
        .FromAssembly(assm)
        .RunAll()
    |> ignore
    0
