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
#load "../../src/Aabel/QueueR.fs"

open Simplee
open Simplee.Collections
open Simplee.Collections.QueueR.ComputationExpression

let puree a = 
    printfn "puree=%A" a
    StateR.retn a

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
    let fold f = QueueR.fold puree enq deq peek isFull f
    let eval s0 f = QueueR.eval puree enq deq peek isFull s0 f

    [1; 2]
    |> Ok
    |> QueueR.Pure
    |> eval []
    |> printfn "puree:%A"

    [1; 2]
    |> QueueR.enqueue
    |> eval []
    |> printfn "enq=%A"

    10
    |> QueueR.dequeue
    |> eval [1..5]
    |> printfn "deq=%A"

    3
    |> QueueR.peek
    |> eval [1..5]
    |> printfn "peek=%A"

    QueueR.isFull
    |> eval [1..100]
    |> printfn "isFull=%A"

let test1() =
    _queueR {
        do! QueueR.enqueue [1;2;]
        let! _ = QueueR.dequeue 3
        do! QueueR.enqueue [3;4;5]
        let! xs = QueueR.dequeue 2
        return xs
    } 
    |> QueueR.eval puree enq deq peek isFull []
    |> printfn "Result: %A"

let test2() =
    _queueR {
        do! QueueR.enqueue [1..5]
        let! xs = QueueR.dequeue 2
        let! ys = QueueR.dequeue 1
        return (xs, ys)
    } 
    |> QueueR.eval puree enq deq peek isFull []
    |> printfn "Result: %A"

test1 ()
test2 ()
