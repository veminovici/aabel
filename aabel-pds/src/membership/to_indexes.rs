use crate::IntoHashPair;
use std::hash::{Hash, Hasher};

use log::debug;
use siphasher::sip128::Hasher128;

pub trait IntoIndexes<H, T>
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn to_indexes(&self, m: usize, k: usize, hasher: H) -> Vec<usize>;
}

impl<H, T> IntoIndexes<H, T> for T
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn to_indexes(&self, m: usize, k: usize, hasher: H) -> Vec<usize> {
        let (hash1, hash2) = self.to_hash_pair(hasher);

        let mut bs = Vec::with_capacity(k);
        if k == 1 {
            let b = hash1 % m as u64;
            bs.push(b as usize);
            return bs;
        }

        for i in 0..k as u64 {
            let h = hash1.wrapping_add(i.wrapping_mul(hash2));
            let b = h % m as u64;
            bs.push(b as usize);
        }

        assert!(bs.len() == k);
        debug!("indexes: {:?}", bs);
        bs
    }
}
