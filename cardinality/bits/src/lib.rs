use std::{
    fmt::Binary,
    mem::size_of,
    ops::{BitAnd, BitAndAssign, BitOrAssign, ShrAssign},
};

use byteorder::{ByteOrder, LittleEndian};

pub trait ToBool {
    fn to_bool(&self) -> bool;
}

impl ToBool for u8 {
    fn to_bool(&self) -> bool {
        self != &0
    }
}

pub trait Zero {
    fn is_zero(&self) -> bool;
}

impl Zero for u8 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

pub trait LastBit {
    fn is_last_bit_one(&self) -> bool;
}

impl LastBit for u8 {
    fn is_last_bit_one(&self) -> bool {
        *self & 1 == 1
    }
}

pub trait Bits<const N: usize> {
    type Inner: 'static
        + Copy
        + Clone
        + Binary
        + BitOrAssign
        + BitAndAssign
        + BitAnd<Output = Self::Inner>
        + ToBool
        + Zero
        + ShrAssign<usize>
        + LastBit;

    const SET_MASKS: &'static [Self::Inner];
    const RESET_MASKS: &'static [Self::Inner];

    fn from_value(xs: [Self::Inner; N]) -> Self;

    fn get_slot(&self, slot: usize) -> Self::Inner;

    fn get_slot_mut(&mut self, slot: usize) -> &mut Self::Inner;

    fn pretty(&self) -> String {
        (0..N).fold("".to_owned(), |acc, i| {
            if i == 0 {
                // VLD_TODO - How we now how many bits wide
                format!("{:08b}", self.get_slot(i))
            } else {
                format!("{acc} {:08b}", self.get_slot(i))
            }
        })
    }

    fn bits(&self) -> usize {
        N * size_of::<Self::Inner>() * 8
    }

    fn slots(&self) -> usize {
        N
    }

    fn get(&self, slot: usize, offset: u8) -> bool {
        let mask = Self::SET_MASKS[offset as usize];
        let v = self.get_slot(slot) & mask;
        v.to_bool()
    }

    fn set(&mut self, slot: usize, offset: u8) {
        let mask = &Self::SET_MASKS[offset as usize];
        let slot = self.get_slot_mut(slot);
        *slot |= *mask;
    }

    fn reset(&mut self, slot: usize, offset: u8) {
        let mask = &Self::RESET_MASKS[offset as usize];
        let slot = self.get_slot_mut(slot);
        *slot &= *mask;
    }

    fn merge_value(&mut self, start_slot: usize, value: &[Self::Inner]) {
        for (i, item) in value.iter().enumerate() {
            let slot = self.get_slot_mut(start_slot + i);
            *slot |= *item;
        }
    }

    fn lsb(&self) -> usize {
        for i in 0..N {
            if self.get_slot(i).is_zero() {
                continue;
            }

            let mut x = self.get_slot(i);
            for j in 0..8 {
                x >>= j;

                if x.is_last_bit_one() {
                    return i * 8 + j;
                }
            }
        }

        N * 8
    }
}

pub struct Bits8<const N: usize> {
    bits: [u8; N],
}

impl<const N: usize> Bits<N> for Bits8<N> {
    type Inner = u8;

    const SET_MASKS: &'static [u8] = [
        0b0000_0001,
        0b0000_0010,
        0b0000_0100,
        0b0000_1000,
        0b0001_0000,
        0b0010_0000,
        0b0100_0000,
        0b1000_0000,
    ]
    .as_slice();

    const RESET_MASKS: &'static [u8] = [
        0b1111_1110,
        0b1111_1101,
        0b1111_1011,
        0b1111_0111,
        0b1110_1111,
        0b1101_1111,
        0b1011_1111,
        0b0111_1111,
    ]
    .as_slice();

    fn from_value(bits: [Self::Inner; N]) -> Self {
        Self { bits }
    }

    fn get_slot(&self, slot: usize) -> Self::Inner {
        self.bits[slot]
    }

    fn get_slot_mut(&mut self, slot: usize) -> &mut Self::Inner {
        &mut self.bits[slot]
    }
}

impl<const N: usize> Bits8<N> {
    pub fn merge_u16(&mut self, start_slot: usize, value: u16) {
        let mut buf = [0u8; 2];
        LittleEndian::write_u16(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }
}

impl<const N: usize> Default for Bits8<N> {
    fn default() -> Self {
        Self { bits: [0; N] }
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let x: u16 = 10 << 8;

        let mut ltl = Bits8::<2>::default();
        ltl.merge_u16(0, x);

        println!("{x:04X}: ltl={:?}", ltl.pretty());

        assert!(!ltl.get(0, 0));
        assert!(!ltl.get(0, 1));
        assert!(!ltl.get(0, 2));
        assert!(!ltl.get(0, 3));

        assert!(!ltl.get(1, 0));
        assert!(ltl.get(1, 1));
        assert!(!ltl.get(1, 2));
        assert!(ltl.get(1, 3));

        ltl.reset(1, 1);
        println!("After reset: ltl={:?}", ltl.pretty());

        ltl.set(1, 1);
        println!("After set: ltl={:?}", ltl.pretty());

        assert_eq!(16, ltl.bits());
        assert_eq!(2, ltl.slots());
        assert_eq!(9, ltl.lsb());
    }
}
