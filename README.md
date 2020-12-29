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
A F# library for common functionality around Result, Async, Reader, State, and Queue monads.

- [Basic Extensions (Result, Async, and Async-Result)](https://github.com/veminovici/aabel#1-basic-extensions)
    - [Documentation](https://github.com/veminovici/aabel/blob/main/readme/AR.md)
    - [Result Monad](https://github.com/veminovici/aabel#11-result)
    - [Async Monad](https://github.com/veminovici/aabel#12-async)
    - [AR Monad](https://github.com/veminovici/aabel#13-ar-monad)
- [Reader](https://github.com/veminovici/aabel#2-reader)
    - [Documentation](https://github.com/veminovici/aabel/blob/main/readme/Reader.md)
    - [Reader Monad](https://github.com/veminovici/aabel#21-reader-monad)
    - [ReaderR Monad](https://github.com/veminovici/aabel#22-readerr-monad)
    - [ReaderR Monad](https://github.com/veminovici/aabel#23-readera-monad)
    - [ReaderAR Monad](https://github.com/veminovici/aabel#24-readerar-monad)
- [State](https://github.com/veminovici/aabel#3-state)
    - [Documentation](https://github.com/veminovici/aabel/blob/main/readme/State.md)
    - [State Monad](https://github.com/veminovici/aabel#31-state-monad)
    - [StateR Monad](https://github.com/veminovici/aabel#32-stater-monad)
    - [StateA Monad](https://github.com/veminovici/aabel#33-statea-monad)
    - [StateAR Monad](https://github.com/veminovici/aabel#34-statear-monad)
- [Queue](https://github.com/veminovici/aabel#4-queue)
    - [Documentation](https://github.com/veminovici/aabel/blob/main/readme/Queue.md)
    - [Queue Monad](https://github.com/veminovici/aabel#41-queue-monad)
    - [QueueR Monad](https://github.com/veminovici/aabel#42-queuer-monad)

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

You can find more details and examples related to the *_ar compuration expression* at the [AR](https://github.com/veminovici/aabel/blob/main/readme/AR.md) documentation file.

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

You can find more details and examples related to the *reader[X] compuration expressions* at the [Reader](https://github.com/veminovici/aabel/blob/main/readme/Reader.md) documentation file.

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

You can find more details and examples related to the *state[X] compuration expressions* at the [State](https://github.com/veminovici/aabel/blob/main/readme/State.md) documentation file.

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

## 4. Queue
Implementations of the **Queue** monad and several related *Queue[X]* monads which return *Result*, *Async*, or a combination of both *Async-Result*.

You can find more details and examples related to the *state[X] compuration expressions* at the [Queue](https://github.com/veminovici/aabel/blob/main/readme/Queue.md) documentation file.

### 4.1 Queue Monad
Implementation of the **Queue** monad.

- Namespaces: *Simplee.Queue*, *Simplee.Queue.ComputationExpression*
- Source: [Queue.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Queue.fs)
- Test: [TQueue.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TQueue.fs)

```fsharp
open Simplee
open Simplee.Collections
open Simplee.Collections.Queue.ComputationExpression

let puree   a = State.retn a
let enq    xs = StateR.retn ()
let deq     n = StateR.retn [1..n]
let peek    n = StateR.retn [1..n]
let isFull () = StateR.retn false
let eval s0 p = Queue.eval puree enq deq peek isFull s0 p

"abcde"
|> Queue.retn
|> Queue.map (fun (s: string) -> s.Length)
|> eval "env"
|> (=) 5
|> Assert.True
```

### 4.2 QueueR Monad
Implementation of the **QueueR** monad, a state transition which returns a **Result** and the error values are automatically handled by the **bind** function, so the flow stops when the first error is encountered.

- Namespaces: *Simplee.QueueR*, *Simplee.QueueR.ComputationExpression*
- Source: [QueueR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/QueueR.fs)
- Test: [TQueueR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TQueueR.fs)

```fsharp
open Simplee
open Simplee.Collections
open Simplee.Collections.QueueR.ComputationExpression

let puree   a = StateR.retn a
let enq    xs = StateR.retn ()
let deq     n = StateR.retn [1..n]
let peek    n = StateR.retn [1..n]
let isFull () = StateR.retn false
let eval s0 p = QueueR.eval puree enq deq peek isFull s0 p

_queueR {
    do! QueueR.enqueue [1..5]
    let! xs = QueueR.dequeue 2
    let! ys = QueueR.dequeue 1
    return (xs, ys)
} 
|> QueueR.eval puree enq deq peek isFull []
|> printfn "Result: %A" // ok ([1;2], [3])
```

### 4.2 QueueAR Monad
Implementation of the **QueueAR** monad, a state transition which returns asynchronously a **Result** and the error values are automatically handled by the **bind** function, so the flow stops when the first error is encountered.

- Namespaces: *Simplee.QueueAR*, *Simplee.QueueAR.ComputationExpression*
- Source: [QueueAR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/QueueAR.fs)
- Test: [TQueueAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TQueueAR.fs)

```fsharp
open Simplee
open Simplee.Collections
open Simplee.Collections.QueueR.ComputationExpression

let puree   a = StateAR.retn a
let enq    xs = StateAR.retn ()
let deq     n = StateAR.retn [1..n]
let peek    n = StateAR.retn [1..n]
let isFull () = StateAR.retn false
let eval s0 p = QueueAR.eval puree enq deq peek isFull s0 p

_queueAR {
    do! QueueAR.enqueue [1..5]
    let! xs = QueueAR.dequeue 2
    let! ys = QueueAR.dequeue 1
    return (xs, ys)
} 
|> QueueAR.eval puree enq deq peek isFull []
|> Async.RunSynchronously
|> printfn "Result: %A"  // Ok ([1;2], [3])
```

<br />


### Thank you!

> You can contact me at veminovici@hotmail.com. Code designed and written in Päädu, on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.