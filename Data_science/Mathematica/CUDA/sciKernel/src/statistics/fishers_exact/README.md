# Fisher's exact test

> https://github.com/cpearce/fishers_exact

[![Build Status](https://travis-ci.org/cpearce/fishers_exact.svg?branch=master)](https://travis-ci.org/cpearce/fishers_exact)

Implements a 2×2 Fishers exact test. Use this to test the independence of two
categorical variables when the sample sizes are small.

For an approachable explanation of Fisher's exact test, see
[Fisher's exact test of independence](http://www.biostathandbook.com/fishers.html) by
John H. McDonald in the [Handbook of Biological Statistics](http://www.biostathandbook.com/).

The test is computed using code ported from Øyvind Langsrud's JavaScript
implementation at [http://www.langsrud.com/fisher.htm](http://www.langsrud.com/fisher.htm),
used with permission.

```
use fishers_exact::fishers_exact;

let p = fishers_exact(&[1,9,11,3]).unwrap();

assert!((p.less_pvalue - 0.001346).abs() < 0.0001);
assert!((p.greater_pvalue - 0.9999663).abs() < 0.0001);
assert!((p.two_tail_pvalue - 0.0027594).abs() < 0.0001);
```