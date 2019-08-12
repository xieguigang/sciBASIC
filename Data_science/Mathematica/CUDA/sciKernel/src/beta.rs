pub mod beta {

    pub extern fn Beta(x: f64, alpha: f64, beta: f64) -> f64{
        return x.powf(alpha - 1.0) * (1.0-x).powf(beta-1.0) * (gamma::lgamma(alpha + beta) - lgamma(alpha) * lgamma(beta)); 
    }
}