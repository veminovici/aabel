namespace Simplee.Collections

module Queue =

    open Simplee

    type Instruction<'T, 'a, 'TErr> =
        | Enqueue of 'T list * (Result<unit,    'TErr> -> 'a)
        | Dequeue of int     * (Result<'T list, 'TErr> -> 'a)
        | Peek    of int     * (Result<'T list, 'TErr> -> 'a)
        | IsFull  of           (Result<bool,    'TErr> -> 'a)

    let private mapI f = function
        | Enqueue (xs, k) -> Enqueue (xs, k >> f)
        | Dequeue (n,  k) -> Dequeue (n,  k >> f)
        | Peek    (n,  k) -> Peek    (n,  k >> f)
        | IsFull       k  -> IsFull  (    k >> f)

    type Program<'T, 'a, 'TErr> =
        | Pure of 'a
        | Free of Instruction<'T, Program<'T, 'a, 'TErr>, 'TErr>

    let rec bind f m =
        match m with
        | Pure a -> f a
        | Free i -> i |> mapI (bind f) |> Free

    let retn a = Pure a

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

    let enqueue xs = Free (Enqueue (xs, Pure))
    let dequeue n  = Free (Dequeue (n,  Pure))
    let peek    n  = Free (Peek    (n,  Pure))
    let isFull     = Free (IsFull       Pure)

    let fold
        (puree:  'a      -> State<'S, 'b>)
        (enq:    'T list -> StateR<'S, unit, 'TErr>)
        (deq:    int     -> StateR<'S, 'T list, 'TErr>)
        (peek:   int     -> StateR<'S, 'T list, 'TErr>)
        (isFull: unit    -> StateR<'S, bool, 'TErr>)
        (flow: Program<'T, 'a, 'TErr>) : State<'S, 'b> =

        let rec loop flow =
            match flow with
            | Pure a -> puree a
            | Free (Enqueue (xs, k)) -> enq xs    |> State.map k |> State.bind loop
            | Free (Dequeue (n,  k)) -> deq  n    |> State.map k |> State.bind loop
            | Free (Peek    (n,  k)) -> peek n    |> State.map k |> State.bind loop
            | Free (IsFull       k)  -> isFull () |> State.map k |> State.bind loop

        loop flow

    let run puree enq deq peek isFull s0 flow =
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

    module CompurationExpression =

        type QueueBuilder () =
            member _.Return(x)     = retn x
            member _.ReturnFrom(m) = m

            member _.Yield(x)     = retn x
            member _.YieldFrom(m) = m

            member _.Zero() = retn ()

            member _.Bind(m, f) = bind f m

        let queue = QueueBuilder()