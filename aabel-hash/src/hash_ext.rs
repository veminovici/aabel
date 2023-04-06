use std::hash::{Hash, Hasher};

pub trait HashExt: Hash {
    /// Returns the hash value for the instance.
    fn get_hash<H>(&self) -> u64
    where
        H: Default + Hasher;

    /// Returns the pair of hash values for the instance.
    fn get_hash_deconstructed<H>(&self) -> (u32, u32)
    where
        H: Default + Hasher;

    /// Retursn the hash value for the collection of items.
    fn get_hash_slice<H>(data: &[Self]) -> u64
    where
        H: Default + Hasher,
        Self: Hash + Sized,
    {
        let mut hasher = <H as Default>::default();
        <Self as Hash>::hash_slice(data, &mut hasher);
        hasher.finish()
    }
}

impl<T> HashExt for T
where
    T: Hash,
{
    fn get_hash<H>(&self) -> u64
    where
        H: Default + Hasher,
    {
        let mut hasher = <H as Default>::default();
        self.hash(&mut hasher);
        hasher.finish()
    }

    fn get_hash_deconstructed<H>(&self) -> (u32, u32)
    where
        H: Default + Hasher,
    {
        let h = <Self as HashExt>::get_hash::<H>(self);
        let h1 = (h & 0xfff_ffff) as u32;
        let h2 = (h >> 32) as u32;
        (h1, h2)
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn hashable_() {
        let x = "test now, and again, and again";
        let h = x.get_hash::<std::collections::hash_map::DefaultHasher>();
        let (h1, h2) = x.get_hash_deconstructed::<std::collections::hash_map::DefaultHasher>();

        assert_ne!(h, 0);
        assert_ne!(h1, 0);
        assert_ne!(h2, 0);
    }
}
