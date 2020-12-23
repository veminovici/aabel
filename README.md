# Simplee |> Aabel

<br />

## Builds
[![Github CI](https://github.com/veminovici/aabel/workflows/CINet/badge.svg)](https://github.com/veminovici/aabel/actions)
[![Languages](https://img.shields.io/github/languages/top/veminovici/aabel?color=%23b845fc)](https://github.com/veminovici/aabel)
[![Release](https://img.shields.io/github/v/release/veminovici/aabel?include_prereleases)](https://github.com/veminovici/aabel/releases)
[![Repo size](https://img.shields.io/github/repo-size/veminovici/aabel)](https://github.com/veminovici/aabel)
[![License](https://img.shields.io/github/license/veminovici/aabel)](https://opensource.org/licenses/Apache-2.0)
[![Coverage Status](https://coveralls.io/repos/github/veminovici/aabel/badge.svg?branch=main&bust=1)](https://coveralls.io/github/veminovici/aabel)

<br />

## Description
A F# library for common functionality around Result, Async, Reader, and State monads.

<br />

## 1. Basic Extensions

### 1.1 Result
A set of extensions for the **Result** type. 

- Namespaces: *Simplee.Result*, *Simplee.Result.ComputationExpression*, *Simplee.Result.Traversals* 
- Source: [Result.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Result.fs)
- Test: [TResult.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TResult.fs)

```fsharp
let okFn  (s: string) = s.Length
let errFn (s: string) = s.Length + 10

"abc"
|> Ok
|> Result.either okFn errFn
|> (=) 3
```

The **Result** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

```fsharp
let x = Ok "ab"
let y = Ok "cd"
let z = Ok "ef"

[x; y; z]
|> Result.sequenceM
|> (=) (Ok ["ab"; "cd"; "ef"])

let x = Ok "ab"
let y = Error "e"
let z = Ok "ef"

[x; y; z]
|> Result.sequenceM
|> (=)  (Error "e")

let x = Ok 1
let y = Error "e"
let z = Error "e1"

[x; y; z]
|> Result.sequenceA
|> (=)  (Error ["e"; "e1"])
```

### 1.2 Async
A set of extensions for the **Async** type.

- Namespaces: *Simplee.Async*, *Simplee.Async.ComputationExpression*, *Simplee.Async.Traversals* 
- Source: [Async.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Async.fs)
- Test: [TAsync.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TAsync.fs)

```fsharp
let f (x: int) (y: int) = x + y
let x = Async.retn 10
let y = Async.retn 20

Async.map2 f x y
|> Async.RunSynchronously
|> (=) 30
```

The **Async** extensions implement the *traverseM* and *sequenceM* which executes sequencially a list of *async* flows, and *traverseA* and *sequenceA* which executes in parallel a list of *async* flows.

```fsharp
[1; 2; 3]
|> List.map Async.retn
|> Async.sequenceM
|> Async.RunSynchronously
|> (=) [1; 2; 3]

[1; 2; 3]
|> List.map Async.retn
|> Async.sequenceA
|> Async.RunSynchronously
|> (=) [1; 2; 3]
```

### 1.3 AR Monad
Implementation for the **AR** (Async-Result) monad.

- Namespaces: *Simplee.AR*, *Simplee.AR.ComputationExpression*, *Simplee.AR.Traversals* 
- Source: [AR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/AR.fs)
- Test: [TAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TAR.fs)

```fsharp
"my error"
|> AR.err
|> AR.mapError (fun (s: string) = s.Length)
|> Async.RunSynchronously
|> (=) (Error 8)
```

The **AR** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

```fsharp
let x1 = AR.retn 1
let x2 = AR.retn 2
let x3 = AR.retn 3
let x4 = AR.retn 4
let x5 = AR.retn 5

[x1; x2; x3; x4; x5]
|> AR.sequenceM
|> Async.RunSynchronously
|> (=)  (Ok [1; 2; 3; 4; 5])

let x1 = AR.retn 1
let x2 = AR.err "e2"
let x3 = AR.retn 3
let x4 = AR.err "e4"
let x5 = AR.retn 5

[x1; x2; x3; x4; x5]
|> AR.sequenceM
|> Async.RunSynchronously
|> (=)  (Error "e2")


let x = AR.retn "ab"
let y = AR.err "e"
let z = AR.err "e1"

[x; y; z]
|> AR.sequenceA
|> Async.RunSynchronously
|> (=)  (Error ["e"; "e1"])
```

<br />

## 2. Reader
Implementations of the **Reader** monad and several related *Reader[X]* monads which return *Result*, *Async*, or a combination of both *Async-Result*.

### 2.1 Reader Monad
Implementation of the **Reader** monad.

- Namespaces: *Simplee.Reader*, *Simplee.Reader.ComputationExpression*, *Simplee.Reader.Traversals*
- Source: [Reader.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Reader.fs)
- Test: [TReader.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReader.fs)

```fsharp
"abcde"
|> Reader.retn
|> Reader.map (fun (s: string) -> s.Length)
|> Reader.run "env"
|> (=) 5
```

### 2.2 ReaderR Monad
Implementation of the **ReaderR** monad, a reader which returns a **Result**.

- Namespaces: *Simplee.ReaderR*, *Simplee.ReaderR.ComputationExpression*, *Simplee.ReaderR.Traversals* 
- Source: [ReaderR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderR.fs)
- Test: [TReaderR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderR.fs)

```fsharp
let f = ReaderR.retn <| fun (s: string) -> s.Length

"abcde"
|> ReaderR.retn
|> ReaderR.apply f
|> ReaderR.run "env"
|> (=) (Ok 5)
```

The **ReaderR** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

### 2.3 ReaderA Monad
Implementation of the **ReaderA** monad, a reader which returns an **Async**.

- Namespaces: *Simplee.ReaderA*, *Simplee.ReaderA.ComputationExpression*, *Simplee.ReaderA.Traversals* 
- Source: [ReaderA.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderA.fs)
- Test: [TReaderA.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderA.fs)

```fsharp
let fn x y = x + y
let x = ReaderA.retn 10
let y = ReaderA.retn 20

(x, y)
||> ReaderA.map2 fn
|> ReaderA.run "env"
|> (=) 30
```

### 2.4 ReaderAR Monad
Implementation of the **ReaderAR** monad, a reader which returns an **AR** (Async-Result).

- Namespaces: *Simplee.ReaderAR*, *Simplee.ReaderAR.ComputationExpression*, *Simplee.ReaderAR.Traversals* 
- Source: [ReaderAR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/ReaderAR.fs)
- Test: [TReaderAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TReaderAR.fs)

```fsharp
let fnOk  (s: string) = s.Length
let fnErr (s: string) = sprintf "%s1" s

"abcde"
|> ReaderAR.retn
|> ReaderAR.mapEither fnOk fnErr
|> ReaderAR.run  "env"
|> (=) (Ok 5)
```

The **ReaderAR** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

<br />

## 3. State
Implementations of the **State** monad and several related *State[X]* monads which return *Result*, *Async*, or a combination of both *Async-Result*.

### 3.1 State Monad
Implementation of the **State** monad.

- Namespaces: *Simplee.State*, *Simplee.State.ComputationExpression*, *Simplee.State.Traversals*
- Source: [State.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/State.fs)
- Test: [TState.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TState.fs)

```fsharp
4
|> State.retn
|> State.map (fun x -> float x * 2.) 
|> State.eval 10.
|> (=) 8.
```

### 3.2 StateR Monad
Implementation of the **StateR** monad, a state transition which returns a **Result**.

- Namespaces: *Simplee.StateR*, *Simplee.StateR.ComputationExpression*, *Simplee.StateR.Traversals*
- Source: [StateR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateR.fs)
- Test: [TStateR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateR.fs)

```fsharp
let f = StateR.retn <| fun (s: string) -> s.Length

"abcde"
|> StateR.retn
|> StateR.apply f
|> StateR.eval "env"
|> (=) (Ok 5)
```

The **StateR** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

### 3.3 StateA Monad
Implementation of the **StateA** monad, a state transition which returns an **Async**.

- Namespaces: *Simplee.StateA*, *Simplee.StateA.ComputationExpression*, *Simplee.StateA.Traversals*
- Source: [StateA.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateA.fs)
- Test: [TStateA.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateA.fs)

```fsharp
let fn x y = x + y
let x = StateA.retn 10
let y = StateA.retn 20

(x, y)
||> StateA.map2 fn
|>  StateA.eval "env"
|>  (=) 30
```

### 3.4 StateAR Monad
Implementation of the **StateAR** monad, a reader which returns an **AR** (Async-Result).

- Namespaces: *Simplee.StateAR*, *Simplee.StateAR.ComputationExpression*, *Simplee.StateAR.Traversals*
- Source: [StateAR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/StateAR.fs)
- Test: [TStateAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TStateAR.fs)

```fsharp
let fnOk  (s: string) = s.Length
let fnErr (s: string) = sprintf "%s1" s

"abcde"
|> StateAR.retn
|> StateAR.mapEither fnOk fnErr
|> StateAR.eval  "env"
|> (=) (Ok 5)
```

The **StateAR** implements both traversals, *traverseM* and *traverseA* (as well as their paired sequence functions).

<br />


### Thank you!

> You can contact me at veminovici@hotmail.com. Code designed and written in Päädu, on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.