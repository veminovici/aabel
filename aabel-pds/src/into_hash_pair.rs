use siphasher::sip128::Hasher128;
use std::hash::{Hash, Hasher};

pub trait IntoHashPair<H, T>
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn hashes(&self, hasher: &mut H) -> (u64, u64);
}

impl<H, T> IntoHashPair<H, T> for T
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn hashes(&self, hasher: &mut H) -> (u64, u64) {
        self.hash(hasher);
        let h = hasher.finish128().as_u128();

        let hash1 = (h & 0xffff_ffff_ffff_ffff) as u64;
        let hash2 = (h >> 64) as u64;

        (hash1, hash2)
    }
}

pub trait IntoBloomBits<H, T>
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn bits(&self, m: usize, k: usize, hasher: &mut H) -> Vec<u64>;
}
