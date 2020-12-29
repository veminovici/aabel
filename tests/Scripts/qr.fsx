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
let retn (a: 'T) : Program<'a, Result<'T, 'TErr>, 'TErr> = a |> Ok |> Pure

// ('T -> Program<'a, Result<'U, 'TErr>, 'TErr>) -> Program<'a, Result<'T, 'TErr>, 'TErr> -> Program<'a, Result<'U, 'TErr>, 'TErr>
let rec bind 
    (f: 'T -> Program<'a, Result<'U, 'TErr>, 'TErr>) 
    (m: Program<'a, Result<'T, 'TErr>, 'TErr>) 
    : Program<'a, Result<'U, 'TErr>, 'TErr> =

    match m with
    | Pure (Ok a)    -> f a
    | Pure (Error e) -> Pure (Error e)
    | Free i          -> i |> mapI (bind f) |> Free


// ('T -> 'U) -> Program<'a, Result<'T, 'TErr>, 'TErr> -> Program<'a, Result<'U, 'TErr>, 'TErr>
let map 
    (f:'T -> 'U) 
    (m: Program<'a, Result<'T, 'TErr>, 'TErr>) 
    : Program<'a, Result<'U, 'TErr>, 'TErr> = 
    
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

type QBuilder () =
    member _.Return(x) = retn x
    member _.ReturnFrom(x) = x
    member _.Bind(m, f) = bind f m

let q = QBuilder()

let flow () = q  {
    do! enqueue [1;2;3]
    return! dequeue 1 }