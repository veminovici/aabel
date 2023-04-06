use std::hash::{Hasher, BuildHasherDefault};

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

/// A builder for default CITYHASH-64 hashers.
pub type City64BuildHasher = BuildHasherDefault<City64Hasher>;

use std::collections::{HashMap, HashSet};

/// A `HashMap` using a default CITY-64 hasher.
pub type City64HashMap<K, V> = HashMap<K, V, City64BuildHasher>;

/// A `HashSet` using a default CITYHASH-64 hasher.
pub type City64HashSet<T> = HashSet<T, City64BuildHasher>;
