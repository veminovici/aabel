use std::{
    mem::size_of,
    ops::{BitAnd, BitAndAssign, BitOrAssign},
};

pub trait Zero {
    fn is_zero(&self) -> bool;
}

impl Zero for u8 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

impl Zero for u16 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

impl Zero for u32 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

impl Zero for u64 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

impl Zero for u128 {
    fn is_zero(&self) -> bool {
        self == &0
    }
}

pub trait BitOps: BitOrAssign + BitAndAssign + BitAnd<Output = Self> + Sized {}

impl BitOps for u8 {}
impl BitOps for u16 {}
impl BitOps for u32 {}
impl BitOps for u64 {}
impl BitOps for u128 {}

pub struct SlotIndex(usize);

impl From<SlotIndex> for usize {
    fn from(value: SlotIndex) -> Self {
        value.0 as Self
    }
}

pub struct Offset(u8);

impl From<Offset> for usize {
    fn from(value: Offset) -> Self {
        value.0 as Self
    }
}

pub struct BitPosition {
    slot_index: SlotIndex,
    offset: Offset,
}

pub trait PrivateBits {
    type Slot: Copy + Clone + Zero + BitOps + 'static;

    const SET_MASKS: &'static [Self::Slot];
    const RESET_MASKS: &'static [Self::Slot];

    fn slot_mut(&mut self, index: SlotIndex) -> &mut Self::Slot;

    fn slot(&self, index: SlotIndex) -> &Self::Slot;

    fn number_of_slots(bits: usize) -> usize {
        if bits == 0 {
            return 0;
        }

        let bits_per_slot = size_of::<Self::Slot>() * 8;
        let len = (bits - 1) / bits_per_slot;
        len + 1
    }

    fn into_position(bit_index: usize) -> BitPosition {
        let bits_per_slot = size_of::<Self::Slot>() * 8;

        let slot_index = SlotIndex(bit_index / bits_per_slot);
        let offset = Offset((bits_per_slot - bit_index % bits_per_slot - 1) as u8);

        BitPosition { slot_index, offset }
    }

    fn set_bit(&mut self, bit_index: usize) {
        let position = Self::into_position(bit_index);

        let mask = Self::SET_MASKS[usize::from(position.offset)];
        let slot = self.slot_mut(position.slot_index);

        *slot |= mask;
    }

    fn reset_bit(&mut self, bit_index: usize) {
        let position = Self::into_position(bit_index);

        let mask = Self::SET_MASKS[usize::from(position.offset)];
        let slot = self.slot_mut(position.slot_index);

        *slot &= mask;
    }

    fn get_bit(&mut self, bit_index: usize) -> bool {
        let position = Self::into_position(bit_index);

        let mask = Self::SET_MASKS[usize::from(position.offset)];
        let slot = self.slot(position.slot_index);

        (*slot & mask).is_zero()
    }
}
