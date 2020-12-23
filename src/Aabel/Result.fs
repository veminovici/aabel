namespace Simplee

[<RequireQualifiedAccess>]
module Result =

    let ok  = Ok
    let singleton = ok
    let err = Error

    let isOk = function
    | Ok    _ -> true
    | Error _ -> false

    let isError = function
    | Error _ -> true
    | Ok    _ -> false

    let either okF errF = function
    | Ok    x -> okF x
    | Error e -> errF e

    let fold = either

    let mapEither okF errorF x =
        either (okF >> Ok) (errorF >> Error) x

    let apply f x = 
        Result.bind (fun f -> 
            Result.bind (f >> Ok) x) f

    let map2 f x y =
        apply (apply (Ok f) x) y

    let zip x y = 
        map2 (fun x y -> x, y) x y

    let map3 f x y z =
        apply (map2 f x y) z

    let ofChoice = function
    | Choice1Of2 x -> Ok x
    | Choice2Of2 x -> Error x

    let requireTrue e = function
    | true  -> Ok ()
    | false -> Error e

    let requireFalse e = function
    | true  -> Error e
    | false -> Ok ()

    let requireSome e = function
    | Some x -> Ok x
    | None   -> Error e

    let requireNone e = function
    | Some _ -> Error e
    | None   -> Ok ()

    let requireNotNull e = function
    | null -> Error e
    | x    -> Ok x

    let requireEmpty e xs = 
        if Seq.isEmpty xs 
        then Ok () 
        else Error e

    let requireNotEmpty e xs =
        if Seq.isEmpty xs
        then Error e
        else Ok ()

    let withError e = 
        Result.mapError (fun _ -> e)

    let teeIf p f = function
    | Ok x when p x -> f x
    | _             -> ()

    let teeErrorIf p f = function
    | Error e when p e -> f e
    | _                -> ()

    let tee f r = 
        teeIf (fun _ -> true) f r

    let teeError f r =
        teeErrorIf (fun _ -> true) f r

    let sequenceAsync r = async {
        match r with
        | Ok a -> 
            let! x = a
            return Ok x
        | Error e ->
            return Error e }

    module Operators =
        let (<!>) m f = Result.map   f m
        let (<*>) f m = apply        m f
        let (>>=) m f = Result.bind  f m

    module ComputationExpression =

        open System

        type ResultBuilder () =
            member _.Return(x)     = Ok x
            member _.ReturnFrom(x) = x

            member _.Yield(x)      = Ok x
            member _.YieldFrom(x)  = x

            member _.Zero ()       = Ok ()

            member _.Bind(x: Result<'T, 'TError>, f: 'T -> Result<'U, 'TError>) = Result.bind f x

            member _.Delay(f) = f

            member _.Run  (f) = f ()

            member this.Combine(r, f) = this.Bind(r, f)

            member this.TryWith(g, h) = try this.Run g with e -> h e
            member this.TryFinally(g, c) = try this.Run g finally c ()

            member this.Using (r : 'T :> IDisposable, f) : Result<_, _> = 
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

            member _.BindReturn(x, f) = Result.map f x
            member _.MergeSources(r1, r2) = zip r1 r2
            member _.Source(r: Result<'T, 'TError>) = r

            /// Helps with for..in and for..do
            member _.Source(s: #seq<_>) = 
                s

        let result = ResultBuilder()

    module Traversals = 

        open ComputationExpression

        [<RequireQualifiedAccess>]
        module Result = 

            // Result<'a list, 'e list> -> ('c -> Result<'a, 'e>) -> 'c list -> Result<'a list, 'b list>
            let _traverseA zro f xs =

                let rec loop acc = function
                    | [] -> acc
                    | h :: tail ->
                        let fR = 
                            h 
                            |> f 
                            |> Result.mapError List.singleton

                        match acc, fR with
                        | Ok ys, Ok y -> 
                            loop (Ok (ys @ [y])) tail
                        | Error errs, Error e -> 
                            loop (Error (errs @ e)) tail
                        | Ok _, Error e 
                        | Error e , Ok _ -> 
                            loop (Error e) tail

                loop zro xs

            let traverseA f xs =
                _traverseA (Ok []) f xs

            let sequenceA xs =
                traverseA id xs

            let private _traverseM (zro : Result<'U list, 'E>) (f : 'T -> Result<'U, 'E>) (xs: 'T list) : Result<'U list, 'E> =

                let rec loop acc = function
                    | [] -> acc
                    | head::tail ->
                        Result.bind (fun x -> 
                            Result.bind (fun xs -> (xs @ [x]) |> Ok) acc) (f head)
                        |> function
                        | Ok    _ as r -> loop r tail
                        | Error _ as r -> r

                loop zro xs

            let traverseM f xs =
                _traverseM (Ok []) f xs

            let sequenceM xs =
                traverseM id xs
