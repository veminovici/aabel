type InterfaceA =
    abstract MethodA: string -> string
type UsingA<'result> = InterfaceA -> 'result
type ReaderM<'dependency, 'result> = 'dependency -> 'result

let run (d: 'dependency) (r: ReaderM<'dependency, 'result>) : 'result = r d
let constant (c: 'c) : ReaderM<_, 'c> = fun _ -> c
let map (f:'a -> 'b) (r: ReaderM<'d, 'a>) : ReaderM<'d, 'b> = r >> f
let apply (f: ReaderM<'d, 'a -> 'b>) (r: ReaderM<'d, 'a>) : ReaderM<'d, 'b> =
    fun dep ->
        let f' = run dep f
        let a  = run dep r
        f' a
let bind (m: ReaderM<'d, 'a>) (f:'a -> ReaderM<'d, 'b>) : ReaderM<'d, 'b> =
    fun dep -> 
        f (run dep m) |> run dep

let lift1 (f: 'd -> 'a -> 'out) : 'a -> ReaderM<'d, 'out> =
    fun a dep -> f dep a

let lift2 (f: 'd -> 'a -> 'b -> 'out) : 'a -> 'b -> ReaderM<'d, 'out> =
    fun a b dep -> f dep a b

let lift3 (f: 'd -> 'a -> 'b -> 'c -> 'out) : 'a -> 'b -> 'c -> ReaderM<'d, 'out> =
    fun a b c dep -> f dep a b c

let liftDep (proj: 'd2 -> 'd1) (r: ReaderM<'d1, 'out>) : ReaderM<'d2, 'out> =
    proj >> r

type ReaderBuilder internal () =
    member _.Bind(m, f) = bind m f
    member _.Return(v)  = constant v
    member _.ReturnFrom(v) = v
    member _.Delay(f) = f ()

let (<?>) = map
let (<*>) = apply
let (>>=) = bind

let Do = ReaderBuilder ()

type IResources =
    abstract GetString: unit -> string

let resource = { new IResources with
    member _.GetString() = "World" }

type IOutput =
    abstract Print: string -> unit

let output = { new IOutput with
    member _.Print s = printfn "%s" s }

type Dependencies = IResources * IOutput
let config = (resource, output)

let getWorld = lift1 (fun (res: IResources) -> res.GetString) ()
let print    = lift1 (fun (out: IOutput) -> out.Print)

let r () = liftDep fst

let computation () = Do {
    let! text = sprintf "Hello %s" <?> liftDep fst getWorld
    do! liftDep snd (print text)
}

computation () |> run config