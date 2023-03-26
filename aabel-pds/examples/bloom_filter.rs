use aabel_pds::BloomFilter;
use log::info;
use siphasher::sip128::SipHasher24;

fn main() {
    env_logger::init();
    info!("Starting the BLOOM example");

    let mut filter = BloomFilter::<SipHasher24, usize>::new(100, 10);
    filter.insert(&10);
    let res = filter.contains(&10);
    println!("Contains: {res}");
}
