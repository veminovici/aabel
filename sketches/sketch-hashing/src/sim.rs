pub fn sim1(mhsig: &Vec<Vec<usize>>, d1: usize, d2: usize) -> (usize, usize) {
    let mut cmn = 0usize;
    let mut ttl = 0usize;

    mhsig.iter().for_each(|row| {
        ttl += 1;

        if row[d1] == row[d2] {
            cmn += 1;
        }
    });

    (cmn, ttl)
}
