use log::debug;
use siphasher::sip128::Hasher128;
use std::hash::{Hash, Hasher};

pub trait HashPair<H, T>
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn hash_pair(&self, hasher: H) -> (u64, u64);
}

impl<H, T> HashPair<H, T> for T
where
    H: Hasher + Hasher128,
    T: Hash,
{
    fn hash_pair(&self, mut hasher: H) -> (u64, u64) {
        self.hash(&mut hasher);
        let h = hasher.finish128().as_u128();
        debug!("ITEM || hash={:?}", h);

        let hash1 = (h & 0xffff_ffff_ffff_ffff) as u64;
        let hash2 = (h >> 64) as u64;

        (hash1, hash2)
    }
}
