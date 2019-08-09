include!("math_macro.rs");
include!("statistics/statistics.rs");
include!("statistics/fishers_exact/fishers.rs");
include!("trigonometric.rs");
include!("randf.rs");
include!("gamma.rs");
include!("beta.rs");

/// #[link(name = "sciKernel", vers = "1.01", author = "xieguigang")];
/// #[crate_type = "dylib"];

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

/// Calculate euclidean distance between two vector.
#[no_mangle]
pub extern fn distance(a: &[f64], b: &[f64], size: i32) -> f64 {
    let mut magnitude = 0.0;

    for i in (0..size as usize) {
        magnitude = magnitude + (a[i] - b[i]).powf(2.0);
    }

    return magnitude.sqrt();
}
