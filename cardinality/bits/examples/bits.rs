use bits::{Bits, Bits8};

fn main() {
    let mut bits = Bits8::<2>::new();
    bits.merge_u16(10);
    println!("Bits: {}", bits.pretty());

    bits.reset(0, 1);
    println!("Bits: {}", bits.pretty());

    bits.set(0, 0);
    println!("Bits: {}", bits.pretty());

    bits.reset(0, 0);
    println!("Bits: {}", bits.pretty());

    let lsb = bits.lsb();
    println!("LSB: {lsb}");
    //let mut bits = BitsU8::with_capacity(16);
    //bits.set(7);
    //bits.set(2);
    //bits.reset(3);

    //println!("{:?}", bits);
    //println!("{}", bits.pretty());
}
