use aabel_hash::hash::HashExt;
use std::{fmt::Debug, hash::Hasher, marker::PhantomData};

use super::{fingerprint::Fingerprint, index::Index};

#[derive(Default, Hash)]
pub struct FIPair<H = std::collections::hash_map::DefaultHasher>
where
    H: Default + Hasher,
{
    pub fp: Fingerprint,
    pub idx: Index,
    _p: PhantomData<H>,
}

impl<H> Debug for FIPair<H>
where
    H: Default + Hasher,
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "({:?}, {:?})", self.fp, self.idx)
    }
}

impl<H> FIPair<H>
where
    H: Default + Hasher,
{
    pub(crate) fn new(fp: Fingerprint, idx: Index) -> Self {
        Self {
            fp,
            idx,
            _p: PhantomData,
        }
    }

    pub fn from_data<T>(data: &T) -> Self
    where
        T: HashExt,
    {
        let (fp, idx) = data.get_hash_deconstructed::<H>();
        let fp = Fingerprint::from(fp);
        let idx = Index::from(idx);
        Self {
            fp,
            idx,
            _p: PhantomData,
        }
    }

    pub fn alt(self) -> Self {
        let idx = self.fp.get_hash::<H>() ^ (*self.idx.as_ref() as u64);
        Self {
            fp: self.fp,
            idx: Index::from(idx as usize),
            _p: PhantomData,
        }
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn debug_() {
        let data = "testing, testing";
        let fi = FIPair::<std::collections::hash_map::DefaultHasher>::from_data::<_>(&data);
        println!("FIPAIR_DBG: {:?}", fi);
    }

    #[test]
    fn fipair_new() {
        let data = "testing, testing";
        let fi = FIPair::<std::collections::hash_map::DefaultHasher>::from_data::<_>(&data);
        assert_ne!(0, *fi.fp.as_ref());
        assert_ne!(0, *fi.idx.as_ref());
    }

    #[test]
    fn fipair_alt() {
        let fp = Fingerprint::from(10);
        let idx = Index::from(123u32);
        let fi1 = FIPair::<std::collections::hash_map::DefaultHasher>::new(fp, idx);

        let fi2 = fi1.alt();
        assert_eq!(&fi2.fp, &fp);

        let fi3 = fi2.alt();
        assert_eq!(&fi3.fp, &fp);
        assert_eq!(&fi3.idx, &idx);
    }

    #[test]
    fn alt_alt() {
        let fp = Fingerprint::from(10);
        let idx = Index::from(123u32);
        let fi1 = FIPair::<std::collections::hash_map::DefaultHasher>::new(fp, idx);

        let fi2 = fi1.alt();
        let fi3 = fi2.alt();

        assert_eq!(&fi3.fp, &fp);
        assert_eq!(&fi3.idx, &idx);
    }
}
