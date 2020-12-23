namespace Simplee

type StateA<'S, 'T> = State<'S, Async<'T>>

[<RequireQualifiedAccess>]
module StateA =

    open System.Threading.Tasks

    let run env (r: StateA<_,_>) =  
        r 
        |> State.run env 
        |> fun (a, s) -> Async.RunSynchronously a, s
    let eval s stt = run s stt |> fst
    let exec s stt = run s stt |> snd

    let retn     x = x |> Async.retn |> State.retn
    let singleton  = retn

    let map f = 
        State.map (Async.map f)

    let bind (f: 'a -> StateA<'TEnv, 'b>) (m: StateA<'TEnv, 'a>) : StateA<'TEnv, 'b> = 
        m
        |> State.bind (Async.RunSynchronously >> f)

    let apply f m = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f x y =
        State.map2 (Async.map2 f) x y

    let zip x y = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let sequenceAsync (ar: StateA<_, _>) =
        ar
        |> State.sequenceAsync

    let concat x y =
        map2 (@) x y

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =

        open System

        type StateABuilder () =
            member _.Return(x) : StateA<_,_>    = retn x
            member _.ReturnFrom(x: StateA<_,_>) = x

            member _.Yield(x) : StateA<_,_>     = retn x
            member _.YieldFrom(x: StateA<_,_>)  = x

            member _.Zero () : StateA<_, unit>  = retn ()

            member _.Bind(m: StateA<'TEnv, 'T>, f: 'T -> StateA<'TEnv, 'U>) : StateA<'TEnv, 'U> =
                bind f m

            member _.Run (f: unit -> StateA<_,_>) = f ()

            member this.Combine(a1, a2) = 
                this.Bind(a1, fun _ -> a2())

            member _.Delay(g) = 
                g

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : StateA<_, _> = 
                this.TryFinally (
                    (fun () -> f r),
                    (fun () -> if not <| obj.ReferenceEquals(r, null) then r.Dispose ()))
                    
            member this.While(g, f) =
                if not <| g() then this.Zero ()
                else this.Bind(this.Run f, fun () -> this.While (g, f))

            member this.For(s: #seq<'T>, f) =
                this.Using(s.GetEnumerator(), fun enum ->
                    this.While(enum.MoveNext,
                        this.Delay(fun () -> f enum.Current)))

            member _.BindReturn(x, f) = 
                map f x

            member _.MergeSources(r1, r2) = 
                zip r1 r2

            member _.Source(r: StateA<_, _>) = 
                r

            member _.Source(s: #seq<_>) = 
                s

            member _.Source(r: Async<_>) : StateA<_, _> = 
                State.retn r

            member _.Source(t: Task<_>) =
                t 
                |> Async.AwaitTask
                |> State.retn

        let stateA = StateABuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module StateA = 

            let private _traversetM (zro: StateA<'TEnv,'U list>) (f: 'T -> StateA<'TEnv,'U>) (xs: 'T list) =

                let rec loop (acc: StateA<'TEnv, 'U list>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = stateA {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs
            
            let traverseM f xs =
                _traversetM (retn []) f xs

            let sequenceM xs =
                traverseM id xs