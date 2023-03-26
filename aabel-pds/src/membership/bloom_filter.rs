use crate::into_hash_pair::IntoHashPair;
use siphasher::sip128::Hasher128;
use std::hash::{Hash, Hasher};

pub trait IntoBloomBits<H, T>
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn bits(&self, m: usize, k: usize, hasher: &mut H) -> Vec<u64>;
}

impl<H, T> IntoBloomBits<H, T> for T
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn bits(&self, m: usize, k: usize, hasher: &mut H) -> Vec<u64> {
        let (hash1, hash2) = self.hashes(hasher);

        let mut bs = Vec::with_capacity(k);
        if k == 1 {
            let b = hash1 % m as u64;
            bs.push(b);
            return bs;
        }

        for i in 0..k as u64 {
            let h = hash1.wrapping_add(i.wrapping_mul(hash2));
            let b = h % m as u64;
            bs.push(b);
        }

        assert!(bs.len() == k);
        bs
    }
}
