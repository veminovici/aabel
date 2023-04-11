use std::hash::Hasher;

pub const CITIES: [(&str, u64); 10] = [
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

pub struct MyHasher {
    cities: &'static [(&'static str, u64)],
    bytes: Vec<u8>,
}

impl Default for MyHasher {
    fn default() -> Self {
        Self::new(&CITIES)
    }
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
