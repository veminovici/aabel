use std::{fmt::Debug, hash::Hasher, marker::PhantomData};

use aabel_hash::hash::HashExt;
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
    len: usize,
    _p: PhantomData<H>,
}

impl<const B: usize, const N: usize, H> Debug for CuckooFilter<B, N, H>
where
    H: Default + Hasher,
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let x = self.buckets.iter().fold("".to_owned(), |acc, b| {
            if acc.is_empty() {
                format!("{b:?}")
            } else {
                format!("{acc} {b:?}")
            }
        });
        write!(f, "{x}")
    }
}

impl<const B: usize, const N: usize, H> Default for CuckooFilter<B, N, H>
where
    H: Default + Hasher,
{
    fn default() -> Self {
        Self::new()
    }
}

impl<const B: usize, const N: usize, H> CuckooFilter<B, N, H>
where
    H: Default + Hasher,
{
    pub fn new() -> Self {
        Self {
            buckets: [Bucket::<N>::new(); B],
            len: 0,
            _p: PhantomData,
        }
    }

    pub fn insert<T>(&mut self, data: &T) -> bool
    where
        T: HashExt,
    {
        // Try to add it to the first index.
        let fi = FIPair::<H>::from_data(data);
        debug!("INSERT | {:?}", &fi);

        if self.put(&fi) {
            debug!("SUCCESS | {:?} | {} | INIT", fi, fi.idx % B);
            return true;
        }

        // Try to add it to the second index.
        let fi = fi.alt();
        if self.put(&fi) {
            debug!("SUCCESS | {:?} | {} | ALT", fi, fi.idx % B);
            return true;
        }

        // Rearrange the fingerprints
        self.rearrange(fi)
    }

    pub fn contains<T>(&self, data: &T) -> bool
    where
        T: HashExt,
    {
        let fi = FIPair::<H>::from_data(data);
        let fp = fi.fp;
        debug!("CONTAINS | {:?} | INIT", &fi);

        let r = self.buckets[fi.idx % B]
            .contains(fi.fp)
            .or_else(|| {
                let fi = fi.alt();
                debug!("CONTAINS | {:?} | ALT", &fi);
                self.buckets[fi.idx % B].contains(fi.fp)
            })
            .is_some();

        debug!("CONTAINS | {:?} | {}", fp, r);
        r
    }

    pub fn is_empty(&self) -> bool {
        self.len == 0
    }

    pub fn len(&self) -> usize {
        self.len
    }

    fn swap(&mut self, fi: &FIPair<H>) -> FIPair<H> {
        let other = self.buckets[fi.idx % B].swap(0, fi.fp);
        FIPair::new(other, fi.idx).alt()
    }

    fn rearrange(&mut self, fi: FIPair<H>) -> bool {
        debug!("REARRANGE | {:?}", &fi);

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
            self.len += 1;
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
    fn insert_() {
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

    #[test]
    fn contains_() {
        let mut filter = CuckooFilter::<12, 1>::new();

        // Add it in the first bucket
        let r = filter.insert(&"AAAA");
        assert!(r);
        println!("CUCKOO_DBG (1): {:?}", filter);

        // Add it to the second bucket
        let r = filter.insert(&"BBBB");
        assert!(r);
        println!("CUCKOO_DBG (2): {:?}", filter);

        // The filter should return true for the two elements
        let r = filter.contains(&"AAAA");
        assert!(r);

        let r = filter.contains(&"BBBB");
        assert!(r);
    }
}
