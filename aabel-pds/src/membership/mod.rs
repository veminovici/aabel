mod bloom_counter;
mod bloom_filter;
mod to_indexes;

pub use bloom_counter::*;
pub use bloom_filter::*;
pub use to_indexes::*;

pub trait ComputeOptimal {
    fn compute_optimal(items: usize, false_positive_rate: f64) -> (usize, usize) {
        let m = optimal_m(items, false_positive_rate);
        let k = optimal_k(items, m);

        (m, k)
    }
}

impl<T> ComputeOptimal for T {}

fn optimal_m(num_items: usize, false_positive_rate: f64) -> usize {
    -(num_items as f64 * false_positive_rate.ln() / (2.0f64.ln().powi(2))).ceil() as usize
}

fn optimal_k(n: usize, m: usize) -> usize {
    let k = (m as f64 / n as f64 * 2.0f64.ln()).round() as usize;
    if k < 1 {
        1
    } else {
        k
    }
}
