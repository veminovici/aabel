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
        let ``ReaderA mapFst`` () =
            ("abcde", 10)
            |> ReaderA.retn
            |> ReaderA.mapFst (fun (s: string) -> s.Length)
            |> ReaderA.run "env"
            |> (=) (5, 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderA mapSnd`` () =
            (10, "abcde")
            |> ReaderA.retn
            |> ReaderA.mapSnd (fun (s: string) -> s.Length)
            |> ReaderA.run "env"
            |> (=) (10, 5)
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
        let ``ReaderA bindLR`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            |> ReaderA.bindLR f
            |> ReaderA.run "env"
            |> (=) ("abcde", 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderA bindL`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            |> ReaderA.bindL f
            |> ReaderA.run "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``ReaderA bindR`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            |> ReaderA.bindR f
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA bindFst`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn
            let a = ("abcd", 10.) |> ReaderA.retn

            a
            |> ReaderA.bindFst f
            |> ReaderA.run "env"
            |> (=) (4, 10.)
            |> Assert.True

        [<Fact>]
        let ``ReaderA bindSnd`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn
            let a = (10., "abcd") |> ReaderA.retn

            a
            |> ReaderA.bindSnd f
            |> ReaderA.run "env"
            |> (=) (10., 4)
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
        let ``ReaderA </!>`` () =
            ("abcde", 10)
            |> ReaderA.retn
            </!> (fun (s: string) -> s.Length)
            |> ReaderA.run "env"
            |> (=) (5, 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderA <!/>`` () =
            (10, "abcde")
            |> ReaderA.retn
            <!/> (fun (s: string) -> s.Length)
            |> ReaderA.run "env"
            |> (=) (10, 5)
            |> Assert.True

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
        let ``ReaderA .>>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            .>>. f
            |> ReaderA.run "env"
            |> (=) ("abcde", 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderA .>>`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            .>> f
            |> ReaderA.run "env"
            |> (=) "abcde"
            |> Assert.True

        [<Fact>]
        let ``ReaderA >>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn

            "abcde"
            |> ReaderA.retn
            >>. f
            |> ReaderA.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderA />>`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn
            let a = ("abcd", 10.) |> ReaderA.retn

            a
            />> f
            |> ReaderA.run "env"
            |> (=) (4, 10.)
            |> Assert.True

        [<Fact>]
        let ``ReaderA >>/`` () =
            let f = fun (s: string) -> s.Length |> ReaderA.retn
            let a = (10., "abcd") |> ReaderA.retn

            a
            >>/ f
            |> ReaderA.run "env"
            |> (=) (10., 4)
            |> Assert.True

        [<Fact>]
        let ``ReaderA ++`` () =
            let a = "abc" |> ReaderA.retn
            let b = 3     |> ReaderA.retn

            a ++ b
            |> ReaderA.run "env"
            |> (=) ("abc", 3)
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE return`` () =

            _readerA {
                return 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE returnFrom`` () =

            _readerA {
                return! (ReaderA.retn 10)
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE yield`` () =

            _readerA {
                yield 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE yieldFrom`` () =

            _readerA {
                yield! (ReaderA.retn 10)
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE zero`` () =
            _readerA { 
                10 |> ignore
            }
            |> ReaderA.run "env"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE bind`` () =

            _readerA {
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

            _readerA {
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
            _readerA {
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
            _readerA {
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

            _readerA {
                use! x = makeResource "hello"
                return 10
            }
            |> ReaderA.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE source async`` () =
            _readerA {
                let! x = Async.retn 10
                return x + 20
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE source task`` () =
            _readerA {
                let! x = Task.FromResult(10)
                let! y = ReaderA.retn 20
                return x + y
            }
            |> ReaderA.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``ReaderA CE MergeSources`` () =
            _readerA {
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

            _readerA {
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

        [<Fact>]
        let ``ReaderA unfold`` () =
            let iter = Reader <| fun env ->
                Async.retn (env + 1)

            0
            |> ReaderA.unfold iter
            |> Seq.take 3
            |> List.ofSeq
            |> (=) [1; 1; 1]
            |> Assert.True

