# Simplee |> Aabel |> AR
Implementation of the **Async-Result** monad which represents an **async** operation which returns a **Result** value.

- Namespaces: *Simplee.AR*, *Simplee.AR.ComputationExpression*, *Simplee.AR.Traversals* 
- Source: [AR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/AR.fs)
- Test: [TAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TAR.fs)
_ Computation expression: **_ar**
- Script: [AR.fsx](https://github.com/veminovici/aabel/blob/main/tests/Scripts/QtAR.fsx)

Open the relevand namespaces:

```fsharp
open Simplee
open Simplee.AR.ComputationExpression
```

## The _ar Computation Expression
The library implements a FSharp computation expression **_ar** around the **AR** monad.

### Monadic Computation
Below is an example, where each of the three async computations return an *int* value wrapped into a *Results*. 
Once the values are computed, the sum of their values is returned.

```fsharp
let getX () = async { return Ok 10 }
let getY () = async { return Ok 20 }
let getZ () = async { return Ok 30 }

_ar {
    let! x = getX ()
    let! y = getY ()
    let! z = getZ ()
    return x + y + z }
|> Async.RunSynchronously
|> printfn "AR result: %A"
```

The following example shows the case where one of the operations returns an error. In this case, since the *getY* returns the error, the *getZ* is not even called.

```fsharp
let getX () = async { return Ok 10 }
let getY () = async { return Error "Something went wrong while computing the y value" }
let getZ () = async { return Ok 30 }

_ar {
    let! x = getX ()
    let! y = getY ()
    let! z = getZ ()
    return x + y + z }
|> Async.RunSynchronously
```

## Applicative Computation
You can use applicative computation where all 3 values are computed, without checking the result of the previous operations.

```fsharp
let getX () = async { return Ok 10 }
let getY () = async { return Error "Something went wrong while computing the y value" }
let getZ () = async { return Ok 30 }

_ar {
    let! x = getX ()
    and! y = getY ()
    and! z = getZ ()
    return x + y + z }
|> Async.RunSynchronously
```

## Aabel Library
For details related to other monads implemented by **Aabel** library, please check the main documentation [file](https://github.com/veminovici/aabel/blob/main/README.md).