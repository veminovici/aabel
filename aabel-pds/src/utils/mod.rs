mod buildhasher;
mod to_hashpair;

pub use buildhasher::*;
pub use to_hashpair::*;

pub(crate) fn generate_random_seed() -> [u8; 16] {
    let mut seed = [0u8; 32];
    getrandom::getrandom(&mut seed).unwrap();
    seed[0..16].try_into().unwrap()
}
