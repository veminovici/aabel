use std::hash::{BuildHasherDefault, Hasher};

use const_fnv1a_hash::fnv1a_hash_64;

#[derive(Default)]
pub struct Fnv64Hasher {
    bytes: Vec<u8>,
}

impl Hasher for Fnv64Hasher {
    fn finish(&self) -> u64 {
        fnv1a_hash_64(&self.bytes, None)
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes);
    }
}

/// A builder for default FNV-64 hashers.
pub type Fnv64BuildHasher = BuildHasherDefault<Fnv64Hasher>;

use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default FNV-64 hasher.
pub type Fnv64HashMap<K, V> = HashMap<K, V, Fnv64BuildHasher>;

/// A `HashSet1 using a default FNV-64 hasher.
pub type Fnv64HashSet<T> = HashSet<T, Fnv64BuildHasher>;

#[cfg(test)]
mod utests {
    use super::*;

    fn fnv1a(bytes: &[u8]) -> u64 {
        let mut hasher = Fnv64Hasher::default();
        hasher.write(bytes);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        assert!(fnv1a(b"") != 0);
    }
}
