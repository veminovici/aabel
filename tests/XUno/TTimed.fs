namespace Simplee.Tests

module TTimed =

    open Simplee
    open Simplee.Diagnostics.Timed
    open Simplee.Diagnostics.Timed.ComputationExpression

    open System

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``Timed ctor`` () =
            let tr = { Result = 1; Elapsed = TimeSpan.Zero }

            tr.Result
            |> (=) 1
            |> Assert.True

            tr.Elapsed
            |> (=) TimeSpan.Zero
            |> Assert.True

        [<Fact>]
        let ``Timed Return`` () =
            let tr = timed.Return 1
            tr.Result
            |> (=) 1
            |> Assert.True

            tr.Elapsed
            |> (=) TimeSpan.Zero
            |> Assert.True

        [<Fact>]
        let ``Timed Delay`` () =
            let oneMinute = TimeSpan.FromMinutes 1.
            let f () = 
                { Result = 5; Elapsed = oneMinute }

            let tr = timed.Delay f

            tr.Result
            |> (=) 5
            |> Assert.True

            tr.Elapsed
            |> (<=) oneMinute
            |> Assert.True
