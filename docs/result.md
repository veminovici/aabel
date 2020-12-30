# Simplee |> Aabel |> Result

<br />

## 1. Result
The library implements several extension functions that enhance the functionality of the **Result** type.
- Namespaces: *Simplee*, *Simplee.Result.Operators*, *Simplee.Result.ComputationExpression*, *Simplee.Result.Traversals*
- Source: [Mapp.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Result.fs)
- Test: [TMapp.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TResult.fs)
- Computation expression: **_result**

### 1.1 Extension Functions
You can find the usual functor, applicative, and monadic functions: **retn**, **map**, **apply**, and **bind** and some of their siblings: **map2**, **map3**, **zip**, **concat**.

```fsharp
(Ok "ab", Ok 2)
||> Result.zip 
|>  (=) (Ok ("ab", 2))
|>  Assert.True
```

### 1.2 Create Functions
The library also provides a set of functions which check if values meet different conditions and returning a **result** value: **requireTrue**, **requireFalse**, **requireSome**, **requireNone** etc.

```fsharp
1
|> Some
|> Result.requireSome "The given value is not Some"
|> (=) (Ok 1)
|> Assert.True
```

## 2 The *_result* Computation Expression
You can use also the **_result** computation expression.

### 2.1 Monadic Style
The *_result* computation expression can be use in monadic style, where an error stops the compuration and it is returned as a result of the computation.

```fsharp
_result {
    let! x = Ok 10
    let! y = Ok 20
    return x + y }
|> (=) (Ok 30)
|> Assert.True
```

In case you want to report an error, you can do:

```fsharp
_result {
    let! x = Error "your error here"
    let! y = Ok 20
    return x + y }
|> (=) (Error "your error here")
|> Assert.True
```

### 2.2 Applicative style
The computation expression supports applicative style:

```fsharp
_result {
    let! x = Ok 10
    and! y = Ok 20
    return x + y }
|> (=) (Ok 30)
|> Assert.True
```

## 3 Traversals
There are supported both **traverseA** and **traverseM** and long with their *sequence* sibling functions:

```fsharp
let x = Ok 1
let y = Ok 2
let z = Ok 3

[x; y; z]
|> Result.sequenceM
|> (=) (Ok [1; 2; 3])
```

The difference between the two set of functions is visible when they handle the errors: the *M* functions returns the first error, while the *A* functions return all the errors:

```fsharp
let x = Ok 1
let y = Error "the 1st error here"
let z = Error "the 2nd error here"

[x; y; z]
|> Result.sequenceM
|> (=)  (Error "the 1st error here")

[x; y; z]
|> Result.sequenceA
|> (=)  (Error ["the 1st error here"; "the 2nd error here"])
```

<br />

## 4. Related Topics

- [Aabel](./index.md)
- [Async](./async.md)
- [Async-Result](./ar.md)

<br />

## 5. Support or Contact
Having trouble with the library? [Contact support](https://github.com/veminovici) and we’ll help you sort it out.

