pub mod trigonometric {
    
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
}