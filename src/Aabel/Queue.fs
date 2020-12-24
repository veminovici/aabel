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

(*
    let flip (a, b) = b, a

    let fold enqImpl deqImpl peekImpl isFullImpl flow zro =

        let rec loop flow stt =
            match flow with
            | Pure a -> stt, a
            | Free (Enqueue (xs, k)) -> stt |> enqImpl xs |> fun (stt, r) -> (stt, k r) |> flip ||> loop
            | Free (Dequeue (n,  k)) -> stt |> deqImpl  n |> fun (stt, r) -> (stt, k r) |> flip ||> loop
            | Free (Peek    (n,  k)) -> stt |> peekImpl n |> fun (stt, r) -> (stt, k r) |> flip ||> loop
            | Free (IsFull       k ) -> stt |> isFullImpl |> fun (stt, r) -> (stt, k r) |> flip ||> loop

        loop flow zro
*)

    type PureeR<'S, 'a, 'b, 'TErr> = 'a      -> StateR<'S, 'b, 'TErr>
    type EnqR<'S, 'T, 'TErr>       = 'T list -> StateR<'S, unit,    'TErr>
    type DeqR<'S, 'T, 'TErr>       = int     -> StateR<'S, 'T list, 'TErr>
    type PeekR<'S, 'T, 'TErr>      = int     -> StateR<'S, 'T list, 'TErr>
    type IsFullR<'S, 'TErr>        = unit    -> StateR<'S, bool,    'TErr>

    let fold 
        (puree: PureeR<'S, 'a, 'b, 'TErr>)
        (enq:   EnqR<'S, 'T, 'TErr>)
        (deq:   DeqR<'S, 'T, 'TErr>)
        (peek:  PeekR<'S, 'T, 'TErr>)
        (isFull: IsFullR<'S, 'TErr>)
        (flow: Program<'T, 'a, 'TErr>) : StateR<'S, 'b, 'TErr> =

        let rec loop (flow: Program<'T, 'a, 'TErr>) : StateR<'S, 'b, 'TErr> =
            match flow with
            | Pure a                 -> a |> puree
            | Free (Enqueue (xs, k)) -> xs |> enq    |> State.map k |> State.bind loop
            | Free (Dequeue (n,  k)) ->  n |> deq    |> State.map k |> State.bind loop
            | Free (Peek    (n,  k)) ->  n |> peek   |> State.map k |> State.bind loop
            | Free (IsFull       k)  -> () |> isFull |> State.map k |> State.bind loop

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