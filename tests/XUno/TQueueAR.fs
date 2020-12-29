namespace Simplee.Tests

module TQueueAR =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.QueueAR.Operators
    open Simplee.Collections.QueueAR.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a = 
            StateAR.retn a
        let enq    xs = 
            StateAR.retn ()
        let deq     n = 
            StateAR.retn [1..n]
        let peek    n = 
            StateAR.retn [1..n]
        let isFull () = 
            StateAR.retn false

        let eval s0 p = 
            p
            |> QueueAR.eval puree enq deq peek isFull s0
            |> Async.RunSynchronously

        [<Fact>]
        let ``QueueAR retn`` () =
            10
            |> QueueAR.retn
            |> eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR map`` () =
            "abcde"
            |> QueueAR.retn
            |> QueueAR.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueAR bind`` () =
            "abcde"
            |> QueueAR.retn
            |> QueueAR.bind (fun (s: string) -> s.Length |> QueueAR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueAR apply`` () =
            let f (s: string) = s.Length
            let f' = QueueAR.retn f

            "abcde"
            |> QueueAR.retn
            |> QueueAR.apply f'
            |> eval "env"
            |> (=) (Ok 5) 
            |> Assert.True

        [<Fact>]
        let ``QueueAR map2`` () =
            let f x y = x + y
            let x = QueueAR.retn 10
            let y = QueueAR.retn 20

            (x, y)
            ||> QueueAR.map2 f
            |> eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``QueueAR zip`` () =
            let x = QueueAR.retn 10
            let y = QueueAR.retn 20

            (x, y)
            ||> QueueAR.zip
            |> eval "env"
            |> (=) (Ok (10, 20))
            |> Assert.True

        [<Fact>]
        let ``QueueAR map2`` () =
            let f x y z = x + y + z
            let x = QueueAR.retn 10
            let y = QueueAR.retn 20
            let z = QueueAR.retn 30

            (x, y, z)
            |||> QueueAR.map3 f
            |> eval "env"
            |> (=) (Ok 60)
            |> Assert.True

        [<Fact>]
        let ``QueueAR concat`` () =
            let x = QueueAR.retn [1;2]
            let y = QueueAR.retn [3;4]

            (x, y)
            ||> QueueAR.concat
            |> eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``QueueAR <!>`` () =
            "abcde"
            |> QueueAR.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueAR <*>`` () =
            let f (s: string) = s.Length
            let f' = QueueAR.retn f

            f'
            <*> (QueueAR.retn "abcde")
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueAR >>=`` () =
            "abcde"
            |> QueueAR.retn
            >>= (fun (s: string) -> s.Length |> QueueAR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueAR puree`` () =

            10
            |> QueueAR.retn
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR enq`` () =

            [10; 20]
            |> QueueAR.enqueue
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Queue deq`` () =

            3
            |> QueueAR.dequeue
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok [1..3])
            |> Assert.True

        [<Fact>]
        let ``Queue peek`` () =

            4
            |> QueueAR.peek
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``QueueAR isFull`` () =

            QueueAR.isFull
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``QueueAR CE Return`` () =
            _queueAR {
                return 10
            } 
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR CE ReturnFrom`` () =
            _queueAR {
                return! (QueueAR.retn 10)
            } 
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR CE Yield`` () =
            _queueAR {
                yield 10
            } 
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR CE YieldFrom`` () =
            _queueAR {
                yield! (QueueAR.retn 10)
            } 
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueAR CE Bind`` () =
            _queueAR {
                let! r  = QueueAR.enqueue [1;2;3]
                let! xs = QueueAR.dequeue 2
                return xs
            } 
            |> QueueAR.eval puree enq deq peek isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok [1; 2])
            |> Assert.True