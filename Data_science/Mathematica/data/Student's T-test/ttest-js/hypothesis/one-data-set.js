'use strict';

const Distribution = require('distributions').Studentt;

const util = require('util');
const AbstactStudentT = require('./abstact.js');

function StudentT(data, options) {
  AbstactStudentT.call(this, options);

  this._df = data.size - 1;
  this._dist = new Distribution(this._df);

  this._se = Math.sqrt(data.variance / data.size);
  this._mean = data.mean;
}
util.inherits(StudentT, AbstactStudentT);
module.exports = StudentT;
