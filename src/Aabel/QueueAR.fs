namespace Simplee.Collections

module QueueAR =

    open Simplee

    type Instruction<'T, 'a, 'TErr> =
        | Enqueue of 'T list * (AR<unit,    'TErr> -> 'a)
        | Dequeue of int     * (AR<'T list, 'TErr> -> 'a)
        | Peek    of int     * (AR<'T list, 'TErr> -> 'a)
        | IsFull  of           (AR<bool,    'TErr> -> 'a)

    let private mapI f = function
        | Enqueue (xs, k) -> Enqueue (xs, k >> f)
        | Dequeue (n,  k) -> Dequeue (n,  k >> f)
        | Peek    (n,  k) -> Peek    (n,  k >> f)
        | IsFull       k  -> IsFull  (    k >> f)

    type Program<'T, 'a, 'TErr> =
        | Pure of 'a
        | Free of Instruction<'T, Program<'T, 'a, 'TErr>, 'TErr>

    let enqueue xs : Program<'a, AR<unit, 'TErr>, 'TErr>    = Free (Enqueue (xs, Pure))
    let dequeue n  : Program<'a, AR<'a list, 'TErr>, 'TErr> = Free (Dequeue (n,  Pure))
    let peek    n  : Program<'a, AR<'a list, 'TErr>, 'TErr> = Free (Peek    (n,  Pure))
    let isFull     : Program<'a, AR<bool, 'TErr>, 'TErr>    = Free (IsFull       Pure)

    let retn a = a |> AR.retn |> Pure
    let err  e = e |> AR.err  |> Pure

    let rec bind 
        (f: 'T -> Program<'a, AR<'U, 'TErr>, 'TErr>) 
        (m: Program<'a, AR<'T, 'TErr>, 'TErr>)
        : Program<'a, AR<'U, 'TErr>, 'TErr> =
        match m with
        | Pure a -> 
            a 
            |> Async.RunSynchronously 
            |> function
            | Ok a -> f a
            | Error e -> e |> Error |> Async.retn |> Pure
        | Free i -> i |> mapI (bind f) |> Free

    let map f m = bind (f >> retn) m

    let apply f m =
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f x y =
        apply (apply (retn f) x) y

    let zip x y = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let concat x y =
        map2 (@) x y

    let fold
        (puree:  'a      -> StateAR<'S, 'b, 'TErr>)
        (enq:    'T list -> StateAR<'S, unit, 'TErr>)
        (deq:    int     -> StateAR<'S, 'T list, 'TErr>)
        (peek:   int     -> StateAR<'S, 'T list, 'TErr>)
        (isFull: unit    -> StateAR<'S, bool, 'TErr>)
        (flow: Program<'T, AR<'a, 'TErr>, 'TErr>) : StateAR<'S, 'b, 'TErr> =

        let rec loop (flow: Program<'T, AR<'a, 'TErr>, 'TErr>) : StateAR<'S, 'b, 'TErr> =
            match flow with
            | Pure ar -> 
                ar 
                |> AR.fold puree (fun e -> e |> StateAR.err) 
                |> Async.RunSynchronously
            | Free (Enqueue (xs, k)) -> enq xs    |> State.map k |> State.bind loop
            | Free (Dequeue (n,  k)) -> deq  n    |> State.map k |> State.bind loop
            | Free (Peek    (n,  k)) -> peek n    |> State.map k |> State.bind loop
            | Free (IsFull       k)  -> isFull () |> State.map k |> State.bind loop

        loop flow

    let run  puree enq deq peek isFull s0 flow =
        flow
        |> fold puree enq deq peek isFull 
        |> State.run s0

    let eval puree enq deq peek isFull s0 flow =
        flow 
        |> run puree enq deq peek isFull s0
        |> fst

    let exec puree enq deq peek isFull s0 flow =
        flow
        |> run puree enq deq peek isFull s0
        |> snd

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =
        open System

        /// The caller has to handle the erorrs.
        type QueueARBuilder () =
            member _.Return(x)     = retn x
            member _.ReturnFrom(m) = m

            member _.Yield(x)     = retn x
            member _.YieldFrom(m) = m

            member _.Zero() = 
                retn ()

            member _.Bind(m, f) =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith(g, h)    = try this.Run g with e -> h e
            member this.TryFinally(g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) = 
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

            member _.Source(r: Program<_,_,_>) = 
                r

        let _queueAR = QueueARBuilder()
