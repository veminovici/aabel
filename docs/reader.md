# Simplee |> Aabel |> Reader

<br />

## 1. Reader Monads
The implementation of the **Reader** monad and some of its variations: **ReaderR** a reader that returns a **Result** value, and **ReaderAR** a reader that returns a **Async-Reeuslt** value.

### Reader
- Namespaces: *Simplee.Reader*, *Simplee.Reader.ComputationExpression*, *Simplee.Reader.Traversals* 
- Source: [Reader.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Reader.fs)
- Test: [TReader.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReader.fs)
- Cmputation expression: **reader**


### ReaderR
- Namespaces: *Simplee.ReaderR*, *Simplee.ReaderR.ComputationExpression*, *Simplee.ReaderR.Traversals* 
- Source: [ReaderR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderR.fs)
- Test: [TReaderR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderR.fs)
- Computation expression: **readerR**


### ReaderA
- Namespaces: *Simplee.ReaderA*, *Simplee.ReaderA.ComputationExpression*, *Simplee.ReaderA.Traversals* 
- Source: [ReaderR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderA.fs)
- Test: [TReaderR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderA.fs)
- Computation expression: **readerA**


### ReaderAR
- Namespaces: *Simplee.ReaderAR*, *Simplee.ReaderAR.ComputationExpression*, *Simplee.ReaderAR.Traversals* 
- Source: [ReaderR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderAR.fs)
- Test: [TReaderR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderAR.fs)
- Computation expression: **readerAR**

More examples: [QtReaderAR.fsx](https://github.com/veminovici/aabel/blob/main/tests/Scripts/QtReaderAR.fsx)

## 2. The *_reader[X]* Computation Expressions
The library implements a FSharp computation expression **reader** around the **Reader** monad, the **readerR** around **ReaderR** monad, **readerA** around **ReaderA** monad, and **readerAR** around **ReaderAR**

### 2.1 Monadic Computation
In below example, we have a configuration that stores the user name and the password. We have two
functions which read the user name and password. The computation flow calls these two functions and
returns the two values as pair.

```fsharp
open Simplee
open Simplee.ReaderAR.ComputationExpression

let env = 
    Map.empty 
    |> Map.add "user" "john" 
    |> Map.add "pwd" "test"

let getUser () = Reader (fun (env: Map<string, string>) ->
    env.["user"] |> Ok |> Async.retn)

let getPassword () = Reader (fun (env: Map<string, string>) ->
    env.["pwd"] |> Ok |> Async.retn)

readerAR {
    let! user = getUser()
    let! pwd  = getPassword()
    return (user, pwd) }
|> ReaderAR.run env
|> printfn "user/pwd: %A"
```

The code below shows the case when one of the functions returns an error. You can see that the error handling is done automatically for you, the flow being the same as the one in the success case.

```fsharp
let getUser () = Reader (fun (env: Map<string, string>) ->
    "User not configured" |> Error |> Async.retn)

let getPassword () = Reader (fun (env: Map<string, string>) ->
    env.["pwd"] |> Ok |> Async.retn)

readerAR {
    let! user = getUser()
    let! pwd  = getPassword()
    return (user, pwd) }
|> ReaderAR.run env
|> printfn "user/pwd: %A"
```

### 2.2 Applicative Computation
You can use applicative computation where all 3 values are computed, without checking the result of the previous operations.

```fsharp
let getUser () = Reader (fun (env: Map<string, string>) ->
    env.["user"] |> Ok |> Async.retn)

let getPassword () = Reader (fun (env: Map<string, string>) ->
    env.["pwd"] |> Ok |> Async.retn)

readerAR {
    let! user = getUser()
    and! pwd  = getPassword()
    return (user, pwd) }
|> ReaderAR.run env
|> printfn "user/pwd: %A"
```

<br />

## 3. Related Topics

- [Aabel](./index.md)
- [State](./state.md)

<br />

## 4. Support or Contact
Having trouble with the library? [Contact support](https://github.com/veminovici) and we’ll help you sort it out.
