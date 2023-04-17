use std::cmp::min;

pub fn min_hash_sig(
    documents: &Vec<Vec<i32>>,
    indexes: &Vec<Vec<usize>>,
    f: usize,
) -> Vec<Vec<usize>> {
    let h = indexes.len();
    let d = documents.len();

    let mut mhs: Vec<Vec<usize>> = (0..h)
        .map(|_| (0..d).map(|_| usize::MAX).collect::<Vec<_>>())
        .collect();

    (0..f).for_each(|fidx| {
        let idx: Vec<_> = indexes.iter().map(|ys: &Vec<usize>| ys[fidx]).collect();

        documents.iter().enumerate().for_each(|(col, doc)| {
            if doc[fidx] == 0 {
                return;
            }

            idx.iter().enumerate().for_each(|(row, value)| {
                mhs[row][col] = min(mhs[row][col], *value);
            })
        });
    });

    mhs
}

