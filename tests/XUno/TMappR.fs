namespace Simplee.Tests

module TMappR =

    open Simplee
    open Simplee.Collections
    open Simplee.Collections.MappR.Operators
    open Simplee.Collections.MappR.ComputationExpression

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        let puree   a  = State.retn a
        let add    kvs = StateR.retn ()
        let del     ks = StateR.retn ()
        let find    ks = ks |> List.map (fun k -> k, sprintf "v%d" k) |> StateR.retn
        let isFull  () = StateR.retn false

        let eval s0 p = MappR.eval puree add del find isFull s0 p
        let exec s0 p = MappR.exec puree add del find isFull s0 p

        [<Fact>]
        let ``MappR retn`` () =
            10
            |> MappR.retn
            |> eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR exec`` () =
            10
            |> MappR.retn
            |> exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``MappR map`` () =
            "abcde"
            |> MappR.retn
            |> MappR.map (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR bind`` () =
            "abcde"
            |> MappR.retn
            |> MappR.bind (fun (s: string) -> s.Length |> MappR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR apply`` () =
            let f (s: string) = s.Length
            let f' = MappR.retn f

            "abcde"
            |> MappR.retn
            |> MappR.apply f'
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR map2`` () =
            let f x y = x + y
            let x = MappR.retn 10
            let y = MappR.retn 20

            (x, y)
            ||> MappR.map2 f
            |> eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``MappR zip`` () =
            let x = MappR.retn 10
            let y = MappR.retn 20

            (x, y)
            ||> MappR.zip
            |> eval "env"
            |> (=) (Ok (10, 20))
            |> Assert.True

        [<Fact>]
        let ``MappR map2`` () =
            let f x y z = x + y + z
            let x = MappR.retn 10
            let y = MappR.retn 20
            let z = MappR.retn 30

            (x, y, z)
            |||> MappR.map3 f
            |> eval "env"
            |> (=) (Ok 60)
            |> Assert.True

        [<Fact>]
        let ``MappR concat`` () =
            let x = MappR.retn [1;2]
            let y = MappR.retn [3;4]

            (x, y)
            ||> MappR.concat
            |> eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``MappR <!>`` () =
            "abcde"
            |> MappR.retn
            <!> (fun (s: string) -> s.Length)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR <*>`` () =
            let f (s: string) = s.Length
            let f' = MappR.retn f

            f'
            <*> (MappR.retn "abcde")
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR >>=`` () =
            "abcde"
            |> MappR.retn
            >>= (fun (s: string) -> s.Length |> MappR.retn)
            |> eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``MappR puree`` () =

            10
            |> MappR.retn
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR add`` () =

            [10; 20]
            |> List.map (fun k -> k, sprintf "v%d" k)
            |> MappR.add
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappR del`` () =

            [10; 20]
            |> MappR.del
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappR find`` () =

            [10; 20]
            |> MappR.find
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ([10;20] |> List.map (fun k -> k, sprintf "v%d" k)))
            |> Assert.True

        [<Fact>]
        let ``MappR isFull`` () =

            MappR.isFull
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``MappR CE Return`` () =
            _mappR {
                return 10
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE ReturnFrom`` () =
            _mappR {
                return! (MappR.retn 10)
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE Yield`` () =
            _mappR {
                yield 10
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE YieldFrom`` () =
            _mappR {
                yield! (MappR.retn 10)
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE Bind`` () =
            _mappR {
                let! r = MappR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappR.del [2]
                return r'
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappR CE Bind 1`` () =
            _mappR {
                let! r  = MappR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappR.del [2]
                let! ys = MappR.find [2]
                return ys
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok [2, "v2"])
            |> Assert.True

        [<Fact>]
        let ``MappR CE Bind 2`` () =
            _mappR {
                let! r  = MappR.add ([1;2;3] |> List.map (fun k -> k, sprintf "v%d" k))
                let! r' = MappR.del [2]
                let! ys = MappR.isFull
                return ys
            } 
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok false)
            |> Assert.True

        [<Fact>]
        let ``MappR CE zero`` () =
            _mappR {
                let! x = MappR.retn 10
                if x = 20 then return ()
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``MappR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _mappR {
                while test() do
                    inc()
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``MappR CE trywith`` () =
            _mappR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE tryfinally`` () =
            _mappR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``MappR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> MappR.retn

            _mappR {
                use! x = makeResource "hello"
                return 10
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``MappR CE MergeSources`` () =
            _mappR {
                let! x = MappR.retn 10
                and! y = MappR.retn 20
                return x + y
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``MappR CE for-loop`` () =
            let mutable x = 0

            _mappR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> MappR.eval puree add del find isFull "env"
            |> (=) (Ok 6)
            |> Assert.True