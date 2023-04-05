use siphasher::sip128::Hasher128;
use std::hash::{Hash, Hasher};

pub trait HasherPair<H128>
where
    H128: Hasher + Hasher128,
{
    fn finish(&self, hasher: H128) -> (u64, u64);
}

impl<H128, T> HasherPair<H128> for T
where
    H128: Hasher + Hasher128,
    T: Hash,
{
    fn finish(&self, mut hasher: H128) -> (u64, u64) {
        self.hash(&mut hasher);
        let h = hasher.finish128();
        (h.h1, h.h2)
    }
}
