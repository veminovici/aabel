use crate::{
    bits::Bits,
    private::{self, PrivateBits},
};

pub struct BitsU8 {
    bits: Vec<u8>,
    lsb: usize,
    capacity: usize,
}

impl BitsU8 {
    pub fn lsb(&self) -> usize {
        self.lsb
    }

    pub fn capacity(&self) -> usize {
        self.capacity
    }
}

impl PrivateBits for BitsU8 {
    type Slot = u8;

    const SET_MASKS: &'static [Self::Slot] = [
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

    const RESET_MASKS: &'static [Self::Slot] = [
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

    #[inline]
    fn slot_mut(&mut self, index: private::SlotIndex) -> &mut Self::Slot {
        let index = usize::from(index);
        &mut self.bits[index]
    }

    #[inline]
    fn slot(&self, index: private::SlotIndex) -> &Self::Slot {
        let index = usize::from(index);
        &self.bits[index]
    }
}

impl Bits for BitsU8 {
    fn with_capacity(capacity: usize) -> Self {
        let number_of_slots = Self::number_of_slots(capacity);
        let bits: Vec<_> = (0..number_of_slots).map(|_v| 0u8).collect();

        Self {
            bits,
            capacity,
            lsb: capacity,
        }
    }

    fn set(&mut self, bit_index: usize) {
        if bit_index < self.lsb {
            self.lsb = bit_index;
        }

        self.set_bit(bit_index)
    }
}
