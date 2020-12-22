namespace Simplee.Tests

module TAsync =

    open Simplee
    open Simplee.Async.Operators

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

            let f (s: string) = s.Length |> Async.singleton

            "abc"
            |> Async.singleton
            |> Async.bind f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async apply`` () =

            let f (s: string) = s.Length

            f
            |> Async.singleton
            |> Async.apply (Async.singleton "abc")
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async map`` () =

            let f (s: string) = s.Length

            "abc"
            |> Async.singleton
            |> Async.map f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async map2`` () =

            let f (x: string) (y: string) = x + y

            Async.map2 f (Async.singleton "ab") (Async.singleton "cd")
            |> Async.RunSynchronously
            |> (=) "abcd"
            |> Assert.True

        [<Fact>]
        let ``Async zip`` () =

            Async.zip (Async.singleton "ab") (Async.singleton 2)
            |> Async.RunSynchronously
            |> (=) ("ab", 2)
            |> Assert.True

        [<Fact>]
        let ``Async map3`` () =

            let f (x: string) (y: string) (z: string) = x + y + z

            Async.map3 f (Async.singleton "ab") (Async.singleton "cd") (Async.singleton "ef")
            |> Async.RunSynchronously
            |> (=) "abcdef"
            |> Assert.True

        [<Fact>]
        let ``Async <!>`` () =

            let f (s: string) = s.Length

            "abc"
            |> Async.singleton
            <!> f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async <*>`` () =

            let f (s: string) = s.Length

            f
            |> Async.singleton
            <*> (Async.singleton "abc")
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async >>=`` () =

            let f (s: string) = s.Length |> Async.singleton

            "abc"
            |> Async.singleton
            >>= f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True
