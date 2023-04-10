use std::{
    fmt::Binary,
    mem::size_of,
    ops::{BitAnd, BitAndAssign, BitOrAssign, ShrAssign},
};

pub trait ToBool {
    fn to_bool(&self) -> bool;
}

pub trait Zero {
    fn is_zero(&self) -> bool;
}

pub trait LastBit {
    fn is_last_bit_one(&self) -> bool;
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
                if x.is_last_bit_one() {
                    return i * 8 + j;
                }

                x >>= 1;
            }
        }

        N * 8
    }
}
