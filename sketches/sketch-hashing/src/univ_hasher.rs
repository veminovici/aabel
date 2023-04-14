use std::{
    hash::{Hash, Hasher},
    num::NonZeroU64,
};

use crate::{marsenne::*, random::get_u64pair, HashExt};

/// Universal hasher. The k and q are randomly chosen `u64` values modulo p with k <> 0.
/// The value for p should be selected as a prime  p >= m. The common choise for p is to
/// take one of the known Marsenne prime numbers.
///
/// # Example
///
/// ```
/// use sketch_hashing::*;
///
/// let k = 12345;
/// let q = 6543;
/// let mut hasher = std::collections::hash_map::DefaultHasher::default();
///
/// let h1 = "text to be hashed 1".universal_hash_m31(&mut hasher, k, q);
/// let h2 = "text to be hashed 2".universal_hash_m31(&mut hasher, k, q);
///
/// assert_ne!(h1, h2)
/// ```
pub struct UniversalHasher<H> {
    hasher: H,
    k: NonZeroU64,
    q: NonZeroU64,
    p: NonZeroU64,
}

impl<H> UniversalHasher<H> {
    pub fn new(hasher: H, k: u64, q: u64, p: u64) -> Self {
        let p = NonZeroU64::new(p).expect("The p value must be non-zero");
        let k = NonZeroU64::new(k % p.get()).expect("The k value must be non-zero");
        let q = NonZeroU64::new(q % p.get()).expect("The q value must be non-zero");

        Self { hasher, k, q, p }
    }

    #[inline]
    pub fn with_hasher_m2(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE2)
    }

    #[inline]
    pub fn with_hasher_m3(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE3)
    }

    #[inline]
    pub fn with_hasher_m5(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE5)
    }

    #[inline]
    pub fn with_hasher_m7(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE7)
    }

    #[inline]
    pub fn with_hasher_m13(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE13)
    }

    #[inline]
    pub fn with_hasher_m17(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE17)
    }

    #[inline]
    pub fn with_hasher_m19(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE19)
    }

    #[inline]
    pub fn with_hasher_m31(hasher: H, k: u64, q: u64) -> Self {
        UniversalHasher::new(hasher, k, q, MARSENNE31)
    }
}

impl<H: Hasher> Hasher for UniversalHasher<H> {
    fn finish(&self) -> u64 {
        let h = self.hasher.finish();
        let k = self.k.get();
        let q = self.q.get();
        let p = self.p.get();

        k.wrapping_mul(h).wrapping_add(q) % p
    }

    fn write(&mut self, bytes: &[u8]) {
        self.hasher.write(bytes)
    }
}

pub trait UniversalHashExt: Hash {
    fn universal_hash<H: Hasher>(&self, hasher: H, k: u64, q: u64, p: u64) -> u64 {
        let mut hasher = UniversalHasher::new(hasher, k, q, p);
        let h = self.hash64(&mut hasher);

        k.wrapping_mul(h).wrapping_add(q) % p
    }

    #[inline]
    fn universal_hash_m2<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m2(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m3<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m3(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m5<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m5(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m7<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m7(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m13<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m13(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m17<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m17(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m19<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m19(hasher, k, q);
        self.hash64(&mut hasher)
    }

    #[inline]
    fn universal_hash_m31<H: Hasher>(&self, hasher: H, k: u64, q: u64) -> u64 {
        let mut hasher = UniversalHasher::with_hasher_m31(hasher, k, q);
        self.hash64(&mut hasher)
    }

    fn hashes<'a, H: 'a + Hasher>(&self, hashers: impl Iterator<Item = &'a mut H>) -> Vec<u64> {
        hashers.map(|hasher| self.hash64(hasher)).collect()
    }
}

impl<T: Hash> UniversalHashExt for T {}

#[inline]
fn get_kq_parameters(n: usize) -> Vec<(u64, u64)> {
    (0..n).map(|_| get_u64pair()).collect()
}

/// Creates a collection of universal hashers each of them using randomly
/// generated values for the k and q parameters.
fn create_universal_hashes<H: Default>(p: u64, n: usize) -> Vec<UniversalHasher<H>> {
    get_kq_parameters(n)
        .iter()
        .map(|(k, q)| UniversalHasher::new(H::default(), *k, *q, p))
        .collect()
}

impl<H: Default> UniversalHasher<H> {
    pub fn create_hashes(p: u64, n: usize) -> Vec<Self> {
        create_universal_hashes(p, n)
    }

    #[inline]
    pub fn create_hashes_m2(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE2, n)
    }

    #[inline]
    pub fn create_hashes_m3(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE3, n)
    }

    #[inline]
    pub fn create_hashes_m5(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE5, n)
    }

    #[inline]
    pub fn create_hashes_m7(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE7, n)
    }

    #[inline]
    pub fn create_hashes_m13(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE13, n)
    }

    #[inline]
    pub fn create_hashes_m17(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE17, n)
    }

    #[inline]
    pub fn create_hashes_m19(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE19, n)
    }

    #[inline]
    pub fn create_hashes_m31(n: usize) -> Vec<Self> {
        Self::create_hashes(MARSENNE31, n)
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let k = 12345;
        let q = 6543;
        let mut hasher = std::collections::hash_map::DefaultHasher::default();

        let h1 = "text to be hashed 1".universal_hash_m31(&mut hasher, k, q);
        let h2 = "text to be hashed 2".universal_hash_m31(&mut hasher, k, q);

        assert_ne!(h1, h2)
    }

    #[test]
    fn hashes_() {
        let mut hashers =
            UniversalHasher::<std::collections::hash_map::DefaultHasher>::create_hashes_m31(10);
        assert_eq!(10, hashers.len());

        let hs1: Vec<_> = hashers
            .iter_mut()
            .map(|hasher| "text to be hashed 1".hash64(hasher))
            .collect();

        let hs2: Vec<_> = hashers
            .iter_mut()
            .map(|hasher| "text to be hashed 2".hash64(hasher))
            .collect();

        assert_ne!(hs1, hs2)
    }
}
