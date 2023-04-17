use std::hash::Hasher;

use crate::HashExt;

pub trait HasherExt: Hasher {
    fn permmutate_indexes(&mut self, n: usize) -> Vec<usize>;
}

impl<H: Hasher> HasherExt for H {
    fn permmutate_indexes(&mut self, n: usize) -> Vec<usize> {
        (0..n).map(|i| i.hash64(self) as usize % n).collect()
    }
}

#[cfg(test)]
mod utests {
    use std::cmp::min;

    use crate::{random, UniversalHasher, MyHasher, min_hash_sig, sim1};

    use super::*;

    #[test]
    fn simple_() {
        let hasher = std::collections::hash_map::DefaultHasher::default();
        let (k, q) = random::get_u64pair();
        let mut hasher = UniversalHasher::with_hasher_m31(hasher, k, q);

        let n = 100;
        let indexes = hasher.permmutate_indexes(n);

        assert_eq!(n, indexes.len());
        assert!(indexes.iter().all(|i| i < &n));
    }

    fn pretty_usize(v: usize) -> String {
        if v == usize::MAX {
            format!("__")
        } else {
            format!("{v:02}")
        }
    }

    fn prett_mhs_row(cols: &[usize; 5]) -> String {
        format!(
            "{} {} {} {} {}",
            pretty_usize(cols[0]),
            pretty_usize(cols[1]),
            pretty_usize(cols[2]),
            pretty_usize(cols[3]),
            pretty_usize(cols[4])
        )
    }

    fn pretty_mhs(mhsig: &[[usize; 5]; 4]) {
        mhsig
            .iter()
            .enumerate()
            .for_each(|(i, row)| println!("H{i}: {}", prett_mhs_row(row)))
    }

    fn sim(mhsig: &[[usize; 5]; 4], d1: usize, d2: usize) -> (usize, usize) {
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

    #[test]
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

    #[test]
    fn myhasher_() {
        // We have 5 documents, each of them with or without 19 features
        let documents = [
            [0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0],
            [0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0],
            [1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0],
            [0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0],
            [0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1],
        ];

        // Have 4 hash functions, generate 4 permutations for our (0..19) indexes.
        let indexes: [Vec<usize>; 4] =
            [(22, 5, 31), (30, 2, 31), (21, 23, 31), (15, 6, 31)].map(|(k, q, p)| {
                (0..19)
                    .map(|i| MyHasher::get_hash(k, q, p, i) % 19)
                    .map(|x| x as usize)
                    .collect()
            });

        //min_hash_sig(5, 4, 19, &documents, &indexes);

        indexes
            .iter()
            .enumerate()
            .for_each(|(i, idx)| println!("INDEXES {i}: {idx:?}"));

        // We have a matrix with 4 rows (one row for each hash function) and 5 columns (one column for each document)
        let mut mhs = [
            [usize::MAX; 5],
            [usize::MAX; 5],
            [usize::MAX; 5],
            [usize::MAX; 5],
        ];

        pretty_mhs(&mhs);

        (0..19).for_each(|fidx| {
            let idx: Vec<_> = indexes.iter().map(|v| v[fidx]).collect();
            println!("FEATURE {fidx}: {:?}", idx);

            documents.iter().enumerate().for_each(|(col, doc)| {
                if doc[fidx] == 0 {
                    return;
                }

                idx.iter().enumerate().for_each(|(row, value)| {
                    mhs[row][col] = min(mhs[row][col], *value);
                })
            });

            pretty_mhs(&mhs);
        });

        let sim12 = sim(&mhs, 1, 2);
        println!(
            "SIM 1-2: {:?}={:02}",
            sim12,
            sim12.0 as f64 / sim12.1 as f64
        );
        assert_eq!(sim12, (1, 4));

        let sim34 = sim(&mhs, 3, 4);
        println!(
            "SIM 3-4: {:?}={:02}",
            sim34,
            sim34.0 as f64 / sim34.1 as f64
        );
        assert_eq!(sim34, (3, 4));

        let sim02 = sim(&mhs, 0, 2);
        println!(
            "SIM 0-2: {:?}={:02}",
            sim02,
            sim02.0 as f64 / sim02.1 as f64
        );
        assert_eq!(sim02, (0, 4));
    }
}
