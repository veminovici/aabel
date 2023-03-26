use crate::{generate_random_seed, BuildHasher, IntoIndexes};
use bit_vec::BitVec;
use siphasher::sip128::Hasher128;
use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

pub struct BloomFilter<H, T> {
    bits: BitVec,
    m: usize,
    k: usize,
    hasher: H,
    _p: PhantomData<T>,
}

impl<H, T> BloomFilter<H, T>
where
    H: Copy + Hasher + Hasher128 + BuildHasher,
    T: Hash,
{
    /// Creates a new Bloom filter.
    pub fn new(m: usize, k: usize) -> Self {
        let seed = generate_random_seed();
        Self {
            m,
            k,
            bits: BitVec::from_elem(m, false),
            hasher: <H as BuildHasher>::with_key(seed),
            _p: PhantomData,
        }
    }

    /// Returns the Bloom indeces for a given item.
    #[inline]
    pub fn indexes(&self, item: &T) -> Vec<usize> {
        item.to_indexes(self.m, self.k, self.hasher)
    }

    /// Determines if an item belongs to the filter.
    pub fn contains(&self, item: &T) -> bool {
        self.indexes(item)
            .iter()
            .all(|&idx| self.bits.get(idx).unwrap())
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
}

#[cfg(test)]
mod utests {
    use super::*;
    use siphasher::sip128::SipHasher24;

    #[test]
    fn simple_() {
        let mut filter = BloomFilter::<SipHasher24, usize>::new(100, 10);
        filter.insert(&10);
        let res = filter.contains(&10);
        assert!(res)
    }
}
