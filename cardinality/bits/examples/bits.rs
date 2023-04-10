use bits::{Bits, BitsU8};

fn main() {
    let mut bits = BitsU8::with_capacity(15);
    bits.set(1);
    bits.set(2);
    bits.reset(3);
}
