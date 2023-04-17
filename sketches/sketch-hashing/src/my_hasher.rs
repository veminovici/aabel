use std::hash::{ Hash, Hasher};
use crate::HashExt;

pub struct MyHasher {
    bytes: Vec<u8>,
    k: u64,
    q: u64,
    p: u64,
}

impl MyHasher {
    fn new(k: u64, q: u64, p: u64) -> Self {
        Self {
            bytes: vec![],
            k,
            q,
            p,
        }
    }

    pub fn get_hash<T: Hash>(k: u64, q: u64, p: u64, item: T) -> u64 {
        let mut me = Self::new(k, q, p);
        item.hash64(&mut me)
    }
}

impl Hasher for MyHasher {
    fn finish(&self) -> u64 {
        let mut h = 0u64;
        self.bytes.iter().enumerate().for_each(|(i, b)| {
            h += (*b as u64) << i * 8;
        });

        (self.k * h + self.q) % self.p
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend(bytes);
    }
}

