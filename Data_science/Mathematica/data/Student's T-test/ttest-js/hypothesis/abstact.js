'use strict';

function AbstactStudentT(options) {
  this._options = options;
}
module.exports = AbstactStudentT;

AbstactStudentT.prototype.testValue = function () {
  const diff = (this._mean - this._options.mu);
  return diff / this._se;
};

AbstactStudentT.prototype.pValue = function () {
  const t = this.testValue();

  switch (this._options.alternative) {
  case 1: // mu > mu[0]
    return 1 - this._dist.cdf(t);
  case -1: // mu < mu[0]
    return this._dist.cdf(t);
  case 0: // mu != mu[0]
    return 2 * (1 - this._dist.cdf(Math.abs(t)));
  }
};

AbstactStudentT.prototype.confidence = function () {
  let pm;
  switch (this._options.alternative) {
  case 1: // mu > mu[0]
    pm = Math.abs(this._dist.inv(this._options.alpha)) * this._se;
    return [this._mean - pm, Infinity];
  case -1: // mu < mu[0]
    pm = Math.abs(this._dist.inv(this._options.alpha)) * this._se;
    return [-Infinity, this._mean + pm];
  case 0: // mu != mu[0]
    pm = Math.abs(this._dist.inv(this._options.alpha / 2)) * this._se;
    return [this._mean - pm, this._mean + pm];
  }
};

AbstactStudentT.prototype.valid = function () {
  return this.pValue() >= this._options.alpha;
};

AbstactStudentT.prototype.freedom = function () {
  return this._df;
}
