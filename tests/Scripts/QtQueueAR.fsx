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
#load "../../src/Aabel/QueueAR.fs"

open Simplee
open Simplee.Collections
open Simplee.Collections.QueueAR.ComputationExpression

let puree a = 
    printfn "puree=%A" a
    StateAR.retn a

let enq    ys = State <| fun xs ->
    printfn "enqueue=%A" xs
    AR.retn (), xs @ ys

let deq n = State <| fun xs ->
    printfn "dequeue=%A" xs
    let l = List.length xs
    if l < n 
    then AR.err "Not enough items", xs 
    else AR.retn (List.take n xs), xs |> List.skip n

let peek n = State <| fun xs ->
    printfn "peek=%A" xs
    let l = List.length xs
    if l < n 
    then AR.err "Not enough items", xs 
    else AR.retn (List.take n xs), xs

let isFull () = State <| fun xs ->
    printfn "isFull=%A" xs
    let l = List.length xs
    AR.retn (l > 10), xs

let test () =
    let fold f = QueueAR.fold puree enq deq peek isFull f
    let eval s0 f = QueueAR.eval puree enq deq peek isFull s0 f

    [1; 2]
    |> AR.retn
    |> QueueAR.Pure
    |> eval []
    |> Async.RunSynchronously
    |> printfn "puree:%A"

    [1; 2]
    |> QueueAR.enqueue
    |> eval []
    |> Async.RunSynchronously
    |> printfn "enq=%A"

    10
    |> QueueAR.dequeue
    |> eval [1..5]
    |> Async.RunSynchronously
    |> printfn "deq=%A"

    3
    |> QueueAR.peek
    |> eval [1..5]
    |> Async.RunSynchronously
    |> printfn "peek=%A"

    QueueAR.isFull
    |> eval [1..100]
    |> Async.RunSynchronously
    |> printfn "isFull=%A"

let test1() =
    _queueAR {
        do! QueueAR.enqueue [1;2;]
        let! _ = QueueAR.dequeue 3
        do! QueueAR.enqueue [3;4;5]
        let! xs = QueueAR.dequeue 2
        return xs
    } 
    |> QueueAR.eval puree enq deq peek isFull []
    |> Async.RunSynchronously
    |> printfn "Result: %A"

let test2() =
    _queueAR {
        do! QueueAR.enqueue [1..5]
        let! xs = QueueAR.dequeue 2
        let! ys = QueueAR.dequeue 1
        return (xs, ys)
    } 
    |> QueueAR.eval puree enq deq peek isFull []
    |> Async.RunSynchronously
    |> printfn "Result: %A"

test1 ()
test2 ()
