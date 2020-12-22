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

### 1.2 Async
A set of extensions for the **Async** type.

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

### 1.3 AR Monad
Implementation for the **AR** (Async-Result) monad.

- Source: [AR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/AR.fs)
- Test: [TAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TAR.fs)

```fsharp
"my error"
|> AR.err
|> AR.mapError (fun (s: string) = s.Length)
|> Async.RunSynchronously
|> (=) (Error 8)
```

<br />

## 2. Reader
Implementations of the **Reader** monad and several related *Reader[X]* monads which return *Result*, *Async*, or a combination of both *Async-Result*.

### 2.1 Reader Monad
Implementation of the **Reader** monad.

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

### 2.3 ReaderA Monad
Implementation of the **ReaderA** monad, a reader which returns an **Async**.

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

<br />

## 2. State
Implementations of the **State** monad and several related *State[X]* monads which return *Result*, *Async*, or a combination of both *Async-Result*.

### 2.1 State Monad
Implementation of the **State** monad.

- Source: [State.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/State.fs)
- Test: [TState.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TState.fs)

```fsharp
4
|> State.retn
|> State.map (fun x -> float x * 2.) 
|> State.eval 10.
|> (=) 8.
```

<br />

### Thank you!

> You can contact me at veminovici@hotmail.com. Code designed and written in Päädu, on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.