#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"
#load "../../src/Aabel/Reader.fs"
#load "../../src/Aabel/ReaderR.fs"
#load "../../src/Aabel/ReaderA.fs"
#load "../../src/Aabel/ReaderAR.fs"
#load "../../src/Aabel/State.fs"
#load "../../src/Aabel/StateR.fs"
#load "../../src/Aabel/StateA.fs"
#load "../../src/Aabel/StateAR.fs"
#load "../../src/Aabel/Queue.fs"

open Simplee
open Simplee.Collections
open Simplee.Collections.Queue.ComputationExpression

let puree a = 
    printfn "puree=%A" a
    State.retn a

let enq    ys = State <| fun xs ->
    printfn "enqueue=%A" xs
    Ok (), xs @ ys

let deq n = State <| fun xs ->
    printfn "dequeue=%A" xs
    let l = List.length xs
    if l < n 
    then Error "Not enough items", xs 
    else Ok (List.take n xs), xs |> List.skip n

let peek n = State <| fun xs ->
    printfn "peek=%A" xs
    let l = List.length xs
    if l < n 
    then Error "Not enough items", xs 
    else Ok (List.take n xs), xs

let isFull () = State <| fun xs ->
    printfn "isFull=%A" xs
    let l = List.length xs
    Ok (l > 10), xs

let test () =
    let fold f = Queue.fold puree enq deq peek isFull f
    let eval s0 f = Queue.eval puree enq deq peek isFull s0 f

    [1; 2]
    |> Ok
    |> Queue.Pure
    |> eval []
    |> printfn "puree:%A"

    [1; 2]
    |> Queue.enqueue
    |> eval []
    |> printfn "enq=%A"

    10
    |> Queue.dequeue
    |> eval [1..5]
    |> printfn "deq=%A"


    3
    |> Queue.peek
    |> eval [1..5]
    |> printfn "peek=%A"

    Queue.isFull
    |> eval [1..100]
    |> printfn "isFull=%A"

let test1() =
    _queue {
        let! _  = Queue.enqueue [1;2;]
        let! xs = Queue.dequeue 3
        let! _  = Queue.enqueue [3;4;5]
        let! xs = Queue.dequeue 2
        return xs
    } 
    |> Queue.eval puree enq deq peek isFull []
    |> printfn "Result: %A"

let test2() =
    _queue {
        let! _ = Queue.enqueue [1..5]
        let! xs = Queue.dequeue 2
        let! ys = Queue.dequeue 1
        return (xs, ys)
    } 
    |> Queue.eval puree enq deq peek isFull []
    |> printfn "Result: %A"

test1 ()
test2 ()
