use std::hash::{Hash, Hasher};

/// Extending the `Hash` trait with a helper function
/// which returns, given a hasher, the `u64` hash value.
///
/// # Example
///
/// ```
/// use sketch_hashing::HashExt;
///
/// let item = "text to be hashed";
/// let mut hasher = std::collections::hash_map::DefaultHasher::default();
/// let h = item.hash64(&mut hasher);
/// assert_ne!(h, 0);
/// ```
pub trait HashExt: Hash {
    fn hash64<H: Hasher>(&self, hasher: &mut H) -> u64 {
        self.hash(hasher);
        hasher.finish()
    }
}

impl<T: Hash> HashExt for T {}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let item = "text to be hashed";
        let mut hasher = std::collections::hash_map::DefaultHasher::default();
        let h = item.hash64(&mut hasher);
        assert_ne!(h, 0);
    }
}
