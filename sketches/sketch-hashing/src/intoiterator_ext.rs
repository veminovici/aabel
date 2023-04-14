pub trait IntoIteratorExt: IntoIterator {
    fn first_by<F>(self, f: &mut F) -> Option<Self::Item>
    where
        Self::Item: Copy,
        Self: Sized,
        F: FnMut(Self::Item) -> bool,
    {
        let check = move |acc: Option<Self::Item>, x: Self::Item| match acc {
            Some(_) => acc,
            None => {
                if f(x) {
                    Some(x)
                } else {
                    None
                }
            }
        };

        self.into_iter().fold(None, check)
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
        assert_eq!(Some(5), res);
    }
}
