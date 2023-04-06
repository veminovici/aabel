use log::info;

fn main() {
    env_logger::init();
    info!("Starting the example CUCKOO");

    // Create the cuckoo filter with 12 buckets, each buckets has 1 slot.
    const BUCKETS: usize = 12;
    const SLOTS: usize = 1;
    let mut filter = cuckoo_filter::CuckooFilter::<BUCKETS, SLOTS>::new();

    // Insert the first element
    info!("Inserting AAAA");
    let r = filter.insert(&"AAAA");
    assert!(r);
    info!("CUCKOO_DBG {:?}", filter);

    // Insert the second element.
    info!("Inserting BBBB");
    let r = filter.insert(&"BBBB");
    assert!(r);
    info!("CUCKOO_DBG {:?}", filter);

    // Check the length
    assert_eq!(2, filter.len());
    assert!(!filter.is_empty());

    // Check if the elements are in the filter
    info!("Checking for AAAA");
    let r = filter.contains(&"AAAA");
    assert!(r);

    info!("Checking for BBBB");
    let r = filter.contains(&"BBBB");
    assert!(r);

    info!("Checking for CCCC");
    let r = filter.contains(&"CCCC");
    assert!(!r);

    info!("Finished the example");
}
