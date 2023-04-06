use std::fmt::Debug;

use crate::fingerprint::Fingerprint;

#[derive(Clone, Copy)]
pub struct Bucket<const N: usize> {
    slots: [Option<Fingerprint>; N],
}

impl<const N: usize> Default for Bucket<N> {
    fn default() -> Self {
        Self::new()
    }
}

impl<const N: usize> Debug for Bucket<N> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        fn tostr(fp: Option<Fingerprint>) -> String {
            match fp {
                Some(fp) => format!("{:?}", fp),
                None => "_".to_owned(),
            }
        }

        write!(f, "[{}]", tostr(self.slots[0]))
    }
}

impl<const N: usize> Bucket<N> {
    pub fn new() -> Self {
        Self { slots: [None; N] }
    }

    pub fn insert(&mut self, fp: Fingerprint) -> bool {
        for entry in &mut self.slots {
            if entry.is_none() {
                *entry = Some(fp);
                return true;
            }
        }

        false
    }

    pub fn swap(&mut self, idx: usize, other: Fingerprint) -> Fingerprint {
        let loc = &mut self.slots[idx];
        let old = loc.unwrap();
        *loc = Some(other);
        old
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn debug_() {
        let mut bucket = Bucket::<1>::new();

        let fp1 = Fingerprint::from(1);
        let _ = bucket.insert(fp1);

        println!("BUCKET_DBG: {:?}", bucket);
    }

    #[test]
    fn insert_() {
        let mut bucket = Bucket::<2>::new();

        let fp1 = Fingerprint::from(1);
        let r = bucket.insert(fp1);
        assert!(r);

        let fp2 = Fingerprint::from(2);
        let r = bucket.insert(fp2);
        assert!(r);

        let fp3 = Fingerprint::from(3);
        let r = bucket.insert(fp3);
        assert!(!r);
    }
}
