use std::num::NonZeroUsize;

/// A k-shingle generator
/// The k should be picked large enough that the probability of any
/// given shingle appearing in any given document is low.
///
/// ## Values for k
/// Consider the alphabet to have 20 characters,
///  therefore there will be 20^k possible shingles.
/// Common values for k are 5 and 9.
pub struct Shingles<'a, T, P> {
    k: NonZeroUsize,
    slice: &'a [T],
    predicate: P,
}

#[inline]
pub fn shingles<T, P>(slice: &[T], predicate: P, k: usize) -> Shingles<'_, T, P> {
    Shingles {
        k: NonZeroUsize::new(k).expect("The k must be a positive number"),
        slice,
        predicate,
    }
}

#[inline]
pub fn shingles_all<T>(slice: &[T], k: usize) -> Shingles<'_, T, impl Fn(&T) -> bool> {
    shingles(slice, |_: &T| true, k)
}

#[inline]
pub fn shingles5<T, P>(slice: &[T], predicate: P) -> Shingles<'_, T, P> {
    shingles(slice, predicate, 5)
}

#[inline]
pub fn shingles5_all<T>(slice: &[T]) -> Shingles<'_, T, impl Fn(&T) -> bool> {
    shingles5(slice, |_: &T| true)
}

#[inline]
pub fn shingles9<T, P>(slice: &[T], predicate: P) -> Shingles<'_, T, P> {
    shingles(slice, predicate, 9)
}

#[inline]
pub fn shingles9_all<T>(slice: &[T]) -> Shingles<'_, T, impl Fn(&T) -> bool> {
    shingles9(slice, |_: &T| true)
}

impl<'a, T, P> Iterator for Shingles<'a, T, P>
where
    P: Fn(&T) -> bool,
{
    type Item = &'a [T];

    fn next(&mut self) -> Option<Self::Item> {
        // Do we have enough elements remaining to populate a shingle?
        if self.k.get() > self.slice.len() {
            return None;
        }

        // Create a new shingle is the element is a starting element
        if (self.predicate)(&self.slice[0]) {
            let shingle = &self.slice[..self.k.get()];
            self.slice = &self.slice[1..];
            return Some(shingle);
        }

        // Don't create a shingle, just move to the next element
        self.slice = &self.slice[1..];
        self.next()
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn every_element() {
        let xs = [1, 2, 3];
        const K: usize = 2;
        let is_start = |_: &i32| true;

        let mut ss = shingles(xs.as_slice(), is_start, K);

        assert_eq!(Some([1, 2].as_slice()), ss.next());
        assert_eq!(Some([2, 3].as_slice()), ss.next());
        assert!(ss.next().is_none());
    }

    #[test]
    fn all() {
        let xs = [1, 2, 3];
        const K: usize = 2;

        let mut ss = shingles_all(xs.as_slice(), K);

        assert_eq!(Some([1, 2].as_slice()), ss.next());
        assert_eq!(Some([2, 3].as_slice()), ss.next());
        assert!(ss.next().is_none());
    }

    #[test]
    fn news() {
        const K: usize = 3;
        let text: Vec<_> = "A spokeperson for the Sudzo Corporation \
        revealed today that studies have shown it is good for people \
        to buy Sudzo products"
            .split_whitespace()
            .collect();
        let stop_words = ["A", "for", "the", "to", "that"].as_slice();
        let is_stop_word = |w: &&str| stop_words.contains(w);

        let mut ss = shingles(text.as_slice(), is_stop_word, K);

        assert_eq!(Some(["A", "spokeperson", "for"].as_slice()), ss.next());
        assert_eq!(Some(["for", "the", "Sudzo"].as_slice()), ss.next());
        assert_eq!(Some(["the", "Sudzo", "Corporation"].as_slice()), ss.next());
    }
}
