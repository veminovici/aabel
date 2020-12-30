namespace Simplee.Tests

module TMappAR =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.MappAR.Operators
    open Simplee.Collections.MappAR.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a  = StateAR.retn a
        let add    kvs = StateAR.retn ()
        let del     ks = StateAR.retn ()
        let find    ks = ks |> List.map (fun k -> k, sprintf "v%d" k) |> StateAR.retn
        let isFull  () = StateAR.retn false

        let eval s0 p = MappAR.eval puree add del find isFull s0 p
        let exec s0 p = MappAR.exec puree add del find isFull s0 p

        [<Fact>]
        let ``MappAR retn`` () =
            10
            |> MappAR.retn
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR exec`` () =
            10
            |> MappAR.retn
            |> exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``MappAR map`` () =
            "abcde"
            |> MappAR.retn
            |> MappAR.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR bind`` () =
            "abcde"
            |> MappAR.retn
            |> MappAR.bind (fun (s: string) -> s.Length |> MappAR.retn)
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR apply`` () =
            let f (s: string) = s.Length
            let f' = MappAR.retn f

            "abcde"
            |> MappAR.retn
            |> MappAR.apply f'
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR map2`` () =
            let f x y = x + y
            let x = MappAR.retn 10
            let y = MappAR.retn 20

            (x, y)
            ||> MappAR.map2 f
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``MappAR zip`` () =
            let x = MappAR.retn 10
            let y = MappAR.retn 20

            (x, y)
            ||> MappAR.zip
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok (10, 20))
            |> Assert.True

        [<Fact>]
        let ``MappAR map2`` () =
            let f x y z = x + y + z
            let x = MappAR.retn 10
            let y = MappAR.retn 20
            let z = MappAR.retn 30

            (x, y, z)
            |||> MappAR.map3 f
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 60)
            |> Assert.True

        [<Fact>]
        let ``MappAR concat`` () =
            let x = MappAR.retn [1;2]
            let y = MappAR.retn [3;4]

            (x, y)
            ||> MappAR.concat
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``MappAR <!>`` () =
            "abcde"
            |> MappAR.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR <*>`` () =
            let f (s: string) = s.Length
            let f' = MappAR.retn f

            f'
            <*> (MappAR.retn "abcde")
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR >>=`` () =
            "abcde"
            |> MappAR.retn
            >>= (fun (s: string) -> s.Length |> MappAR.retn)
            |> eval "env"
            |> Async.RunSynchronously
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappAR puree`` () =

            10
            |> MappAR.retn
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR add`` () =

            [10; 20]
            |> List.map (fun k -> k, sprintf "v%d" k)
            |> MappAR.add
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappAR del`` () =

            [10; 20]
            |> MappAR.del
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappAR find`` () =

            [10; 20]
            |> MappAR.find
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ([10;20] |> List.map (fun k -> k, sprintf "v%d" k)))
            |> Assert.True

        [<Fact>]
        let ``MappAR isFull`` () =

            MappAR.isFull
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE Return`` () =
            _mappAR {
                return 10
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE ReturnFrom`` () =
            _mappAR {
                return! (MappAR.retn 10)
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE Yield`` () =
            _mappAR {
                yield 10
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE YieldFrom`` () =
            _mappAR {
                yield! (MappAR.retn 10)
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE Bind`` () =
            _mappAR {
                let! r  = MappAR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappAR.del [2]
                return r'
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappAR CE Bind 1`` () =
            _mappAR {
                let! r  = MappAR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappAR.del [2]
                let! ys = MappAR.find [2]
                return ys
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok [2, "v2"])
            |> Assert.True

        [<Fact>]
        let ``MapAR CE Bind 2`` () =
            _mappAR {
                let! r  = MappAR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappAR.del [2]
                let! ys = MappAR.isFull
                return ys
            } 
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE zero`` () =
            _mappAR {
                let! x = MappAR.retn 10
                if x = 20 then return ()
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappAR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _mappAR {
                while test() do
                    inc()
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``MappAR CE trywith`` () =
            _mappAR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE tryfinally`` () =
            _mappAR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> MappAR.retn

            _mappAR {
                use! x = makeResource "hello"
                return 10
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE MergeSources`` () =
            _mappAR {
                let! x = MappAR.retn 10
                and! y = MappAR.retn 20
                return x + y
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``MappAR CE for-loop`` () =
            let mutable x = 0

            _mappAR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> MappAR.eval puree add del find isFull "env"
            |> Async.RunSynchronously
            |> (=) (Ok 6)
            |> Assert.True