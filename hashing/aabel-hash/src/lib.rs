#[cfg(feature = "farm")]
mod farm64;

#[cfg(feature = "fnv")]
mod fnv64;

mod hash_ext;

pub mod hash {
    #[cfg(feature = "farm")]
    pub use crate::farm64::*;
    
    #[cfg(feature = "fnv")]
    pub use crate::fnv64::*;
    
    pub use crate::hash_ext::*;
}
