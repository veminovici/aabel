namespace Simplee.Tests

module TStateR =

    open Simplee
    open Simplee.StateR.Operators
    open Simplee.StateR.ComputationExpression
    open Simplee.StateR.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``StateR run`` () =
            3
            |> StateR.retn
            |> StateR.eval "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``StateR exec`` () =
            3
            |> StateR.retn
            |> StateR.exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``StateR retn`` () =
            10
            |> StateR.retn
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``State singleton`` () =
            10
            |> StateR.singleton
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR map`` () =
            let fn (s: string) = s.Length

            "abcde"
            |> StateR.retn
            |> StateR.map  fn
            |> StateR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR mapError`` () =
            let fn (s: string) = sprintf "%s1" s

            "abcde"
            |> StateR.err
            |> StateR.mapError  fn
            |> StateR.eval "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``StateR mapEither ok`` () =
            let fnOk (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> StateR.retn
            |> StateR.mapEither fnOk fnErr
            |> StateR.eval  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR mapEither err`` () =
            let fnOk (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> StateR.err
            |> StateR.mapEither fnOk fnErr
            |> StateR.eval  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``StateR apply`` () =
            let f = StateR.retn <| fun (s: string) -> s.Length

            "abcde"
            |> StateR.retn
            |> StateR.apply f
            |> StateR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR bind`` () =
            let f = fun (s: string) -> s.Length |> StateR.retn

            "abcde"
            |> StateR.retn
            |> StateR.bind f
            |> StateR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR bind chain`` () =
            let f i = i |> (+) 1 |> StateR.retn

            1
            |> StateR.retn
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR bind chain err`` () =
            let f i = 
                if i < 3 
                then i |> (+) 1 |> StateR.retn
                else i |> sprintf "e%d" |> StateR.err

            1
            |> StateR.retn
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.bind f
            |> StateR.eval "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``StateR map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> StateR.retn
            let y = "cd" |> StateR.retn

            (x, y)
            ||> StateR.map2 f
            |> StateR.eval "env"
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``StateR zip`` () =
            let x = "ab" |> StateR.retn
            let y = "cd" |> StateR.retn
            
            (x, y)
            ||> StateR.zip
            |> StateR.eval "env"
            |> (=) (Ok ("ab", "cd"))
            |> Assert.True

        [<Fact>]
        let ``StateR map3`` () =
            let x = StateR.retn 1
            let y = StateR.retn 2
            let z = StateR.retn 3
            let f x y z = x + y + z

            (x, y, z)
            |||> StateR.map3 f
            |>   StateR.eval "env"
            |>   (=) (Ok 6)
            |>   Assert.True

        [<Fact>]
        let ``StateR ignore`` () =
            1
            |> StateR.retn
            |> StateR.ignore
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireTrue`` () =
            true
            |> State.retn
            |> StateR.requireTrue "e"
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireFalse`` () =
            false
            |> State.retn
            |> StateR.requireFalse "e"
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireSome`` () =
            1
            |> Some
            |> State.retn
            |> StateR.requireSome "e"
            |> StateR.eval "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``StateR requireNone`` () =
            None
            |> State.retn
            |> StateR.requireNone "e"
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireNotNull`` () =
            box 1
            |> State.retn
            |> StateR.requireNotNull "e"
            |> StateR.eval "env"
            |> (=) (Ok (box 1))
            |> Assert.True

        [<Fact>]
        let ``StateR requireEmpty`` () =
            []
            |> State.retn
            |> StateR.requireEmpty "e"
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireNotEmpty`` () =
            ["a"; "b"]
            |> State.retn
            |> StateR.requireNotEmpty "e"
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR withError`` () =
            1
            |> StateR.retn
            |> StateR.withError "e"
            |> StateR.eval "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``StateR withError err`` () =
            1
            |> StateR.err
            |> StateR.withError "e"
            |> StateR.eval "env"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``StateR teeIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> StateR.retn
            |> StateR.teeIf predicate fn
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``StateR teeErrorIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> StateR.err
            |> StateR.teeErrorIf predicate fn
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``StateR tee`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> StateR.retn
            |> StateR.tee fn
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateR teeError`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> StateR.err
            |> StateR.teeError fn
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateR <!>`` () =
            let f (s: string) = s.Length

            "abcde"
            |> StateR.retn
            <!> f
            |> State.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateR <*>`` () =
            let f (s: string) = s.Length

            f
            |> StateR.retn
            <*> (StateR.retn "abcde")
            |> StateR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``State >>=`` () =
            let f i = i |> (+) 1 |> StateR.retn

            1
            |> StateR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> State.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``State >>= err`` () =
            let f i = 
                if i < 3
                then i |> (+) 1 |> StateR.retn
                else i |> sprintf "e%d" |> StateR.err

            1
            |> StateR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> State.eval "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``StateR CE return`` () =

            stateR {
                return 10
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE returnFrom`` () =

            stateR {
                return! (StateR.retn 10)
            }
            |> State.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE yield`` () =

            stateR {
                yield 10
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE yieldFrom`` () =

            stateR {
                yield! (StateR.retn 10)
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE zero`` () =
            stateR {
                let! x = StateR.retn 10
                if x = 20 then return ()
            }
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR CE bind`` () =

            stateR {
                let! x = StateR.retn 1
                let! y = StateR.retn 2
                let! z = StateR.retn 3
                return x + y + z
            }
            |> StateR.eval "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``StateR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            stateR {
                while test() do
                    inc()
            }
            |> StateR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateR CE trywith`` () =
            stateR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE tryfinally`` () =
            stateR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> StateR.eval "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``StateR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> StateR.retn

            stateR {
                use! x = makeResource "hello"
                return 10
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE source result`` () =
            stateR {
                let! x = Ok 10
                return x
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE source choice`` () =
            stateR {
                let! x = Choice1Of2 10
                return x
            }
            |> StateR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateR CE MergeSources`` () =
            stateR {
                let! x = StateR.retn 10
                and! y = StateR.retn 20
                return x + y
            }
            |> StateR.eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``StateR CE for loop`` () =
            let mutable x = 0

            stateR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> StateR.eval "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``StateR traverseM`` () =
            let f (x: string) = StateR.retn x.Length

            ["ab"; "abc";"a"]
            |> StateR.traverseM f
            |> StateR.eval "env"
            |> (=) (Ok [2; 3; 1])
            |> Assert.True

        [<Fact>]
        let ``StateR sequenceM`` () =
            ["ab"; "abc"; "a"]
            |> List.map StateR.retn
            |> StateR.sequenceM
            |> StateR.eval "env"
            |> (=) (Ok ["ab"; "abc";"a"])
            |> Assert.True

        [<Fact>]
        let ``StateR traverseA ok`` () =
            let f (i: int) = StateR.retn (i * 2)

            [1..5]
            |> StateR.traverseA f
            |> StateR.eval "env"
            |> (=) (Ok [2; 4; 6; 8; 10])
            |> Assert.True

        [<Fact>]
        let ``StateR traverseA error`` () =
            let f (i: int) = 
                if i % 2 = 1 
                then StateR.retn (i * 2) 
                else i |> sprintf "e%i" |> StateR.err

            [1..5]
            |> StateR.traverseA f
            |> StateR.eval "env"
            |> (=) (Error ["e2"; "e4"])
            |> Assert.True

        [<Fact>]
        let ``StateR sequenceA`` () =
            [1..5]
            |> List.map StateR.retn
            |> StateR.sequenceA
            |> StateR.eval "env"
            |> (=) (Ok [1..5])
            |> Assert.True

        [<Fact>]
        let ``StateR concat`` () =
            let x = StateR.retn [1; 2]
            let y = StateR.retn [3; 4]

            (x, y)
            ||> StateR.concat
            |>  StateR.eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True


