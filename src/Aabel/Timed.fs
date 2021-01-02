namespace Simplee.Diagnostics

open System
open System.Diagnostics

module Timed = 

    type Timed<'T> = {
        Result: 'T
        Elapsed: TimeSpan }

    module ComputationExpression = 

        type TimedBuilder () = 
            member __.Return(value) = 
                { Result = value; Elapsed = TimeSpan.Zero }

            member __.Delay(func) =
                let stopwatch = Stopwatch.StartNew()
                let timedResult = func()
                stopwatch.Stop ()

                { timedResult with Elapsed = timedResult.Elapsed + stopwatch.Elapsed }

        let timed = TimedBuilder()