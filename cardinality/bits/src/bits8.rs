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

    fn is_last_bit_zero(&self) -> bool {
        *self & 1 == 0
    }
}

#[derive(Clone, Copy)]
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
    pub fn new() -> Self {
        Self::default()
    }

    pub fn merge_u16_as_position(&mut self, start_slot: usize, value: u16) {
        debug_assert!(2 <= N);

        let mut buf = [0u8; 2];
        LittleEndian::write_u16(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u16(&mut self, value: u16) {
        self.merge_u16_as_position(0, value)
    }

    pub fn merge_u32_as_position(&mut self, start_slot: usize, value: u32) {
        debug_assert!(4 <= N);

        let mut buf = [0u8; 4];
        LittleEndian::write_u32(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u32(&mut self, value: u32) {
        self.merge_u32_as_position(0, value)
    }

    pub fn merge_u64_at_position(&mut self, start_slot: usize, value: u64) {
        debug_assert!(8 <= N);

        let mut buf = [0u8; 8];
        LittleEndian::write_u64(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u64(&mut self, value: u64) {
        self.merge_u64_at_position(0, value)
    }

    pub fn merge_u128_at_position(&mut self, start_slot: usize, value: u128) {
        debug_assert!(16 <= N);

        let mut buf = [0u8; 8];
        LittleEndian::write_u128(&mut buf, value);

        self.merge_value(start_slot, buf.as_slice());
    }

    pub fn merge_u128(&mut self, value: u128) {
        self.merge_u128_at_position(0, value)
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

        let mut bits = Bits8::<2>::default();
        bits.merge_u16(x);

        println!("{x:04X}: bits={:?}", bits.pretty());

        assert!(!bits.get(0));
        assert!(!bits.get(1));
        assert!(!bits.get(2));
        assert!(!bits.get(3));

        assert!(!bits.get(8));
        assert!(bits.get(9));
        assert!(!bits.get(10));
        assert!(bits.get(11));

        bits.reset(9);
        println!("After reset: bits={:?}", bits.pretty());

        bits.set(9);
        println!("After set: bits={:?}", bits.pretty());

        assert_eq!(16, bits.bits());
        assert_eq!(2, bits.slots());
        assert_eq!(9, bits.lsb());
    }
}
