use log::info;

fn main() {
    env_logger::init();
    info!("Starting the example CUCKOO");

    // Create the cuckoo filter.
    const BUCKETS: usize = 12;
    const SLOTS: usize = 1;
    let mut filter = cuckoo_filter::CuckooFilter::<BUCKETS, SLOTS>::new();

    // Insert the first element
    let r = filter.insert(&"A");
    assert!(r);
    info!("CUCKOO_DBG {:?}", filter);

    // Insert the second element.
    let r = filter.insert(&"A");
    assert!(r);
    info!("CUCKOO_DBG {:?}", filter);

    info!("Finished the example");
}