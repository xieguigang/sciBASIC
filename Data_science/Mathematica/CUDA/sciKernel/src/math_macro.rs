macro_rules! math_log {
    ($x: expr) => {
        $x.ln();
    };
}

macro_rules! math_sqrt {
    ($x: expr) => {
        $x.sqrt();
    };
}

macro_rules! math_cos {
    ($x: expr) => {
        $x.cos();
    };
}

macro_rules! math_sin {
    ($x: expr) => {
        $x.sin();
    };
}

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