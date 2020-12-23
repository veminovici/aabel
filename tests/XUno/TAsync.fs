namespace Simplee.Tests

module TAsync =

    open Simplee
    open Simplee.Async.Operators
    open Simplee.Async.Traversals

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``Async retn`` () =

            "abc"
            |> Async.retn
            |> Async.RunSynchronously
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``Async singleton`` () =

            "abc"
            |> Async.singleton
            |> Async.RunSynchronously
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``Async bind`` () =

            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            |> Async.bind f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async apply`` () =

            let f (s: string) = s.Length

            f
            |> Async.retn
            |> Async.apply (Async.retn "abc")
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async map`` () =

            let f (s: string) = s.Length

            "abc"
            |> Async.retn
            |> Async.map f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async map2`` () =

            let f (x: string) (y: string) = x + y

            Async.map2 f (Async.retn "ab") (Async.retn "cd")
            |> Async.RunSynchronously
            |> (=) "abcd"
            |> Assert.True

        [<Fact>]
        let ``Async zip`` () =

            Async.zip (Async.retn "ab") (Async.retn 2)
            |> Async.RunSynchronously
            |> (=) ("ab", 2)
            |> Assert.True

        [<Fact>]
        let ``Async map3`` () =

            let f (x: string) (y: string) (z: string) = x + y + z

            Async.map3 f (Async.retn "ab") (Async.retn "cd") (Async.retn "ef")
            |> Async.RunSynchronously
            |> (=) "abcdef"
            |> Assert.True

        [<Fact>]
        let ``Async <!>`` () =

            let f (s: string) = s.Length

            "abc"
            |> Async.retn
            <!> f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async <*>`` () =

            let f (s: string) = s.Length

            f
            |> Async.retn
            <*> (Async.retn "abc")
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async >>=`` () =

            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            >>= f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async sequenceM`` () =

            [1; 2; 3]
            |> List.map Async.retn
            |> Async.sequenceM
            |> Async.RunSynchronously
            |> (=) [1; 2; 3]
            |> Assert.True

        [<Fact>]
        let ``Async sequenceA`` () =

            [1; 2; 3]
            |> List.map Async.retn
            |> Async.sequenceA
            |> Async.RunSynchronously
            |> (=) [1; 2; 3]
            |> Assert.True
