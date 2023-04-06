# SIMPLEE / AABEL / PROBABILISTIC DATA STRUCTURES

[![Rust](https://github.com/veminovici/aabel/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/veminovici/aabel/actions/workflows/ci.yml)
![GitHub top language](https://img.shields.io/github/languages/top/veminovici/aabel)
[![License:MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/veminovici/aabel)
![GitHub last commit](https://img.shields.io/github/last-commit/veminovici/aabel)

A project for probabilistic data structures and large data sets.

---

## AABEL-HASHING
The [aabel-hash](./hashing/aabel-hash/) crate defines the **HashExt** trait. The crate also does the default implementation for all types which implement the **Hash** trait.

The crate also implements or re-exports several common hash functions:
- [CityHash](https://github.com/google/cityhash)
- [FarmHash](https://github.com/google/farmhash)
- [Fowler-Noll-Vo](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function)
- [Murmur3](https://en.wikipedia.org/wiki/MurmurHash)
- [SipHash](https://en.wikipedia.org/wiki/SipHash)

For more details please check the crate's [readme](./hashing/aabel-hash/README.md) file.

---

## AABEL-MEMBERSHIP
The [aabel-membership](./aabel-membership/) crate implements several probabilistic data structures which determine if a given elements is present in a collection.

- [Bloom Filter and Counter](./membership/bloom-filter/)
- [Cuckoo Filter](./membership/cuckoo-filter/)

For more details please check the crate's [readme](./aabel-membership//README.md) file.

---

## ABOUT

> Code designed and written on the beautiful island of [**Saaremaa**](https://goo.gl/maps/DmB9ewY2R3sPGFnTA), Estonia.