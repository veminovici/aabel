namespace Simplee.Tests

module TQueue =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.Queue.Operators

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a = a |> StateR.retn
        let enq    xs = () |> StateR.retn
        let deq     n = [1..n] |> StateR.retn
        let peek    n = [1..n] |> StateR.retn
        let isFull () = false |> StateR.retn

        let eval s0 p = Queue.eval puree enq deq peek isFull s0 p

        [<Fact>]
        let ``Queue retn`` () =
            10
            |> Queue.retn
            |> eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Queue map`` () =
            "abcde"
            |> Queue.retn
            |> Queue.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue bind`` () =
            "abcde"
            |> Queue.retn
            |> Queue.bind (fun (s: string) -> s.Length |> Queue.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue apply`` () =
            let f (s: string) = s.Length
            let f' = Queue.retn f

            "abcde"
            |> Queue.retn
            |> Queue.apply f'
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue map2`` () =
            let f x y = x + y
            let x = Queue.retn 10
            let y = Queue.retn 20

            (x, y)
            ||> Queue.map2 f
            |> eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``Queue zip`` () =
            let x = Queue.retn 10
            let y = Queue.retn 20

            (x, y)
            ||> Queue.zip
            |> eval "env"
            |> (=) (Ok (10, 20))
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
            |> (=) (Ok 60)
            |> Assert.True

        [<Fact>]
        let ``Queue concat`` () =
            let x = Queue.retn [1;2]
            let y = Queue.retn [3;4]

            (x, y)
            ||> Queue.concat
            |> eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``Queue CE <!>`` () =
            "abcde"
            |> Queue.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue CE <*>`` () =
            let f (s: string) = s.Length
            let f' = Queue.retn f

            f'
            <*> (Queue.retn "abcde")
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue CE >>=`` () =
            "abcde"
            |> Queue.retn
            >>= (fun (s: string) -> s.Length |> Queue.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``Queue puree`` () =

            10
            |> Queue.retn
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Queue enq`` () =

            [10; 20]
            |> Queue.enqueue
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok (Ok ()))
            |> Assert.True

        [<Fact>]
        let ``Queue deq`` () =

            3
            |> Queue.dequeue
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok (Ok [1..3]))
            |> Assert.True

        [<Fact>]
        let ``Queue peek`` () =

            4
            |> Queue.peek
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok (Ok [1..4]))
            |> Assert.True

        [<Fact>]
        let ``Queue isFull`` () =

            Queue.isFull
            |> Queue.eval puree enq deq peek isFull "env"
            |> (=) (Ok (Ok false))
            |> Assert.True
