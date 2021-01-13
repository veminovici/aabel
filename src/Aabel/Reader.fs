namespace Simplee

[<Struct>]
[<NoComparison>]
type Reader<'E, 'T> = Reader of ('E -> 'T)

[<RequireQualifiedAccess>]
module Reader = 

    let run env (Reader r) = r env

    let retn x = Reader <| fun _ -> x
    let singleton =  retn

    let map f r = Reader <| fun env ->
        r 
        |> run env 
        |> f

    let apply f r = Reader <| fun env ->
        let f = run env f
        let r = run env r
        f r

    let bind f r = Reader <| fun env ->
        r
        |> run env
        |> f
        |> run env

    let map2 f x y = 
        apply (apply (retn f) x) y

    let zip x y =
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let lift1 (f: 'E -> 'a -> 'b) : 'a -> Reader<'E, 'b> =
        fun a -> Reader <| fun e -> f e a

    let lift2 (f: 'E -> 'a -> 'b -> 'c) : 'a -> 'b -> Reader<'E, 'c> =
        fun a b -> Reader <| fun e -> f e a b

    let liftE (f:'E2 -> 'E1) (Reader fn) : Reader<'E2, 'a> =
        Reader (f >> fn)

    let sequenceAsync (r: Reader<_, Async<_>>) = async {
        return 
            Reader <| fun env ->
                r
                |> run env
                |> Async.RunSynchronously }

    let concat (x: Reader<'TEnv, 'T list>) (y: Reader<'TEnv, 'T list>) : Reader<'TEnv, 'T list> =
        map2 (@) x y

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

        type ReaderBuilder () =
            member _.Return(x) : Reader<_,_>    = retn x
            member _.ReturnFrom(x: Reader<_,_>) = x

            member _.Yield(x) : Reader<_,_>     = retn x
            member _.YieldFrom(x: Reader<_,_>)  = x

            member _.Zero ()                    = retn ()

            member _.Bind(m, f) =
                bind f m

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith(g, h)    = try this.Run g with e -> h e
            member this.TryFinally(g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : Reader<_, _> = 
                this.TryFinally (
                    (fun () -> f r),
                    (fun () -> if not <| obj.ReferenceEquals(r, null) then r.Dispose ()))
                    
            member this.While(g, f) =
                if not <| g() then this.Zero ()
                else this.Bind(this.Run f, fun _ -> this.While (g, f))

            member this.For(s: #seq<'T>, f) =
                this.Using(s.GetEnumerator(), fun enum ->
                    this.While(enum.MoveNext,
                        this.Delay(fun () -> f enum.Current)))

            member _.BindReturn (x, f) = 
                map f x

            member _.MergeSources(t1, t2) = 
                zip t1 t2

            /// Helps with for..in and for..do
            member _.Source(s: #seq<_>) = 
                s

            member _.Source(r: Reader<_,_>) = 
                r

            member _.Source(r: Result<_,_>) = 
                r
                |> retn

            member _.Source(c: Choice<_,_>) = 
                c 
                |> Result.ofChoice 
                |> retn

        let _reader = ReaderBuilder ()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module Reader = 

            let private _traverseM (zro : Reader<'TEnv, 'U list>) (f:'T -> Reader<'TEnv,'U>) (xs: 'T list) =

                let rec loop (acc: Reader<'TEnv, 'U list>) = function
                    | [] -> acc
                    | h::tail ->
                        let stt = _reader {
                            let! x  = f h
                            let! xs = acc
                            return xs @ [x] }

                        loop stt tail

                loop zro xs

            let traverseM f xs =
                _traverseM (retn []) f xs

            let sequenceM xs =
                traverseM id xs
