namespace Simplee.Diagnostics

open System
open System.Diagnostics

[<RequireQualifiedAccess>]
module Timed = 

    type Timed<'T> = internal {
        Result: 'T
        Elapsed: TimeSpan }

    let ofValue value = { Result = value; Elapsed = TimeSpan.Zero }

    let withResult  r t = { t with Result = r }
    let withElapsed e t = { t with Elapsed = e }

    let addElapsed e t = { t with Elapsed = t.Elapsed +  e}

    let result  t = t.Result
    let elapsed t = t.Elapsed

    module ComputationExpression = 

        type TimedBuilder () = 
            member __.Return(value) = value |> ofValue

            member __.Delay(func) =
                let stopwatch = Stopwatch.StartNew()
                let timedResult = func()
                stopwatch.Stop ()

                timedResult |> addElapsed stopwatch.Elapsed

        let _timed = TimedBuilder()