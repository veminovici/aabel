use std::fmt::Debug;

#[derive(Default, Hash, PartialEq, Eq, Clone, Copy)]
pub struct Fingerprint(u8);

impl Debug for Fingerprint {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "FNG:{:04X}", self.0)
    }
}

impl From<u32> for Fingerprint {
    fn from(value: u32) -> Self {
        Self(value as u8)
    }
}

impl AsRef<u8> for Fingerprint {
    fn as_ref(&self) -> &u8 {
        &self.0
    }
}

#[cfg(test)]
mod utests {
    use super::*;

    #[test]
    fn debug_() {
        let fp = Fingerprint::from(10);
        println!("FINGERPRINT_DBG: {:?}", fp);
    }
}
