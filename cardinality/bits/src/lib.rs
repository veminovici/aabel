use std::{mem::size_of, fmt::Binary, ops::{BitAnd, BitOrAssign, BitAndAssign, ShrAssign}};

use byteorder::{ByteOrder, LittleEndian};

pub trait IntoBool {
    fn into_bool(&self) -> bool;
}

impl IntoBool for u8 {
    fn into_bool(&self) -> bool {
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

pub trait MyBits<const N: usize>
{
    type Inner: 'static + Copy +  Clone + Binary + BitOrAssign + BitAndAssign + BitAnd<Output = Self::Inner> + IntoBool + Zero + ShrAssign<usize> + LastBit;

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
        v.into_bool()
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

        for i in 0..value.len() {
            let slot = self.get_slot_mut(start_slot + i);
            *slot |= value[i];
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
                    return (i * 8 + j) as usize;
                }
            }
        }

        N * 8
    }
}

pub struct MyBits8<const N: usize> {
    bits: [u8; N],
}

impl<const N: usize> MyBits<N> for MyBits8<N> {
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
        Self {
            bits
        }
    }

    fn get_slot(&self, slot: usize) -> Self::Inner {
        self.bits[slot]
    }

    fn get_slot_mut(&mut self, slot: usize) -> &mut Self::Inner {
        let xs = &mut self.bits[slot];
        xs
    }

}

impl<const N: usize> MyBits8<N> {
    pub fn new() -> Self {
        Self {
            bits: [0; N]
        }
    }

    pub fn merge_u16(&mut self, start_slot: usize, value: u16) {
        let mut buf = [0u8; 2];
        LittleEndian::write_u16(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn lsb(&self) -> usize {
        for i in 0..N {
            if self.bits[i] == 0 {
                continue;
            }

            let mut x = self.bits[i];
            for j in 0..8 {
                x >>= j;
                if (x & 1) == 1 {
                    return (i * 8 + j) as usize;
                }
            }
        }

        N * 8
    }
}

pub struct Bits<const N: usize> {
    bits: [u8; N],
}

impl<const N: usize> Bits<N> {
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

    pub fn new() -> Self {
        Self { bits: [0; N] }
    }

    pub fn from_value(xs: [u8; N]) -> Self {
        Self { bits: xs }
    }

    pub fn pretty(&self) -> String {
        (0..N).fold("".to_owned(), |acc, i| {
            if i == 0 {
                format!("{:08b}", self.bits[i])
            } else {
                format!("{acc} {:08b}", self.bits[i])
            }
        })
    }

    /// Returns the total number of bits that can be stored.
    pub fn bits(&self) -> usize {
        N * size_of::<u8>() * 8
    }

    /// Returns the total number of `u8` slots.
    pub fn slots(&self) -> usize {
        N
    }

    /// Returns the bit value from a givem position.
    pub fn get(&self, slot: usize, offset: u8) -> bool {
        let mask = Self::SET_MASKS[offset as usize];
        self.bits[slot] & mask != 0
    }

    /// Sets to 1 the bit value on a given position.
    pub fn set(&mut self, slot: usize, offset: u8) {
        let mask = Self::SET_MASKS[offset as usize];
        self.bits[slot] |= mask;
    }

    /// Resets to 0 the bit value on a given position.
    pub fn reset(&mut self, slot: usize, offset: u8) {
        let mask = Self::RESET_MASKS[offset as usize];
        self.bits[slot] &= mask;
    }

    pub fn merge_u16(&mut self, start_slot: usize, value: u16) {
        let mut ltl = [0u8; 2];
        LittleEndian::write_u16(&mut ltl, value);

        for i in 0..2 {
            self.bits[start_slot + i] |= ltl[i];
        }
    }
}

impl<const N: usize> From<[u8; N]> for Bits<N> {
    fn from(value: [u8; N]) -> Self {
        Self::from_value(value)
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let x: u16 = 10 << 8;

        let mut ltl = MyBits8::<2>::new();
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
