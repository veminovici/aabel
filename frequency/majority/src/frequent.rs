use std::collections::hash_map::IntoIter;
use std::collections::HashMap;
use std::hash::Hash;

pub struct Frequent<const N: usize, T> {
    items: HashMap<T, usize>,
}

impl<const N: usize, T> Default for Frequent<N, T>
where
    T: Copy,
{
    fn default() -> Self {
        let items = HashMap::new();
        Self { items }
    }
}

impl<const N: usize, T> Frequent<N, T>
where
    T: Copy + Eq + Hash,
{
    pub fn update(&mut self, item: T) {
        if !self.items.contains_key(&item) && self.items.len() < N {
            self.items.insert(item, 0);
        }

        if self.items.get_mut(&item).map(|v| *v += 1).is_none() {
            let mut keys = vec![];
            for (k, v) in self.items.iter_mut() {
                *v -= 1;
                if *v == 0 {
                    keys.push(*k);
                }
            }

            let _: Vec<_> = keys
                .iter()
                .inspect(|k| {
                    self.items.remove(k);
                })
                .collect();
        }
    }

    pub fn frequent(xs: impl Iterator<Item = T>) -> IntoIter<T, usize> {
        let mut me = Frequent::<N, _>::default();
        let _: Vec<_> = xs.inspect(|x| me.update(*x)).collect();
        me.items.into_iter()
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn frequency_() {
        let xs = [4u64, 4, 4, 4, 6, 2, 3, 5, 4, 4, 3, 3, 4, 2, 3, 3, 3, 2].iter();
        let res = Frequent::<2, _>::frequent(xs);
        println!("res={:?}", res);
    }
}
