#[cfg(feature = "city")]
mod city64;

#[cfg(feature = "farm")]
mod farm64;

#[cfg(feature = "fnv")]
mod fnv64;

mod hash128_ext;
mod hash_ext;
mod hasher_ext;

#[cfg(feature = "murmur")]
mod murmur32;

#[cfg(feature = "sip")]
mod sip24;

pub mod hash {
    pub use crate::hash128_ext::*;
    pub use crate::hash_ext::*;
    pub use crate::hasher_ext::*;

    #[cfg(feature = "city")]
    pub use crate::city64;

    #[cfg(feature = "farm")]
    pub use crate::farm64::*;

    #[cfg(feature = "fnv")]
    pub use crate::fnv64::*;

    #[cfg(feature = "murmur")]
    pub use crate::murmur32::*;

    #[cfg(feature = "sip")]
    pub use siphasher::sip128::*;

    #[cfg(feature = "sip")]
    pub use crate::sip24::*;
}
