namespace Simplee.Tests

module TResult =

    open Simplee
    open Simplee.Result.Operators
    open Simplee.Result.ComputationExpression
    open Simplee.Result.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``Result ok`` () =

            "abc"
            |> Result.ok
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Result singleton`` () =

            "abc"
            |> Result.singleton
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Result err`` () =

            "e"
            |> Result.err
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result IsOk`` () =

            "abc"
            |> Ok
            |> Result.isOk
            |> Assert.True

            "e"
            |> Error
            |> Result.isOk
            |> Assert.False

        [<Fact>]
        let ``Result isError`` () =

            "abc"
            |> Ok
            |> Result.isError
            |> Assert.False

            "e"
            |> Error
            |> Result.isError
            |> Assert.True

        [<Fact>]
        let ``Result either`` () =

            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Ok
            |> Result.either okF errF
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Result either err`` () =
            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Error
            |> Result.either okF errF
            |> (=) 13
            |> Assert.True

        [<Fact>]
        let ``Result fold`` () =

            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Ok
            |> Result.fold okF errF
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Result fold err`` () =

            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Error
            |> Result.fold okF errF
            |> (=) 13
            |> Assert.True

        [<Fact>]
        let ``Result mapEither ok`` () =

            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Ok
            |> Result.mapEither okF errF
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``Result mapEither err`` () =

            let okF  (s: string) = s.Length
            let errF (s: string) = s.Length + 10

            "abc"
            |> Error
            |> Result.mapEither okF errF
            |> (=) (Error 13)
            |> Assert.True

        [<Fact>]
        let ``Result apply`` () =
            let f (s: string) = s.Length

            "abc"
            |> Ok
            |> Result.apply (Ok f)
            |> (=) (Ok 3)
            |> Assert.True

            "abc"
            |> Ok
            |> Result.apply (Error "e")
            |> Result.isError
            |> Assert.True

            "e"
            |> Error
            |> Result.apply (Ok f)
            |> Result.isError
            |> Assert.True

        [<Fact>]
        let ``Result map2`` () =
            let f (x: string) (y: string ) = x + y

            Result.map2 f (Ok "ab") (Ok "cd")
            |> (=) (Ok "abcd")
            |> Assert.True

        [<Fact>]
        let ``Result map3`` () =
            let f (x: string) (y: string ) (z: string) = x + y + z

            Result.map3 f (Ok "ab") (Ok "cd") (Ok "ef")
            |> (=) (Ok "abcdef")
            |> Assert.True

        [<Fact>]
        let ``Result join ok`` () =
            let x = Ok 10

            x
            |> Ok
            |> Result.join
            |> (=) (Ok 10)
            |> Assert.True

            let y = Error "e"

            y
            |> Ok
            |> Result.join
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result join ok`` () =
    
            "e"
            |> Error
            |> Result.join
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result zip`` () =
            Result.zip (Ok "ab") (Ok 2)
            |> (=) (Ok ("ab", 2))
            |> Assert.True

        [<Fact>]
        let ``Result ofChoice`` () =

            "ab"
            |> Choice1Of2
            |> Result.ofChoice
            |> (=) (Ok "ab")
            |> Assert.True

            "ab"
            |> Choice2Of2
            |> Result.ofChoice
            |> (=) (Error "ab")
            |> Assert.True

        [<Fact>]
        let ``Result requireTrue`` () =

            true
            |> Result.requireTrue "e"
            |> (=) (Ok ())
            |> Assert.True

            false
            |> Result.requireTrue "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireFalse`` () =

            false
            |> Result.requireFalse "e"
            |> (=) (Ok ())
            |> Assert.True

            true
            |> Result.requireFalse "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireSome`` () =

            1
            |> Some
            |> Result.requireSome "e"
            |> (=) (Ok 1)
            |> Assert.True

            None
            |> Result.requireSome "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireNone`` () =

            None
            |> Result.requireNone "e"
            |> (=) (Ok ())
            |> Assert.True

            1
            |> Some
            |> Result.requireNone "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireNotNull`` () =

            1
            |> box
            |> Result.requireNotNull "e"
            |> (=) (Ok (box 1))
            |> Assert.True

            null
            |> Result.requireNotNull "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireEmpty`` () =

            []
            |> Result.requireEmpty "e"
            |> (=) (Ok ())
            |> Assert.True

            ["a"]
            |> Result.requireEmpty "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result requireNotEmpty`` () =

            ["a"]
            |> Result.requireNotEmpty "e"
            |> (=) (Ok ())
            |> Assert.True

            []
            |> Result.requireNotEmpty "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result withError`` () =

            1
            |> Ok
            |> Result.withError "e"
            |> (=) (Ok 1)
            |> Assert.True

            1
            |> Error
            |> Result.withError "e"
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result teeIf`` () =

            let mutable a = 0

            1
            |> Ok
            |> Result.teeIf (fun x -> x = 1) (fun x -> a <- x)
            
            a
            |> (=) 1
            |> Assert.True

            10
            |> Ok
            |> Result.teeIf (fun x -> x = 1) (fun x -> a <- x)

            a
            |> (=) 1
            |> Assert.True

            "e"
            |> Error
            |> Result.teeIf (fun x -> x = 1) (fun x -> a <- x)

            a
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``Result teeErrorIf`` () =

            let mutable a = ""

            1
            |> Ok
            |> Result.teeErrorIf (fun x -> x = "e") (fun x -> a <- x)
            
            a
            |> (=) ""
            |> Assert.True

            "e"
            |> Error
            |> Result.teeErrorIf (fun e -> e = "e") (fun x -> a <- x)

            a
            |> (=) "e"
            |> Assert.True

            "ee"
            |> Error
            |> Result.teeErrorIf (fun e -> e = "e") (fun x -> a <- x)

            a
            |> (=) "e"
            |> Assert.True

        [<Fact>]
        let ``Result tee`` () =

            let mutable a = 0

            10
            |> Ok
            |> Result.tee (fun x -> a <- x)

            a
            |> (=) 10
            |> Assert.True

            "e"
            |> Error
            |> Result.tee (fun x -> a <- x)

            a
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``Result teeError`` () =

            let mutable a = ""

            1
            |> Ok
            |> Result.teeError (fun x -> a <- x)
            
            a
            |> (=) ""
            |> Assert.True

            "e"
            |> Error
            |> Result.teeError (fun x -> a <- x)

            a
            |> (=) "e"
            |> Assert.True

        [<Fact>]
        let ``Result sequenceAsync`` () =

            let a = async { return 1 }

            a
            |> Ok
            |> Result.sequenceAsync
            |> Async.RunSynchronously
            |> (=) (Ok 1)
            |> Assert.True

            "e"
            |> Error
            |> Result.sequenceAsync
            |> Async.RunSynchronously
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result <!>`` () =
            let f (s: string) = s.Length

            "abc"
            |> Ok
            <!> f
            |> (=) (Ok 3)
            |> Assert.True

            "e"
            |> Error
            <!> f
            |> Result.isError
            |> Assert.True

        [<Fact>]
        let ``Result <*>`` () =
            let f (s: string) = s.Length

            "abc"
            |> Ok
            <*> (Ok f)
            |> (=) (Ok 3)
            |> Assert.True

            "e"
            |> Error
            <*> (Ok f)
            |> Result.isError
            |> Assert.True

        [<Fact>]
        let ``Result >>=`` () =
            let f (s: string) = Ok s.Length

            "abc"
            |> Ok
            >>= f
            |> (=) (Ok 3)
            |> Assert.True

            "e"
            |> Error
            >>= f
            |> Result.isError
            |> Assert.True

        [<Fact>]
        let ``Result CE return`` () =
            _result { return "abc" }
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Result CE returnFrom`` () =
            _result { return! Ok "abc" }
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Result CE zero`` () =
            let a = Ok "abc"
            let f (s: string) = Ok s.Length

            _result.Zero ()
            |> (=) (Ok ())
            |> Assert.True

        [<Fact>]
        let ``Result CE yield`` () =
            _result { 
                yield "ab"
                yield "cd" }
            |> (=) (Ok "cd")
            |> Assert.True

        [<Fact>]
        let ``Result CE yieldFrom`` () =
            _result { yield! Ok "abc" }
            |> (=) (Ok "abc")
            |> Assert.True

        [<Fact>]
        let ``Result CE combine`` () =
            let a = Ok "abc"
            let f (s: string) = Ok s.Length

            _result.Combine (a, f)
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``Result CE bind`` () =
            _result {
                let! x = Ok 10
                return x
            }
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Result CE bind error`` () =
            _result {
                let! x = Error "e"
                return x + 10
            }
            |> (=) (Error "e")
            |> Assert.True

        [<Fact>]
        let ``Result CE while`` () =
            let mutable i = 1
            let test() = i < 5
            let inc() = i <- i + 1

            _result {
                while test() do
                    inc()
            }
            |> (=) (Ok ())
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``Result CE for loop`` () =
            let mutable x = 0

            _result {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> (=) (Ok 6)
            |> Assert.True

        [<Fact>]
        let ``Result CE trywith`` () =
            _result {
                try
                    failwith "error"
                    return 0
                with
                | e -> return 10
            }
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``Result CE tryfinally`` () =
            _result {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``Result CE using`` () =

            let makeResource name =
                Ok { 
                new System.IDisposable with
                member _.Dispose() = () }

            _result {
                use! x = makeResource "hello"
                return 1
            }
            |> (=) (Ok 1)
            |> Assert.True

        [<Fact>]
        let ``Result CE BindReturn`` () =
            let a = Ok "abc"
            let f (s: string) = s.Length

            _result.BindReturn (a, f)
            |> (=) (Ok 3)
            |> Assert.True

        [<Fact>]
        let ``AReader CE MergeSources`` () =
            _result {
                let! x = Ok 10
                and! y = Ok 20
                return x + y
            }
            |> (=) (Ok 30)
            |> Assert.True

        [<Fact>]
        let ``Result sequenceM`` () =
            let x = Ok "ab"
            let y = Ok "cd"
            let z = Ok "ef"

            [x; y; z]
            |> Result.sequenceM
            |> (=) (Ok ["ab"; "cd"; "ef"])

        [<Fact>]
        let ``Result sequenceM with error`` () =
            let x = Ok "ab"
            let y = Error "e"
            let z = Ok "ef"

            [x; y; z]
            |> Result.sequenceM
            |> (=)  (Error "e")

        [<Fact>]
        let ``Result sequenceA`` () =
            let x = Ok 1
            let y = Ok 2
            let z = Ok 3

            [x; y; z]
            |> Result.sequenceA
            |> (=)  (Ok [1; 2; 3])

        [<Fact>]
        let ``Result sequenceA with error`` () =
            let x = Ok 1
            let y = Error "e"
            let z = Error "e1"

            [x; y; z]
            |> Result.sequenceA
            |> (=)  (Error ["e"; "e1"])

        [<Fact>]
        let ``Result concat ok ok`` () =
            let x = [1; 2] |> Ok
            let y = [3; 4] |> Ok

            (x, y)
            ||> Result.concat
            |> (=)  (Ok [1..4])

        [<Fact>]
        let ``Result concat ok err`` () =
            let x = [1; 2] |> Ok
            let y = "ey" |> Error

            (x, y)
            ||> Result.concat
            |> (=)  (Error "ey")

        [<Fact>]
        let ``Result concat err ok`` () =
            let x = "ex" |> Error
            let y = [3; 4] |> Ok

            (x, y)
            ||> Result.concat
            |> (=)  (Error "ex")

        [<Fact>]
        let ``Result concat err err`` () =
            let x = "ex" |> Error
            let y = "ey" |> Error

            (x, y)
            ||> Result.concat
            |> (=)  (Error "ex")
