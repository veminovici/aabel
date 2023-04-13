use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

use crate::{shingles, Shingles};

pub struct HashedShingles<'a, T, P, H = std::collections::hash_map::DefaultHasher> {
    shingles: Shingles<'a, T, P>,
    _ph: PhantomData<H>,
}

#[inline]
pub fn hashed_shingles<T, P, H>(
    slice: &[T],
    predicate: P,
    k: usize,
) -> HashedShingles<'_, T, P, H> {
    let shingles = shingles(slice, predicate, k);
    HashedShingles {
        shingles,
        _ph: PhantomData,
    }
}

impl<'a, T, P, H> Iterator for HashedShingles<'a, T, P, H>
where
    T: Hash,
    P: Fn(&T) -> bool,
    H: Default + Hasher,
{
    type Item = u32;

    fn next(&mut self) -> Option<Self::Item> {
        if let Some(s) = self.shingles.next() {
            let mut hasher = H::default();
            s.hash(&mut hasher);
            let h = hasher.finish();

            Some(h as u32)
        } else {
            None
        }
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn every_elelemt() {
        let xs = [1, 2, 3];
        const K: usize = 2;
        let is_start = |_: &i32| true;

        let mut ss: HashedShingles<_, _, std::collections::hash_map::DefaultHasher> =
            hashed_shingles(xs.as_slice(), is_start, K);

        assert!(ss.next().is_some());
        assert!(ss.next().is_some());
        assert!(ss.next().is_none());
    }

    #[test]
    fn news() {
        const K: usize = 3;
        let text: Vec<_> = "A spokeperson for the Sudzo Corporation \
        revealed today that studies have shown it is good for people \
        to buy Sudzo products"
            .split_whitespace()
            .collect();
        let stop_words = ["A", "for", "the", "to", "that"].as_slice();
        let is_stop_word = |w: &&str| stop_words.contains(w);

        let mut ss: HashedShingles<_, _, std::collections::hash_map::DefaultHasher> =
            hashed_shingles(text.as_slice(), is_stop_word, K);

        assert!(ss.next().is_some());
        assert!(ss.next().is_some());
        assert!(ss.next().is_some());
    }
}
