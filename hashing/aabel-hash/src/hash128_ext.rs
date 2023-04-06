use std::hash::{Hash, Hasher};
use siphasher::sip128::Hasher128;

pub trait Hash128Ext: Hash {
    /// Returns the deconstructed hash for the instance.
    fn get_hash128_deconstructed<H>(&self, hasher: H) -> (u64, u64)
    where
        H: Hasher + Hasher128;

    /// Returns the deconstructed hash for the instance using the default hasher value.
    fn get_hash128_deconstructed_default<H>(&self) -> (u64, u64)
    where
        H: Default + Hasher + Hasher128
    {
        let hasher = <H as Default>::default();
        self.get_hash128_deconstructed(hasher)
    }

    fn get_hashes<H>(&self, k: usize, hasher: H) -> Vec<u64>
    where
        H: Hasher + Hasher128
    {
        let (hash1, hash2) = self.get_hash128_deconstructed(hasher);
        let mut bs = Vec::with_capacity(k);

        for i in 0..k as u64 {
            let h = hash1.wrapping_add(i.wrapping_mul(hash2));
            bs.push(h);
        }

        bs
    }

}

impl<T> Hash128Ext for T
where
    T: Hash,
{
    fn get_hash128_deconstructed<H>(&self, mut hasher: H) -> (u64, u64)
    where
        H: Hasher + Hasher128
    {
        self.hash(&mut hasher);
        let h = hasher.finish128().as_u128();

        let hash1 = (h & 0xffff_ffff_ffff_ffff) as u64;
        let hash2 = (h >> 64) as u64;

        (hash1, hash2)
    }
}