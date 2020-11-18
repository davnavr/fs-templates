module MyProject.Benchmarks.Benchmarks

open System

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<MemoryDiagnoser>]
[<MinColumn; MaxColumn>]
type Benchmarks() =
    let data = [ 1..5 ]

    [<Benchmark>]
    member _.MyBenchmark() = List.sum data

[<EntryPoint>]
let main argv =
    let assm = Reflection.Assembly.GetExecutingAssembly()
    BenchmarkSwitcher
        .FromAssembly(assm)
        .Run(args = argv)
    |> ignore
    0
