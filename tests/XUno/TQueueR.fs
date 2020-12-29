namespace Simplee.Tests

module TQueueR =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.QueueR.Operators
    open Simplee.Collections.QueueR.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a = 
            StateR.retn a
        let enq    xs = 
            StateR.retn ()
        let deq     n = 
            StateR.retn [1..n]
        let peek    n = 
            StateR.retn [1..n]
        let isFull () = 
            StateR.retn false

        let eval s0 p = QueueR.eval puree enq deq peek isFull s0 p

        [<Fact>]
        let ``QueueR retn`` () =
            10
            |> QueueR.retn
            |> eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR map`` () =
            "abcde"
            |> QueueR.retn
            |> QueueR.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueR bind`` () =
            "abcde"
            |> QueueR.retn
            |> QueueR.bind (fun (s: string) -> s.Length |> QueueR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue apply`` () =
            let f (s: string) = s.Length
            let f' = QueueR.retn f

            "abcde"
            |> QueueR.retn
            |> QueueR.apply f'
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueR map2`` () =
            let f x y = x + y
            let x = QueueR.retn 10
            let y = QueueR.retn 20

            (x, y)
            ||> QueueR.map2 f
            |> eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``QueueR zip`` () =
            let x = QueueR.retn 10
            let y = QueueR.retn 20

            (x, y)
            ||> QueueR.zip
            |> eval "env"
            |> (=) (Ok (10, 20))
            |> Assert.True

        [<Fact>]
        let ``QueueR map2`` () =
            let f x y z = x + y + z
            let x = QueueR.retn 10
            let y = QueueR.retn 20
            let z = QueueR.retn 30

            (x, y, z)
            |||> QueueR.map3 f
            |> eval "env"
            |> (=) (Ok 60)
            |> Assert.True

        [<Fact>]
        let ``QueueR concat`` () =
            let x = QueueR.retn [1;2]
            let y = QueueR.retn [3;4]

            (x, y)
            ||> QueueR.concat
            |> eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``QueueR <!>`` () =
            "abcde"
            |> QueueR.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueR <*>`` () =
            let f (s: string) = s.Length
            let f' = QueueR.retn f

            f'
            <*> (QueueR.retn "abcde")
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueR >>=`` () =
            "abcde"
            |> QueueR.retn
            >>= (fun (s: string) -> s.Length |> QueueR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``QueueR puree`` () =

            10
            |> QueueR.retn
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR enq`` () =

            [10; 20]
            |> QueueR.enqueue
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``QueueR deq`` () =

            3
            |> QueueR.dequeue
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1..3])
            |> Assert.True

        [<Fact>]
        let ``QueueR peek`` () =

            4
            |> QueueR.peek
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``QueueR isFull`` () =

            QueueR.isFull
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``QueueR CE Return`` () =
            _queueR {
                return 10
            } 
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR CE ReturnFrom`` () =
            _queueR {
                return! (QueueR.retn 10)
            } 
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR CE Yield`` () =
            _queueR {
                yield 10
            } 
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR CE YieldFrom`` () =
            _queueR {
                yield! (QueueR.retn 10)
            } 
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``QueueR CE Bind`` () =
            _queueR {
                let! r  = QueueR.enqueue [1;2;3]
                let! xs = QueueR.dequeue 2
                return xs
            } 
            |> QueueR.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1; 2])
            |> Assert.True