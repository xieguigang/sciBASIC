// #[link(name = "sciKernel", vers = "1.01", author = "xieguigang")];
// #[crate_type = "dylib"];

/// get number sign
macro_rules! f64_sign {
    ($x: expr) => {
        if $x > 0.0 {
            1
        } else if $x < 0.0 {
            -1
        } else {
            0
        }
    };
}

/// Pearson correlations
#[no_mangle]
pub extern fn pearson(x: &[f64], y: &[f64], size: i32) -> f64 {
    let n : usize = size as usize;
    let mut yt: f64;
    let mut xt: f64;
    let mut syy = 0.0;
    let mut sxy = 0.0;
    let mut sxx = 0.0;
    let mut ay = 0.0;
    let mut ax = 0.0;

    // finds the mean
    for j in 0..n {
        ax = ax + x[j];
        ay = ay + y[j];
    }

    ax = ax / (n as f64);
    ay = ay / (n as f64);

    // compute correlation coefficient
    for j in 0..n {
        xt = x[j] - ax;
        yt = y[j] - ay;
        sxx = sxx + xt * xt;
        syy = syy + yt * yt;
        sxy = sxy + xt * yt;
    }

    // will regularize the unusual case of complete correlation
    let TINY : f64 = 1.0E-20;
    let pcc = sxy / ((sxx * syy).sqrt() + TINY);

    return pcc;
}

/// 1/2 PI
const halfPI: f64 = std::f64::consts::PI / 2.0;

/// Taylor Atan
#[no_mangle]
pub extern fn atn(x: f64, atanPrecise: i32) -> f64 { 
    if x == 1.0 {
        return std::f64::consts::PI  / 4.0;
    } else if f64_sign!(x) == -1 {
        return -atn(-1.0 * x, atanPrecise);
    }

    if x > 1.0 {
        return halfPI - atn(1.0 / x, atanPrecise);
    } else if x > 0.5 {
        return atn(1.0, atanPrecise) + atn((x - 1.0) / (1.0 + x), atanPrecise);
    }

    let xPow2 = x * x;
    let mut n1 : i32 = atanPrecise;
    let mut y = 1.0 / (2.0 * (n1 as f64) + 1.0);
    let mut i = atanPrecise;

    while i > 0 {
        y = (1.0 / (2.0 * (n1 as f64) - 1.0)) - (xPow2 * y);
        i = i - 1;
        n1 = n1 - 1;
    }

    return x * y;
}