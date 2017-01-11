#ttest

> Perform the Student t hypothesis test

## Installation

```sheel
npm install ttest
```

## Example

```javascript
var ttest = require('ttest');

// One sample t-test
ttest([0,1,1,1], {mu: 1}).valid() // true

// Two sample t-test
ttest([0,1,1,1], [1,2,2,2], {mu: -1}).valid() // true
```

## Documentation

```javascript
var ttest = require('ttest');
```

The `ttest` module supports both one and two sample t-testing, and both
equal and none equal variance.

If one array of data is given its a one sample t-test, and if two data arrays
are given its a two sample t-test.

`ttest()` supports data in the following format:

* an array of values, e.g. `ttest([1, 2, 3])`
* a [`Summary`](https://github.com/AndreasMadsen/summary) object,
  e.g. `ttest(new Summary([1, 2, 3]))`
* an object with the following properties: `mean`, `variance`,
  `size`, e.g. `ttest({mean: 123, variance: 1, size: 42})`

In all cases you can also pass an extra optional object, there takes the
following properties:

```javascript
const options = {
  // Default: 0
  // One sample case: this is the µ that the mean will be compared with.
  // Two sample case: this is the ∂ value that the mean diffrence will be compared with.
  mu: Number,

  // Default: false
  // If true don't assume variance is equal and use the Welch approximation.
  // This only applies of two samples are used.
  varEqual: Boolean,

  // Default: 0.05
  // The significance level of the test
  alpha: Number,

  // Default "not equal"
  // What should the alternative hypothesis be:
  // - One sample case: could the mean be less, greater or not equal to mu property.
  // - Two sample case: could the mean diffrence be less, greater or not equal to mu property.
  alternative: "less" || "greater" || "not equal"
};
```

The t-test object is finally created by calling the `ttest` constructor.

```javascript
const stat = ttest(sample, options);
const stat = ttest(sampleA, sampleB, options);
```

When the `ttest` object is created you can get the following information.

##### stat.testValue()

Returns the `t` value also called the `statistic` value.

##### stat.pValue()

Returns the `p-value`.

##### stat.confidence()

Returns an array containing the confidence interval, where the confidence level
is calculated as `1 - options.alpha`. Where the lower limit has index `0` and
the upper limit has index `1`. If the alternative hypothesis is `less` or
`greater` one of the sides will be `+/- Infinity`.

##### stat.valid()

Simply returns true if the `p-value` is greater or equal to the `alpha` value.

##### stat.freedom()

Returns the degrees of freedom used in the t-test.
