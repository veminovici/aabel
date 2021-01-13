namespace Simplee

type StateR<'S, 'T, 'E> = State<'S, Result<'T, 'E>>

[<RequireQualifiedAccess>]
module StateR =

    let run env (s: StateR<_,_,_>) =  
        s
        |> State.run env
    let eval s stt = run s stt |> fst
    let exec s stt = run s stt |> snd


    let retn     x : StateR<_,_,_> = x |> Result.ok  |> State.retn
    let singleton                   = retn
    let err      e : StateR<_,_,_> = e |> Result.err |> State.retn

    let map f (m: StateR<'S, 'T, 'E>) : StateR<'S, 'U, 'E> =
        m 
        |> State.map (Result.map f)

    let mapError f (m: StateR<'S, 'T, 'E>) : StateR<'S, 'T, 'F> = 
        m
        |> State.map (Result.mapError f)

    let mapEither f g (m: StateR<'S, 'T, 'E>) : StateR<'S, 'U, 'F> =
        m
        |> State.map (Result.mapEither f g)

    let bind (f: 'T -> StateR<'S, 'U, 'E>) (m: StateR<'S, 'T, 'E>) : StateR<'S, 'U, 'E> =
        m 
        |> State.bind (Result.fold f (Error >> State.retn))

    let apply f (m: StateR<'S, 'T, 'E>) : StateR<'S, 'U, 'E> = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f (x: StateR<'S, 'T, 'E>) (y: StateR<'S, 'U, 'E>) : StateR<'S, 'Z, 'E> =
        State.map2 (Result.map2 f) x y

    let zip (x: StateR<'S, 'T, 'E>) (y: StateR<'S, 'U, 'E>) = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let ignore (rr: StateR<_,_,_>) =
        map ignore rr

    let requireTrue e =
        State.map (Result.requireTrue e)

    let requireFalse e =
        State.map (Result.requireFalse e)

    let requireSome e =
        State.map (Result.requireSome e)

    let requireNone e =
        State.map (Result.requireNone e)

    let requireNotNull e =
        State.map (Result.requireNotNull e)

    let requireEmpty e =
        State.map (Result.requireEmpty e)

    let requireNotEmpty e =
        State.map (Result.requireNotEmpty e)

    let withError e =
        State.map (Result.withError e)

    let teeIf p f =
        State.map (Result.teeIf p f >> Ok)

    let teeErrorIf p f =
        State.map (Result.teeErrorIf p f >> Ok)

    let tee f =
        State.map (Result.tee f >> Ok)

    let teeError f =
        State.map (Result.teeError f >> Ok)

    let concat x y =
        map2 (@) x y

    let get<'S, 'TErr> : StateR<'S, 'S, 'TErr> = State.get |> State.map Ok
    let put s : StateR<'S, unit, 'TErr> = s |> State.put |> State.map Ok

    let lift1 (f: 'S -> 'a -> Result<'b, 'TErr>) : 'a -> StateR<'S, 'b, 'TErr> =
        fun a -> State <| fun s -> f s a, s

    let lift2 (f:'S -> 'a -> 'b -> Result<'c, 'TErr>) : 'a -> 'b -> StateR<'S, 'c, 'TErr> =
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

        type StateRBuilder () =
            member _.Return(x) : StateR<_,_,_>    = retn x
            member _.ReturnFrom(x: StateR<_,_,_>) = x

            member _.Yield(x) : StateR<_,_,_>     = retn x
            member _.YieldFrom(x: StateR<_,_,_>)  = x

            member _.Zero ()                       = retn ()

            member _.Bind(m: StateR<'TEnv, 'T, 'E>, f: 'T -> StateR<'TEnv, 'U, 'E>) : StateR<'TEnv, 'U, 'E> =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : StateR<_, _,_> = 
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

            member _.Source(r: StateR<_,_,_>) = 
                r

            member _.Source(r: Result<_,_>) : StateR<_,_,_> = 
                r
                |> State.retn

            member _.Source(c: Choice<_,_>) : StateR<_,_,_> = 
                c 
                |> Result.ofChoice 
                |> State.retn

        let _stateR = StateRBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module StateR = 

            open Simplee.State.ComputationExpression
            open Simplee.Result.Traversals

            // StateR<'S, 'a list, 'b list> -> ('c -> StateR<'S, 'a, 'b>) -> 'c list -> StateR<'S, 'a list, 'b list)
            let private _traverseA (zro: StateR<'S, 'a list, 'b list>) (f:'c -> StateR<'S, 'a, 'b>) (xs:'c list) : StateR<'S, 'a list, 'b list> =

                let rec loop (acc: StateR<'S, 'a list, 'b list>) (xs: 'c list) =
                    match xs with
                    | [] -> acc
                    | h::tail ->
                        let r = 
                            _state {
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

            let private _traversetM (zro: StateR<'S, 'U list, 'E>) (f: 'T -> StateR<'S , 'U, 'E>) (xs: 'T list) : StateR<'S, 'U list, 'E> =

                let rec loop (acc: StateR<'S, 'U list, 'E>) (xs: 'T list) =
                    match xs with
                    | [] -> acc
                    | h :: tail -> 
                        _stateR {
                            let! ys = acc
                            let! y  = f h
                            return ys @ [y] }
                        |> bind (fun acc -> loop (retn acc) tail)

                loop zro xs
            
            let traverseM f xs = 
                _traversetM (singleton []) f xs

            let sequenceM xs =
                traverseM id xs