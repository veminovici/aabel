namespace Simplee

type ReaderR<'TEnv, 'T, 'E> = Reader<'TEnv, Result<'T, 'E>>

[<RequireQualifiedAccess>]
module ReaderR =

    let run env (r: ReaderR<_,_,_>) =  
        r 
        |> Reader.run env

    let retn     x : ReaderR<_,_,_> = x |> Result.ok  |> Reader.retn
    let singleton                   = retn
    let err      e : ReaderR<_,_,_> = e |> Result.err |> Reader.retn

    let map f (m: ReaderR<'TEnv, 'T, 'E>) : ReaderR<'TEnv, 'U, 'E> =
        m 
        |> Reader.map (Result.map f)

    let mapError f (m: ReaderR<'TEnv, 'T, 'E>) : ReaderR<'TEnv, 'T, 'F> = 
        m
        |> Reader.map (Result.mapError f)

    let mapEither f g (m: ReaderR<'TEnv, 'T, 'E>) : ReaderR<'TEnv, 'U, 'F> =
        m
        |> Reader.map (Result.mapEither f g)

    let bind (f: 'T -> ReaderR<'TEnv, 'U, 'E>) (m: ReaderR<'TEnv, 'T, 'E>) : ReaderR<'TEnv, 'U, 'E> =
        m 
        |> Reader.bind (Result.fold f (Error >> Reader.retn))

    let apply f (m: ReaderR<'TEnv, 'T, 'E>) : ReaderR<'TEnv, 'U, 'E> = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f (x: ReaderR<'TEnv, 'T, 'E>) (y: ReaderR<'TEnv, 'U, 'E>) : ReaderR<'TEnv, 'Z, 'E> =
        Reader.map2 (Result.map2 f) x y

    let zip (x: ReaderR<'TEnv, 'T, 'E>) (y: ReaderR<'TEnv, 'U, 'E>) = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let ignore (rr: ReaderR<_,_,_>) =
        map ignore rr

    let requireTrue e =
        Reader.map (Result.requireTrue e)

    let requireFalse e =
        Reader.map (Result.requireFalse e)

    let requireSome e =
        Reader.map (Result.requireSome e)

    let requireNone e =
        Reader.map (Result.requireNone e)

    let requireNotNull e =
        Reader.map (Result.requireNotNull e)

    let requireEmpty e =
        Reader.map (Result.requireEmpty e)

    let requireNotEmpty e =
        Reader.map (Result.requireNotEmpty e)

    let withError e =
        Reader.map (Result.withError e)

    let teeIf p f =
        Reader.map (Result.teeIf p f >> Ok)

    let teeErrorIf p f =
        Reader.map (Result.teeErrorIf p f >> Ok)

    let tee f =
        Reader.map (Result.tee f >> Ok)

    let teeError f =
        Reader.map (Result.teeError f >> Ok)

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =
        open System

        type ReaderRBuilder () =
            member _.Return(x) : ReaderR<_,_,_>    = retn x
            member _.ReturnFrom(x: ReaderR<_,_,_>) = x

            member _.Yield(x) : ReaderR<_,_,_>     = retn x
            member _.YieldFrom(x: ReaderR<_,_,_>)  = x

            member _.Zero ()                       = retn ()

            member _.Bind(m: ReaderR<'TEnv, 'T, 'E>, f: 'T -> ReaderR<'TEnv, 'U, 'E>) : ReaderR<'TEnv, 'U, 'E> =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : ReaderR<_, _,_> = 
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

            member _.Source(r: ReaderR<_,_,_>) = 
                r

            member _.Source(r: Result<_,_>) : ReaderR<_,_,_> = 
                r
                |> Reader.retn

            member _.Source(c: Choice<_,_>) : ReaderR<_,_,_> = 
                c 
                |> Result.ofChoice 
                |> Reader.retn

        let readerR = ReaderRBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module ReaderR = 
(*
            let private _traverseA state f xs =

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

                loop state xs

            let traverseA f xs =
                _traverseA (singleton []) f xs

            let sequenceA xs =
                traverseA id xs
*)
            let private _traversetM (zro: ReaderR<'TEnv, 'U list, 'E>) (f: 'T -> ReaderR<'TEnv , 'U, 'E>) (xs: 'T list) : ReaderR<'TEnv, 'U list, 'E> =

                let rec loop (acc: ReaderR<'TEnv, 'U list, 'E>) (xs: 'T list) =
                    match xs with
                    | [] -> acc
                    | h :: tail -> 
                        readerR {
                            let! ys = acc
                            let! y  = f h
                            return ys @ [y] }
                        |> bind (fun acc -> loop (retn acc) tail)

                loop zro xs
            
            let traverseM f xs = 
                _traversetM (singleton []) f xs

            let sequenceM xs =
                traverseM id xs