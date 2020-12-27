#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"
#load "../../src/Aabel/State.fs"
#load "../../src/Aabel/StateR.fs"
#load "../../src/Aabel/StateA.fs"
#load "../../src/Aabel/StateAR.fs"

open Simplee
open Simplee.StateR.ComputationExpression
open Simplee.StateAR.ComputationExpression

let stt = 0

let tstStateOk () =
    let getUser () = State (fun (stt: int) ->
        sprintf "User%d" stt |> Ok |> Async.retn, stt + 1)

    let getPassword () = State (fun (stt: int) ->
        sprintf "Pwd%d" stt |> Ok |> Async.retn, stt + 1)

    stateAR {
        let! user = getUser()
        let! pwd  = getPassword()
        return (user, pwd)
    }
    |> StateAR.run stt
    |> printfn "user/pwd: %A"

let tstStateErr () =
    let getUser () = StateAR.err "My error"

    let getPassword () = State (fun (stt: int) ->
        sprintf "Pwd%d" stt |> Ok |> Async.retn, stt + 1)

    stateAR {
        let! user = getUser()
        let! pwd  = getPassword()
        return (user, pwd)
    }
    |> StateAR.run stt
    |> printfn "user/pwd: %A"

let tstStateApplicative () =
    let getUser () = State (fun (stt: int) ->
        sprintf "User%d" stt |> Ok |> Async.retn, stt + 1)

    let getPassword () = State (fun (stt: int) ->
        sprintf "Pwd%d" stt |> Ok |> Async.retn, stt + 1)

    stateAR {
        let! user = getUser()
        and! pwd  = getPassword()
        return (user, pwd)
    }
    |> StateAR.run stt
    |> printfn "user/pwd: %A"

tstStateApplicative ()