# Simplee / Aabel

[![Rust](https://github.com/veminovici/aabel/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/veminovici/aabel/actions/workflows/ci.yml)
![GitHub top language](https://img.shields.io/github/languages/top/veminovici/aabel)
[![License:MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/veminovici/aabel)
![GitHub last commit](https://img.shields.io/github/last-commit/veminovici/aabel)

A project for large data sets.

---

## HASHING
A collection of crates which implement different hashes and hashing extenstions.

### AABEL-HASH
It is a [create](./hashing/aabel-hash/) which defines the *Hashable* trait. The crate also does the default implementation for all types which implement the *Hash* trait.

```rust
pub trait HashExt: Hash {
    /// Returns the hash value for the instance.
    fn get_hash<H: Default + Hasher>(&self) -> u64

    /// Returns the pair of hash values for the instance.
    fn get_hash_deconstructed<H: Default + Hasher>(&self) -> (u32, u32)

    /// Retursn the hash value for the collection of items.
    fn get_hash_slice<H: Default + Hasher>(data: &[Self]) -> u64 where Self: Hash + Sized;
}
```

---

## MEMBERSHIP
A collection of crates which implemement different algorithms that can check if a given item is a member of a collection.

### CUCKOO FILTER
The [crate](./membership/cuckoo-filter/) implements the [cuckoo filter](https://en.wikipedia.org/wiki/Cuckoo_filter). To see how you can use the create, please check the [example](./membership/cuckoo-filter/examples/cuckoo.rs).

```rust
// Create the cuckoo filter with 12 buckets, each buckets has 1 slot.
const BUCKETS: usize = 12;
const SLOTS: usize = 1;
let mut filter = cuckoo_filter::CuckooFilter::<BUCKETS, SLOTS>::new();

// Insert an element
let r = filter.insert(&"AAAA");
assert!(r);

// Check if the elements is in the collection.
let r = filter.contains(&"AAAA");
assert!(r);
```

---

## About

> Code designed and written on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.