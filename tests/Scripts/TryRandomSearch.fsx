#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"
#load "../../src/Aabel/State.fs"

open Simplee
open Simplee.State.ComputationExpression

type RSState = {
    Best: int
    Current: int list }

[<RequireQualifiedAccess>]
module RS = 

    // We just increment a counter.
    let private newSolution = State <| fun (stt: RSState) ->
        let sln = List.length stt.Current
        sln, {stt with Current = stt.Current @ [sln] }

    // The best solution, is the biggest int that divides with 5.
    let private cmpSolution sln = State <| fun (stt: RSState) ->
        match sln with
        | sln when sln > stt.Best && sln % 5 = 0 -> sln, {stt with Best = sln }
        | _                                      -> stt.Best, stt

    /// run this function each iteration.
    let iteration = _state {
        let! sln = newSolution
        let! bst = cmpSolution sln
        return bst }

    let run = State.unfold iteration

{ Best = 0; Current = [] }
|> RS.run
|> Seq.take 20
|> Seq.iter (printfn "%d")
