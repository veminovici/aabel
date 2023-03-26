pub struct RandSeed([u8; 16]);

impl RandSeed {
    pub fn new() -> Self {
        let mut seed = [0u8; 32];
        getrandom::getrandom(&mut seed).unwrap();
        let seed = seed[0..16].try_into().unwrap();
        RandSeed(seed)
    }

    pub(crate) fn value(&self) -> &[u8; 16] {
        &self.0
    }
}

impl Default for RandSeed {
    fn default() -> Self {
        Self::new()
    }
}
