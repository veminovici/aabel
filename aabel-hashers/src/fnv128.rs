use std::hash::{BuildHasherDefault, Hasher};

use const_fnv1a_hash::fnv1a_hash_128;
use siphasher::sip128::{Hash128, Hasher128};

#[derive(Default)]
pub struct Fnv128Hasher {
    bytes: Vec<u8>,
}

impl Hasher for Fnv128Hasher {
    fn finish(&self) -> u64 {
        (fnv1a_hash_128(&self.bytes, None) >> 64) as u64
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes);
    }
}

impl Hasher128 for Fnv128Hasher {
    fn finish128(&self) -> Hash128 {
        fnv1a_hash_128(&self.bytes, None).into()
    }
}

/// A builder for default FNV-128 hashers.
pub type Fnv128BuildHasher = BuildHasherDefault<Fnv128Hasher>;

use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default FNV-64 hasher.
pub type Fnv128HashMap<K, V> = HashMap<K, V, Fnv128BuildHasher>;

/// A `HashSet1 using a default FNV-64 hasher.
pub type Fnv128HashSet<T> = HashSet<T, Fnv128BuildHasher>;

#[cfg(test)]
mod utests {
    use super::*;

    fn fnv1a(bytes: &[u8]) -> u64 {
        let mut hasher = Fnv128Hasher::default();
        hasher.write(bytes);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        assert!(fnv1a(b"") != 0);
    }
}
