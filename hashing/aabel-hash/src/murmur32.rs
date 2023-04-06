use std::hash::Hasher;

use const_murmur3::murmur3_32;

pub struct Murmur32Hasher {
    seed: u32,
    bytes: Vec<u8>,
}

impl Murmur32Hasher {
    pub fn with_seed(seed: u32) -> Self {
        Self {
            seed,
            bytes: vec![],
        }
    }
}

impl Default for Murmur32Hasher {
    fn default() -> Self {
        let seed: u64 = 0xcbf29ce484222325;
        Self::with_seed(seed as u32)
    }
}

impl Hasher for Murmur32Hasher {
    fn finish(&self) -> u64 {
        murmur3_32(&self.bytes, self.seed) as u64
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes);
    }
}

/// A builder for default MURMUR3-32 hashers.
pub type Murmur32BuildHasher = BuildHasherDefault<Murmur32Hasher>;

use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default MURMUR3-32 hasher.
pub type Murmur32HashMap<K, V> = HashMap<K, V, Murmur32BuildHasher>;

/// A `HashSet` using a default MURMUR3-32 hasher.
pub type Murmur32HashSet<T> = HashSet<T, Murmur32BuildHasher>;



#[cfg(test)]
mod utests {
    use super::*;

    fn murmur(bytes: &[u8]) -> u64 {
        let mut hasher = Murmur32Hasher::default();
        hasher.write(bytes);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        assert!(murmur(b"") != 0);
    }
}
