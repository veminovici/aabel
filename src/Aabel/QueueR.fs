namespace Simplee.Collections

module QueueR =

    open Simplee

    //
    // Queue
    //

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

    //
    // Queue operations
    //

    let enqueue xs : Program<'a, Result<unit, 'TErr>, 'TErr>    = Free (Enqueue (xs, Pure))
    let dequeue n  : Program<'a, Result<'a list, 'TErr>, 'TErr> = Free (Dequeue (n,  Pure))
    let peek    n  : Program<'a, Result<'a list, 'TErr>, 'TErr> = Free (Peek    (n,  Pure))
    let isFull     : Program<'a, Result<bool, 'TErr>, 'TErr>    = Free (IsFull       Pure)

    //
    // Monad operations
    //

    // 'T -> Program<'a, Result<'T, 'TErr>, 'TErr>
    let retn a = a |> Ok |> Pure

    // ('T -> Program<'a, Result<'U, 'TErr>, 'TErr>) -> Program<'a, Result<'T, 'TErr>, 'TErr> -> Program<'a, Result<'U, 'TErr>, 'TErr>
    let rec bind f m =
        match m with
        | Pure (Ok a)    -> f a
        | Pure (Error e) -> Pure (Error e)
        | Free i          -> i |> mapI (bind f) |> Free

    // ('T -> 'U) -> Program<'a, Result<'T, 'TErr>, 'TErr> -> Program<'a, Result<'U, 'TErr>, 'TErr>
    let map f m =
        bind (f >> retn) m
     
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
        (puree:  'a      -> StateR<'S, 'b, 'TErr>)
        (enq:    'T list -> StateR<'S, unit, 'TErr>)
        (deq:    int     -> StateR<'S, 'T list, 'TErr>)
        (peek:   int     -> StateR<'S, 'T list, 'TErr>)
        (isFull: unit    -> StateR<'S, bool, 'TErr>)
        (flow: Program<'T, Result<'a, 'TErr>, 'TErr>) : StateR<'S, 'b, 'TErr> =

        let rec loop (flow: Program<'T, Result<'a, 'TErr>, 'TErr>) =
            match flow with
            | Pure (Ok a)    -> puree a
            | Pure (Error e) -> StateR.err e
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
        type QueueRBuilder internal () =
            member _.Return(x) = retn x
            member _.ReturnFrom(x) = x

            member _.Yield(x) = retn x
            member _.YieldFrom(x) = x
            
            member _.Bind(m, f) = bind f m

        let _queueR = QueueRBuilder()

