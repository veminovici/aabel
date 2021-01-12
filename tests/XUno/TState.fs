namespace Simplee.Tests

module TState =

    open Simplee
    open Simplee.State.Operators
    open Simplee.State.ComputationExpression
    open Simplee.State.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``State run`` () =
            let s0 = "abc"
            let flow = State <| fun (s: string) -> s.Length, s + "d"

            let (l, s) = State.run s0 flow 

            l
            |> (=) s0.Length
            |> Assert.True

            s
            |> (=) (s0 + "d")
            |> Assert.True

        [<Fact>]
        let ``State eval`` () =
            let s0 = "abc"
            let flow = State <| fun (s: string) -> s.Length, s + "d"

            flow
            |> State.eval s0 
            |> (=) s0.Length
            |> Assert.True

        [<Fact>]
        let ``State exec`` () =
            let s0 = "abc"

            fun (s: string) -> s.Length, s + "d"
            |> State
            |> State.exec s0 
            |> (=) (s0 + "d")
            |> Assert.True

        [<Fact>]
        let ``State return`` () =
            4
            |> State.retn
            |> State.eval 10.
            |> (=) 4
            |> Assert.True

        [<Fact>]
        let ``State map`` () =
            4
            |> State.retn
            |> State.map (fun x -> float x * 2.) 
            |> State.eval 10.
            |> (=) 8.
            |> Assert.True

        [<Fact>]
        let ``State map2`` () =
            let x = State.retn "abc"
            let y = State.retn "def"
            let f (x: string) (y: string) = x + y 
            
            State.map2 f x y 
            |> State.eval 10.
            |> (=) "abcdef"
            |> Assert.True

        [<Fact>]
        let ``State zip`` () =
            let x = State.retn "abc"
            let y = State.retn 100
            
            State.zip x y 
            |> State.eval 10.
            |> (=) ("abc", 100)
            |> Assert.True

        [<Fact>]
        let ``State apply`` () =
            let x =  State.retn "abc"
            let fn = State.retn (fun (x: string) -> x.Length) 

            x
            |> State.apply fn
            |> State.eval 10.
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State bind`` () =
            let x =  State.retn "abc"
            let fn = fun (s: string) -> State.retn (s.Length)

            x
            |> State.bind fn
            |> State.eval 10.
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State combine`` () =
            let x = State.retn "abc"
            let y = State.retn 100
            
            State.combine x y 
            |> State.eval 10.
            |> (=) 100
            |> Assert.True

        [<Fact>]
        let ``State kleisli`` () =
            let fx = fun (x: string) -> State.retn x.Length
            let fy = sprintf "%d" >> State.retn
            
            fy
            |> State.kleisi fx
            |> fun f -> f "abc"
            |> State.eval 10.
            |> (=) "3"
            |> Assert.True

        [<Fact>]
        let ``State <!>`` () =
            4
            |> State.retn
            <!> (fun x -> float x * 2.) 
            |> State.eval 10.
            |> (=) 8.
            |> Assert.True

        [<Fact>]
        let ``State <*>`` () =
            let x =  State.retn "abc"
            let fn = State.retn (fun (x: string) -> x.Length) 

            fn
            <*> x
            |> State.eval 10.
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State >>=`` () =
            let x =  State.retn "abc"
            let fn = fun (s: string) -> State.retn (s.Length)

            x
            >>= fn
            |> State.eval 10.
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State CE return`` () =

            state {
                return 10
            }
            |> State.eval "abc"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``State CE returnFrom`` () =
            let flow = State <| fun (env: string) -> env.Length, env

            state {
                return! flow
            }

            |> State.eval "abc"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State CE yield`` () =

            state {
                yield 10
            }
            |> State.eval "abc"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``State CE yieldFrom`` () =
            let flow = State <| fun (env: string) -> env.Length, env

            state {
                yield! flow
            }
            |> State.eval "abc"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State CE zero`` () =
            state {
                let! x = State.retn 10
                if x = 20 then return ()
            }
            |> State.eval "abc"
            |> (=) ()
            |> Assert.True

        [<Fact>]
        let ``State CE bind`` () =
            let flow = State <| fun (env: string) -> env.Length, env

            state {
                let! x = flow
                return x + 1
            }
            |> State.eval "abc"
            |> (=) 4
            |> Assert.True

        [<Fact>]
        let ``State CE while`` () =
            let mutable i = 1
            let test() = i < 5
            let inc() = i <- i + 1

            state {
                while test() do
                    inc()
            }
            |> State.eval "abc"
            |> (=) ()
            |> Assert.True

            i
            |> (=) 5
            |> Assert.True

        [<Fact>]
        let ``State CE trywith`` () =
            state {
                try
                    failwith "error"
                    return 0
                with
                | e -> return 10
            }
            |> State.eval "abc"
            |> (=) 10
            |> Assert.True

        [<Fact>]
        let ``State CE tryfinally`` () =
            state {
                let mutable x = 0
                try
                    x <- x + 1
                finally
                    x <- x + 2

                return x
            }
            |> State.eval "abc"
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``State CE using`` () =

            let makeResource name = 
                { new System.IDisposable with
                member _.Dispose() = () }

            state {
                use x = makeResource "hello"
                return 1
            }
            |> State.eval "abc"
            |> (=) 1
            |> Assert.True

        [<Fact>]
        let ``State CE source result`` () =
            state {
                let! x = Ok 10
                return x
            }
            |> State.eval "abc"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``State CE source choice`` () =
            state {
                let! x = Choice1Of2 10
                return x
            }
            |> State.eval "abc"
            |> (=) (Ok 10)
            |> Assert.True

        [<Fact>]
        let ``State CE MergeSources`` () =
            state {
                let! x = State.retn 10
                and! y = State.retn 20
                return x + y
            }
            |> State.eval "abc"
            |> (=) 30
            |> Assert.True

        [<Fact>]
        let ``State CE for`` () =
            let mutable x = 0
            state {
                for i in [1; 2; 3] do
                    x <- x + i

                return x
            }
            |> State.eval "abc"
            |> (=) 6
            |> Assert.True

        [<Fact>]
        let ``State sequenceAsync`` () =
            state {
                async {return 10 }
            }
            |> State.sequenceAsync
            |> Async.RunSynchronously
            |> State.eval "env"
            |> (=) 10 
            |> Assert.True

        [<Fact>]
        let ``State traverse`` () =
            let f = fun (x: string) -> State.retn x.Length

            ["ab"; "abc";"a"]
            |> State.traverseM f
            |> State.eval 10.
            |> (=) [2;3;1]
            |> Assert.True

        [<Fact>]
        let ``State sequence`` () =
            ["ab"; "abc";"a"]
            |> List.map State.retn
            |> State.sequenceM
            |> State.eval 10.
            |> (=) ["ab"; "abc";"a"]
            |> Assert.True

        [<Fact>]
        let ``State concat`` () =
            let x = State.retn [1; 2]
            let y = State.retn [3; 4]

            (x, y)
            ||> State.concat
            |>  State.eval "env"
            |> (=) [1..4]
            |> Assert.True

        [<Fact>]
        let ``State get`` () =
            State.get
            |>  State.eval "env"
            |> (=) "env"
            |> Assert.True

        [<Fact>]
        let ``State put`` () =
            State.put "abc"
            |> State.bind (fun _ -> State.get)
            |>  State.eval "env"
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``State lift1`` () =
            let f e (a: 'a) = sprintf "%s-%A" e a
            let f' a = a |> State.lift1 f

            10
            |> f'
            |> State.eval "env"
            |> (=) "env-10"
            |> Assert.True

        [<Fact>]
        let ``State lift2`` () =
            let f e (a: 'a) (b: 'b) = sprintf "%s-%A-%A" e a b
            let f' a b = (a, b) ||> State.lift2 f

            (10, 11)
            ||> f'
            |> State.eval "env"
            |> (=) "env-10-11"
            |> Assert.True

        [<Fact>]
        let ``State liftE`` () =

            let stt x = State <| fun s ->
                sprintf "%A-%A" s x, s

            1
            |> stt
            |> State.liftE fst
            |> State.eval ("env", 10)
            |> (=) "\"env\"-1"
            |> Assert.True

            1
            |> stt
            |> State.liftE snd
            |> State.eval ("env", 10)
            |> (=) "10-1"
            |> Assert.True

        [<Fact>]
        let ``State unfold`` () =
            let iter = State <| fun stt ->
                stt, stt + 1

            0
            |> State.unfold iter
            |> Seq.take 3
            |> List.ofSeq
            |> (=) [0; 1; 2]
            |> Assert.True

