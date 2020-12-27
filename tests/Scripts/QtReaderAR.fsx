#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"
#load "../../src/Aabel/Reader.fs"
#load "../../src/Aabel/ReaderR.fs"
#load "../../src/Aabel/ReaderA.fs"
#load "../../src/Aabel/ReaderAR.fs"

open Simplee
open Simplee.ReaderAR.ComputationExpression

let env = 
    Map.empty 
    |> Map.add "user" "john" 
    |> Map.add "pwd" "test"

let tstReaderOk () =
    let getUser () = Reader (fun (env: Map<string, string>) ->
        env.["user"] |> Ok |> Async.retn)

    let getPassword () = Reader (fun (env: Map<string, string>) ->
        env.["pwd"] |> Ok |> Async.retn)

    readerAR {
        let! user = getUser()
        let! pwd  = getPassword()
        return (user, pwd)
    }
    |> ReaderAR.run env
    |> printfn "user/pwd: %A"

let tstReaderErr () =
    let getUser () = Reader (fun (env: Map<string, string>) ->
        "User not configured"|> Error |> Async.retn)

    let getPassword () = Reader (fun (env: Map<string, string>) ->
        env.["pwd"] |> Ok |> Async.retn)

    readerAR {
        let! user = getUser()
        let! pwd  = getPassword()
        return (user, pwd)
    }
    |> ReaderAR.run env
    |> printfn "user/pwd: %A"

let tstReaderApplicative () =
    let getUser () = Reader (fun (env: Map<string, string>) ->
        env.["user"] |> Ok |> Async.retn)

    let getPassword () = Reader (fun (env: Map<string, string>) ->
        env.["pwd"] |> Ok |> Async.retn)

    readerAR {
        let! user = getUser()
        and! pwd  = getPassword()
        return (user, pwd)
    }
    |> ReaderAR.run env
    |> printfn "user/pwd: %A"

tstReaderErr ()
tstReaderApplicative ()
