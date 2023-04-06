# Simplee / Aabel-hash
A crate whihc implements or exposes different extensions and hashes.

## HashExt Trait
The **HashExt** trait is an extension of the **std::hash::Hash** trait. The extension allows the user to compute the hash into one single function call.

```rust
pub trait HashExt: Hash {
    /// Returns the hash value for the instance.
    fn get_hash<H: Default + Hasher>(&self) -> u64

    /// Returns the pair of hash values for the instance.
    fn get_hash_deconstructed<H: Default + Hasher>(&self) -> (u32, u32)
    /// Retursn the hash value for the collection of items.
    fn get_hash_slice<H: Default + Hasher>(data: &[Self]) -> u64 { ... }
```

---

## Hashers
The create implements or re-exports different hashers. Each of these hashers is its own feature so they can be imported separately in any project.

### City32 Hasher
The **City32** exposes as a hasher the Google's **CityHash** hashing [algorithm](https://github.com/google/cityhash) implemented by [cityhash](https://docs.rs/cityhash/latest/cityhash/) crate. The hasher is under **city** feature flag.

### Farm64 Hasher
The **Farm64** exposes as a hasher the Google's **FarmHash** hashing [algorithm](https://github.com/google/farmhash) implemented by [farmhash](https://docs.rs/farmhash/latest/farmhash/) crate. The hasher is under **farm** feature flag.

### Fowler-Noll-Vo (FNV) Hasher
The **FNV** exposes as a hasher the **FNV** hashing [algorithm](https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function) implemented by [const-fnv1a-hash](https://docs.rs/const-fnv1a-hash/latest/const_fnv1a_hash/) crate. Teh hasher is under **fnv** feature flag.

### Murmur3 Hasher
The **Murmur3** exposes as a hasher the **Murmu3** hashing [algorith](https://en.wikipedia.org/wiki/MurmurHash) implemented by [const-murmur3](https://docs.rs/const-murmur3/latest/const_murmur3/) crate. The hasher is under **murmur** feature flag.
