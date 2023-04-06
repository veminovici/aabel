use aabel_hash::hash::{Hash128Ext, Hasher128, HasherExt, SipHasher24};
use bit_vec::BitVec;
use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

pub struct BloomFilter<T, H = SipHasher24> {
    bits: BitVec,
    // Number of bits
    m: usize,
    /// Number of hash functions.
    k: usize,
    hasher: H,
    _p: PhantomData<T>,
}

impl<T, H> BloomFilter<T, H>
where
    H: Copy + Hasher + Hasher128 + HasherExt,
    T: Hash,
{
    /// Creates a `BloomFilter` with *m* number of bits and *k* number of hash functions.
    pub fn new(m: usize, k: usize) -> Self {
        let bits = BitVec::from_elem(m, false);
        let hasher = <H as HasherExt>::with_rnd_seed();

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

    /// Returns the number of bits in the filter
    pub fn number_of_bits(&self) -> usize {
        self.m
    }

    /// Returns the number of hash functions.
    pub fn number_of_hashes(&self) -> usize {
        self.k
    }

    /// Returns the bits in the filter.
    pub fn to_bytes(&self) -> Vec<u8> {
        self.bits.to_bytes()
    }

    /// Inserts a new item in the filter.
    pub fn insert(&mut self, item: &T) {
        self.get_indexes(item)
            .iter()
            .for_each(|&idx| self.bits.set(idx, true))
    }

    /// Determines if an item belongs to the filter.
    pub fn contains(&self, item: &T) -> bool {
        self.get_indexes(item)
            .iter()
            .all(|&idx| self.bits.get(idx).unwrap())
    }

    /// Returns the indexes that represent a givenitem
    fn get_indexes(&self, item: &T) -> Vec<usize> {
        item.get_hashes(self.k, self.hasher)
            .iter()
            .map(|h| (*h as usize) % self.m)
            .collect()
    }

    /// Returns the optimal size of the filter and the number of hash functions.
    fn compute_optimal(items: usize, false_positive_rate: f64) -> (usize, usize) {
        let m = optimal_m(items, false_positive_rate);
        let k = optimal_k(items, m);

        (m, k)
    }
}

fn optimal_m(num_items: usize, false_positive_rate: f64) -> usize {
    -(num_items as f64 * false_positive_rate.ln() / (2.0f64.ln().powi(2))).ceil() as usize
}

fn optimal_k(n: usize, m: usize) -> usize {
    let k = (m as f64 / n as f64 * 2.0f64.ln()).round() as usize;
    if k < 1 {
        1
    } else {
        k
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
