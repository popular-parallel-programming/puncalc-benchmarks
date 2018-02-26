open System.IO
open System.Text.RegularExpressions

[<Measure>] type ms
let asMilliseconds v = v * 1.0<ms>


// Read a single data line.
let parseLine line =
    let m = Regex.Match (line, "(\\d+[,.]\\d+)ms")
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
                 (match ors with
                  | [] -> None
                  | ors ->
                        let om = List.average ors in
                        Some (name,
                              om,
                              sdev <| List.map (fun x -> om / x) ors,
                              bm / om,
                              sdev <| List.map (fun x -> bm / x) ors)))
    |> List.choose id


let unpack (name, ms, ms_sdev, speedup, speedup_sdev) f = f name (ms / 1.0<ms>) ms_sdev speedup speedup_sdev

// Print results as plain data list.
let printPlain results =
    for result in results do
        printfn "%-30s %10f ±%10f %10f ±%10f" |> unpack result


// Print results as fancy text
let printFancy header bar results =
    printfn header
    printfn bar
    for result in results do
        printfn "| %-30s | %10f | %10f | %10f | %10f |" |> unpack result

let printMarkdown = printFancy "| Sheet | MS | StDev | Speed up | StDev |"
                               "|:------|---:|------:|---------:|------:|"
let printOrg      = printFancy "| Sheet | MS | StDev | Speed up | StDev |"
                               "|-------+----+-------+----------+-------|"


let main args =
    let baseline = Array.tryItem 1 args
    let other    = Array.tryItem 2 args
    let format   = Array.tryItem 3 args

    match baseline, other with
        | Some baseline, Some other ->
            let writer = (match format with
                          | Some "md"  -> printMarkdown
                          | Some "org" -> printOrg
                          | _          -> printPlain)
            printfn "# Speedup of %s over %s:" other baseline
            writer <| compare baseline other
            0

        | _ ->
            printfn "Usage: > fsi analyze.fsx path/to/baseline/folder path/to/other/folder [md | org]";
            1


main fsi.CommandLineArgs
