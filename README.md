# SIMPLEE / AABEL / PROBABILISTIC DATA STRUCTURES

[![Rust](https://github.com/veminovici/aabel/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/veminovici/aabel/actions/workflows/ci.yml)
![GitHub top language](https://img.shields.io/github/languages/top/veminovici/aabel)
[![License:MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/veminovici/aabel)
![GitHub last commit](https://img.shields.io/github/last-commit/veminovici/aabel)

A project for probabilistic data structures and large data sets.

---

## Hash Functions
The [aabel-hash](./aabel-hash/) crate defines several traits.

- [HashExt](./aabel-hash/src/hash_ext.rs) trait. The trait exposes functionality that extends the *std::Hash* functionality.
- [Hash128Ext](./aabel-hash/src/hash128_ext.rs) trait. The trait exposes functionality available when the hasher is 128bits one.
- [HasherExt](./aabel-hash/src/hasher_ext.rs) trait. The trait exposes *std::Hasher* functionality.

The [aabel-hash](./aabel-hash/) crate also implements or re-exports several common hash functions:
- [CityHash](https://github.com/google/cityhash) (source [city64.rs](./aabel-hash/src/city64.rs))
- [FarmHash](https://github.com/google/farmhash) (source [farm64.rs](./aabel-hash/src/farm64.rs))
- [Fowler-Noll-Vo](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function) (source [fnv64.rs](./aabel-hash/src/fnv64.rs))
- [Murmur3](https://en.wikipedia.org/wiki/MurmurHash) (source [murmur3.rs](./aabel-hash/src/murmur32.rs))
- [SipHash](https://en.wikipedia.org/wiki/SipHash) (source [sip24.rs](./aabel-hash/src/sip24.rs))

For more details please check the crate's [readme](./hashing/aabel-hash/README.md) file.

---

## Membership Data Structures
The [aabel-membership](./aabel-membership/) crate implements several probabilistic data structures which determine if a given elements is present in a collection.

- [Bloom Filter](https://en.wikipedia.org/wiki/Bloom_filter) (source [filter.rs](./aabel-membership/src/bloom/filter.rs))
- [Bloom Counter](https://en.wikipedia.org/wiki/Counting_Bloom_filter) (source [counter.rs](./aabel-membership/src/bloom/counter.rs)) 
- [Cuckoo Filter](https://en.wikipedia.org/wiki/Cuckoo_filter) (source [filter.rs](./aabel-membership/src/cuckoo/filter.rs))

For more details please check the crate's [readme](./aabel-membership//README.md) file.

---

## ABOUT

> Code designed and written on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.