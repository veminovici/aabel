use farmhash::hash64;
use std::hash::{Hasher, BuildHasherDefault};

/// A hasher that uses the Google's [farm](https://github.com/google/farmhash) algorithm.
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

/// A builder for default FARMHASH-64 hashers.
pub type Farm64BuildHasher = BuildHasherDefault<Farm64Hasher>;

use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default FARMHASH-64 hasher.
pub type Farm64HashMap<K, V> = HashMap<K, V, Farm64BuildHasher>;

/// A `HashSet` using a default FARMHASH-64 hasher.
pub type Farm64HashSet<T> = HashSet<T, Farm64BuildHasher>;
