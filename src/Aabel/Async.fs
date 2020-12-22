namespace Simplee

[<RequireQualifiedAccess>]
module Async =

    let retn    x = async.Return x
    let singleton = retn

    let bind f m = async.Bind(m, f)

    let apply m f = 
        bind (fun f ->
            bind (f >> singleton) m) f

    let map f m = m |> bind (f >> singleton)

    let map2 f x y =
        f
        |> singleton
        |> apply x
        |> apply y

    let zip x y = map2 (fun x y -> x, y) x y

    let map3 f x y z =
        y
        |> map2 f x
        |> apply z

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply m f
        let (>>=) m f = bind  f m