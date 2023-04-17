use sketch_hashing::*;

fn mhs() {
    // We have 5 documents, each of them with or without 19 features
    let documents = [
        [0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0].to_vec(),
        [0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0].to_vec(),
        [1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0].to_vec(),
        [0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0].to_vec(),
        [0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1].to_vec(),
    ]
    .to_vec();

    // Have 4 hash functions, generate 4 permutations for our (0..19) indexes.
    let indexes: Vec<Vec<usize>> = [(22, 5, 31), (30, 2, 31), (21, 23, 31), (15, 6, 31)]
        .map(|(k, q, p)| {
            (0..19)
                .map(|i| MyHasher::get_hash(k, q, p, i) % 19)
                .map(|x| x as usize)
                .collect()
        })
        .to_vec();

    // Minhash-Sig
    let mhs = min_hash_sig(&documents, &indexes, 19);

    let sim12 = sim1(&mhs, 1, 2);
    println!(
        "SIM 1-2: {:?}={:02}",
        sim12,
        sim12.0 as f64 / sim12.1 as f64
    );
    assert_eq!(sim12, (1, 4));

    let sim34 = sim1(&mhs, 3, 4);
    println!(
        "SIM 3-4: {:?}={:02}",
        sim34,
        sim34.0 as f64 / sim34.1 as f64
    );
    assert_eq!(sim34, (3, 4));

    let sim02 = sim1(&mhs, 0, 2);
    println!(
        "SIM 0-2: {:?}={:02}",
        sim02,
        sim02.0 as f64 / sim02.1 as f64
    );
    assert_eq!(sim02, (0, 4));
}

fn main() {
    mhs()
}