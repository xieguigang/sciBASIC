import stdlib from 'stdlib.dll';

const x = stdlib.stats.runif(10,0.8,0.9);
const y = stdlib.stats.runif(10,0.5,0.85);

console.log(`x: ${ JSON.stringify(x) }`)
console.log(`y: ${JSON.stringify(y)}`);
console.log(`t-test(x,y): ${JSON.stringify(stdlib.stats.ttest(x,y))}`);
console.log(`t-test(x): ${JSON.stringify(stdlib.stats.ttest(x))}`);
console.log(`t-test(y): ${JSON.stringify(stdlib.stats.ttest(y))}`);
