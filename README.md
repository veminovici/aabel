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
The [aabel-hash](./hashing/aabel-hash/) crate defines the **HashExt** trait. The crate also does the default implementation for all types which implement the **Hash** trait.

The crate also implements or re-exports several common hash functions:
- [CityHash](https://github.com/google/cityhash)
- [FarmHash](https://github.com/google/farmhash)
- [Fowler-Noll-Vo](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)
- [Murmur3](https://en.wikipedia.org/wiki/MurmurHash)
- [SipHash](https://en.wikipedia.org/wiki/SipHash)

For more details please check the crate's [readme](./hashing/aabel-hash/README.md) file.

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