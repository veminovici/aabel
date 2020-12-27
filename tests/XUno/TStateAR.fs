namespace Simplee.Tests

module TStateAR =

    open Simplee
    open Simplee.StateAR.Operators
    open Simplee.StateAR.ComputationExpression
    open Simplee.StateAR.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``StateA exec`` () =

            "abcde"
            |> StateAR.retn
            |> StateAR.exec "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``StateAR retn`` () =
            10
            |> StateAR.retn
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR singleton`` () =
            10
            |> StateAR.singleton
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR map`` () =
            let fn (s: string) = s.Length

            "abcde"
            |> StateAR.retn
            |> StateAR.map  fn
            |> StateAR.eval  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR mapError`` () =
            let fn (s: string) = sprintf "%s1" s

            "abcde"
            |> StateAR.err
            |> StateAR.mapError fn
            |> StateAR.eval  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``StateAR mapEither ok`` () =
            let fnOk  (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> StateAR.retn
            |> StateAR.mapEither fnOk fnErr
            |> StateAR.eval  "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR mapEither err`` () =
            let fnOk  (s: string) = s.Length
            let fnErr (s: string) = sprintf "%s1" s

            "abcde"
            |> StateAR.err
            |> StateAR.mapEither fnOk fnErr
            |> StateAR.eval  "env"
            |> (=) (Error "abcde1")
            |> Assert.True

        [<Fact>]
        let ``StateAR apply`` () =
            let f = StateAR.retn <| fun (s: string) -> s.Length

            "abcde"
            |> StateAR.retn
            |> StateAR.apply f
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR bind`` () =
            let f = fun (s: string) -> s.Length |> StateAR.retn

            "abcde"
            |> StateAR.retn
            |> StateAR.bind f
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR bind chain`` () =
            let f i = i |> (+) 1 |> StateAR.retn

            1
            |> StateAR.retn
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR bind chain err`` () =
            let f i = 
                if i < 3 
                then i |> (+) 1 |> StateAR.retn
                else i |> sprintf "e%d" |> StateAR.err

            1
            |> StateAR.retn
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.bind f
            |> StateAR.eval "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``StateAR map2`` () =
            let f (a: string) (b: string) = a + b
            let x = "ab" |> StateAR.retn
            let y = "cd" |> StateAR.retn

            (x, y)
            ||> StateAR.map2 f
            |> StateAR.eval "env"
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``StateAR zip`` () =
            let x = "ab" |> StateAR.retn
            let y = "cd" |> StateAR.retn
            
            (x, y)
            ||> StateAR.zip
            |>  StateAR.eval "env"
            |> (=) (Ok ("ab", "cd"))
            |> Assert.True

        [<Fact>]
        let ``StateAR map3`` () =
            let x = StateAR.retn 1
            let y = StateAR.retn 2
            let z = StateAR.retn 3
            let f x y z = x + y + z

            (x, y, z)
            |||> StateAR.map3 f
            |>   StateAR.eval "env"
            |>   (=) (Ok 6)
            |>   Assert.True

        [<Fact>]
        let ``StateAR ignore`` () =
            1
            |> StateAR.retn
            |> StateAR.ignore
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR requireTrue`` () =
            true
            |> StateA.retn
            |> StateAR.requireTrue "e"
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR requireFalse`` () =
            false
            |> StateA.retn
            |> StateAR.requireFalse "e"
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR requireSome`` () =
            1
            |> Some
            |> StateA.retn
            |> StateAR.requireSome "e"
            |> StateAR.eval "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``StateAR requireNone`` () =
            None
            |> StateA.retn
            |> StateAR.requireNone "e"
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR requireNotNull`` () =
            box 1
            |> StateA.retn
            |> StateAR.requireNotNull "e"
            |> StateAR.eval "env"
            |> (=) (Ok (box 1))
            |> Assert.True

        [<Fact>]
        let ``StateR requireEmpty`` () =
            []
            |> StateA.retn
            |> StateAR.requireEmpty "e"
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateR requireNotEmpty`` () =
            ["a"; "b"]
            |> StateA.retn
            |> StateAR.requireNotEmpty "e"
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR withError`` () =
            1
            |> StateAR.retn
            |> StateAR.withError "e"
            |> StateAR.eval "env"
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``StateAR withError err`` () =
            1
            |> StateAR.err
            |> StateAR.withError "e"
            |> StateAR.eval "env"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``StateAR teeIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> StateAR.retn
            |> StateAR.teeIf predicate fn
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``StateAR teeErrorIf`` () =

            let mutable a = 0
            let predicate i = i < 3
            let fn i = a <- 1

            1
            |> StateAR.err
            |> StateAR.teeErrorIf predicate fn
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``StateAR tee`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> StateAR.retn
            |> StateAR.tee fn
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateAR teeError`` () =

            let mutable a = 0
            let fn i = a <- 10

            1
            |> StateAR.err
            |> StateAR.teeError fn
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``StateAR <!>`` () =
            let f (s: string) = s.Length

            "abcde"
            |> StateAR.retn
            <!> f
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR <*>`` () =
            let f (s: string) = s.Length

            f
            |> StateAR.retn
            <*> (StateAR.retn "abcde")
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR >>=`` () =
            let f i = i |> (+) 1 |> StateAR.retn

            1
            |> StateAR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> StateAR.eval "env"
            |> (=) (Ok 5)
            |> Assert.True

        [<Fact>]
        let ``StateAR >>= err`` () =
            let f i = 
                if i < 3
                then i |> (+) 1 |> StateAR.retn
                else i |> sprintf "e%d" |> StateAR.err

            1
            |> StateAR.retn
            >>= f
            >>= f
            >>= f
            >>= f
            |> StateAR.eval "env"
            |> (=) (Error "e3")
            |> Assert.True

        [<Fact>]
        let ``StateAR CE return`` () =

            stateAR {
                return 10
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE returnFrom`` () =

            stateAR {
                return! (StateAR.retn 10)
            }
            |> StateA.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE yield`` () =

            stateAR {
                yield 10
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE yieldFrom`` () =

            stateAR {
                yield! (StateAR.retn 10)
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE zero`` () =
            stateAR {
                let! x = StateAR.retn 10
                if x = 20 then return ()
            }
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``StateAR CE bind`` () =

            stateAR {
                let! x = StateAR.retn 1
                let! y = StateAR.retn 2
                let! z = StateAR.retn 3
                return x + y + z
            }
            |> StateAR.eval "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE while`` () =
            let mutable i = 1
            let test   () = i < 5
            let inc    () = i <- i + 1

            stateAR {
                while test() do
                    inc()
            }
            |> StateAR.eval "env"
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``StateAR CE trywith`` () =
            stateAR {
                try
                    failwith "error"
                    return 0
                with
                | _ -> return 10
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE tryfinally`` () =
            stateAR {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> StateAR.eval "env"
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }
                |> StateAR.retn

            stateAR {
                use! x = makeResource "hello"
                return 10
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE source result`` () =
            stateAR {
                let! x = Ok 10
                return x
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE source choice`` () =
            stateAR {
                let! x = Choice1Of2 10
                return x
            }
            |> StateAR.eval "env"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE MergeSources`` () =
            stateAR {
                let! x = StateAR.retn 10
                and! y = StateAR.retn 20
                return x + y
            }
            |> StateAR.eval "env"
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``StateAR CE for loop`` () =
            let mutable x = 0

            stateAR {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> StateAR.eval "env"
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``StateAR traverse`` () =
            let f (x: string) = StateAR.retn x.Length

            ["ab"; "abc";"a"]
            |> StateAR.traverseM f
            |> StateAR.eval "env"
            |> (=) (Ok [2; 3; 1])
            |> Assert.True

        [<Fact>]
        let ``StateAR sequence`` () =
            ["ab"; "abc"; "a"]
            |> List.map StateAR.retn
            |> StateAR.sequenceM
            |> StateAR.eval "env"
            |> (=) (Ok ["ab"; "abc";"a"])
            |> Assert.True

        [<Fact>]
        let ``StateAR traverseA ok`` () =
            let f (i: int) = StateAR.retn (i * 2)

            [1..5]
            |> StateAR.traverseA f
            |> StateAR.eval "env"
            |> (=) (Ok [2; 4; 6; 8; 10])
            |> Assert.True

        [<Fact>]
        let ``StateAR traverseA error`` () =
            let f (i: int) = 
                if i % 2 = 1 
                then StateAR.retn (i * 2) 
                else i |> sprintf "e%i" |> StateAR.err

            [1..5]
            |> StateAR.traverseA f
            |> StateAR.eval "env"
            |> (=) (Error ["e2"; "e4"])
            |> Assert.True

        [<Fact>]
        let ``StateAR sequenceA`` () =
            [1..5]
            |> List.map StateAR.retn
            |> StateAR.sequenceA
            |> StateAR.eval "env"
            |> (=) (Ok [1..5])
            |> Assert.True

        [<Fact>]
        let ``StateAR concat`` () =
            let x = StateAR.retn [1; 2]
            let y = StateAR.retn [3; 4]

            (x, y)
            ||> StateAR.concat
            |>  StateAR.eval "env"
            |> (=) (Ok [1..4])
            |> Assert.True

        [<Fact>]
        let ``StateAR put and get`` () =
            StateAR.put "abc"
            |> StateAR.bind (fun _ -> StateAR.get)
            |>  StateAR.eval "env"
            |> (=) (Ok "abc")
            |> Assert.True

