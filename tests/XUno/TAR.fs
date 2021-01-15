namespace Simplee.Tests

module TAR =

    open Simplee
    open Simplee.AR.ComputationExpression
    open Simplee.AR.Operators
    open Simplee.AR.Traversals

    open System.Threading.Tasks

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``AR retn`` () =

            "abc"
            |> AR.retn
            |> Async.RunSynchronously
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``AR singleton`` () =

            "abc"
            |> AR.singleton
            |> Async.RunSynchronously
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``AR error`` () =

            "e"
            |> AR.err
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``AR map`` () =

            let f (s: string) = s.Length

            "abc"
            |> AR.retn
            |> AR.map f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR mapError`` () =

            let f (s: string) = s.Length

            "e"
            |> AR.err
            |> AR.mapError f
            |> Async.RunSynchronously
            |> (=) (Error 1)
            |> Assert.True

        [<Fact>]
        let ``AR bind`` () =

            let f (s: string) = AR.retn s.Length

            "abc"
            |> AR.retn
            |> AR.bind f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR bind error`` () =

            let f (s: string) = AR.retn s.Length

            "e"
            |> AR.err
            |> AR.bind f
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``AR bind chain`` () =

            let f i = 
                if i < 3 
                then i |> (+) 1 |> AR.retn
                else i |> sprintf "e%d" |> AR.err

            1
            |> AR.retn
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> Async.RunSynchronously
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``AR bindLR`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            |> AR.bindLR f
            |> Async.RunSynchronously
            |> (=) (Ok ("abc", 3))
            |> Assert.True

        [<Fact>]
        let ``Async bindL`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            |> AR.bindL f
            |> Async.RunSynchronously
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Async bindR`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            |> AR.bindR f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR apply`` () =

            let f (s: string) = s.Length

            "abc"
            |> AR.retn
            |> AR.apply (AR.retn f)
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR fold OK`` () =
            let fnOk  (i: int)    = sprintf "%d" i
            let fnErr (e: string) = e

            1
            |> AR.retn
            |> AR.fold fnOk fnErr
            |> Async.RunSynchronously
            |> (=) "1"
            |> Assert.True

        [<Fact>]
        let ``AR fold OK`` () =
            let fnOk  (i: int)    = sprintf "%d" i
            let fnErr (e: string) = e

            "e"
            |> AR.err
            |> AR.fold fnOk fnErr
            |> Async.RunSynchronously
            |> (=) "e"
            |> Assert.True

        [<Fact>]
        let ``AR map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> AR.retn
            let y = "cd" |> AR.retn

            (x, y)
            ||> AR.map2 f
            |> Async.RunSynchronously
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``AR zip`` () =

            let x = "ab" |> AR.retn
            let y = "cd" |> AR.retn

            (x, y)
            ||> AR.zip
            |> Async.RunSynchronously
            |> (=) (Ok ("ab", "cd"))
            |> Assert.True

        [<Fact>]
        let ``AR map3`` () =
            let f a b c = a + b + c

            let a = 1 |> AR.retn
            let b = 2 |> AR.retn
            let c = 3 |> AR.retn

            (a, b, c)
            |||> AR.map3 f
            |> Async.RunSynchronously
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``AR ignore`` () =

            "ab"
            |> AR.retn
            |> AR.ignore
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR ofChoice ok`` () =
            1
            |> Choice1Of2
            |> AR.ofChoice
            |> Async.RunSynchronously
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``AR ofChoice err`` () =
            1
            |> Choice2Of2
            |> AR.ofChoice
            |> Async.RunSynchronously
            |> (=) (Error 1)
            |> Assert.True

        [<Fact>]
        let ``AR ofTask`` () =

            Task.FromResult("ab")
            |> AR.ofTask
            |> Async.RunSynchronously
            |> (=) (Ok "ab")
            |> Assert.True

        [<Fact>]
        let ``AR ofAction`` () =
            use t = Task.Factory.StartNew(fun _ -> ())
            
            t 
            |> AR.ofAction 
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireTrue`` () =

            true
            |> Async.retn
            |> AR.requireTrue "e"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireFalse`` () =

            false
            |> Async.retn
            |> AR.requireFalse "e"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireSome`` () =

            1
            |> Some
            |> Async.retn
            |> AR.requireSome "e"
            |> Async.RunSynchronously
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``AR requireNone`` () =

            None
            |> Async.retn
            |> AR.requireNone "e"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireNotNull`` () =

            box 1
            |> Async.retn
            |> AR.requireNotNull "e"
            |> Async.RunSynchronously
            |> (=) (Ok (box 1))
            |> Assert.True

        [<Fact>]
        let ``AR requireEmpty`` () =
            []
            |> Async.retn
            |> AR.requireEmpty "e"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireNotEmpty`` () =
            ["a"]
            |> Async.retn
            |> AR.requireNotEmpty "e"
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR requireWithError`` () =
            1
            |> AR.err
            |> AR.withError "e"
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``AR tee`` () =
            let mutable a = 0

            1
            |> AR.retn
            |> AR.tee (fun _ -> a <- 10)
            |> Async.RunSynchronously

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``AR teeError`` () =
            let mutable a = 0

            "e"
            |> AR.err
            |> AR.teeError (fun _ -> a <- 10)
            |> Async.RunSynchronously

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``AR teeIf`` () =
            let mutable a = 0

            1
            |> AR.retn
            |> AR.teeIf (fun i -> i = 1) (fun _ -> a <- 10)
            |> Async.RunSynchronously

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``AR teeErrorIf`` () =
            let mutable a = 0

            "e"
            |> AR.err
            |> AR.teeErrorIf (fun i -> i = "e") (fun _ -> a <- 10)
            |> Async.RunSynchronously

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``AR <!>`` () =

            let f (s: string) = s.Length

            "abc"
            |> AR.retn
            <!> f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR <*>`` () =

            let f (s: string) = s.Length

            f
            |> AR.retn
            <*> (AR.retn "abc")
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR >>=`` () =

            let f (s: string) = AR.retn s.Length

            "abc"
            |> AR.retn
            >>= f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR >>= error`` () =

            let f (s: string) = AR.retn s.Length

            "e"
            |> AR.err
            >>= f
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``AR >>= chain`` () =

            let f i = 
                if i < 3 
                then i |> (+) 1 |> AR.retn
                else i |> sprintf "e%d" |> AR.err

            1
            |> AR.retn
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> AR.bind f
            |> Async.RunSynchronously
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``AR .>>.`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            .>>. f
            |> Async.RunSynchronously
            |> (=) (Ok ("abc", 3))
            |> Assert.True

        [<Fact>]
        let ``Async .>>`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            .>> f
            |> Async.RunSynchronously
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Async >>.`` () =
            let f (s: string) = s.Length |> AR.retn

            "abc"
            |> AR.retn
            >>. f
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``Async ++`` () =
            let a = "abc" |> AR.retn
            let b = 3     |> AR.retn

            a ++ b
            |> Async.RunSynchronously
            |> (=) (Ok ("abc", 3))
            |> Assert.True

        [<Fact>]
        let ``AR CE return`` () =
            _ar {
                return 10
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR CE returnFrom`` () =
            _ar {
                return! AR.retn 10
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR CE zero`` () =
            _ar {
                let! x = AR.retn 10
                if x = 20 then return ()
            }
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR CE zero1`` () =
            _ar {
                let! x = AR.retn 10
                if x = 10 then return ()
            }
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``AR CE bind`` () =
            _ar {
                let! x = AR.retn 10
                let! y = AR.retn 20
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE bind error`` () =
            _ar {
                let! x = AR.err "e"
                let! y = AR.retn 10
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``AR CE bind chain`` () =
            _ar {
                let! x1 = AR.retn 1
                let! x2 = AR.retn 2
                let! x3 = AR.retn 3
                let! x4 = AR.err "e4"
                let! x5 = AR.retn 5
                let! x6 = AR.retn 6
                return x1 + x2 + x3 + x4 + x5 + x6
            }
            |> Async.RunSynchronously
            |> (=) (Error "e4")
            |> Assert.True

        [<Fact>]
        let ``AR CE yield`` () =
            _ar {
                yield 10
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR CE yieldFrom`` () =
            _ar {
                yield! (AR.retn 10)
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR CE source result`` () =
            _ar {
                let! x = Ok 10
                let! y = AR.retn 20
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE source choice`` () =
            _ar {
                let! x = Choice1Of2 10
                let! y = AR.retn 20
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE trywith`` () =
            _ar {
                try
                    failwith "e"
                    return 0
                with
                | e -> return 10
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR CE while`` () =
            let mutable i = 1
            let test() = i < 5
            let inc() = i <- i + 1

            _ar {
                while test() do
                    inc()
            }
            |> Async.RunSynchronously
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``AR CE tryfinally`` () =
            _ar {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> Async.RunSynchronously
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AR CE using`` () =

            let makeResource name =
                { 
                new System.IDisposable with
                member _.Dispose() = () 
                } 
                |> AR.retn

            _ar {
                use! x = makeResource "hello"
                return 1
            }
            |> Async.RunSynchronously
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``AR CE for`` () =
            let mutable x = 0
            _ar {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> Async.RunSynchronously
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``AR CE source task`` () =
            _ar {
                let! x = Task.FromResult(10)
                let! y = AR.retn 20
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE source action`` () =
            use t = Task.Factory.StartNew(fun _ -> ())

            _ar {
                let! x = t
                let! y = AR.retn 20
                let  z = if x = () then 10 + y else 10
                return z
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE MergeSources`` () =
            _ar {
                let! x = AR.retn 10
                and! y = AR.retn 20
                return x + y
            }
            |> Async.RunSynchronously
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``AR CE`` () =
            _ar {
                let! x = AR.retn 10
                match x with
                | 10 -> return 10
                | _  -> return 20
            }
            |> Async.RunSynchronously
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``AR sequenceM`` () =
            let x1 = AR.retn 1
            let x2 = AR.retn 2
            let x3 = AR.retn 3
            let x4 = AR.retn 4
            let x5 = AR.retn 5

            [x1; x2; x3; x4; x5]
            |> AR.sequenceM
            |> Async.RunSynchronously
            |> (=)  (Ok [1; 2; 3; 4; 5])

        [<Fact>]
        let ``AR sequenceM with error`` () =
            let x1 = AR.retn 1
            let x2 = AR.err "e2"
            let x3 = AR.retn 3
            let x4 = AR.err "e4"
            let x5 = AR.retn 5

            [x1; x2; x3; x4; x5]
            |> AR.sequenceM
            |> Async.RunSynchronously
            |> (=)  (Error "e2")

        [<Fact>]
        let ``AR sequenceA`` () =
            let x = AR.retn "ab"
            let y = AR.retn "cd"
            let z = AR.retn "ef"

            [x; y]
            |> AR.sequenceA
            |> Async.RunSynchronously
            |> (=)  (Ok ["ab"; "cd"; "ef"])

        [<Fact>]
        let ``AR sequenceA with error`` () =
            let x = AR.retn "ab"
            let y = AR.err "e"
            let z = AR.err "e1"

            [x; y; z]
            |> AR.sequenceA
            |> Async.RunSynchronously
            |> (=)  (Error ["e"; "e1"])

        [<Fact>]
        let ``AR sequenceA with error 1`` () =
            let x = AR.retn "ab"
            let y = AR.err "e"
            let z = AR.err "e1"

            [y; x; z]
            |> AR.sequenceA
            |> Async.RunSynchronously
            |> (=)  (Error ["e"; "e1"])

        [<Fact>]
        let ``AR concat ok ok`` () =
            let x = AR.retn [1; 2]
            let y = AR.retn [3; 4]

            (x, y)
            ||> AR.concat
            |> Async.RunSynchronously
            |> (=)  (Ok [1..4])

        [<Fact>]
        let ``AR concat ok err`` () =
            let x = AR.retn [1; 2]
            let y = AR.err "ey"

            (x, y)
            ||> AR.concat
            |> Async.RunSynchronously
            |> (=)  (Error "ey")

        [<Fact>]
        let ``AR concat err ok`` () =
            let x = AR.err "ex"
            let y = AR.retn [3; 4]

            (x, y)
            ||> AR.concat
            |> Async.RunSynchronously
            |> (=)  (Error "ex")

        [<Fact>]
        let ``AR concat err err`` () =
            let x = AR.err "ex"
            let y = AR.err "ey"

            (x, y)
            ||> AR.concat
            |> Async.RunSynchronously
            |> (=)  (Error "ex")
