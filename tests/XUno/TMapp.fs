namespace Simplee.Tests

module TMapp =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.Mapp.Operators
    open Simplee.Collections.Mapp.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a  = State.retn a
        let add    kvs = StateR.retn ()
        let del     ks = StateR.retn ()
        let find    ks = ks |> List.map (fun k -> k, sprintf "v%d" k) |> StateR.retn
        let isFull  () = StateR.retn false

        let eval s0 p = Mapp.eval puree add del find isFull s0 p
        let exec s0 p = Mapp.exec puree add del find isFull s0 p

        [<Fact>]
        let ``Mapp retn`` () =
            10
            |> Mapp.retn
            |> eval "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp exec`` () =
            10
            |> Mapp.retn
            |> exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``Mapp map`` () =
            "abcde"
            |> Mapp.retn
            |> Mapp.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp bind`` () =
            "abcde"
            |> Mapp.retn
            |> Mapp.bind (fun (s: string) -> s.Length |> Mapp.retn)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp apply`` () =
            let f (s: string) = s.Length
            let f' = Mapp.retn f

            "abcde"
            |> Mapp.retn
            |> Mapp.apply f'
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp map2`` () =
            let f x y = x + y
            let x = Mapp.retn 10
            let y = Mapp.retn 20

            (x, y)
            ||> Mapp.map2 f
            |> eval "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``Mapp zip`` () =
            let x = Mapp.retn 10
            let y = Mapp.retn 20

            (x, y)
            ||> Mapp.zip
            |> eval "env"
            |> (=) (10, 20)
            |> Assert.True

        [<Fact>]
        let ``Mapp map2`` () =
            let f x y z = x + y + z
            let x = Mapp.retn 10
            let y = Mapp.retn 20
            let z = Mapp.retn 30

            (x, y, z)
            |||> Mapp.map3 f
            |> eval "env"
            |> (=) 60
            |> Assert.True

        [<Fact>]
        let ``Mapp concat`` () =
            let x = Mapp.retn [1;2]
            let y = Mapp.retn [3;4]

            (x, y)
            ||> Mapp.concat
            |> eval "env"
            |> (=) [1..4]
            |> Assert.True

        [<Fact>]
        let ``Mapp <!>`` () =
            "abcde"
            |> Mapp.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp <*>`` () =
            let f (s: string) = s.Length
            let f' = Mapp.retn f

            f'
            <*> (Mapp.retn "abcde")
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp >>=`` () =
            "abcde"
            |> Mapp.retn
            >>= (fun (s: string) -> s.Length |> Mapp.retn)
            |> eval "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp puree`` () =

            10
            |> Mapp.retn
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp add`` () =

            [10; 20]
            |> List.map (fun k -> k, sprintf "v%d" k)
            |> Mapp.add
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Mapp del`` () =

            [10; 20]
            |> Mapp.del
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Mapp find`` () =

            [10; 20]
            |> Mapp.find
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok ([10;20] |> List.map (fun k -> k, sprintf "v%d" k)))
            |> Assert.True

        [<Fact>]
        let ``Mapp isFull`` () =

            Mapp.isFull
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``Mapp CE Return`` () =
            _mapp {
                return 10
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE ReturnFrom`` () =
            _mapp {
                return! (Mapp.retn 10)
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE Yield`` () =
            _mapp {
                yield 10
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE YieldFrom`` () =
            _mapp {
                yield! (Mapp.retn 10)
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE Bind`` () =
            _mapp {
                let! r = Mapp.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = Mapp.del [2]
                return r'
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Mapp CE Bind 1`` () =
            _mapp {
                let! r  = Mapp.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = Mapp.del [2]
                let! ys = Mapp.find [2]
                return ys
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok [2, "v2"])
            |> Assert.True

        [<Fact>]
        let ``Queue CE Bind 2`` () =
            _mapp {
                let! r  = Mapp.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = Mapp.del [2]
                let! ys = Mapp.isFull
                return ys
            } 
            |> Mapp.eval puree add del find isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``Mapp CE zero`` () =
            _mapp {
                let! x = Mapp.retn 10
                if x = 20 then return ()
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``Mapp CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _mapp {
                while test() do
                    inc()
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) ()
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Mapp CE trywith`` () =
            _mapp {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE tryfinally`` () =
            _mapp {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Mapp CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> Mapp.retn

            _mapp {
                use! x = makeResource "hello"
                return 10
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Mapp CE MergeSources`` () =
            _mapp {
                let! x = Mapp.retn 10
                and! y = Mapp.retn 20
                return x + y
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``Mapp CE for-loop`` () =
            let mutable x = 0

            _mapp {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> Mapp.eval puree add del find isFull "env"
            |> (=) 6
            |> Assert.True