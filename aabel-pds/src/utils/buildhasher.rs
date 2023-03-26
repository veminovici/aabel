use siphasher::sip128::SipHasher24;

pub trait BuildHasher {
    fn with_key(key: [u8; 16]) -> Self;
}

impl BuildHasher for SipHasher24 {
    fn with_key(key: [u8; 16]) -> Self {
        SipHasher24::new_with_key(&key)
    }
}
