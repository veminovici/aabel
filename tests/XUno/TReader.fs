namespace Simplee.Tests

module TReader =

    open Simplee
    open Simplee.Reader.Operators
    open Simplee.Reader.ComputationExpression
    open Simplee.Reader.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``Reader run`` () =
            let flow = Reader <| fun (env: string) -> env.Length

            flow
            |> Reader.run "env"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Reader retn`` () =
            10
            |> Reader.retn
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader singleton`` () =
            10
            |> Reader.singleton
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True


        [<Fact>]
        let ``Reader map`` () =
            "abcde"
            |> Reader.retn
            |> Reader.map (fun (s: string) -> s.Length)
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader apply`` () =
            let f = Reader.retn <| fun (s: string) -> s.Length

            "abcde"
            |> Reader.retn
            |> Reader.apply f
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader bind`` () =
            let f = fun (s: string) -> s.Length |> Reader.retn

            "abcde"
            |> Reader.retn
            |> Reader.bind f
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader bind chain`` () =
            let f i = i |> (+) 1 |> Reader.retn

            1
            |> Reader.retn
            |> Reader.bind f
            |> Reader.bind f
            |> Reader.bind f
            |> Reader.bind f
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> Reader.retn
            let y = "cd" |> Reader.retn

            (x, y)
            ||> Reader.map2 f
            |> Reader.run "env"
            |> (=) "abcd"
            |> Assert.True

        [<Fact>]
        let ``Reader zip`` () =
            let x = "ab" |> Reader.retn
            let y = "cd" |> Reader.retn
            
            (x, y)
            ||> Reader.zip
            |> Reader.run "env"
            |> (=) ("ab", "cd")
            |> Assert.True

        [<Fact>]
        let ``Reader map3`` () =
            let x = Reader.retn 1
            let y = Reader.retn 2
            let z = Reader.retn 3
            let f x y z = x + y + z

            (x, y, z)
            |||> Reader.map3 f
            |>   Reader.run "env"
            |>   (=) 6
            |>   Assert.True

        [<Fact>]
        let ``Reader <!>`` () =
            let f (s: string) = s.Length

            "abcde"
            |> Reader.retn
            <!> f
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader <*>`` () =
            let f (s: string) = s.Length

            f
            |> Reader.retn
            <*> (Reader.retn "abcde")
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader >>=`` () =
            let f i = i |> (+) 1 |> Reader.retn

            1
            |> Reader.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> Reader.run "env"
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader >>.`` () =
            let f i = i |> (+) 1 |> Reader.retn

            1
            |> Reader.retn
            >>. f
            >>. f
            >>. f
            >>. f
            |>  Reader.run "env"
            |>  (=) 5
            |>  Assert.True

        [<Fact>]
        let ``Reader .>>.`` () =
            let f i = i |> (+) 1 |> Reader.retn

            1
            |>   Reader.retn
            .>>. f
            |>   Reader.run "env"
            |>   (=) (1, 2)
            |>   Assert.True

        [<Fact>]
        let ``Reader .>>`` () =
            let f i = i |> (+) 1 |> Reader.retn

            1
            |>  Reader.retn
            .>> f
            |>  Reader.run "env"
            |>  (=) 1
            |>  Assert.True

        [<Fact>]
        let ``Reader ++`` () =

            let a = Reader.retn 1
            let b = Reader.retn "abc"

            a
            ++ b
            |> Reader.run "env"
            |> (=) (1, "abc")
            |> Assert.True

        [<Fact>]
        let ``Reader CE return`` () =

            _reader {
                return 10
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE returnFrom`` () =

            _reader {
                return! (Reader.retn 10)
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE yield`` () =

            _reader {
                yield 10
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE yieldFrom`` () =

            _reader {
                yield! (Reader.retn 10)
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE zero`` () =
            _reader {
                let! x = Reader.retn 10
                if x = 20 then return ()
            }
            |> Reader.run "env"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``Reader CE bind`` () =

            _reader {
                let! x = Reader.retn 1
                let! y = Reader.retn 2
                let! z = Reader.retn 3
                return x + y + z
            }
            |> Reader.run "env"
            |> (=) 6
            |> Assert.True

        [<Fact>]
        let ``Reader CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _reader {
                while test() do
                    inc()
            }
            |> Reader.run "env"
            |> (=) ()
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Reader CE trywith`` () =
            _reader {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE tryfinally`` () =
            _reader {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> Reader.run "env"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Reader CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> Reader.retn

            _reader {
                use! x = makeResource "hello"
                return 10
            }
            |> Reader.run "env"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Reader CE source result`` () =
            _reader {
                let! x = Ok 10
                return x
            }
            |> Reader.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Reader CE source choice`` () =
            _reader {
                let! x = Choice1Of2 10
                return x
            }
            |> Reader.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Reader CE MergeSources`` () =
            _reader {
                let! x = Reader.retn 10
                and! y = Reader.retn 20
                return x + y
            }
            |> Reader.run "env"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``Reader CE for-loop`` () =
            let mutable x = 0

            _reader {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> Reader.run "env"
            |> (=) 6
            |> Assert.True

        [<Fact>]
        let ``Reader sequenceAsync`` () =
            _reader {
                async {return 10 }
            }
            |> Reader.sequenceAsync
            |> Async.RunSynchronously
            |> Reader.run "env"
            |> (=) 10 
            |> Assert.True

        [<Fact>]
        let ``Reader traverse`` () =
            let f (x: string) = Reader.retn x.Length

            ["ab"; "abc";"a"]
            |> Reader.traverseM f
            |> Reader.run "env"
            |> (=) [2; 3; 1]
            |> Assert.True

        [<Fact>]
        let ``Reader sequence`` () =
            ["ab"; "abc"; "a"]
            |> List.map Reader.retn
            |> Reader.sequenceM
            |> Reader.run "env"
            |> (=) ["ab"; "abc";"a"]
            |> Assert.True

        [<Fact>]
        let ``Reader concat`` () =

            let x = Reader.retn [1; 2]
            let y = Reader.retn [3; 4]

            (x, y)
            ||> Reader.concat
            |> Reader.run "env"
            |> (=) [1..4]
            |> Assert.True

        [<Fact>]
        let ``Reader lift1`` () =
            let f e (a: 'a) = sprintf "%s-%A" e a
            let f' a = a |> Reader.lift1 f

            10
            |> f'
            |> Reader.run "env"
            |> (=) "env-10"
            |> Assert.True

        [<Fact>]
        let ``Reader lift2`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b
            let f' a b = (a, b) ||> Reader.lift2 f

            (10, 11)
            ||> f'
            |> Reader.run "env"
            |> (=) "env-10-11"
            |> Assert.True

        [<Fact>]
        let ``Reader liftE`` () =

            let rdr x = Reader <| fun env ->
                sprintf "%A-%A" env x

            1
            |> rdr
            |> Reader.liftE fst
            |> Reader.run ("env", 10)
            |> (=) "\"env\"-1"
            |> Assert.True

            1
            |> rdr
            |> Reader.liftE snd
            |> Reader.run ("env", 10)
            |> (=) "10-1"
            |> Assert.True

        [<Fact>]
        let ``Reader unfold`` () =
            let iter = Reader <| fun env ->
                env + 1

            0
            |> Reader.unfold iter
            |> Seq.take 3
            |> List.ofSeq
            |> (=) [1; 1; 1]
            |> Assert.True

