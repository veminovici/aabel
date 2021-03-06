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
        let ``Async mapFst`` () =
            ("abcde", 10)
            |> Async.retn
            |> Async.mapFst (fun (s: string) -> s.Length)
            |> Async.RunSynchronously
            |> (=) (5, 10)
            |> Assert.True

        [<Fact>]
        let ``Async mapSnd`` () =
            (10, "abcde")
            |> Async.retn
            |> Async.mapSnd (fun (s: string) -> s.Length)
            |> Async.RunSynchronously
            |> (=) (10, 5)
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
        let ``Async bindLR`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            |> Async.bindLR f
            |> Async.RunSynchronously
            |> (=) ("abc", 3)
            |> Assert.True

        [<Fact>]
        let ``Async bindL`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            |> Async.bindL f
            |> Async.RunSynchronously
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``Async bindR`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            |> Async.bindR f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async bindFst`` () =
            let f = fun (s: string) -> s.Length |> Async.retn
            let a = ("abcd", 10.) |> Async.retn

            a
            |> Async.bindFst f
            |> Async.RunSynchronously
            |> (=) (4, 10.)
            |> Assert.True

        [<Fact>]
        let ``Async bindSnd`` () =
            let f = fun (s: string) -> s.Length |> Async.retn
            let a = (10., "abcd") |> Async.retn

            a
            |> Async.bindSnd f
            |> Async.RunSynchronously
            |> (=) (10., 4)
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
        let ``Async </!>`` () =
            ("abcde", 10)
            |> Async.retn
            </!> (fun (s: string) -> s.Length)
            |> Async.RunSynchronously
            |> (=) (5, 10)
            |> Assert.True

        [<Fact>]
        let ``Async <!/>`` () =
            (10, "abcde")
            |> Async.retn
            <!/> (fun (s: string) -> s.Length)
            |> Async.RunSynchronously
            |> (=) (10, 5)
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
        let ``Async .>>.`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            .>>. f
            |> Async.RunSynchronously
            |> (=) ("abc", 3)
            |> Assert.True

        [<Fact>]
        let ``Async .>>`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            .>> f
            |> Async.RunSynchronously
            |> (=) "abc"
            |> Assert.True

        [<Fact>]
        let ``Async >>.`` () =
            let f (s: string) = s.Length |> Async.retn

            "abc"
            |> Async.retn
            >>. f
            |> Async.RunSynchronously
            |> (=) 3
            |> Assert.True

        [<Fact>]
        let ``Async />>`` () =
            let f = fun (s: string) -> s.Length |> Async.retn
            let a = ("abcd", 10.) |> Async.retn

            a
            />> f
            |> Async.RunSynchronously
            |> (=) (4, 10.)
            |> Assert.True

        [<Fact>]
        let ``Async >>/`` () =
            let f = fun (s: string) -> s.Length |> Async.retn
            let a = (10., "abcd") |> Async.retn

            a
            >>/ f
            |> Async.RunSynchronously
            |> (=) (10., 4)
            |> Assert.True

        [<Fact>]
        let ``Async ++`` () =
            let a = "abc" |> Async.retn
            let b = 3     |> Async.retn

            a ++ b
            |> Async.RunSynchronously
            |> (=) ("abc", 3)
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

        [<Fact>]
        let ``Async concat`` () =
            let x = [1; 2] |> Async.retn
            let y = [3; 4] |> Async.retn

            (x, y)
            ||> Async.concat
            |>  Async.RunSynchronously
            |> (=)  [1..4]