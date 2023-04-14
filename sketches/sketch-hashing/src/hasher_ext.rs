use std::hash::Hasher;

use crate::HashExt;

pub trait HasherExt: Hasher {
    fn permmutate_indexes(&mut self, n: usize) -> Vec<usize>;
}

impl<H: Hasher> HasherExt for H {
    fn permmutate_indexes(&mut self, n: usize) -> Vec<usize> {
        (0..n).map(|i| i.hash64(self) as usize % n).collect()
    }
}

#[cfg(test)]
mod utests {
    use crate::{random, UniversalHasher};

    use super::*;

    #[test]
    fn simple_() {
        let hasher = std::collections::hash_map::DefaultHasher::default();
        let (k, q) = random::get_u64pair();
        let mut hasher = UniversalHasher::with_hasher_m31(hasher, k, q);

        let n = 100;
        let indexes = hasher.permmutate_indexes(n);

        assert_eq!(n, indexes.len());
        assert!(indexes.iter().all(|i| i < &n));
    }
}
