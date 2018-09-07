'use strict';

const Distribution = require('distributions').Studentt;

const util = require('util');
const AbstactStudentT = require('./abstact.js');

function StudentT(left, right, options) {
  AbstactStudentT.call(this, options);

  const leftSE = left.variance / left.size;
  const rightSE = right.variance / right.size;
  const commonVariance = leftSE + rightSE;

  this._df = Math.pow(commonVariance, 2) / (
    Math.pow(leftSE, 2) / (left.size - 1) +
    Math.pow(rightSE, 2) / (right.size - 1)
  );
  this._dist = new Distribution(this._df);

  this._se = Math.sqrt(commonVariance);
  this._mean = left.mean - right.mean;
}
util.inherits(StudentT, AbstactStudentT);
module.exports = StudentT;
