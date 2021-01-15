namespace Simplee.Tests

module TReaderR =

    open Simplee
    open Simplee.ReaderR.Operators
    open Simplee.ReaderR.ComputationExpression
    open Simplee.ReaderR.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``ReaderR run`` () =
            let flow = Reader <| fun (env: string) -> env.Length |> Ok

            flow
            |> ReaderR.run "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``ReaderR retn`` () =
            10
            |> ReaderR.retn
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Reader singleton`` () =
            10
            |> ReaderR.singleton
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR map`` () =
            let fn (s: string) = s.Length

            "abcde"
            |> ReaderR.retn
            |> ReaderR.map  fn
            |> ReaderR.run  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR mapError`` () =
            let fn (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderR.err
            |> ReaderR.mapError  fn
            |> ReaderR.run  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``ReaderR mapEither ok`` () =
            let fnOk (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderR.retn
            |> ReaderR.mapEither fnOk fnErr
            |> ReaderR.run  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR mapEither err`` () =
            let fnOk (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderR.err
            |> ReaderR.mapEither fnOk fnErr
            |> ReaderR.run  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``ReaderR apply`` () =
            let f = ReaderR.retn <| fun (s: string) -> s.Length

            "abcde"
            |> ReaderR.retn
            |> ReaderR.apply f
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR bind`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            |> ReaderR.bind f
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR bind chain`` () =
            let f i = i |> (+) 1 |> ReaderR.retn

            1
            |> ReaderR.retn
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR bind chain err`` () =
            let f i = 
                if i < 3 
                then i |> (+) 1 |> ReaderR.retn
                else i |> sprintf "e%d" |> ReaderR.err

            1
            |> ReaderR.retn
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.bind f
            |> ReaderR.run "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``ReaderR bindLR`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            |> ReaderR.bindLR f
            |> ReaderR.run "env"
            |> (=) (Ok ("abcde", 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderR bindL`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            |> ReaderR.bindL f
            |> ReaderR.run "env"
            |> (=) (Ok "abcde")
            |> Assert.True

        [<Fact>]
        let ``ReaderR bindR`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            |> ReaderR.bindR f
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR bindFst`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn
            let a = ("abcd", 10.) |> ReaderR.retn

            a
            |> ReaderR.bindFst f
            |> ReaderR.run "env"
            |> (=) (Ok (4, 10.))
            |> Assert.True

        [<Fact>]
        let ``ReaderR bindSnd`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn
            let a = (10., "abcd") |> ReaderR.retn

            a
            |> ReaderR.bindSnd f
            |> ReaderR.run "env"
            |> (=) (Ok (10., 4))
            |> Assert.True

        [<Fact>]
        let ``ReaderR map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> ReaderR.retn
            let y = "cd" |> ReaderR.retn

            (x, y)
            ||> ReaderR.map2 f
            |> ReaderR.run "env"
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``ReaderR zip`` () =
            let x = "ab" |> ReaderR.retn
            let y = "cd" |> ReaderR.retn
            
            (x, y)
            ||> ReaderR.zip
            |> ReaderR.run "env"
            |> (=) (Ok ("ab", "cd"))
            |> Assert.True

        [<Fact>]
        let ``ReaderR map3`` () =
            let x = ReaderR.retn 1
            let y = ReaderR.retn 2
            let z = ReaderR.retn 3
            let f x y z = x + y + z

            (x, y, z)
            |||> ReaderR.map3 f
            |>   ReaderR.run "env"
            |>   (=) (Ok 6)
            |>   Assert.True

        [<Fact>]
        let ``ReaderR ignore`` () =
            1
            |> ReaderR.retn
            |> ReaderR.ignore
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireTrue`` () =
            true
            |> Reader.retn
            |> ReaderR.requireTrue "e"
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireFalse`` () =
            false
            |> Reader.retn
            |> ReaderR.requireFalse "e"
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireSome`` () =
            1
            |> Some
            |> Reader.retn
            |> ReaderR.requireSome "e"
            |> ReaderR.run "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireNone`` () =
            None
            |> Reader.retn
            |> ReaderR.requireNone "e"
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireNotNull`` () =
            box 1
            |> Reader.retn
            |> ReaderR.requireNotNull "e"
            |> ReaderR.run "env"
            |> (=) (Ok (box 1))
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireEmpty`` () =
            []
            |> Reader.retn
            |> ReaderR.requireEmpty "e"
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireNotEmpty`` () =
            ["a"; "b"]
            |> Reader.retn
            |> ReaderR.requireNotEmpty "e"
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR withError`` () =
            1
            |> ReaderR.retn
            |> ReaderR.withError "e"
            |> ReaderR.run "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``ReaderR withError err`` () =
            1
            |> ReaderR.err
            |> ReaderR.withError "e"
            |> ReaderR.run "env"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``ReaderR teeIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> ReaderR.retn
            |> ReaderR.teeIf predicate fn
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``ReaderR teeErrorIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> ReaderR.err
            |> ReaderR.teeErrorIf predicate fn
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``ReaderR tee`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> ReaderR.retn
            |> ReaderR.tee fn
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderR teeError`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> ReaderR.err
            |> ReaderR.teeError fn
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderR <!>`` () =
            let f (s: string) = s.Length

            "abcde"
            |> ReaderR.retn
            <!> f
            |> Reader.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR <*>`` () =
            let f (s: string) = s.Length

            f
            |> ReaderR.retn
            <*> (ReaderR.retn "abcde")
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR >>=`` () =
            let f i = i |> (+) 1 |> ReaderR.retn

            1
            |> ReaderR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> Reader.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR >>= err`` () =
            let f i = 
                if i < 3
                then i |> (+) 1 |> ReaderR.retn
                else i |> sprintf "e%d" |> ReaderR.err

            1
            |> ReaderR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> Reader.run "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``ReaderR .>>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            .>>. f
            |> ReaderR.run "env"
            |> (=) (Ok ("abcde", 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderR .>>`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            .>> f
            |> ReaderR.run "env"
            |> (=) (Ok "abcde")
            |> Assert.True

        [<Fact>]
        let ``ReaderR >>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn

            "abcde"
            |> ReaderR.retn
            >>. f
            |> ReaderR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderR />>`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn
            let a = ("abcd", 10.) |> ReaderR.retn

            a
            />> f
            |> ReaderR.run "env"
            |> (=) (Ok (4, 10.))
            |> Assert.True

        [<Fact>]
        let ``ReaderR >>/`` () =
            let f = fun (s: string) -> s.Length |> ReaderR.retn
            let a = (10., "abcd") |> ReaderR.retn

            a
            >>/ f
            |> ReaderR.run "env"
            |> (=) (Ok (10., 4))
            |> Assert.True

        [<Fact>]
        let ``ReaderR ++`` () =
            let a = "abc" |> ReaderR.retn
            let b = 3     |> ReaderR.retn

            a ++ b
            |> ReaderR.run "env"
            |> (=) (Ok ("abc", 3))
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE return`` () =

            _readerR {
                return 10
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE returnFrom`` () =

            _readerR {
                return! (ReaderR.retn 10)
            }
            |> Reader.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Reader CE yield`` () =

            _readerR {
                yield 10
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE yieldFrom`` () =

            _readerR {
                yield! (ReaderR.retn 10)
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE zero`` () =
            _readerR {
                let! x = ReaderR.retn 10
                if x = 20 then return ()
            }
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE bind`` () =

            _readerR {
                let! x = ReaderR.retn 1
                let! y = ReaderR.retn 2
                let! z = ReaderR.retn 3
                return x + y + z
            }
            |> ReaderR.run "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _readerR {
                while test() do
                    inc()
            }
            |> ReaderR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE trywith`` () =
            _readerR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE tryfinally`` () =
            _readerR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> ReaderR.run "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> ReaderR.retn

            _readerR {
                use! x = makeResource "hello"
                return 10
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE source result`` () =
            _readerR {
                let! x = Ok 10
                return x
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE source choice`` () =
            _readerR {
                let! x = Choice1Of2 10
                return x
            }
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE MergeSources`` () =
            _readerR {
                let! x = ReaderR.retn 10
                and! y = ReaderR.retn 20
                return x + y
            }
            |> ReaderR.run "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``ReaderR CE for loop`` () =
            let mutable x = 0

            _readerR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> ReaderR.run "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``ReaderR traverseM`` () =
            let f (x: string) = ReaderR.retn x.Length

            ["ab"; "abc";"a"]
            |> ReaderR.traverseM f
            |> ReaderR.run "env"
            |> (=) (Ok [2; 3; 1])
            |> Assert.True

        [<Fact>]
        let ``ReaderR sequenceM`` () =
            ["ab"; "abc"; "a"]
            |> List.map ReaderR.retn
            |> ReaderR.sequenceM
            |> ReaderR.run "env"
            |> (=) (Ok ["ab"; "abc";"a"])
            |> Assert.True

        [<Fact>]
        let ``ReaderR traverseA ok`` () =
            let f (i: int) = ReaderR.retn (i * 2)

            [1..5]
            |> ReaderR.traverseA f
            |> ReaderR.run "env"
            |> (=) (Ok [2; 4; 6; 8; 10])
            |> Assert.True

        [<Fact>]
        let ``ReaderR traverseA error`` () =
            let f (i: int) = 
                if i % 2 = 1 
                then ReaderR.retn (i * 2) 
                else i |> sprintf "e%i" |> ReaderR.err

            [1..5]
            |> ReaderR.traverseA f
            |> ReaderR.run "env"
            |> (=) (Error ["e2"; "e4"])
            |> Assert.True

        [<Fact>]
        let ``ReaderR sequenceA`` () =
            [1..5]
            |> List.map ReaderR.retn
            |> ReaderR.sequenceA
            |> ReaderR.run "env"
            |> (=) (Ok [1..5])
            |> Assert.True

        [<Fact>]
        let ``ReaderR concat`` () =
            let x = ReaderR.retn [1; 2]
            let y = ReaderR.retn [3; 4]

            (x, y)
            ||> ReaderR.concat
            |>  ReaderR.run "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``ReaderR lift1`` () =
            let f e (a: 'a) = a |> sprintf "%s-%A" e |> Ok
            let f' a = a |> ReaderR.lift1 f

            10
            |> f'
            |> ReaderR.run "env"
            |> (=) (Ok "env-10")
            |> Assert.True

        [<Fact>]
        let ``ReaderR lift2`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b |> Ok
            let f' a b = (a, b) ||> ReaderR.lift2 f

            (10, 11)
            ||> f'
            |> ReaderR.run "env"
            |> (=) (Ok "env-10-11")
            |> Assert.True

        [<Fact>]
        let ``ReaderR unfold`` () =
            let mutable i = 0

            let iter = Reader <| fun env ->
                i <- i + 1
                if i < 3 then Ok (env + 1) else Error "my error"

            0
            |> ReaderR.unfold iter
            |> List.ofSeq
            |> (=) [1; 1]
            |> Assert.True

