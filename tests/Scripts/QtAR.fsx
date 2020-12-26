#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"

open Simplee
open Simplee.AR.ComputationExpression

let tstAR () =

    let getX () = async { return Ok 10 }
    let getY () = async { return Ok 20 }
    let getZ () = async { return Ok 30 }

    _ar {
        let! x = getX ()
        let! y = getY ()
        let! z = getZ ()
        return x + y + z }
    |> Async.RunSynchronously
    |> printfn "AR Success: %A"

let tstARErr () =
    let getX () = async { return Ok 10 }
    let getY () = async { return Error "Something went wrong while computing the y value" }
    let getZ () = async { return Ok 30 }

    _ar {
        let! x = getX ()
        let! y = getY ()
        let! z = getZ ()
        return x + y + z }
    |> Async.RunSynchronously
    |> printfn "AR Error: %A"

let tstARApplicative () = 
    let getX () = async { return Ok 10 }
    let getY () = async { return Error "Something went wrong while computing the y value" }
    let getZ () = async { return Ok 30 }

    _ar {
        let! x = getX ()
        and! y = getY ()
        and! z = getZ ()
        return x + y + z }
    |> Async.RunSynchronously
    |> printfn "AR applicative: %A"

//tstAR ()
//tstARErr ()
tstARApplicative ()