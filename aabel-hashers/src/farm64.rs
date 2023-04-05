use std::hash::Hasher;

use farmhash::hash64;

#[derive(Default)]
pub struct Farm64Hasher {
    bytes: Vec<u8>,
}

impl Hasher for Farm64Hasher {
    fn finish(&self) -> u64 {
        hash64(&self.bytes)
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes)
    }
}