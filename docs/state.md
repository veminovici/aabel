# Simplee |> Aabel |> State
The implementation of the **State** monad and some of its variations: **StateR** a state transition that returns a **Result** value, and **StateAR** a state transition that returns a **Async-Reeuslt** value.

### State
- Namespaces: *Simplee.State*, *Simplee.State.ComputationExpression*, *Simplee.State.Traversals* 
- Source: [State.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/State.fs)
- Test: [TState.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TState.fs)
- Cmputation expression: **state**


### StateR
- Namespaces: *Simplee.StateR*, *Simplee.StateR.ComputationExpression*, *Simplee.StateR.Traversals* 
- Source: [StateR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateR.fs)
- Test: [TStateR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateR.fs)
- Computation expression: **stateR**


### StateA
- Namespaces: *Simplee.StateA*, *Simplee.StateA.ComputationExpression*, *Simplee.StateA.Traversals* 
- Source: [StateR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateA.fs)
- Test: [TStateR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateA.fs)
- Computation expression: **stateA**


### StateAR
- Namespaces: *Simplee.StateAR*, *Simplee.StateAR.ComputationExpression*, *Simplee.StateAR.Traversals* 
- Source: [StateR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateAR.fs)
- Test: [TStateR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateAR.fs)
- Computation expression: **stateAR**


More examples: [QtStateAR.fsx](https://github.com/veminovici/aabel/blob/main/tests/Scripts/QtStateAR.fsx)

## The Computation Expressions
The library implements a FSharp computation expression **state** around the **State** monad, the **stateR** around **StateR** monad, **stateA** around **StateA** monad, and **stateAR** around **StateAR**

### Monadic Computation

In below example, we have an internal state represented by an integer value. We have two
functions which return the user and password built using the internal state. The computation flow calls these two functions and
returns the two values as pair.

```fsharp
open Simplee
open Simplee.StateAR.ComputationExpression

let stt = 0

let getUser () = State (fun (stt: int) ->
    stt |> sprintf "User%d" |> Ok |> Async.retn, stt + 1)

let getPassword () = State (fun (stt: int) ->
    stt |> sprintf "Pwd%d" |> Ok |> Async.retn, stt + 1)

stateAR {
    let! user = getUser()
    let! pwd  = getPassword()
    return (user, pwd)
}
|> StateAR.run stt
|> printfn "user/pwd: %A"
```

The code below shows the case when one of the functions returns an error. You can see that the error handling is done automatically for you, the flow being the same as the one in the success case.

```fsharp
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
```

## Applicative Computation
You can use applicative computation where all 3 values are computed, without checking the result of the previous operations.

```fsharp
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
```

## Aabel Library
For details related to other monads implemented by **Aabel** library, please check the main documentation [file](https://github.com/veminovici/aabel/blob/main/README.md).