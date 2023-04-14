pub trait IntoIteratorExt: IntoIterator {
    fn first_by<F>(self, f: &mut F) -> Option<(usize, Self::Item)>
    where
        Self::Item: std::fmt::Debug + Copy,
        Self: Sized,
        F: FnMut(Self::Item) -> bool,
    {
        let check = move |acc, (idx, x)| {
            println!("acc={acc:?} x={x:?}");

            match acc {
                Some(_) => acc,
                None => {
                    if f(x) {
                        Some((idx, x))
                    } else {
                        None
                    }
                }
            }
        };

        self.into_iter().enumerate().fold(None, check)
    }
}

impl<T: IntoIterator> IntoIteratorExt for T {}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let xs = 1..20;
        let res = xs.first_by(&mut |x| x % 5 == 0);

        assert!(res.is_some());
        assert_eq!(Some((4, 5)), res);
    }

    #[test]
    fn minhash() {
        let xs = [1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0];
        let ys = [
            16, 13, 12, 4, 17, 10, 1, 2, 9, 14, 8, 5, 15, 3, 6, 18, 11, 7, 0,
        ];

        let res = ys.first_by(&mut |x| {
            println!("x={x}");
            xs[x] == 1
        });

        // println!("res={res:?}");
        assert_eq!(Some((2, 12)), res);
    }
}
