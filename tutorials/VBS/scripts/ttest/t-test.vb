#include "stdlib.dll"

Imports stdlib

Dim x = stats.runif(10,0.8,0.9)
Dim y = stats.runif(10,0.5,0.85)

console.writeLine($"x: { x.json_str() }")
console.writeLine($"y: {y.json_str()}")
console.writeLine($"t-test(x,y): {stats.ttest(x,y).json_str()}")
console.writeLine($"t-test(x): {stats.ttest(x).json_str()}")
console.writeLine($"t-test(y): {stats.ttest(y).json_str()}")