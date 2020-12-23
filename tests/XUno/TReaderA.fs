namespace Simplee.Tests

module TReaderA =

    open Simplee
    open Simplee.ReaderA.ComputationExpression
    open Simplee.ReaderA.Operators
    open Simplee.ReaderA.Traversals

    open System.Threading.Tasks

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``ReaderA retn`` () =

            "abcde"
            |> ReaderA.retn
            |> ReaderA.run "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``ReaderA singleton`` () =

            "abcde"
            |> ReaderA.singleton
            |> ReaderA.run "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``ReaderA map`` () =

            let length (s: string) = s.Length

            "abcde"
            |> ReaderA.retn
            |> ReaderA.map length
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA bind`` () =

            let length (s: string) = s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            |> ReaderA.bind length
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA apply`` () =

            let fn = (fun (s: string) -> s.Length) |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            |> ReaderA.apply fn
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA map2`` () =

            let fn x y = x + y
            let x = ReaderA.retn 10
            let y = ReaderA.retn 20

            (x, y)
            ||> ReaderA.map2 fn
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA map3`` () =

            let fn x y z = x + y + z
            let x = ReaderA.retn 1
            let y = ReaderA.retn 2
            let z = ReaderA.retn 3

            (x, y, z)
            |||> ReaderA.map3 fn
            |>   ReaderA.run "env"
            |>   (=) 6
            |>   Assert.True

        [<Fact>]
        let ``ReaderA zip`` () =

            let x = ReaderA.retn "ab"
            let y = ReaderA.retn 2

            (x, y)
            ||> ReaderA.zip
            |>  ReaderA.run "env"
            |> (=) ("ab", 2)
            |> Assert.True

        [<Fact>]
        let ``ReaderA <!>`` () =

            let length (s: string) = s.Length

            "abcde"
            |>  ReaderA.retn
            <!> length
            |>  ReaderA.run "env"
            |>  (=) 5
            |>  Assert.True

        [<Fact>]
        let ``ReaderA <*>`` () =

            let fn = (fun (s: string) -> s.Length) |> ReaderA.retn

            fn
            <*> (ReaderA.retn "abcde")
            |>  ReaderA.run "env"
            |>  (=) 5
            |>  Assert.True

        [<Fact>]
        let ``ReaderA >>=`` () =

            let fn i = i |> (+) 1 |> ReaderA.retn

            1
            |> ReaderA.retn
            >>= fn
            >>= fn
            >>= fn
            >>= fn
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE return`` () =

            readerA {
                return 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE returnFrom`` () =

            readerA {
                return! (ReaderA.retn 10)
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE yield`` () =

            readerA {
                yield 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE yieldFrom`` () =

            readerA {
                yield! (ReaderA.retn 10)
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE zero`` () =
            readerA { 
                10 |> ignore
            }
            |> ReaderA.run "env"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE bind`` () =

            readerA {
                let! x = (ReaderA.retn 10)
                let! y = (ReaderA.retn 20)
                return x + y
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ResultA CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            readerA {
                while test() do
                    inc()
            }
            |> ReaderA.run "env"
            |> (=) ()
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE trywith`` () =
            readerA {
                try
                    failwith "e"
                    return 0
                with
                | e -> return 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE tryfinally`` () =
            readerA {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> ReaderA.run "env"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``AResult CE using`` () =

            let makeResource name =
                ReaderA.retn { 
                new System.IDisposable with
                member _.Dispose() = () }

            readerA {
                use! x = makeResource "hello"
                return 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE source async`` () =
            readerA {
                let! x = Async.retn 10
                return x + 20
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE source task`` () =
            readerA {
                let! x = Task.FromResult(10)
                let! y = ReaderA.retn 20
                return x + y
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE MergeSources`` () =
            readerA {
                let! x = ReaderA.retn 10
                and! y = ReaderA.retn 20
                return x + y
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE for`` () =
            let mutable x = 0

            readerA {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> ReaderA.run "env"
            |> (=) 6
            |> Assert.True

        [<Fact>]
        let ``ReaderA sequenceAsync`` () =

            ReaderA.retn 10
            |> ReaderA.sequenceAsync
            |> Async.RunSynchronously
            |> Reader.run "env"
            |> (=) 10 
            |> Assert.True

        [<Fact>]
        let ``ReaderA traverse`` () =
            let f = fun (x: string) -> ReaderA.retn x.Length

            ["ab"; "abc"; "a"]
            |> ReaderA.traverseM f
            |> Reader.run "env"
            |> Async.RunSynchronously
            |> (=) [2; 3; 1]
            |> Assert.True

        [<Fact>]
        let ``ReaderA sequence`` () =
            ["ab"; "abc"; "a"]
            |> List.map ReaderA.retn
            |> ReaderA.sequenceM
            |> Reader.run "env"
            |> Async.RunSynchronously
            |> (=) ["ab"; "abc"; "a"]
            |> Assert.True

        [<Fact>]
        let ``ReadeA concat`` () =
            let x = ReaderA.retn [1; 2]
            let y = ReaderA.retn [3; 4]

            (x, y)
            ||> ReaderA.concat
            |>  ReaderA.run "env"
            |> (=) [1..4]
            |> Assert.True
