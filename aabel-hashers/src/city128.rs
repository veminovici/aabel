use std::hash::Hasher;

use cityhash::cityhash_1::city_hash_128;
use siphasher::sip128::{Hash128, Hasher128};

pub struct City128Hasher {
    bytes: Vec<u8>,
}

impl Hasher for City128Hasher {
    fn finish(&self) -> u64 {
        (city_hash_128(&self.bytes) >> 64) as u64
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes)
    }
}

impl Hasher128 for City128Hasher {
    fn finish128(&self) -> Hash128 {
        city_hash_128(&self.bytes).into()
    }
}
