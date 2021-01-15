namespace Simplee.Tests

module TReaderAR =

    open Simplee
    open Simplee.ReaderAR.Operators
    open Simplee.ReaderAR.ComputationExpression
    open Simplee.ReaderAR.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``ReaderAR retn`` () =
            10
            |> ReaderR.retn
            |> ReaderR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR singleton`` () =
            10
            |> ReaderAR.singleton
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR map`` () =
            let fn (s: string) = s.Length

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.map  fn
            |> ReaderAR.run  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR mapFst`` () =
            ("abcde", 10)
            |> ReaderAR.retn
            |> ReaderAR.mapFst (fun (s: string) -> s.Length)
            |> ReaderAR.run "env"
            |> (=) (Ok (5, 10))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR mapSnd`` () =
            (10, "abcde")
            |> ReaderAR.retn
            |> ReaderAR.mapSnd (fun (s: string) -> s.Length)
            |> ReaderAR.run "env"
            |> (=) (Ok (10, 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR mapError`` () =
            let fn (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderAR.err
            |> ReaderAR.mapError fn
            |> ReaderAR.run  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR mapEither ok`` () =
            let fnOk  (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.mapEither fnOk fnErr
            |> ReaderAR.run  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR mapEither err`` () =
            let fnOk  (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> ReaderAR.err
            |> ReaderAR.mapEither fnOk fnErr
            |> ReaderAR.run  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR apply`` () =
            let f = ReaderAR.retn <| fun (s: string) -> s.Length

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.apply f
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bind`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.bind f
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bind chain`` () =
            let f i = i |> (+) 1 |> ReaderAR.retn

            1
            |> ReaderAR.retn
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bind chain err`` () =
            let f i = 
                if i < 3 
                then i |> (+) 1 |> ReaderAR.retn
                else i |> sprintf "e%d" |> ReaderAR.err

            1
            |> ReaderAR.retn
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.bind f
            |> ReaderAR.run "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bindLR`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.bindLR f
            |> ReaderAR.run "env"
            |> (=) (Ok ("abcde", 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bindL`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.bindL f
            |> ReaderAR.run "env"
            |> (=) (Ok "abcde")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bindR`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            |> ReaderAR.bindR f
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bindFst`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn
            let a = ("abcd", 10.) |> ReaderAR.retn

            a
            |> ReaderAR.bindFst f
            |> ReaderAR.run "env"
            |> (=) (Ok (4, 10.))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR bindSnd`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn
            let a = (10., "abcd") |> ReaderAR.retn

            a
            |> ReaderAR.bindSnd f
            |> ReaderAR.run "env"
            |> (=) (Ok (10., 4))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> ReaderAR.retn
            let y = "cd" |> ReaderAR.retn

            (x, y)
            ||> ReaderAR.map2 f
            |> ReaderAR.run "env"
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR zip`` () =
            let x = "ab" |> ReaderAR.retn
            let y = "cd" |> ReaderAR.retn
            
            (x, y)
            ||> ReaderAR.zip
            |>  ReaderAR.run "env"
            |> (=) (Ok ("ab", "cd"))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR map3`` () =
            let x = ReaderAR.retn 1
            let y = ReaderAR.retn 2
            let z = ReaderAR.retn 3
            let f x y z = x + y + z

            (x, y, z)
            |||> ReaderAR.map3 f
            |>   ReaderAR.run "env"
            |>   (=) (Ok 6)
            |>   Assert.True

        [<Fact>]
        let ``ReaderAR ignore`` () =
            1
            |> ReaderAR.retn
            |> ReaderAR.ignore
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR requireTrue`` () =
            true
            |> ReaderA.retn
            |> ReaderAR.requireTrue "e"
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR requireFalse`` () =
            false
            |> ReaderA.retn
            |> ReaderAR.requireFalse "e"
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR requireSome`` () =
            1
            |> Some
            |> ReaderA.retn
            |> ReaderAR.requireSome "e"
            |> ReaderAR.run "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR requireNone`` () =
            None
            |> ReaderA.retn
            |> ReaderAR.requireNone "e"
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR requireNotNull`` () =
            box 1
            |> ReaderA.retn
            |> ReaderAR.requireNotNull "e"
            |> ReaderAR.run "env"
            |> (=) (Ok (box 1))
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireEmpty`` () =
            []
            |> ReaderA.retn
            |> ReaderAR.requireEmpty "e"
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderR requireNotEmpty`` () =
            ["a"; "b"]
            |> ReaderA.retn
            |> ReaderAR.requireNotEmpty "e"
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR withError`` () =
            1
            |> ReaderAR.retn
            |> ReaderAR.withError "e"
            |> ReaderAR.run "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR withError err`` () =
            1
            |> ReaderAR.err
            |> ReaderAR.withError "e"
            |> ReaderAR.run "env"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR teeIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> ReaderAR.retn
            |> ReaderAR.teeIf predicate fn
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``ReaderAR teeErrorIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> ReaderAR.err
            |> ReaderAR.teeErrorIf predicate fn
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``ReaderAR tee`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> ReaderAR.retn
            |> ReaderAR.tee fn
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderAR teeError`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> ReaderAR.err
            |> ReaderAR.teeError fn
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``ReaderAR <!>`` () =
            let f (s: string) = s.Length

            "abcde"
            |> ReaderAR.retn
            <!> f
            |> ReaderA.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR </!>`` () =
            ("abcde", 10)
            |> ReaderAR.retn
            </!> (fun (s: string) -> s.Length)
            |> ReaderAR.run "env"
            |> (=) (Ok (5, 10))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR <!/>`` () =
            (10, "abcde")
            |> ReaderAR.retn
            <!/> (fun (s: string) -> s.Length)
            |> ReaderAR.run "env"
            |> (=) (Ok (10, 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR <*>`` () =
            let f (s: string) = s.Length

            f
            |> ReaderAR.retn
            <*> (ReaderAR.retn "abcde")
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR >>=`` () =
            let f i = i |> (+) 1 |> ReaderAR.retn

            1
            |> ReaderAR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> ReaderA.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR >>= err`` () =
            let f i = 
                if i < 3
                then i |> (+) 1 |> ReaderAR.retn
                else i |> sprintf "e%d" |> ReaderAR.err

            1
            |> ReaderAR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> ReaderA.run "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE return`` () =

            _readerAR {
                return 10
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE returnFrom`` () =

            _readerAR {
                return! (ReaderAR.retn 10)
            }
            |> ReaderA.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE yield`` () =

            _readerAR {
                yield 10
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE yieldFrom`` () =

            _readerAR {
                yield! (ReaderAR.retn 10)
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE zero`` () =
            _readerAR {
                let! x = ReaderAR.retn 10
                if x = 20 then return ()
            }
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE bind`` () =

            _readerAR {
                let! x = ReaderAR.retn 1
                let! y = ReaderAR.retn 2
                let! z = ReaderAR.retn 3
                return x + y + z
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            _readerAR {
                while test() do
                    inc()
            }
            |> ReaderAR.run "env"
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE trywith`` () =
            _readerAR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE tryfinally`` () =
            _readerAR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> ReaderAR.retn

            _readerAR {
                use! x = makeResource "hello"
                return 10
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE source result`` () =
            _readerAR {
                let! x = Ok 10
                return x
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE source choice`` () =
            _readerAR {
                let! x = Choice1Of2 10
                return x
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE MergeSources`` () =
            _readerAR {
                let! x = ReaderAR.retn 10
                and! y = ReaderAR.retn 20
                return x + y
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR CE for loop`` () =
            let mutable x = 0

            _readerAR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> ReaderAR.run "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR traverseM`` () =
            let f (x: string) = ReaderAR.retn x.Length

            ["ab"; "abc";"a"]
            |> ReaderAR.traverseM f
            |> ReaderAR.run "env"
            |> (=) (Ok [2; 3; 1])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR sequenceM`` () =
            ["ab"; "abc"; "a"]
            |> List.map ReaderAR.retn
            |> ReaderAR.sequenceM
            |> ReaderAR.run "env"
            |> (=) (Ok ["ab"; "abc";"a"])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR traverseA ok`` () =
            let f (i: int) = ReaderAR.retn (i * 2)

            [1..5]
            |> ReaderAR.traverseA f
            |> ReaderAR.run "env"
            |> (=) (Ok [2; 4; 6; 8; 10])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR traverseA error`` () =
            let f (i: int) = 
                if i % 2 = 1 
                then ReaderAR.retn (i * 2) 
                else i |> sprintf "e%i" |> ReaderAR.err

            [1..5]
            |> ReaderAR.traverseA f
            |> ReaderAR.run "env"
            |> (=) (Error ["e2"; "e4"])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR sequenceA`` () =
            [1..5]
            |> List.map ReaderAR.retn
            |> ReaderAR.sequenceA
            |> ReaderAR.run "env"
            |> (=) (Ok [1..5])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR concat`` () =
            let x = ReaderAR.retn [1; 2]
            let y = ReaderAR.retn [3; 4]

            (x, y)
            ||> ReaderAR.concat
            |>  ReaderAR.run "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift1`` () =
            let f e (a: 'a) = a |> sprintf "%s-%A" e |> AR.retn
            let f' a = a |> ReaderAR.lift1 f

            10
            |> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift1R`` () =
            let f e (a: 'a) = a |> sprintf "%s-%A" e |> Ok
            let f' a = a |> ReaderAR.lift1R f

            10
            |> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift1A`` () =
            let f e (a: 'a) = a |> sprintf "%s-%A" e |> Async.retn
            let f' a = a |> ReaderAR.lift1A f

            10
            |> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift2`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b |> AR.retn
            let f' a b = (a, b) ||> ReaderAR.lift2 f

            (10, 11)
            ||> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10-11")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift2R`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b |> Ok
            let f' a b = (a, b) ||> ReaderAR.lift2R f

            (10, 11)
            ||> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10-11")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR lift2A`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b |> Async.retn
            let f' a b = (a, b) ||> ReaderAR.lift2A f

            (10, 11)
            ||> f'
            |> ReaderAR.run "env"
            |> (=) (Ok "env-10-11")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR unfold`` () =
            let mutable i = 0

            let iter = Reader <| fun env ->
                i <- i + 1
                let r = if i < 3 then Ok (env + 1) else Error "my error"
                Async.retn r

            0
            |> ReaderAR.unfold iter
            |> List.ofSeq
            |> (=) [1; 1]
            |> Assert.True

        [<Fact>]
        let ``ReaderAR .>>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            .>>. f
            |> ReaderAR.run "env"
            |> (=) (Ok ("abcde", 5))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR .>>`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            .>> f
            |> ReaderAR.run "env"
            |> (=) (Ok "abcde")
            |> Assert.True

        [<Fact>]
        let ``ReaderAR >>.`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn

            "abcde"
            |> ReaderAR.retn
            >>. f
            |> ReaderAR.run "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``ReaderAR />.`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn
            let a = ("abcd", 10.) |> ReaderAR.retn

            a
            />> f
            |> ReaderAR.run "env"
            |> (=) (Ok (4, 10.))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR >>/`` () =
            let f = fun (s: string) -> s.Length |> ReaderAR.retn
            let a = (10., "abcd") |> ReaderAR.retn

            a
            >>/ f
            |> ReaderAR.run "env"
            |> (=) (Ok (10., 4))
            |> Assert.True

        [<Fact>]
        let ``ReaderAR ++`` () =
            let a = "abc" |> ReaderAR.retn
            let b = 3     |> ReaderAR.retn

            a ++ b
            |> ReaderAR.run "env"
            |> (=) (Ok ("abc", 3))
            |> Assert.True