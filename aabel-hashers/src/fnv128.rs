use std::hash::{BuildHasherDefault, Hasher};

use const_fnv1a_hash::fnv1a_hash_128;

pub struct Fnv128Hasher(u128);

impl Default for Fnv128Hasher {
    #[inline]
    fn default() -> Self {
        Self::with_key(0)
    }
}

impl Fnv128Hasher {
    pub fn with_key(key: u128) -> Self {
        Self(key)
    }
}

impl Hasher for Fnv128Hasher {
    fn finish(&self) -> u64 {
        (self.0 >> 64) as u64
    }

    fn write(&mut self, bytes: &[u8]) {
        let Self(mut hash) = *self;
        let h = fnv1a_hash_128(bytes, None);

        hash ^= h;
        hash = hash.wrapping_mul(0x100000001b3);

        *self = Self(hash);
    }
}

/// A builder for default FNV-128 hashers.
pub type Fnv128BuildHasher = BuildHasherDefault<Fnv128Hasher>;

// #[cfg(feature = "std")]
use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default FNV-64 hasher.
// #[cfg(feature = "std")]
pub type Fnv128HashMap<K, V> = HashMap<K, V, Fnv128BuildHasher>;

/// A `HashSet1 using a default FNV-64 hasher.
// #[cfg(feature = "std")]
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
