use siphasher::sip128::SipHasher24;

use crate::RandSeed;

pub trait WithSeed {
    fn with_seed(seed: &RandSeed) -> Self;
}

impl WithSeed for SipHasher24 {
    fn with_seed(seed: &RandSeed) -> Self {
        SipHasher24::new_with_key(seed.value())
    }
}
