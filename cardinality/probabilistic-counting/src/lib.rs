use std::hash::Hasher;

pub struct MyHasher {
    cities: &'static [(&'static [u8], u64)],
    bytes: Vec<u8>,
}

impl MyHasher {
    pub fn new(cities: &'static [(&'static [u8], u64)]) -> Self {
        Self {
            cities,
            bytes: vec![],
        }
    }
}

impl Hasher for MyHasher {
    fn finish(&self) -> u64 {
        for city in self.cities {
            // println!("bytes: {:?} city={:?}", self.bytes, city.0);
            if self.bytes.starts_with(city.0) {
                return city.1;
            }
        }

        0
    }

    fn write(&mut self, bytes: &[u8]) {
        self.bytes.extend_from_slice(bytes)
    }
}


fn lsb(n: u64) -> usize {
    let mut n = n;
    for i in 0..64 {
        if n & 1 == 1 {
            return i as usize;
        }

        n = n >> 1;
    }

    64
}


#[cfg(test)]
mod utests {
    use std::hash::Hash;

    use super::*;

    const ATHENS: &[u8] = "Athens".as_bytes();
    const BERLIN: &[u8] = "Berlin".as_bytes();
    const KIEV: &[u8] = "Kiev".as_bytes();
    const LISBON: &[u8] = "Lisbon".as_bytes();
    const LONDON: &[u8] = "London".as_bytes();
    const MADRID: &[u8] = "Madrid".as_bytes();
    const PARIS: &[u8] = "Paris".as_bytes();
    const ROME: &[u8] = "Rome".as_bytes();
    const VIENNA: &[u8] = "Vienna".as_bytes();
    const WASHINGTON: &[u8] = "Washington".as_bytes();

    const CITIES: [(&[u8], u64); 10] = [
        (ATHENS, 4161497820),
        (BERLIN, 3680793991),
        (KIEV, 3491299693),
        (LISBON, 629555247),
        (LONDON, 3450927422),
        (MADRID, 2970154142),
        (PARIS, 2673248856),
        (ROME, 50122705),
        (VIENNA, 3271070806),
        (WASHINGTON, 4039747979),
    ];

    fn hash_str(value: &str) -> u64 {
        let mut hasher = MyHasher::new(CITIES.as_slice());
        value.hash(&mut hasher);
        hasher.finish()
    }
    
    fn test_city(city: &str) {
        
        let h = hash_str(city);
        let m = 3;
        let r = h % m;
        let q = h / m;
        println!("{city}: {} | {h:b} | {h} | {r} | {q} | {q:b} | {}", lsb(h), lsb(q));
    }

    #[test]
    fn simple_() {
        test_city("Athens");
        test_city("Berlin");
        test_city("Kiev");
        test_city("Lisbon");
        test_city("London");
        test_city("Madrid");
        test_city("Paris");
        test_city("Rome");
        test_city("Vienna");
        test_city("Washington");
    }

    // #[test]
    // fn sizes() {
    //     println!("u8: {}", size_of::<u8>() * 8);
    //     println!("u16: {}", size_of::<u16>() * 8);
    //     println!("u32: {}", size_of::<u32>() * 8);
    //     println!("u64: {}", size_of::<u64>() * 8);
    //     println!("u128: {}", size_of::<u128>() * 8);
    // }
}
