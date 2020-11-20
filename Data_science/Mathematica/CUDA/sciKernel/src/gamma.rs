pub mod gamma{

    const g : i32 = 7;
    const g_ln: f64 = 607.0 / 128.0;

    const p_ln : [f64; 15] = [
                0.99999999999999711,
                57.156235665862923,
                -59.597960355475493,
                14.136097974741746,
                -0.49191381609762019,
                0.000033994649984811891,
                0.000046523628927048578,
                -0.000098374475304879565,
                0.00015808870322491249,
                -0.00021026444172410488,
                0.00021743961811521265,
                -0.00016431810653676389,
                0.00008441822398385275,
                -0.000026190838401581408,
                0.0000036899182659531625
    ];

    const p :[f64; 9] = [
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            0.0000099843695780195716,
            0.00000015056327351493116
    ];

    pub extern fn lngamma(z: f64) -> f64 {
        if z < 0.0 {
            return 0.0;
        }

        let mut x: f64 = p_ln[0 as usize];

         for i in (p_ln.len() as i32-1)..0 {
             x = x + p_ln[i as usize] / (z + i as f64);
         }

         let t: f64 = z + g_ln + 0.5;
         let lngm = 0.5 * (2.0 * std::f64::consts::PI).ln() + (z+ 0.5) * t.ln() - t + x.ln() - z.ln();

        return lngm;
    }

    pub extern fn gamma(z: f64) -> f64 {
        if z < 0.5 {
            return std::f64::consts::PI / ((z * std::f64::consts::PI).sin() *gamma(1.0 - z));
        } else if z > 100.0 {
            return lngamma(z).exp();
        } else {
            let x : f64 = p[0 as usize];

            z = z -1.0;

             for i in 1..(g+1) {
                 x = x + (p[i  as usize]/(z+ i as f64));
             }

             let t = z + (g as f64) + 0.5;

             return (2.0 * std::f64::consts::PI).sqrt() * t.powf(z + 0.5) * (-t).exp() * x;
        }
    }

    pub extern fn lgamma(x: f64) -> f64 {
        let logterm = (x * (1.0 + x) * (2.0 + x)).ln();
        let xp3 = 3.0 + x;

        return  -2.081061 - x + 0.0833333 / xp3 - logterm + (2.5 + x) * xp3.ln();
    }
} 