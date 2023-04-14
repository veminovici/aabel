use std::ops::Deref;

use sprs::CsMat;

pub struct Matrix<N> {
    matrix: CsMat<N>
}