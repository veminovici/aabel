use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

use aabel_hash::hash::{Hash128Ext, Hasher128, HasherExt, SipHasher24};

use crate::compute_optimal;

pub struct BloomCounter<T, H = SipHasher24> {
    counters: Vec<u8>,
    len: usize,
    m: usize,
    k: usize,
    hasher: H,
    _p: PhantomData<T>,
}

impl<T, H> BloomCounter<T, H>
where
    H: Copy + Hasher + Hasher128 + HasherExt,
    T: Hash,
{
    /// Creates a new Bloom filter.
    pub fn new(m: usize, k: usize) -> Self {
        let counters = vec![0; m];
        let hasher = <H as HasherExt>::with_rnd_seed();

        Self {
            m,
            k,
            counters,
            len: 0,
            hasher,
            _p: PhantomData,
        }
    }

    /// Creates a new Bloom filter which is expected to store a given number of
    /// elements and with an expected false positive rate.
    pub fn with_capacity_fpr(num_items: usize, false_positive_rate: f64) -> Self {
        let (m, k) = compute_optimal(num_items, false_positive_rate);
        Self::new(m, k)
    }

    /// Returns the number of bits in the filter
    pub fn number_of_bits(&self) -> usize {
        self.m
    }

    /// Returns the number of hash functions.
    pub fn number_of_hashes(&self) -> usize {
        self.k
    }

    /// Returns the indexes that represent a givenitem
    fn get_indexes(&self, item: &T) -> Vec<usize> {
        item.get_hashes(self.k, self.hasher)
            .iter()
            .map(|h| (*h as usize) % self.m)
            .collect()
    }

    /// Inserts an item in the bloom filter.
    pub fn insert(&mut self, item: &T) {
        self.get_indexes(item)
            .iter()
            .for_each(|&idx| self.counters[idx] = self.counters[idx].saturating_add(1));

        self.len += 1;
    }

    /// Determines if an item belongs to the filter.
    pub fn contains(&self, item: &T) -> bool {
        self.get_indexes(item)
            .iter()
            .all(|&idx| self.counters[idx] > 0)
    }

    pub fn delete(&mut self, item: &T) {
        if self.contains(item) {
            self.get_indexes(item)
                .iter()
                .for_each(|&idx| self.counters[idx] = self.counters[idx].saturating_sub(1));

            self.len -= 1;
        }
    }

    pub fn count(&self, item: &T) -> u8 {
        let mut c = u8::MAX;

        for idx in self.get_indexes(item) {
            if self.counters[idx] == 0 {
                return 0;
            }

            if self.counters[idx] < c {
                c = self.counters[idx];
            }
        }

        c
    }
}

#[cfg(test)]
mod utests {
    use super::*;
    use quickcheck_macros::quickcheck;

    #[test]
    fn simple_() {
        let mut filter = BloomCounter::<usize>::new(100, 10);
        filter.insert(&10);

        let res = filter.contains(&10);
        assert!(res);

        let c = filter.count(&10);
        assert_eq!(1, c);
    }

    #[quickcheck]
    fn prop_bloom_filter(xs: Vec<usize>) -> bool {
        let mut filter = BloomCounter::<usize>::new(100000, 10);

        let _: Vec<_> = xs.iter().inspect(|x| filter.insert(x)).collect();

        let mut ys = Vec::new();
        ys.extend(xs);

        ys.iter_mut().all(|x| filter.contains(x));
        true
    }
}
