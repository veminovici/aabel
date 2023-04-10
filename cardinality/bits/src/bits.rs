use crate::private::PrivateBits;

pub trait Bits: PrivateBits {
    fn with_capacity(capacity: usize) -> Self;

    fn set(&mut self, bit_index: usize) {
        self.set_bit(bit_index)
    }

    fn reset(&mut self, bit_index: usize) {
        self.reset_bit(bit_index)
    }

    fn get(&mut self, bit_index: usize) -> bool {
        self.get_bit(bit_index)
    }
}
