use test_similarity::shingles_with_stop_words;

fn with_stop_words() {
    const K: usize = 3;
    let text: Vec<_> = "A spokeperson for the Sudzo Corporation \
    revealed today that studies have shown it is good for people \
    to buy Sudzo products"
        .split_whitespace()
        .collect();
    let stop_words = ["A", "for", "the", "to", "that"].as_slice();

    shingles_with_stop_words(text.as_slice(), K, stop_words).for_each(|s| {
        println!("Shingle: {:?}", s);
    });
}

fn main() {
    with_stop_words()
}
