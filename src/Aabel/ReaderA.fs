namespace Simplee

type ReaderA<'TEnv, 'T> = Reader<'TEnv, Async<'T>>

[<RequireQualifiedAccess>]
module ReaderA =

    open System.Threading.Tasks

    let run env (r: ReaderA<_,_>) =  
        r 
        |> Reader.run env 
        |> Async.RunSynchronously

    let retn     x = x |> Async.retn |> Reader.retn
    let singleton  = retn

    let map f = 
        Reader.map (Async.map f)

    let bind (f: 'a -> ReaderA<'TEnv, 'b>) (m: ReaderA<'TEnv, 'a>) : ReaderA<'TEnv, 'b> = 
        m
        |> Reader.bind (Async.RunSynchronously >> f)

    let apply f m = 
        bind (fun f -> 
            bind (f >> retn) m) f

    let map2 f x y =
        Reader.map2 (Async.map2 f) x y

    let zip x y = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let sequenceAsync (ar: ReaderA<_, _>) =
        ar
        |> Reader.sequenceAsync

    let concat x y =
        map2 (@) x y

    let lift1 (f: 'E -> 'a -> Async<'b>) : 'a -> ReaderA<'E, 'b> =
        fun a -> Reader <| fun e -> f e a

    let lift2 (f: 'E -> 'a -> 'b -> Async<'c>) : 'a -> 'b -> ReaderA<'E, 'c> =
        fun a b -> Reader <| fun e -> f e a b

    let unfold iter =
        let gen env = 
            iter
            |> run env
            |> fun r -> Some (r, env)

        Seq.unfold gen

    module Operators =
        let (<!>) m f = map   f m
        let (<*>) f m = apply f m
        let (>>=) m f = bind  f m

    module ComputationExpression =

        open System

        type ReaderABuilder () =
            member _.Return(x) : ReaderA<_,_>    = retn x
            member _.ReturnFrom(x: ReaderA<_,_>) = x

            member _.Yield(x) : ReaderA<_,_>     = retn x
            member _.YieldFrom(x: ReaderA<_,_>)  = x

            member _.Zero () : ReaderA<_, unit>  = retn ()

            member _.Bind(m: ReaderA<'TEnv, 'T>, f: 'T -> ReaderA<'TEnv, 'U>) : ReaderA<'TEnv, 'U> =
                bind f m

            member _.Run (f: unit -> ReaderA<_,_>) = f ()

            member this.Combine(a1, a2) = 
                this.Bind(a1, fun _ -> a2())

            member _.Delay(g) = 
                g

            member this.TryWith    (g, h) = try this.Run g with e -> h e
            member this.TryFinally (g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : ReaderA<_, _> = 
                this.TryFinally (
                    (fun () -> f r),
                    (fun () -> if not <| obj.ReferenceEquals(r, null) then r.Dispose ()))
                    
            member this.While(g, f) =
                if not <| g() then this.Zero ()
                else this.Bind(this.Run f, fun () -> this.While (g, f))

            member this.For(s: #seq<'T>, f) =
                this.Using(s.GetEnumerator(), fun enum ->
                    this.While(enum.MoveNext,
                        this.Delay(fun () -> f enum.Current)))

            member _.BindReturn(x, f) = 
                map f x

            member _.MergeSources(r1, r2) = 
                zip r1 r2

            member _.Source(r: ReaderA<_, _>) = 
                r

            member _.Source(s: #seq<_>) = 
                s

            member _.Source(r: Async<_>) : ReaderA<_, _> = 
                Reader.retn r

            member _.Source(t: Task<_>) =
                t 
                |> Async.AwaitTask
                |> Reader.retn

        let readerA = ReaderABuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module ReaderA = 

            let private _traversetM (zro: ReaderA<'TEnv,'U list>) (f: 'T -> ReaderA<'TEnv,'U>) (xs: 'T list) =

                let rec loop (acc: ReaderA<'TEnv, 'U list>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = readerA {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs
            
            let traverseM f xs =
                _traversetM (retn []) f xs

            let sequenceM xs =
                traverseM id xs