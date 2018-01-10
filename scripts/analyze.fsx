open System.IO
open System.Text.RegularExpressions

[<Measure>] type ms
let asMilliseconds v = v * 1.0<ms>

// Read a single data line.
let parseLine line =
    let m = Regex.Match (line, "(\\d+\\,\\d+)ms")
    if m.Success then Some (float >> asMilliseconds <| m.Value.Replace(",", ".").Replace("ms", "")) else None

// Read a whole file and return the measured time in milliseconds
let readFile =
     System.IO.File.ReadLines
     >> Seq.map parseLine
     >> Seq.choose id
     >> Seq.toList

// Read all output files in a directory
let readFiles path =
    Directory.EnumerateFiles (path, "*.out")
    |> Seq.map (fun f -> Path.GetFileNameWithoutExtension f, readFile f)
    |> Seq.toList

// Compute mean and standard deviation
let meanAndSdev (mss : float<ms> list) =
    let n    = double <| List.length mss
    let mean = List.average mss
    let sst  = List.map (fun x -> x * x) >> List.sum <| mss
    mean, sqrt ((sst - mean * mean * n) / (n - 1.0))

// Compare a baseline result to another result.
let compare baseline other =
    let baselineResults = readFiles baseline |> Seq.map (fun (f, mss) -> f, meanAndSdev mss)
    let otherResults    = readFiles other    |> Seq.map (fun (f, mss) -> f, meanAndSdev mss)
    Seq.map2 (fun (f, (bsm, bss)) (_, (osm, oss)) -> f, bsm / osm) // TODO: Compute stdev
             baselineResults
             otherResults

match fsi.CommandLineArgs with
    | [| _; baseline; other|] ->
        printfn "# Comparing %s to %s:" other baseline
        for result in compare baseline other do
            printfn "%-30s%f" <|| result
        0
    | _ ->
        printfn "%A" fsi.CommandLineArgs;
        printfn "Usage: > fsi analyze.fsx path/to/baseline/folder path/to/other/folder";
        1
