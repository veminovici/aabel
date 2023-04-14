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
    use std::hash::Hash;

    use crate::{random, UniversalHasher};

    use super::*;

    struct MyHasher {
        bytes: Vec<u8>,
        k: u64,
        q: u64,
        p: u64,
    }

    impl MyHasher {
        fn new(k: u64, q: u64, p: u64) -> Self {
            Self {
                bytes: vec![],
                k,
                q,
                p,
            }
        }

        pub fn get_hash<T: Hash>(k: u64, q: u64, p: u64, item: T) -> u64 {
            let mut me = Self::new(k, q, p);
            item.hash64(&mut me)
        }
    }

    impl Hasher for MyHasher {
        fn finish(&self) -> u64 {
            let mut h = 0u64;
            self.bytes.iter().enumerate().for_each(|(i, b)| {
                h += (*b as u64) << i * 8;
            });

            (self.k * h + self.q) % self.p
        }

        fn write(&mut self, bytes: &[u8]) {
            self.bytes.extend(bytes);
        }
    }

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

    #[test]
    fn myhasher_() {
        let h1: Vec<_> = (0..19)
            .map(|i| MyHasher::get_hash(22, 5, 31, i) % 19)
            .collect();
        let h2: Vec<_> = (0..19)
            .map(|i| MyHasher::get_hash(30, 2, 31, i) % 19)
            .collect();
        let h3: Vec<_> = (0..19)
            .map(|i| MyHasher::get_hash(21, 23, 31, i) % 19)
            .collect();
        let h4: Vec<_> = (0..19)
            .map(|i| MyHasher::get_hash(15, 6, 31, i) % 19)
            .collect();

        println!("h1: {h1:?}");
        println!("h2: {h2:?}");
        println!("h3: {h3:?}");
        println!("h4: {h4:?}");
    }
}
