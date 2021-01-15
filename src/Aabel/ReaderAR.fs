namespace Simplee

type ReaderAR<'TEnv, 'T, 'E> = Reader<'TEnv, Async<Result<'T, 'E>>>

[<RequireQualifiedAccess>]
module ReaderAR =

    let run env (r: ReaderAR<_,_,_>) =  
        r 
        |> ReaderA.run env

    let retn     x : ReaderAR<_,_,_> = x |> Result.ok  |> ReaderA.retn
    let singleton                    = retn
    let err      e : ReaderAR<_,_,_> = e |> Result.err |> ReaderA.retn

    let mapEither f g m =
        m
        |> Reader.map (Async.map (Result.mapEither f g))

    let map (f: 'T -> 'U) (m: ReaderAR<'TEnv, 'T, 'E>) : ReaderAR<'TEnv, 'U, 'E> =
        m
        |> Reader.map (Async.map (Result.map f))

    let mapError (f: 'E -> 'F) (m: ReaderAR<'TEnv, 'T, 'E>) : ReaderAR<'TEnv, 'T, 'F> =
        m
        |> Reader.map (Async.map (Result.mapError f))

    let bind (f: 'T -> ReaderAR<'TEnv, 'U, 'E>) (m: ReaderAR<'TEnv, 'T, 'E>) : ReaderAR<'TEnv, 'U, 'E> =
        m
        |> ReaderA.bind (fun x -> 
            match x with
            | Ok x    -> x |>  f
            | Error e -> e |> err )

    let bindLR f r = 
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

    let apply f (m: ReaderAR<'TEnv, 'T, 'E>) : ReaderAR<'TEnv, 'U, 'E> = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f (x: ReaderAR<'TEnv, 'T, 'E>) (y: ReaderAR<'TEnv, 'U, 'E>) : ReaderAR<'TEnv, 'Z, 'E> =
        ReaderA.map2 (Result.map2 f) x y

    let zip (x: ReaderAR<'TEnv, 'T, 'E>) (y: ReaderAR<'TEnv, 'U, 'E>) = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let ignore (rr: ReaderAR<_,_,_>) =
        map ignore rr

    let requireTrue e =
        ReaderA.map (Result.requireTrue e)

    let requireFalse e =
        ReaderA.map (Result.requireFalse e)

    let requireSome e =
        ReaderA.map (Result.requireSome e)

    let requireNone e =
        ReaderA.map (Result.requireNone e)

    let requireNotNull e =
        ReaderA.map (Result.requireNotNull e)

    let requireEmpty e =
        ReaderA.map (Result.requireEmpty e)

    let requireNotEmpty e =
        ReaderA.map (Result.requireNotEmpty e)

    let withError e =
        ReaderA.map (Result.withError e)

    let teeIf p f =
        ReaderA.map (Result.teeIf p f >> Ok)

    let teeErrorIf p f =
        ReaderA.map (Result.teeErrorIf p f >> Ok)

    let tee f =
        ReaderA.map (Result.tee f >> Ok)

    let teeError f =
        ReaderA.map (Result.teeError f >> Ok)

    let concat x y =
        map2 (@) x y

    let lift1R (f: 'E -> 'a -> Result<'b, 'TErr>) : 'a -> ReaderAR<'E, 'b, 'TErr> =
        fun a -> Reader <| fun e -> f e a |> Async.retn

    let lift1A (f: 'E -> 'a -> Async<'b>) : 'a -> ReaderAR<'E, 'b, 'TErr> =
        fun a -> Reader <| fun e -> f e a |> Async.map Ok

    let lift1 (f: 'E -> 'a -> AR<'b, 'TErr>) : 'a -> ReaderAR<'E, 'b, 'TErr> =
        fun a -> Reader <| fun e -> f e a

    let lift2R (f: 'E -> 'a -> 'b -> Result<'c, 'TErr>) : 'a -> 'b -> ReaderAR<'E, 'c, 'TErr> =
        fun a b -> Reader <| fun e -> f e a b |> Async.retn

    let lift2A (f: 'E -> 'a -> 'b -> Async<'c>) : 'a -> 'b -> ReaderAR<'E, 'c, 'TErr> =
        fun a b -> Reader <| fun e -> f e a b |> Async.map Ok

    let lift2 (f: 'E -> 'a -> 'b -> AR<'c, 'TErr>) : 'a -> 'b -> ReaderAR<'E, 'c, 'TErr> =
        fun a b -> Reader <| fun e -> f e a b

    let unfold iter =
        let gen env = 
            iter
            |> run env
            |> function
            | Ok r    -> Some (r, env)
            | Error _ -> None

        Seq.unfold gen

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m

        let (.>>.) m f = bindLR f m
        let (>>.)  m f = bindLR f m |> map snd
        let (.>>)  m f = bindLR f m |> map fst

        //let (>>=) m f  = bindLR f m |> map snd
        let (>>=) = (>>.)

        let (++) a b  = zip a b

        let (/>>) m f = bindFst f m
        let (>>/) m f = bindSnd f m

    module ComputationExpression =
        open System

        type ReaderARBuilder () =
            member _.Return(x) : ReaderAR<_,_,_>    = retn x
            member _.ReturnFrom(x: ReaderAR<_,_,_>) = x

            member _.Yield(x) : ReaderAR<_,_,_>     = retn x
            member _.YieldFrom(x: ReaderAR<_,_,_>)  = x

            member _.Zero ()                       = retn ()

            member _.Bind(m: ReaderAR<'TEnv, 'T, 'E>, f: 'T -> ReaderAR<'TEnv, 'U, 'E>) : ReaderAR<'TEnv, 'U, 'E> =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : ReaderAR<_, _,_> = 
                this.TryFinally (
                    (fun () -> f r),
                    (fun () -> if not <| obj.ReferenceEquals(r, null) then r.Dispose ()))
                    
            member this.While(g, f) =
                if not <| g() then this.Zero ()
                else this.Bind(this.Run f, fun _ -> this.While (g, f))

            member this.For(s: #seq<'T>, f) =
                this.Using(s.GetEnumerator(), fun enum ->
                    this.While(enum.MoveNext,
                        this.Delay(fun () -> f enum.Current)))

            member _.BindReturn (x, f) = 
                map f x

            member _.MergeSources(t1, t2) = 
                zip t1 t2

            /// Helps with for..in and for..do
            member _.Source(s: #seq<_>) = 
                s

            member _.Source(r: ReaderAR<_,_,_>) = 
                r

            member _.Source(r: Result<_,_>) : ReaderAR<_,_,_> = 
                r
                |> ReaderA.retn

            member _.Source(c: Choice<_,_>) : ReaderAR<_,_,_> = 
                c 
                |> Result.ofChoice 
                |> ReaderA.retn

        let _readerAR = ReaderARBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module ReaderAR = 

            open Simplee.ReaderA.ComputationExpression
            open Simplee.Result.Traversals

            // ReaderR<'TEnv, 'a list, 'b list> -> ('c -> ReaderR<'TEnv, 'a, 'b>) -> 'c list -> ReaderR<'TEnv, 'a list, 'b list)
            let private _traverseA (zro: ReaderAR<'TEnv, 'a list, 'b list>) (f:'c -> ReaderAR<'TEnv, 'a, 'b>) (xs:'c list) : ReaderAR<'TEnv, 'a list, 'b list> =

                let rec loop (acc: ReaderAR<'TEnv, 'a list, 'b list>) (xs: 'c list) =
                    match xs with
                    | [] -> acc
                    | h::tail ->
                        let r = 
                            _readerA {
                                let! y = f h
                                let! ys = acc
                                let yys = Result._traverseA ys (fun _ -> y) [h]
                                return yys
                            }
                        loop r tail

                loop zro xs

            let traverseA f xs =
                _traverseA (retn []) f xs

            let sequenceA xs =
                traverseA id xs

            let private _traversetM (zro: ReaderAR<'TEnv,'U list, 'E>) (f: 'T -> ReaderAR<'TEnv,'U, 'E>) (xs: 'T list) =

                let rec loop (acc: ReaderAR<'TEnv, 'U list, 'E>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = _readerAR {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs
            
            let traverseM f xs =
                _traversetM (retn []) f xs

            let sequenceM xs =
                traverseM id xs
