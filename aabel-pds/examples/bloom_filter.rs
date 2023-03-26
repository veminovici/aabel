use aabel_pds::BloomFilter;
use log::info;

fn main() {
    env_logger::init();
    info!("Starting the BLOOM example");

    let mut filter = BloomFilter::<usize>::new(100, 10);
    filter.insert(&10);
    let res = filter.contains(&10);
    println!("Contains: {res}");
}
