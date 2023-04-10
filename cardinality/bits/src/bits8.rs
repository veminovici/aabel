use byteorder::{ByteOrder, LittleEndian};

use crate::{Bits, LastBit, ToBool, Zero};

impl ToBool for u8 {
    fn to_bool(&self) -> bool {
        self != &0
    }
}

impl Zero for u8 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

impl LastBit for u8 {
    fn is_last_bit_one(&self) -> bool {
        *self & 1 == 1
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

    fn get_slot(&self, slot: usize) -> Self::Inner {
        self.bits[slot]
    }

    fn get_slot_mut(&mut self, slot: usize) -> &mut Self::Inner {
        &mut self.bits[slot]
    }
}

impl<const N: usize> Bits8<N> {
    pub fn merge_u16(&mut self, start_slot: usize, value: u16) {
        debug_assert!(2 <= N);

        let mut buf = [0u8; 2];
        LittleEndian::write_u16(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u32(&mut self, start_slot: usize, value: u32) {
        debug_assert!(4 <= N);

        let mut buf = [0u8; 4];
        LittleEndian::write_u32(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u64(&mut self, start_slot: usize, value: u64) {
        debug_assert!(8 <= N);

        let mut buf = [0u8; 8];
        LittleEndian::write_u64(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u128(&mut self, start_slot: usize, value: u128) {
        debug_assert!(16 <= N);

        let mut buf = [0u8; 8];
        LittleEndian::write_u128(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }
}

impl<const N: usize> Default for Bits8<N> {
    fn default() -> Self {
        Self { bits: [0; N] }
    }
}

impl<const N: usize> From<u16> for Bits8<N> {
    fn from(value: u16) -> Self {
        let mut bits = Self::default();
        bits.merge_u16(0, value);
        bits
    }
}

impl<const N: usize> From<u32> for Bits8<N> {
    fn from(value: u32) -> Self {
        let mut bits = Self::default();
        bits.merge_u32(0, value);
        bits
    }
}

impl<const N: usize> From<u64> for Bits8<N> {
    fn from(value: u64) -> Self {
        let mut bits = Self::default();
        bits.merge_u64(0, value);
        bits
    }
}

impl<const N: usize> From<u128> for Bits8<N> {
    fn from(value: u128) -> Self {
        let mut bits = Self::default();
        bits.merge_u128(0, value);
        bits
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
