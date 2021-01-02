namespace Simplee.Tests

module TTimed =

    open Simplee
    open Simplee.Diagnostics
    open Simplee.Diagnostics.Timed.ComputationExpression

    open System

    open Xunit
    open Xunit.Abstractions

    type Tests (output: ITestOutputHelper) =

        [<Fact>]
        let ``Timed make`` () =

            let t = Timed.ofValue 1 in

            t
            |> Timed.result
            |> (=) 1
            |> Assert.True

            t
            |> Timed.elapsed
            |> (=) TimeSpan.Zero
            |> Assert.True

        [<Fact>]
        let ``Timed with`` () =

            let t = 
                1 
                |> Timed.ofValue 
                |> Timed.withResult 2 
                |> Timed.withElapsed (TimeSpan.FromMilliseconds 10.)

            t
            |> Timed.result
            |> (=) 2
            |> Assert.True

            t
            |> Timed.elapsed
            |> (=) (TimeSpan.FromMilliseconds 10.)
            |> Assert.True

        [<Fact>]
        let ``Timed add`` () =

            let t = 
                1 
                |> Timed.ofValue 
                |> Timed.withResult 2 
                |> Timed.addElapsed (TimeSpan.FromMilliseconds 10.)
                |> Timed.addElapsed (TimeSpan.FromMilliseconds 20.)

            t
            |> Timed.result
            |> (=) 2
            |> Assert.True

            t
            |> Timed.elapsed
            |> (=) (TimeSpan.FromMilliseconds 30.)
            |> Assert.True

        [<Fact>]
        let ``Timed CE Return`` () =

            let flow = 
                _timed {
                    return 1
                }

            flow
            |> Timed.result
            |> (=) 1
            |> Assert.True

            flow
            |> Timed.elapsed
            |> (<>) TimeSpan.Zero
            |> Assert.True

        [<Fact>]
        let ``Timed Delay`` () =
            let oneMinute = TimeSpan.FromMinutes 1.
            let f () = Timed.ofValue 5 |> Timed.withElapsed oneMinute

            let tr = _timed.Delay f

            tr
            |> Timed.result
            |> (=) 5
            |> Assert.True

            tr
            |> Timed.elapsed
            |> (<=) oneMinute
            |> Assert.True