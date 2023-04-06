use std::{fmt::Debug, hash::Hasher, marker::PhantomData};

use aabel_hash::hash::Hashable;
use bucket::Bucket;
use fi_pair::FIPair;
use log::debug;

pub(crate) mod bucket;
pub(crate) mod fi_pair;
pub(crate) mod fingerprint;
pub(crate) mod index;

pub struct CuckooFilter<
    const B: usize,
    const N: usize,
    H = std::collections::hash_map::DefaultHasher,
> where
    H: Default + Hasher,
{
    buckets: [Bucket<N>; B],
    _p: PhantomData<H>,
}

impl<const B: usize, const N: usize, H> Debug for CuckooFilter<B, N, H>
where
    H: Default + Hasher,
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let x = self.buckets.iter().fold("".to_owned(), |acc, b| {
            if acc.is_empty() {
                format!("{:?}", b)
            } else {
                format!("{} {:?}", acc, b)
            }
        });
        write!(f, "{}", x)
    }
}

impl<const B: usize, const N: usize, H> CuckooFilter<B, N, H>
where
    H: Default + Hasher,
{
    pub fn new() -> Self {
        Self {
            buckets: [Bucket::<N>::new(); B],
            _p: PhantomData,
        }
    }

    pub fn insert<T>(&mut self, data: &T) -> bool
    where
        T: Hashable,
    {
        // Try to add it to the first index.
        let fi = FIPair::<H>::from_data(data);
        debug!("INSERT | pair {:?}", &fi);

        if self.put(&fi) {
            debug!("SUCCESS | pair {:?} | INIT", &fi);
            return true;
        }

        // Try to add it to the second index.
        let fi = fi.alt();
        if self.put(&fi) {
            debug!("SUCCESS | pair {:?} | ALT", &fi);
            return true;
        }

        // Rearrange the fingerprints
        self.rearrange(fi)
    }

    fn swap(&mut self, fi: &FIPair<H>) -> FIPair<H> {
        let other = self.buckets[fi.idx % B].swap(0, fi.fp);
        FIPair::new(other, fi.idx).alt()
    }

    fn rearrange(&mut self, fi: FIPair<H>) -> bool {
        let mut current = fi;

        for _ in 0..3 {
            {
                current = self.swap(&current);
            }

            if self.put(&current) {
                return true;
            }
        }

        false
    }

    fn put(&mut self, fi: &FIPair<H>) -> bool {
        let idx = fi.idx % B;
        if self.buckets[idx].insert(fi.fp) {
            true
        } else {
            false
        }
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let mut filter = CuckooFilter::<12, 1>::new();

        // Add it in the first bucket
        let r = filter.insert(&"testing");
        assert!(r);
        println!("CUCKOO_DBG (1): {:?}", filter);

        // Add it to the second bucket
        let r = filter.insert(&"testing");
        assert!(r);
        println!("CUCKOO_DBG (2): {:?}", filter);

        // No place to add it.
        let r = filter.insert(&"testing");
        assert!(!r);
        println!("CUCKOO_DBG (3): {:?}", filter);
    }
}
