namespace Simplee

[<Struct>]
[<NoComparison>]
type State<'S, 'T> = State of ('S -> 'T * 'S)

[<RequireQualifiedAccess>]
module State = 

    let run s (State f) = f s
    let eval s stt = run s stt |> fst
    let exec s stt = run s stt |> snd

    let retn v = State <| fun s -> 
        v, s
    let singleton = retn

    let map f m = State <| fun s -> 
        let (t, s') = run s m
        f t, s'

    let bind f m = State <| fun s ->
        let (x, s') = run s m
        let m = f x
        run s' m

    let apply f m = 
        bind (fun f ->
            bind (f >> retn) m) f

    let map2 f x y = State <| fun s ->
        let (x, s' ) = run s  x
        let (y, s'') = run s' y
        f x y, s''

    let zip x y = 
        map2 (fun x y -> x, y) x y

    let combine x y = 
        bind (fun _ -> y) x

    let kleisi f1 f2 = fun s ->
        let x = f1 s
        bind f2 x

    let lift1 (f: 'S -> 'a -> 'b) : 'a -> State<'S, 'b> =
        fun a -> State <| fun s -> f s a, s

    let lift2 (f:'S -> 'a -> 'b -> 'c) : 'a -> 'b -> State<'S, 'c> =
        fun a b -> State <| fun s -> f s a b, s

    let liftE (f:'S2 -> 'S1) (State fn) : State<'S2, 'T> =
        State <| fun s2 ->
            let s1 = f s2
            let (t, _) = fn s1
            t, s2

    let sequenceAsync (r: State<_, Async<_>>) = async {
        return 
            State <| fun env ->
                let (x, env') = r |> run env
                x |> Async.RunSynchronously, env' }

    let concat x y =
        map2 (@) x y

    let get = State (fun s -> s, s)
    let put s = State (fun _ -> (), s)

    module Operators = 
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =
        open System

        type StateBuilder () =
            member _.Return(x) : State<_,_>    = retn x
            member _.ReturnFrom(x: State<_,_>) = x

            member _.Yield(x) : State<_,_>     = retn x
            member _.YieldFrom(x: State<_,_>)  = x

            member _.Zero ()                    = retn ()

            member _.Bind(m, f) =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith(g, h)    = try this.Run g with e -> h e
            member this.TryFinally(g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : State<_, _> = 
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

            member _.Source(r: State<_,_>) = 
                r

            member _.Source(r: Result<_,_>) = 
                r
                |> singleton

            member _.Source(c: Choice<_,_>) = 
                c 
                |> Result.ofChoice 
                |> singleton

        let state = StateBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module State = 

            let private _traverseM (zro: State<'S,'U list>) (f: 'T -> State<'S,'U>) (xs: 'T list) =

                let rec loop (acc: State<'S, 'U list>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = state {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs

            let traverseM f xs =
                _traverseM (retn []) f xs

            let sequenceM xs =
                traverseM id xs