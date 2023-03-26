pub struct Distances;

impl Distances {
    /// Returns the [Manhattan][m] distance between two points, a and b.
    /// The points are represented as collections of coordinates.
    /// The coordinate values must be convertable into `f64` values.
    ///
    /// [m]: https://en.wikipedia.org/wiki/Taxicab_geometry
    pub fn manhattan<A, B>(a: impl Iterator<Item = A>, b: impl Iterator<Item = B>) -> f64
    where
        A: Into<f64>,
        B: Into<f64>,
    {
        a.zip(b).map(|(ka, kb)| (ka.into() - kb.into()).abs()).sum()
    }

    /// Returns the square of the [Euclidean][e] distance between two points, a and b.
    /// The points are represented as collection of coordinates.
    /// The coordinate values must be convertable into `f64` values.
    ///
    /// [e]: https://en.wikipedia.org/wiki/Euclidean_distance
    pub fn euclidean2<A, B>(a: impl Iterator<Item = A>, b: impl Iterator<Item = B>) -> f64
    where
        A: Into<f64>,
        B: Into<f64>,
    {
        a.zip(b)
            .map(|(ka, kb)| {
                let ka = ka.into();
                let kb = kb.into();
                (ka - kb) * (ka - kb)
            })
            .sum()
    }
}

#[cfg(test)]
mod utests {
    use super::*;
    use quickcheck::*;
    use quickcheck_macros::quickcheck;

    #[derive(Clone, Copy, Debug)]
    pub struct XYArgument {
        pub xs: [u8; 8],
        pub ys: [u8; 8],
    }

    fn gen_u8s(g: &mut Gen) -> [u8; 8] {
        let u0 = u8::arbitrary(g) % 2;
        let u1 = u8::arbitrary(g) % 2;
        let u2 = u8::arbitrary(g) % 2;
        let u3 = u8::arbitrary(g) % 2;
        let u4 = u8::arbitrary(g) % 2;
        let u5 = u8::arbitrary(g) % 2;
        let u6 = u8::arbitrary(g) % 2;
        let u7 = u8::arbitrary(g) % 2;

        let xs = [u0, u1, u2, u3, u4, u5, u6, u7];
        xs
    }

    impl quickcheck::Arbitrary for XYArgument {
        fn arbitrary(g: &mut quickcheck::Gen) -> Self {
            let xs = gen_u8s(g);
            let ys = gen_u8s(g);

            XYArgument { xs, ys }
        }
    }

    #[quickcheck]
    fn prop_manhattan(arg: XYArgument) -> bool {
        let xs = arg.xs.iter().map(|x| f64::from(*x));
        let ys = arg.ys.iter().map(|y| f64::from(*y));
        let res = Distances::manhattan(xs, ys);

        res >= 0.
    }
}
