use siphasher::sip128::SipHasher24;

use crate::hasher_ext::HasherExt;

impl HasherExt for SipHasher24 {
    fn with_seed(seed: &[u8; 16]) -> Self {
        SipHasher24::new_with_key(seed)
    }

    fn with_rnd_seed() -> Self {
        let mut seed = [0u8; 32];
        getrandom::getrandom(&mut seed).unwrap();
        let seed = seed[0..16].try_into().unwrap();

        Self::with_seed(seed)
    }
}
