pub fn jaccard_similarity_sorted<F>(
    xs: &mut impl Iterator<Item = F>,
    ys: &mut impl Iterator<Item = F>,
) -> (u64, u64)
where
    F: PartialEq + PartialOrd,
{
    let mut ttl = 0u64;
    let mut cmn = 0u64;

    fn get_other<F>(head: Option<F>, other: &mut impl Iterator<Item = F>) -> Option<F> {
        match head {
            Some(_) => head,
            None => other.next(),
        }
    }

    fn inspect<F: PartialEq + PartialOrd>(
        this: &mut impl Iterator<Item = F>,
        head: Option<F>,
        other: &mut impl Iterator<Item = F>,
        ttl: &mut u64,
        cmn: &mut u64,
    ) {
        let x = this.next();
        let y = get_other(head, other);
        match (x, y) {
            (None, None) => (),
            (Some(_x), None) => {
                *ttl += 1;
                inspect(this, None, other, ttl, cmn);
            }
            (None, Some(_y)) => {
                *ttl += 1;
                inspect(other, None, this, ttl, cmn);
            }
            (Some(x), Some(y)) if x < y => {
                *ttl += 1;
                inspect(this, Some(y), other, ttl, cmn);
            }
            (Some(x), Some(y)) if y < x => {
                *ttl += 1;
                inspect(other, Some(x), this, ttl, cmn);
            }
            (Some(x), Some(y)) if x == y => {
                *ttl += 1;
                *cmn += 1;
                inspect(this, None, other, ttl, cmn);
            }
            _ => panic!("We should not be here"),
        }
    }

    inspect(xs, None, ys, &mut ttl, &mut cmn);

    (cmn, ttl)
}

pub fn jaccard_similarity_bits<'a>(
    xs: &mut impl Iterator<Item = &'a bool>,
    ys: &mut impl Iterator<Item = &'a bool>,
) -> (u64, u64) {
    let mut ttl = 0u64;
    let mut cmn = 0u64;

    xs.zip(ys).for_each(|(x, y)| {
        if x | y {
            ttl += 1;

            if x & y {
                cmn += 1;
            }
        }
    });

    (cmn, ttl)
}

pub fn get_f64(ct: (u64, u64)) -> f64 {
    if ct.1 == 0 {
        0f64
    } else {
        ct.0 as f64 / ct.1 as f64
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn jaccard_similarity_sorted_() {
        let xs = vec![1, 2, 3, 4, 5, 6];
        let ys = vec![3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];

        let j1 = jaccard_similarity_sorted(&mut xs.iter(), &mut ys.iter());
        println!("SORTED j1: {:?} => {}", j1, get_f64(j1));

        let j2 = jaccard_similarity_sorted(&mut ys.iter(), &mut xs.iter());
        println!("SORTED j2: {:?} => {}", j2, get_f64(j2));

        assert_eq!(j1, j2);
    }

    #[test]
    fn jaccard_similarity_bits_() {
        let xs = vec![
            true, false, true, false, true, false, true, true, false, false, true, false, true,
            false,
        ];
        let ys = vec![
            true, false, true, false, false, false, false, true, false, false, true, false, false,
            false,
        ];

        let j1 = jaccard_similarity_bits(&mut xs.iter(), &mut ys.iter());
        println!("BITS j1: {:?} => {}", j1, get_f64(j1));

        let j2 = jaccard_similarity_bits(&mut ys.iter(), &mut xs.iter());
        println!("BITS j2: {:?} => {}", j2, get_f64(j2));

        assert_eq!(j1, j2);
    }
}
