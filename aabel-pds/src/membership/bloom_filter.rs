use crate::{ComputeOptimal, IntoIndexes, RandSeed, WithSeed};
use bit_vec::BitVec;
use siphasher::sip128::{Hasher128, SipHasher24};
use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

pub struct BloomFilter<T, H = SipHasher24> {
    bits: BitVec,
    m: usize,
    k: usize,
    hasher: H,
    _p: PhantomData<T>,
}

impl<T, H> BloomFilter<T, H>
where
    H: Copy + Hasher + Hasher128 + WithSeed,
    T: Hash,
{
    /// Creates a new Bloom filter.
    pub fn new(m: usize, k: usize) -> Self {
        let seed = RandSeed::new();
        let bits = BitVec::from_elem(m, false);
        let hasher = <H as WithSeed>::with_seed(&seed);

        Self {
            m,
            k,
            bits,
            hasher,
            _p: PhantomData,
        }
    }

    /// Creates a new Bloom filter which is expected to store a given number of
    /// elements and with an expected false positive rate.
    pub fn with_capacity_fpr(num_items: usize, false_positive_rate: f64) -> Self {
        let (m, k) = Self::compute_optimal(num_items, false_positive_rate);
        Self::new(m, k)
    }

    /// Returns the Bloom indeces for a given item.
    #[inline]
    fn indexes(&self, item: &T) -> Vec<usize> {
        item.to_indexes(self.m, self.k, self.hasher)
    }

    /// Returns the bytes.
    pub fn to_bytes(&self) -> Vec<u8> {
        self.bits.to_bytes()
    }

    /// Returns the number of hash functions used by the filter.
    pub fn hashes(&self) -> usize {
        self.k
    }

    /// Returns the number of bits in the filter.
    pub fn size(&self) -> usize {
        self.m
    }

    /// Inserts an item in the bloom filter.
    pub fn insert(&mut self, item: &T) {
        self.indexes(item)
            .iter()
            .for_each(|&b| self.bits.set(b, true))
    }

    /// Determines if an item belongs to the filter.
    pub fn contains(&self, item: &T) -> bool {
        self.indexes(item)
            .iter()
            .all(|&idx| self.bits.get(idx).unwrap())
    }
}

#[cfg(test)]
mod utests {
    use super::*;
    use quickcheck_macros::quickcheck;

    #[test]
    fn simple_() {
        let mut filter = BloomFilter::<usize>::new(100, 10);
        filter.insert(&10);
        let res = filter.contains(&10);
        assert!(res)
    }

    #[quickcheck]
    fn prop_bloom_filter(xs: Vec<usize>) -> bool {
        let mut filter = BloomFilter::<usize>::new(100000, 10);

        let _: Vec<_> = xs.iter().inspect(|x| filter.insert(x)).collect();

        let mut ys = Vec::new();
        ys.extend(xs);

        ys.iter_mut().all(|x| filter.contains(x));
        true
    }
}
