#[cfg(feature = "city")]
mod city64;

#[cfg(feature = "farm")]
mod farm64;

#[cfg(feature = "fnv")]
mod fnv64;

mod hash_ext;

#[cfg(feature = "murmur")]
mod murmur32;

pub mod hash {
    #[cfg(feature = "city")]
    pub use crate::city64;

    #[cfg(feature = "farm")]
    pub use crate::farm64::*;
    
    #[cfg(feature = "fnv")]
    pub use crate::fnv64::*;
    
    pub use crate::hash_ext::*;

    #[cfg(feature = "murmur")]
    pub use crate::murmur32::*;

    #[cfg(feature = "sip")]
    pub use siphasher::sip128::*;
}
