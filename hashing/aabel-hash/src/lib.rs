#[cfg(feature = "farm")]
mod farm64;

mod hash_ext;

pub mod hash {
    #[cfg(feature = "farm")]
    pub use crate::farm64::*;
    
    pub use crate::hash_ext::*;
}
