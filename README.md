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

### Result
A set of extensions for the **Result** type. 

- Source: [Result.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Result.fs)
- Test: [TResult.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TResult.fs)

```fsharp
let okF  (s: string) = s.Length
let errF (s: string) = s.Length + 10

"abc"
|> Ok
|> Result.either okF errF
|> (=) 3
```

### Async
A set of extensions for the **Async** type.

- Source: [Result.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Async.fs)
- Test: [TResult.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TAsync.fs)

```fsharp
let f (x: string) (y: string) = x + y

Async.map2 f (Async.retn "ab") (Async.retn "cd")
|> Async.RunSynchronously
|> (=) "abcd"
|> Assert.True
```

<br />

### Thank you!

> You can contact me at veminovici@hotmail.com. Code designed and written in Päädu, on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.