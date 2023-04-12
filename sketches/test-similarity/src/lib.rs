pub fn jaccard_similarity<F>(
    xs: &mut impl Iterator<Item = F>,
    ys: &mut impl Iterator<Item = F>,
) -> (u64, u64)
where
    F: PartialEq + PartialOrd,
{
    let mut ttl = 0u64;
    let mut cmn = 0u64;
    // we expect that the collections are sorted.

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
    fn jaccard_similarity_() {
        let mut xs = vec![1, 2, 3, 4, 5, 6];
        let mut ys = vec![3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];
        let j1 = jaccard_similarity(&mut xs.iter_mut(), &mut ys.iter_mut());
        println!("j1={:04}", get_f64(j1));

        let j2 = jaccard_similarity(&mut ys.iter_mut(), &mut xs.iter_mut());
        println!("j2={:04}", get_f64(j2));

        assert_eq!(j1, j2);
    }
}
