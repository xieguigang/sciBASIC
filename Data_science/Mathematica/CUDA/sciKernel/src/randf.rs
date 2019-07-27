pub mod crandn {

    const Mlng : i64 =  2147483648;
    const namda : i64 = 314159269;
    const C: i64 = 453806245;
    const PI: f64 = std::f64::consts::PI;

    #[no_mangle]
    pub extern fn randn(m: usize, seed: i32) -> Vec<f64> {
        let mut gauss: Vec<f64> = Vec::new();
        let mut tmp1: f64;
        let mut u : Vec<f64>;
        let mut k : usize;
        let stop: usize;

        if m % 2 == 0 {
            u = rand(m, seed);
            stop = m;
        } else {
            u = rand(m + 1, seed);
            stop = m - 1;
        }

        while k < stop {
            tmp1 = math_sqrt!(-2.0 * math_log!(1.0 - u[k]));
            gauss[k] = tmp1 * math_cos!(2.0 * PI * u[k + 1]);
            gauss[k + 1] = tmp1 * math_sin!(2.0 * PI * u[k + 1]);

            k = k + 2;
        }

        if m % 2 == 0 {
            tmp1 = math_sqrt!(-2.0 * math_log!(1.0 - u[m - 1]));
            gauss[m - 1] = tmp1 * math_cos!(2.0 * PI * u[m]);
        }

        return gauss;
    }

    #[no_mangle]
    pub extern fn rand(m: usize, seed: i32) -> Vec<f64> {
        let mut goal : Vec<f64> = Vec::new();
        let mut x0 : i64 = seed as i64;
        let mut x1: i64;

        for k in 0..m {
            x1 = (x0 * namda + C) % Mlng;
            goal[k] = (x1 as f64) * 1.0 / (Mlng as f64);
            x0 = x1;
        }

        return goal;
    }
}