'use strict';

const test = require('tap').test;
const ttest = require('../../hypothesis.js');
const equals = require('../equals.js');

const summary = require('summary');

test('testing not equal alternative', function (t) {
  const res = ttest([1, 2, 2, 2, 4], [0, 3, 3, 3, 2], {
    mu: 1,
    varEqual: false,
    alpha: 0.05,
    alternative: 'not equal'
  });

  equals(t, res, {
    valid: true,
    freedom: 7.769053117782910966582,

    pValue: 0.2266153983841363017682,
    testValue: -1.313064328597225660644,

    confidence: [
      -1.765329169241792062195,
      1.765329169241792062195
    ]
  });

  t.end();
});

test('testing not equal alternative', function (t) {
  const res = ttest(summary([1, 2, 2, 2, 4]), summary([0, 3, 3, 3, 2]), {
    mu: 1,
    varEqual: false,
    alpha: 0.05,
    alternative: 'not equal'
  });

  equals(t, res, {
    valid: true,
    freedom: 7.769053117782910966582,

    pValue: 0.2266153983841363017682,
    testValue: -1.313064328597225660644,

    confidence: [
      -1.765329169241792062195,
      1.765329169241792062195
    ]
  });

  t.end();
});

test('testing less alternative', function (t) {
  const res = ttest([1, 2, 2, 2, 4], [0, 3, 3, 3, 2], {
    mu: 1,
    varEqual: false,
    alpha: 0.05,
    alternative: 'less'
  });

  equals(t, res, {
    valid: true,
    freedom: 7.769053117782910966582,

    pValue: 0.1133076991920681508841,
    testValue: -1.313064328597225660644,

    confidence: [
      -Infinity,
      1.42166652935669435287
    ]
  });

  t.end();
});

test('testing greater alternative', function (t) {
  const res = ttest([1, 2, 2, 2, 4], [0, 3, 3, 3, 2], {
    mu: 1,
    varEqual: false,
    alpha: 0.05,
    alternative: 'greater'
  });

  equals(t, res, {
    valid: true,
    freedom: 7.769053117782910966582,

    pValue: 0.8866923008079318213603,
    testValue: -1.313064328597225660644,

    confidence: [
      -1.421666529356694574915,
      Infinity
    ]
  });

  t.end();
});
