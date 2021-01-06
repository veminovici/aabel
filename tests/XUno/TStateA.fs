namespace Simplee.Tests

module TStateA =

    open Simplee
    open Simplee.StateA.ComputationExpression
    open Simplee.StateA.Operators
    open Simplee.StateA.Traversals

    open System.Threading.Tasks

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``StateA exec`` () =

            "abcde"
            |> StateA.retn
            |> StateA.exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``StateA retn`` () =

            "abcde"
            |> StateA.retn
            |> StateA.eval "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``StateA singleton`` () =

            "abcde"
            |> StateA.singleton
            |> StateA.eval "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``StateA map`` () =

            let length (s: string) = s.Length

            "abcde"
            |> StateA.retn
            |> StateA.map length
            |> StateA.eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateA bind`` () =

            let length (s: string) = s.Length |> StateA.retn

            "abcde"
            |> StateA.retn
            |> StateA.bind length
            |> StateA.eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateA apply`` () =

            let fn = (fun (s: string) -> s.Length) |> StateA.retn

            "abcde"
            |> StateA.retn
            |> StateA.apply fn
            |> StateA.eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateA map2`` () =

            let fn x y = x + y
            let x = StateA.retn 10
            let y = StateA.retn 20

            (x, y)
            ||> StateA.map2 fn
            |> StateA.eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``StateA map3`` () =

            let fn x y z = x + y + z
            let x = StateA.retn 1
            let y = StateA.retn 2
            let z = StateA.retn 3

            (x, y, z)
            |||> StateA.map3 fn
            |>   StateA.eval "env"
            |>   (=) 6
            |>   Assert.True

        [<Fact>]
        let ``StateA zip`` () =

            let x = StateA.retn "ab"
            let y = StateA.retn 2

            (x, y)
            ||> StateA.zip
            |>  StateA.eval "env"
            |> (=) ("ab", 2)
            |> Assert.True

        [<Fact>]
        let ``StateA <!>`` () =

            let length (s: string) = s.Length

            "abcde"
            |>  StateA.retn
            <!> length
            |>  StateA.eval "env"
            |>  (=) 5
            |>  Assert.True

        [<Fact>]
        let ``StateA <*>`` () =

            let fn = (fun (s: string) -> s.Length) |> StateA.retn

            fn
            <*> (StateA.retn "abcde")
            |>  StateA.eval "env"
            |>  (=) 5
            |>  Assert.True

        [<Fact>]
        let ``StateA >>=`` () =

            let fn i = i |> (+) 1 |> StateA.retn

            1
            |> StateA.retn
            >>= fn
            >>= fn
            >>= fn
            >>= fn
            |> StateA.eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateA CE return`` () =

            stateA {
                return 10
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE returnFrom`` () =

            stateA {
                return! (StateA.retn 10)
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE yield`` () =

            stateA {
                yield 10
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE yieldFrom`` () =

            stateA {
                yield! (StateA.retn 10)
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE zero`` () =
            stateA { 
                10 |> ignore
            }
            |> StateA.eval "env"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``StateA CE bind`` () =

            stateA {
                let! x = (StateA.retn 10)
                let! y = (StateA.retn 20)
                return x + y
            }
            |> StateA.eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ResultA CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            stateA {
                while test() do
                    inc()
            }
            |> StateA.eval "env"
            |> (=) ()
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateA CE trywith`` () =
            stateA {
                try
                    failwith "e"
                    return 0
                with
                | e -> return 10
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE tryfinally`` () =
            stateA {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> StateA.eval "env"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``AResult CE using`` () =

            let makeResource name =
                StateA.retn { 
                new System.IDisposable with
                member _.Dispose() = () }

            stateA {
                use! x = makeResource "hello"
                return 10
            }
            |> StateA.eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateA CE source async`` () =
            stateA {
                let! x = Async.retn 10
                return x + 20
            }
            |> StateA.eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``StateA CE source task`` () =
            stateA {
                let! x = Task.FromResult(10)
                let! y = StateA.retn 20
                return x + y
            }
            |> StateA.eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``StateA CE MergeSources`` () =
            stateA {
                let! x = StateA.retn 10
                and! y = StateA.retn 20
                return x + y
            }
            |> StateA.eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``StateA CE for`` () =
            let mutable x = 0

            stateA {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> StateA.eval "env"
            |> (=) 6
            |> Assert.True

        [<Fact>]
        let ``StateA sequenceAsync`` () =

            StateA.retn 10
            |> StateA.sequenceAsync
            |> Async.RunSynchronously
            |> State.eval "env"
            |> (=) 10 
            |> Assert.True

        [<Fact>]
        let ``StateA traverse`` () =
            let f = fun (x: string) -> StateA.retn x.Length

            ["ab"; "abc"; "a"]
            |> StateA.traverseM f
            |> State.eval "env"
            |> Async.RunSynchronously
            |> (=) [2; 3; 1]
            |> Assert.True

        [<Fact>]
        let ``StateA sequence`` () =
            ["ab"; "abc"; "a"]
            |> List.map StateA.retn
            |> StateA.sequenceM
            |> State.eval "env"
            |> Async.RunSynchronously
            |> (=) ["ab"; "abc"; "a"]
            |> Assert.True

        [<Fact>]
        let ``StateA concat`` () =
            let x = StateA.retn [1; 2]
            let y = StateA.retn [3; 4]

            (x, y)
            ||> StateA.concat
            |>  StateA.eval "env"
            |> (=) [1..4]
            |> Assert.True

        [<Fact>]
        let ``StateA put and get`` () =
            StateA.put "abc"
            |> StateA.bind (fun _ -> StateA.get)
            |>  StateA.eval "env"
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``StateA lift1`` () =
            let f e (a: 'a) = sprintf "%s-%A" e a |> Async.retn
            let f' a = a |> StateA.lift1 f

            10
            |> f'
            |> StateA.eval "env"
            |> (=) "env-10"
            |> Assert.True

        [<Fact>]
        let ``StateA lift2`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b |> Async.retn
            let f' a b = (a, b) ||> StateA.lift2 f

            (10, 11)
            ||> f'
            |> StateA.eval "env"
            |> (=) "env-10-11"
            |> Assert.True

