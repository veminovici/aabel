use bits::{Bits, Bits8};

fn main() {
    let mut bits = Bits8::<2>::new();
    bits.merge_u16(10);
    println!("Bits: {}", bits.pretty());

    bits.reset(1);
    println!("Bits: {}", bits.pretty());

    bits.set(0);
    println!("Bits: {}", bits.pretty());

    bits.reset(0);
    println!("Bits: {}", bits.pretty());

    let lsb = bits.lsb();
    println!("LSB: {lsb}");
}
