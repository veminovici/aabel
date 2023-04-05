# AABEL / HASHERS
Crate which implements different hashes.

## Hash128 Trait
This trait defines a hash function which return a pair of `u64` values.

## FarmHash
The FarmHash is exposed for 64. This is just a simple wrapper around the
[farmhash](https://docs.rs/farmhash/latest/farmhash/) crate.

## Fowler/Noll/Vo (FNV)
The FNV hash is exposed for 64 and 128. This is just a simple wrapper around the
[const-fnv1a-hash](https://docs.rs/const-fnv1a-hash/latest/const_fnv1a_hash/) crate.

## MurmurHash
The Murmur has is using the murmur3. This is just a simple wrapper around the [const-murmur3](https://docs.rs/const-murmur3/latest/const_murmur3/) crate.

## CityHash
The CityHash is exposed for 64 and 128. This is just a simple wrapper around the [cityhash](https://docs.rs/cityhash/latest/cityhash/) crate.