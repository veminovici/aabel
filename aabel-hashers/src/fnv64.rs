use std::hash::{BuildHasherDefault, Hasher};

use const_fnv1a_hash::fnv1a_hash_64;

pub struct Fnv64Hasher(u64);

impl Default for Fnv64Hasher {
    #[inline]
    fn default() -> Self {
        Self::with_key(0)
    }
}

impl Fnv64Hasher {
    pub fn with_key(key: u64) -> Self {
        Self(key)
    }
}

impl Hasher for Fnv64Hasher {
    fn finish(&self) -> u64 {
        self.0
    }

    fn write(&mut self, bytes: &[u8]) {
        let Self(mut hash) = *self;
        let h = fnv1a_hash_64(bytes, None);

        hash ^= h;
        hash = hash.wrapping_mul(0x100000001b3);

        *self = Self(hash);
    }
}

/// A builder for default FNV-64 hashers.
pub type Fnv64BuildHasher = BuildHasherDefault<Fnv64Hasher>;

#[cfg(feature = "std")]
use std::hash::collections::{HashMap, HashSet};

/// A `HashMap` using a default FNV-64 hasher.
#[cfg(feature = "std")]
pub type Fnv64HashMap<K, V> = HashMap<K, V, Fnv64BuildHasher>;

/// A `HashSet1 using a default FNV-64 hasher.
#[cfg(feature = "std")]
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
