pub fn get_u64() -> u64 {
    let mut seed = [0u8; 8];
    getrandom::getrandom(&mut seed).unwrap();

    seed.map(|s| s as u64)
        .iter_mut()
        .enumerate()
        .fold(0u64, |acc, (i, seed)| {
            let s = *seed << (i * 8);
            acc + s
        })
}

#[inline]
pub fn get_u64pair() -> (u64, u64) {
    let x = get_u64();
    let y = get_u64();
    (x, y)
}
