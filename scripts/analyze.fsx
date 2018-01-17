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
let sdev xs =
    let n    = double <| List.length xs
    let mean = List.average xs
    let sst  = List.map (fun x -> x * x) >> List.sum <| xs
    float <| sqrt ((sst - mean * mean * n) / (n - 1.0))

// Compare a baseline result to another result.
let compare baseline other =
    let baselineMeans = readFiles baseline |> List.map (fun (f, mss) -> f, List.average mss)
    let otherResults  = readFiles other
    baselineMeans
    |> List.map (fun (name, bm) ->
                 let _, ors = List.find (fst >> (=) name) otherResults in
                 match ors with
                 | [] -> None
                 | ors -> Some (name,
                                bm / List.average ors,
                                sdev <| List.map (fun x -> bm / x) ors))
    |> List.choose id


match fsi.CommandLineArgs with
    | [| _; baseline; other|] ->
        printfn "# Speedup of %s over %s:" other baseline
        for result in compare baseline other do
            printfn "%-30s %10f %10f" <||| result
        0
    | _ ->
        printfn "Usage: > fsi analyze.fsx path/to/baseline/folder path/to/other/folder";
        1
