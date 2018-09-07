'use strict';

function equals(t, actual, expected) {

  t.strictEqual(actual.valid(), expected.valid, 'valid result match');

  t.equal(actual.freedom(), expected.freedom, 'degree of freedom match');

  t.ok(Math.abs(actual.testValue() - expected.testValue) <= 0.0000005, 'test value match');

  t.ok(Math.abs(actual.pValue() - expected.pValue) <= 0.0000005, 'p value match');

  if (Number.isFinite(expected.confidence[0])) {
    t.ok(Math.abs(actual.confidence()[0] - expected.confidence[0]) <= 0.0000005, 'left confidence match');
  } else {
    t.equal(actual.confidence()[0], expected.confidence[0], 'left confidence match');
  }

  if (Number.isFinite(expected.confidence[1])) {
    t.ok(Math.abs(actual.confidence()[1] - expected.confidence[1]) <= 0.0000005, 'right confidence match');
  } else {
    t.equal(actual.confidence()[1], expected.confidence[1], 'right confidence match');
  }
}
module.exports = equals;
