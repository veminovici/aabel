pub trait HasherExt {
    /// Create a hasher from a seed
    fn with_seed(seed: &[u8; 16]) -> Self;

    /// Create a hasher with a random seed.
    fn with_rnd_seed() -> Self;
}
