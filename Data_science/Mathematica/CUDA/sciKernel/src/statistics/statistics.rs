pub mod statistics {

    /// calculate mean of the target data set
    #[no_mangle]
    pub extern fn average(x: &[f64]) -> f64 {
        let sum = x.iter().sum();
        let len = x.len();

        return sum / len;
    }

    #[no_mangle]
    pub extern fn stdErr(x: &[f64]) -> f64 {
        let mean : f64 = x.average();
        let sum: f64 = x.projects(|a: f64| (a - mean).powf(2.0)).sum();
        let result : f64 = (sum / x.len() as f64).sqrt();

        return result;
    }

    #[no_mangle]
    pub extern fn RSD(x: &[f64]) -> f64 {
        return x.stdErr() / x.average();
    }

    /// Returns the PDF value at x for the specified Poisson distribution.
    #[no_mangle]
    pub extern fn poissonPDF(x: i32, lambda : f64) -> f64 {
        let mut result : f64 = std::f64::consts::E.powf(-lambda);
        let mut k : i32 = x;

        while k >= 1 {
            result = result * lambda / (k as f64);
            k = k-1;
        }

        return result;
    }

    /// Root mean square.
    #[no_mangle]
    pub extern fn RMS(x: &[f64]) -> f64 {
        let sqrSum = x.projects(|a: f64| a.powf(2.0)).sum();
        let result = sqrSum / (x.len() as f64);

        return result;
    }

    impl [f64] {

        pub fn average(&self) -> f64 {
            return statistics::average(self);
        }

        pub fn stdErr(&self) -> f64 {
            return statistics::stdErr(self);
        }

        pub fn RSD(&self) -> f64 {
            return statistics::RSD(self);
        }

        pub fn RMS(&self) -> f64 {
            return statistics::RMS(self);
        }

        /// Linq function
        pub fn aggregate<F>(&self, reduce: F) -> f64 
            where F: Fn(f64, f64) -> f64 {

            let mut result = 0.0;

            for x in self {
                result = reduce(result, x);
            }

            return result;
        } 
    }

    trait Linq<T> {

        fn projects<T, V, F>(&self, projector : F) -> [V] where F : Fn(T) -> V;     
    }

    impl Linq<T> for [T] {
        fn projects<T, F>(&self, projector : F) -> [f64] where F : Fn(T) -> f64 {
            let n = self.len();
            let mut result: [f64, n];
            
            for i in 0..n {
                result[i] = projector(self[i]);
            }

            return result;
        }
    }
}