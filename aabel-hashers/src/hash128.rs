use siphasher::sip128::Hasher128;
use std::hash::{Hash, Hasher};

pub trait Hash128<H128>
where
    H128: Hasher + Hasher128,
{
    fn hash128(&self, hasher: H128) -> (u64, u64);
}

impl<H128, T> Hash128<H128> for T
where
    H128: Hasher + Hasher128,
    T: Hash,
{
    fn hash128(&self, mut hasher: H128) -> (u64, u64) {
        self.hash(&mut hasher);
        let h = hasher.finish128();
        (h.h1, h.h2)
    }
}

#[cfg(test)]
mod utests {
    use siphasher::sip128::SipHasher;

    use crate::Fnv128Hasher;

    use super::*;

    fn test_hasher_pair(hasher: impl Hasher + Hasher128) {
        let value = b"test";
        let (h1, h2) = value.hash128(hasher);

        assert_ne!(h1, 0);
        assert_ne!(h2, 0);
    }

    #[test]
    fn simple_fnv() {
        let hasher = Fnv128Hasher::default();
        test_hasher_pair(hasher);
    }

    #[test]
    fn simple_sip() {
        let hasher = SipHasher::default();
        test_hasher_pair(hasher);
    }
}
