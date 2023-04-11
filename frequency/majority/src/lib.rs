/// Implementation of the Boyer-Moore algorithm.
pub struct Majority<T> {
    counter: usize,
    element: Option<T>,
}

impl<T> Default for Majority<T> {
    fn default() -> Self {
        Self::new()
    }
}

impl<T> Majority<T>
{
    pub fn new() -> Self {
        Self {
            counter: 0,
            element: None,
        }
    }
}


impl<T> Majority<T>
where
    T: Copy + Clone + PartialEq,
{   
    pub fn update(&mut self, item: T) {
        if self.counter == 0 {
            self.element = Some(item);
        }

        match self.element {
            Some(x) if x == item => self.counter += 1,
            _ => self.counter -= 1,
        }
    }

    pub fn majority(xs: impl Iterator<Item = T>) -> Option<T> {
        let mut me = Self::new();
        let _: Vec<_> = xs.inspect(|x| me.update(*x)).collect();
        me.element
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn majority_() {
        let xs = [1, 2, 3, 3, 2, 1, 2, 3, 1, 1];
        let res = Majority::majority(xs.iter());
        println!("res={:?}", res);

        assert!(res.is_some());
        assert_eq!(&1, res.unwrap());
    }
}
