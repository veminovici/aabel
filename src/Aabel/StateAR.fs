namespace Simplee

type StateAR<'S, 'T, 'E> = State<'S, Async<Result<'T, 'E>>>

[<RequireQualifiedAccess>]
module StateAR =

    open Simplee

    let run env (r: StateAR<_,_,_>) =  
        r 
        |> StateA.run env
    let eval s stt = run s stt |> fst
    let exec s stt = run s stt |> snd

    let retn     x : StateAR<_,_,_> = x |> Result.ok  |> StateA.retn
    let singleton                    = retn
    let err      e : StateAR<_,_,_> = e |> Result.err |> StateA.retn

    let mapEither f g m =
        m
        |> State.map (Async.map (Result.mapEither f g))

    let map (f: 'T -> 'U) (m: StateAR<'TEnv, 'T, 'E>) : StateAR<'TEnv, 'U, 'E> =
        m
        |> State.map (Async.map (Result.map f))

    let mapError (f: 'E -> 'F) (m: StateAR<'TEnv, 'T, 'E>) : StateAR<'TEnv, 'T, 'F> =
        m
        |> State.map (Async.map (Result.mapError f))

    let bind (f: 'T -> StateAR<'TEnv, 'U, 'E>) (m: StateAR<'TEnv, 'T, 'E>) : StateAR<'TEnv, 'U, 'E> =
        m
        |> StateA.bind (fun x -> 
            match x with
            | Ok x    -> x |>  f
            | Error e -> e |> err )

    let apply f (m: StateAR<'TEnv, 'T, 'E>) : StateAR<'TEnv, 'U, 'E> = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f (x: StateAR<'TEnv, 'T, 'E>) (y: StateAR<'TEnv, 'U, 'E>) : StateAR<'TEnv, 'Z, 'E> =
        StateA.map2 (Result.map2 f) x y

    let zip (x: StateAR<'TEnv, 'T, 'E>) (y: StateAR<'TEnv, 'U, 'E>) = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let ignore (rr: StateAR<_,_,_>) =
        map ignore rr

    let requireTrue e =
        StateA.map (Result.requireTrue e)

    let requireFalse e =
        StateA.map (Result.requireFalse e)

    let requireSome e =
        StateA.map (Result.requireSome e)

    let requireNone e =
        StateA.map (Result.requireNone e)

    let requireNotNull e =
        StateA.map (Result.requireNotNull e)

    let requireEmpty e =
        StateA.map (Result.requireEmpty e)

    let requireNotEmpty e =
        StateA.map (Result.requireNotEmpty e)

    let withError e =
        StateA.map (Result.withError e)

    let teeIf p f =
        StateA.map (Result.teeIf p f >> Ok)

    let teeErrorIf p f =
        StateA.map (Result.teeErrorIf p f >> Ok)

    let tee f =
        StateA.map (Result.tee f >> Ok)

    let teeError f =
        StateA.map (Result.teeError f >> Ok)

    let concat x y =
        map2 (@) x y

    let get<'S, 'TErr> : StateAR<'S, 'S, 'TErr> = State.get |> State.map AR.retn
    let put s : StateAR<'S, unit, 'TErr> = s |> State.put |> State.map AR.retn

    let lift1R (f: 'S -> 'a -> Result<'b, 'TErr>) : 'a -> StateAR<'S, 'b, 'TErr> =
        fun a -> State <| fun s -> f s a |> Async.retn, s 

    let lift1A (f: 'S -> 'a -> Async<'b>) : 'a -> StateAR<'S, 'b, 'TErr> =
        fun a -> State <| fun s -> f s a |> Async.map Ok, s 

    let lift1 (f: 'S -> 'a -> AR<'b, 'TErr>) : 'a -> StateAR<'S, 'b, 'TErr> =
        fun a -> State <| fun s -> f s a, s

    let lift2R (f:'S -> 'a -> 'b -> Result<'c, 'TErr>) : 'a -> 'b -> StateAR<'S, 'c, 'TErr> =
        fun a b -> (State <| fun s -> f s a b, s) |> State.map Async.retn

    let lift2A (f:'S -> 'a -> 'b -> Async<'c>) : 'a -> 'b -> StateAR<'S, 'c, 'TErr> =
        fun a b -> State <| fun s -> f s a b |> Async.map Ok, s

    let lift2 (f:'S -> 'a -> 'b -> AR<'c, 'TErr>) : 'a -> 'b -> StateAR<'S, 'c, 'TErr> =
        fun a b -> State <| fun s -> f s a b, s

    let unfold iter =
        let gen stt = 
            iter
            |> run stt
            |> function
            | (Ok a, s)     -> Some (a, s)
            | (Error e, s)  -> None

        Seq.unfold gen

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =
        open System

        type StateARBuilder () =
            member _.Return(x) : StateAR<_,_,_>    = retn x
            member _.ReturnFrom(x: StateAR<_,_,_>) = x

            member _.Yield(x) : StateAR<_,_,_>     = retn x
            member _.YieldFrom(x: StateAR<_,_,_>)  = x

            member _.Zero ()                       = retn ()

            member _.Bind(m: StateAR<'TEnv, 'T, 'E>, f: 'T -> StateAR<'TEnv, 'U, 'E>) : StateAR<'TEnv, 'U, 'E> =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : StateAR<_, _,_> = 
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

            member _.Source(r: StateAR<_,_,_>) = 
                r

            member _.Source(r: Result<_,_>) : StateAR<_,_,_> = 
                r
                |> StateA.retn

            member _.Source(c: Choice<_,_>) : StateAR<_,_,_> = 
                c 
                |> Result.ofChoice 
                |> StateA.retn

        let stateAR = StateARBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module StateAR = 

            open Simplee.StateA.ComputationExpression
            open Simplee.Result.Traversals

            // ReaderR<'TEnv, 'a list, 'b list> -> ('c -> ReaderR<'TEnv, 'a, 'b>) -> 'c list -> ReaderR<'TEnv, 'a list, 'b list)
            let private _traverseA (zro: StateAR<'S, 'a list, 'b list>) (f:'c -> StateAR<'S, 'a, 'b>) (xs:'c list) : StateAR<'S, 'a list, 'b list> =

                let rec loop (acc: StateAR<'S, 'a list, 'b list>) (xs: 'c list) =
                    match xs with
                    | [] -> acc
                    | h::tail ->
                        let r = 
                            stateA {
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

            let private _traversetM (zro: StateAR<'TEnv,'U list, 'E>) (f: 'T -> StateAR<'TEnv,'U, 'E>) (xs: 'T list) =

                let rec loop (acc: StateAR<'TEnv, 'U list, 'E>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = stateAR {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs
            
            let traverseM f xs =
                _traversetM (retn []) f xs

            let sequenceM xs =
                traverseM id xs
