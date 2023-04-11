use std::hash::Hasher;

pub struct MyHasher {
    cities: &'static [(&'static str, u64)],
    bytes: Vec<u8>,
}

impl MyHasher {
    pub fn new(cities: &'static [(&'static str, u64)]) -> Self {
        Self {
            cities,
            bytes: vec![],
        }
    }
}

impl Hasher for MyHasher {
    fn finish(&self) -> u64 {
        for city in self.cities {
            if self.bytes.starts_with(city.0.as_bytes()) {
                return city.1;
            }
        }

        0
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes)
    }
}

#[cfg(test)]
mod utests {
    use std::hash::Hash;

    use bits::{Bits, Bits8};

    use super::*;

    const CITIES: [(&str, u64); 10] = [
        ("Athens", 4161497820),
        ("Berlin", 3680793991),
        ("Kiev", 3491299693),
        ("Lisbon", 629555247),
        ("London", 3450927422),
        ("Madrid", 2970154142),
        ("Paris", 2673248856),
        ("Rome", 50122705),
        ("Vienna", 3271070806),
        ("Washington", 4039747979),
    ];

    fn lsb(n: u64) -> usize {
        let mut n = n;
        for i in 0..64 {
            if n & 1 == 1 {
                return i as usize;
            }
    
            n >>= 1;
        }
    
        64
    }
    
    fn hash_str(value: &str) -> u64 {
        let mut hasher = MyHasher::new(CITIES.as_slice());
        value.hash(&mut hasher);
        hasher.finish()
    }

    #[test]
    fn simple_() {
        let mut bits = Bits8::<8>::default();

        let _: Vec<_> = CITIES.iter().inspect(|city| {
            let h = hash_str(city.0);
            let r = lsb(h);
            bits.set(r);

            println!("{}: {h} | {h:b} | {r}", city.0);
        }).collect();

        let lzb = bits.lzb();
        println!("Bits: {} - LZB={lzb}", bits.pretty());

        let n = 2u64.pow(lzb as u32) as f64 / 0.77351;
        println!("n={n}");
    }

    #[test]
    fn fm() {
        let m = 3;
        let mut bits = vec![
            Bits8::<8>::default(),
            Bits8::<8>::default(),
            Bits8::<8>::default(),
        ];

        let _: Vec<_> = CITIES
            .iter()
            .inspect(|city| {
                let h = hash_str(city.0);
                let r = h % m;
                let q = h / m;
                let j = lsb(q);

                println!("{}: {h} | {r} | {q} | {j}", city.0);

                let bs = &mut bits[r as usize];
                bs.set(j);
            })
            .collect();

        let rs: Vec<usize> = bits
            .iter()
            .enumerate()
            .map(|(i, bs)| {
                let lzb = bs.lzb();
                println!("BITS{i}: {} | {lzb}", bs.pretty());
                lzb
            })
            .collect();

        let r: usize = rs.iter().sum();
        let n = 3f64 * 2f64.powf(r as f64 / 3f64) / 0.77351;
        println!("n={n}");
    }
}
