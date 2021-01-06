#load "../../src/Aabel/Result.fs"
#load "../../src/Aabel/Async.fs"
#load "../../src/Aabel/AR.fs"
#load "../../src/Aabel/Reader.fs"
#load "../../src/Aabel/ReaderR.fs"

open Simplee
open Simplee.Reader.Operators
open Simplee.Reader.ComputationExpression

//
// The interfaces
//

type IResources =
    abstract GetString: unit -> string

type IOutput =
    abstract Print: string -> unit

//
// Workflow
//

let getWorld = Reader.lift1 (fun (res: IResources) -> res.GetString) ()
let print    = Reader.lift1 (fun (out: IOutput)    -> out.Print)

let flow () = reader {
    let! text = 
        getWorld                // a reader that uses an IResources to obtain a string
        |> Reader.liftE fst     // obtain the IResource from a pair of values by getting the first value
        <!> sprintf "Hello %s"  // map the result of reader.

    do! 
        text
        |> print                // a reader taht uses an IOutput to print and return unit
        |> Reader.liftE snd }   // obtain the IOutput from a pair of values by getting the second value

//
// Run the workflow using spefic implementations
//

let resources = { new IResources with
    member _.GetString() = "World" }

let output = { new IOutput with
    member _.Print s = printfn "%s" s }

()
|> flow 
|> Reader.run (resources, output)

