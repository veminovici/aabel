namespace Simplee.Tests

module TQueue =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.Queue.Operators
    open Simplee.Collections.Queue.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a = 
            State.retn a
        let enq    xs = 
            StateR.retn ()
        let deq     n = 
            StateR.retn [1..n]
        let peek    n = 
            StateR.retn [1..n]
        let isFull () = 
            StateR.retn false

        let eval s0 p = Queue.eval puree enq deq peek isFull s0 p

        [<Fact>]
        let ``Queue retn`` () =
            10
            |> Queue.retn
            |> eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue map`` () =
            "abcde"
            |> Queue.retn
            |> Queue.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue bind`` () =
            "abcde"
            |> Queue.retn
            |> Queue.bind (fun (s: string) -> s.Length |> Queue.retn)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue apply`` () =
            let f (s: string) = s.Length
            let f' = Queue.retn f

            "abcde"
            |> Queue.retn
            |> Queue.apply f'
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue map2`` () =
            let f x y = x + y
            let x = Queue.retn 10
            let y = Queue.retn 20

            (x, y)
            ||> Queue.map2 f
            |> eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``Queue zip`` () =
            let x = Queue.retn 10
            let y = Queue.retn 20

            (x, y)
            ||> Queue.zip
            |> eval "env"
            |> (=) (10, 20)
            |> Assert.True

        [<Fact>]
        let ``Queue map2`` () =
            let f x y z = x + y + z
            let x = Queue.retn 10
            let y = Queue.retn 20
            let z = Queue.retn 30

            (x, y, z)
            |||> Queue.map3 f
            |> eval "env"
            |> (=) 60
            |> Assert.True

        [<Fact>]
        let ``Queue concat`` () =
            let x = Queue.retn [1;2]
            let y = Queue.retn [3;4]

            (x, y)
            ||> Queue.concat
            |> eval "env"
            |> (=) [1..4]
            |> Assert.True

        [<Fact>]
        let ``Queue <!>`` () =
            "abcde"
            |> Queue.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue <*>`` () =
            let f (s: string) = s.Length
            let f' = Queue.retn f

            f'
            <*> (Queue.retn "abcde")
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue >>=`` () =
            "abcde"
            |> Queue.retn
            >>= (fun (s: string) -> s.Length |> Queue.retn)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Queue puree`` () =

            10
            |> Queue.retn
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue enq`` () =

            [10; 20]
            |> Queue.enqueue
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Queue deq`` () =

            3
            |> Queue.dequeue
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1..3])
            |> Assert.True

        [<Fact>]
        let ``Queue peek`` () =

            4
            |> Queue.peek
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``Queue isFull`` () =

            Queue.isFull
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``Queue CE Return`` () =
            _queue {
                return 10
            } 
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue CE ReturnFrom`` () =
            _queue {
                return! (Queue.retn 10)
            } 
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue CE Yield`` () =
            _queue {
                yield 10
            } 
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue CE YieldFrom`` () =
            _queue {
                yield! (Queue.retn 10)
            } 
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Queue CE Bind`` () =
            _queue {
                let! r  = Queue.enqueue [1;2;3]
                let! xs = Queue.dequeue 2
                return xs
            } 
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok [1; 2])
            |> Assert.True