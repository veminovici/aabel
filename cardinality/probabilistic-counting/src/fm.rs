use bits::{Bits, Bits8};
use std::{
    hash::{Hash, Hasher},
    marker::PhantomData,
};

pub struct FlajoletMartin<H, const M: usize, const N: usize> {
    bits: [Bits8<N>; M],
    _ph: PhantomData<H>,
}

impl<H, const M: usize, const N: usize> Default for FlajoletMartin<H, M, N> {
    fn default() -> Self {
        Self {
            bits: [Bits8::<N>::default(); M],
            _ph: Default::default(),
        }
    }
}

impl<H, const M: usize, const N: usize> FlajoletMartin<H, M, N>
where
    H: Default + Hasher,
{
    pub fn add_item<T: Hash>(&mut self, item: T) {
        let h = Self::get_hash(item);
        let m = M as u64;
        let r = h % m;
        let q = h / m;
        let j = Self::lsb(q);

        let bs = &mut self.bits[r as usize];
        bs.set(j);
    }

    pub fn n(&self) -> f64 {
        let r: usize = self.bits.iter().map(|bs| bs.lzb()).sum();
        3f64 * 2f64.powf(r as f64 / 3f64) / 0.77351
    }

    fn get_hash<T: Hash>(item: T) -> u64 {
        let mut hasher = H::default();
        item.hash(&mut hasher);
        hasher.finish()
    }

    fn lsb(n: u64) -> usize {
        let mut n = n;
        for i in 0..64 {
            if n & 1 == 1 {
                return i as usize;
            }

            n >>= 1;
        }

        64
    }
}

#[cfg(test)]
mod utests {
    use std::hash::Hash;

    use bits::{Bits, Bits8};

    use crate::myhasher::{MyHasher, CITIES};

    use super::*;

    fn lsb(n: u64) -> usize {
        let mut n = n;
        for i in 0..64 {
            if n & 1 == 1 {
                return i as usize;
            }

            n >>= 1;
        }

        64
    }

    fn hash_str(value: &str) -> u64 {
        let mut hasher = MyHasher::new(CITIES.as_slice());
        value.hash(&mut hasher);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        let mut bits = Bits8::<8>::default();

        let _: Vec<_> = CITIES
            .iter()
            .inspect(|city| {
                let h = hash_str(city.0);
                let r = lsb(h);
                bits.set(r);

                // println!("{}: {h} | {h:b} | {r}", city.0);
            })
            .collect();

        let lzb = bits.lzb();
        //println!("Bits: {} - LZB={lzb}", bits.pretty());

        let n = 2u64.pow(lzb as u32) as f64 / 0.77351;
        assert!(20f64 < n);
        assert!(n < 21f64);
        println!("PC={n}");
    }

    #[test]
    fn fm_() {
        let mut fm = FlajoletMartin::<MyHasher, 3, 8>::default();

        let _: Vec<_> = CITIES
            .iter()
            .inspect(|city| {
                fm.add_item(city.0);
            })
            .collect();

        let n = fm.n();
        println!("FN={n}");
        assert!(12f64 < n);
        assert!(n < 13f64);
    }
}
