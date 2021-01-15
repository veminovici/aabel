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

    let mapFst f r =
        let f' (a, b) = f a, b
        map f' r

    let mapSnd f r =
        let f' (a, b) = a, f b
        map f' r

    let map2 f x y =
        f
        |> singleton
        |> apply x
        |> apply y

    let zip x y = map2 (fun x y -> x, y) x y

    let bindLR (f:'a -> Async<'b>) (r: Async<'a>) : Async<'a * 'b> = 
        let f' a = a |> f |> map (fun b -> a, b)
        bind f' r

    let bindL f r = bindLR f r |> map fst
    let bindR f r = bindLR f r |> map snd

    let bindFst f r =
        let f' (a, b) = a |> f |> map (fun a -> a, b)
        bind f' r

    let bindSnd f r =
        let f' (a, b) = b |> f |> map (fun b -> a, b)
        bind f' r

    let map3 f x y z =
        y
        |> map2 f x
        |> apply z

    let concat (x: Async<'T list>) (y: Async<'T list>) =
        map2 (@) x y

    module Operators =
        let (<!>)  m f = map    f m
        let (</!>) m f = mapFst f m
        let (<!/>) m f = mapSnd f m

        let (<*>) f m = apply m f

        let (++)  a b = zip a b

        let (.>>.) m f = bindLR f m
        let (>>.)  m f = bindLR f m |> map snd
        let (.>>)  m f = bindLR f m |> map fst

        // let (>>=) m f = bind  f m
        let (>>=) = (>>.)

        let (/>>) m f = bindFst f m
        let (>>/) m f = bindSnd f m

    module Traversals = 

        [<RequireQualifiedAccess>]
        module Async = 

            // Result<'a list, 'e list> -> ('c -> Result<'a, 'e>) -> 'c list -> Result<'a list, 'b list>
            let _traverseA (zro: Async<'U list>) (f:'T -> Async<'U>) (xs:'T list) : Async<'U list> =

                let xs' = 
                    xs
                    |> List.map f
                    |> Async.Parallel
                    |> map List.ofArray

                (zro, xs') ||> concat

            let traverseA f xs =
                _traverseA (retn []) f xs

            let sequenceA xs =
                traverseA id xs

            let private _traverseM (zro : Async<'U list>) (f : 'T -> Async<'U>) (xs: 'T list) : Async<'U list> =

                let rec loop (acc: Async<'U list>) (xs:'T list ) =
                    match xs with
                    | [] -> acc
                    | h::tail ->
                        let acc =
                            async {
                            let! y = f h
                            let! ys = acc
                            return ys @ [y] }
                        loop acc tail

                loop zro xs

            let traverseM f xs =
                _traverseM (retn []) f xs

            let sequenceM xs =
                traverseM id xs
