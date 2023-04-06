use std::{fmt::Debug, ops::Rem};

#[derive(Default, Hash, PartialEq, Eq, Clone, Copy)]
pub struct Index(usize);

impl Debug for Index {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "IDX:{:016X}", self.0)
    }
}

impl Rem<usize> for Index {
    type Output = usize;

    fn rem(self, rhs: usize) -> Self::Output {
        self.0 % rhs
    }
}

impl From<usize> for Index {
    fn from(value: usize) -> Self {
        Self(value)
    }
}

impl From<u32> for Index {
    fn from(value: u32) -> Self {
        Self(value as usize)
    }
}

impl From<Index> for usize {
    fn from(value: Index) -> Self {
        value.0
    }
}

impl From<Index> for u64 {
    fn from(value: Index) -> Self {
        value.0 as u64
    }
}

impl AsRef<usize> for Index {
    fn as_ref(&self) -> &usize {
        &self.0
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn debug_() {
        let idx = Index::from(10u32);
        println!("INDEX_DBG: {:?}", idx);
    }

    #[test]
    fn rem_() {
        let idx = Index::from(10u32);
        let idx1 = idx % 3;
        assert_eq!(1, idx1)
    }

    #[test]
    fn as_ref_() {
        let idx = Index::from(10u32);
        assert_eq!(&10, idx.as_ref())
    }
}
