use std::hash::Hasher;

use cityhash::cityhash_1::city_hash_64;

#[derive(Default)]
pub struct City64Hasher {
    bytes: Vec<u8>,
}

impl Hasher for City64Hasher {
    fn finish(&self) -> u64 {
        city_hash_64(&self.bytes)
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes);
    }
}
