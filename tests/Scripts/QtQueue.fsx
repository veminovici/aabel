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
open Simplee.Collections.Queue.Operators

let puree   a = 
    printfn "Puree implementation: %A" a
    State.retn a
let enq    xs = 
    printfn "Enq implementation: %A" xs
    StateR.retn ()
let deq     n = 
    printfn "Deq implementation: %d" n
    StateR.retn [1..n]
let peek    n = 
    printfn "Peek implementation: %d" n
    StateR.retn [1..n]
let isFull () = 
    printfn "IsFull implementation"
    StateR.retn false

let fold f = Queue.fold puree enq deq peek isFull f
let run s0 f = Queue.run puree enq deq peek isFull s0 f
let eval s0 f = Queue.eval puree enq deq peek isFull s0 f

"x"
|> Queue.Pure
|> eval "evn"
|> printfn "puree:%A"

[1; 2]
|> Queue.enqueue
|> eval "env"
|> printfn "enq=%A"

3
|> Queue.dequeue
|> eval "env"
|> printfn "deq=%A"

3
|> Queue.peek
|> eval "env"
|> printfn "peek=%A"

Queue.isFull
|> eval "env"
|> printfn "isFull=%A"