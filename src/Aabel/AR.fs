namespace Simplee

type AR<'T, 'TErr> = Async<Result<'T, 'TErr>>

[<RequireQualifiedAccess>]
module AR =

    open System.Threading.Tasks

    let retn    x = x |> Ok |> async.Return
    let singleton = retn
    let err     e = e |> Error |> async.Return

    let map      f ar = Async.map (Result.map f) ar
    let mapError f ar = Async.map (Result.mapError f) ar

    let bind f ar = async {
        match! ar with
        | Ok    x -> return! f x
        | Error e -> return! async.Return (Error e) }

    let bindLR (f:'a -> AR<'b, 'E>) (r: AR<'a, 'E>) : AR<'a * 'b, 'E> = 
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

    let apply f x =
        bind (fun f -> 
            bind (f >> retn) x) f

    let fold okF errF ar = 
        Async.map (Result.fold okF errF) ar

    let ofChoice c =
        c
        |> Result.ofChoice
        |> async.Return

    let ofTask tsk = 
        tsk
        |> Async.AwaitTask
        |> Async.Catch
        |> Async.map Result.ofChoice

    let ofAction (tsk: Task) =
        tsk
        |> Async.AwaitTask
        |> Async.Catch
        |> Async.map Result.ofChoice

    let map2 f a1 a2 = 
        Async.map2 (Result.map2 f) a1 a2

    let zip a1 a2 = 
        Async.map2 Result.zip a1 a2

    let map3 f a1 a2 a3 =
        Async.map3 (Result.map3 f) a1 a2 a3

    let ignore ar = 
        map ignore ar

    let requireTrue e =
        Async.map (Result.requireTrue e)

    let requireFalse e =
        Async.map (Result.requireFalse e)

    let requireSome e =
        Async.map (Result.requireSome e)

    let requireNone e =
        Async.map (Result.requireNone e)

    let requireNotNull e =
        Async.map (Result.requireNotNull e)

    let requireEmpty e =
        Async.map (Result.requireEmpty e)

    let requireNotEmpty e =
        Async.map (Result.requireNotEmpty e)

    let withError e =
        Async.map (Result.withError e)

    let teeIf p f =
        Async.map (Result.teeIf p f)

    let teeErrorIf p f =
        Async.map (Result.teeErrorIf p f)

    let tee f =
        Async.map (Result.tee f)

    let teeError f =
        Async.map (Result.teeError f)

    let concat (x: AR<'a list, 'E>) (y: AR<'a list, 'E>) =
        map2 (@) x y

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m

        let (++)  a b = zip a b

        let (.>>.) m f = bindLR f m
        let (>>.)  m f = bindLR f m |> map snd
        let (.>>)  m f = bindLR f m |> map fst

        // let (>>=) m f = bind  f m
        let (>>=) = (>>.)

        let (/>>) m f = bindFst f m
        let (>>/) m f = bindSnd f m

    module ComputationExpression =
        type ARBuilder () =
            member _.Return(x) : AR<_,_>    = retn x
            member _.ReturnFrom(x: AR<_,_>) = x

            member _.Yield(x) : AR<_,_>     = retn x
            member _.YieldFrom(x: AR<_,_>)  = x

            member _.Zero ()                           = retn ()

            member _.Bind(m: AR<'T, 'TError>, f: 'T -> AR<'U, 'TError>) : AR<'U, 'TError> =
                bind f m

            member this.Combine(a1, a2) = 
                this.Bind(a1, fun _ -> a2)

            member _.Delay(f) =  async.Delay f

            member _.TryWith    (g, h) = async.TryWith(g, h)
            member _.TryFinally (g, c) = async.TryFinally(g, c)

            member _.Using(r, f) =
                async.Using(r, f)

            member this.While(g, c) =
                if not <| g ()
                then this.Zero()
                else this.Bind(c, fun _ -> this.While(g, c))

            member this.For(sequence: #seq<_>, f) =
                this.Using(sequence.GetEnumerator (), fun enum ->
                    this.While(enum.MoveNext,
                        this.Delay(fun () -> f enum.Current)))

            member _.BindReturn (x, f) = 
                map f x

            member _.MergeSources(t1, t2) = 
                zip t1 t2

            /// Helps with for..in and for..do
            member _.Source(s: #seq<_>) = 
                s

            member _.Source(r: AR<_,_>) = 
                r

            member _.Source(r: Result<_,_>) = 
                r
                |> Async.singleton

            member _.Source(c: Choice<_,_>) = 
                c 
                |> Result.ofChoice 
                |> Async.singleton
(*
            member _.Source(a: Async<_>) = 
                a 
                |> Async.map Ok
*)
            member _.Source(t: Task<_>) =
                t 
                |> Async.AwaitTask 
                |> Async.map Ok

        let _ar = ARBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module AR = 

            // AR<'a list, 'b list> -> ('c -> AR<'a, 'b>) -> 'c list -> AR<'a list, 'b list>
            let private _traverseA (zro: AR<'U list, 'E list>) (f:'T -> AR<'U, 'E>) (xs:'T list) : AR<'U list, 'E list> =

                let rec loop acc = function
                    | [] -> acc
                    | h :: tail ->
                        async {
                            let! s = acc
                            let! fR = 
                                h 
                                |> f 
                                |> mapError List.singleton

                            match s, fR with
                            | Ok ys, Ok y -> 
                                return! loop (singleton (ys @ [y])) tail
                            | Error errs, Error e -> 
                                return! loop (err (errs @ e)) tail
                            | Ok _, Error e 
                            | Error e , Ok _  -> 
                                return! loop (err e) tail }

                loop zro xs

            let traverseA f xs =
                _traverseA (singleton []) f xs

            let sequenceA xs =
                traverseA id xs

            let private _traversetM (zro : AR<'U list, 'TError>) (f :'T -> AR<'U, 'TError>) (xs:'T list) : AR<'U list, 'TError> =

                let rec loop acc xs =
                    match xs with
                    | [] -> acc
                    | h :: tail -> 
                        async {
                            let! r = _ar {
                                let! ys = acc
                                let! y = f h
                                return ys @ [y] }

                            match r with
                            | Ok _ -> 
                                return! loop (Async.singleton r) tail
                            | Error _ -> 
                                return r }

                loop zro xs
            
            let traverseM f xs = 
                _traversetM (singleton []) f xs

            let sequenceM xs =
                traverseM id xs