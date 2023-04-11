use std::{hash::Hash, cmp::min, f64::consts};

use aabel_hash::hash::{Hasher128, SipHasher24};

/// Implements CountMin Sketch
pub struct CountMinSketch<const M: usize, const K: usize> {
    len: usize,                 // the total number of elements.
    counters: [[u32; M]; K],    // the counters.
    hasher: SipHasher24,
}

impl<const M: usize, const K: usize> CountMinSketch<M, K> {
    pub fn new() -> Self {
        let key = generate_random_key();
        let hasher = create_hasher_with_key(key);

        Self {
            len: 0,
            counters: [[0; M]; K],
            hasher,
        }
    }

    pub fn insert<T: Hash>(&mut self, item: &T) {
        self.get_indices(item, self.hasher).iter().enumerate().for_each(|(k, &idx)| {
            self.counters[k][idx] = self.counters[k][idx].saturating_add(1);
        });
        self.len += 1;
    }

    pub fn estimated_count<T: Hash>(&self, key: &T) -> u32 {
        let bucket_indices = self.get_indices(key, self.hasher);
        let mut estimated_count = u32::MAX;
        for (ki, &bi) in bucket_indices.iter().enumerate() {
            if self.counters[ki][bi] == 0 {
                return 0;
            } else {
                estimated_count = min(estimated_count, self.counters[ki][bi])
            }
        }
        estimated_count
    }

    fn get_indices<T: Hash>(&self, item: &T, hasher: SipHasher24) -> Vec<usize> {
        let (hash1, hash2) = self.get_hash_pair(item, hasher);

        let mut indices = Vec::with_capacity(K);
        if K == 1 {
            let bit = hash1 % M as u64;
            indices.push(bit as usize);
        } else {
            for k in 0..K as u64 {
                let hash = hash1.wrapping_add(k.wrapping_mul(hash2));
                let bit = hash % M as u64;
                indices.push(bit as usize);
            }
        }

        indices
    }

    fn get_hash_pair<T: Hash>(&self, item: &T, mut hasher: SipHasher24) -> (u64, u64) {
        item.hash(&mut hasher);
        let hash128 = hasher.finish128().as_u128();
        let hash1 = (hash128 & 0xffff_ffff_ffff_ffff) as u64;
        let hash2 = (hash128 >> 64) as u64;
        (hash1, hash2)
    }
}

fn create_hasher_with_key(key: [u8; 16]) -> SipHasher24 {
    SipHasher24::new_with_key(&key)
}

fn generate_random_key() -> [u8; 16] {
    let mut seed = [0u8; 32];
    getrandom::getrandom(&mut seed).unwrap();
    seed[0..16].try_into().unwrap()
}

pub fn optimal_m(epsilon: f64) -> usize {
    (consts::E / epsilon).ceil() as usize
}

pub fn optimal_k(delta: f64) -> usize {
    (1.0 / delta).ln().ceil() as usize
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn cms_() {
        let epsilon = 0.2f64;
        let delta = 0.01f64;
        let m = optimal_m(epsilon);
        let k = optimal_k(delta);

        println!("m={m} k={k}");

        let mut cms = CountMinSketch::<14, 5>::new();

        for _ in 0..1000000 {
            cms.insert(&"a1");
            cms.insert(&"a2");
            cms.insert(&"a3");
            cms.insert(&"a4");
            cms.insert(&"a5");
            cms.insert(&"a6");
            cms.insert(&"a7");
        }

        assert_eq!(cms.estimated_count(&"a1"), 1000000);
        assert_eq!(cms.estimated_count(&"a2"), 1000000);
        assert_eq!(cms.estimated_count(&"b1"), 0);
    }
}
