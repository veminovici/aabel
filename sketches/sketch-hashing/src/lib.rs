mod hash_ext;
mod hasher_ext;
mod intoiterator_ext;
mod marsenne_consts;
mod random;
mod univ_hasher;

use std::{hash::Hasher, ops::Deref};

pub use hash_ext::*;
pub use hasher_ext::*;
pub use intoiterator_ext::*;
pub use marsenne_consts::*;
pub use random::*;
pub use univ_hasher::*;

fn first_index_by<T, Source, H, F>(source: Source, hasher: &mut H, f: &mut F) -> Option<usize>
where
    Source: Deref<Target = [T]>,
    F: FnMut(&T) -> bool,
    H: Hasher,
{
    let n = source.deref().len();
    let indexes = hasher.permmutate_indexes(n);

    let mut predicate = move |i| {
        let item = source.deref().get(i).unwrap();
        f(item)
    };

    indexes.first_by(&mut predicate)
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn simple_() {
        let hasher = std::collections::hash_map::DefaultHasher::default();
        let (k, q) = random::get_u64pair();
        let mut hasher = UniversalHasher::with_hasher_m31(hasher, k, q);

        let n = 100;
        let indexes = hasher.permmutate_indexes(n);
        println!("indexes={indexes:?}");

        let res = indexes.first_by(&mut |x| x % 5 == 0);
        println!("res={res:?}");
    }
}
