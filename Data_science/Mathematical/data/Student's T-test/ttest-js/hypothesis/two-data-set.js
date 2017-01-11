'use strict';

const Distribution = require('distributions').Studentt;

const util = require('util');
const AbstactStudentT = require('./abstact.js');

function StudentT(left, right, options) {
  AbstactStudentT.call(this, options);

  this._df = left.size + right.size - 2;
  this._dist = new Distribution(this._df);

  const commonVariance = ((left.size - 1) * left.variance +
                          (right.size - 1) * right.variance
                         ) / this._df;

  this._se = Math.sqrt(commonVariance * (1 / left.size + 1 / right.size));
  this._mean = left.mean - right.mean;
}
util.inherits(StudentT, AbstactStudentT);
module.exports = StudentT;
