use std::hash::Hasher;

use const_murmur3::murmur3_32;

pub struct MurmurHasher {
    seed: u32,
    bytes: Vec<u8>,
}

impl MurmurHasher {
    pub fn with_seed(seed: u32) -> Self {
        Self {
            seed,
            bytes: vec![],
        }
    }
}

impl Default for MurmurHasher {
    fn default() -> Self {
        let seed: u64 = 0xcbf29ce484222325;
        Self::with_seed(seed as u32)
    }
}

impl Hasher for MurmurHasher {
    fn finish(&self) -> u64 {
        murmur3_32(&self.bytes, self.seed) as u64
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes);
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    fn murmur(bytes: &[u8]) -> u64 {
        let mut hasher = MurmurHasher::default();
        hasher.write(bytes);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        assert!(murmur(b"") != 0);
    }
}
