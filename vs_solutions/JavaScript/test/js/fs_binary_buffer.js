// Buffer is a global in both Node and LiteJs.
const b1 = Buffer.from('Hello');
console.log('b1 length:', b1.length);
console.log('b1 text:', b1.toString('utf8'));
console.log('b1 hex:', b1.toString('hex'));
console.log('b1 raw:', b1);

const b2 = Buffer.from([72, 105, 33]); // "Hi!"
console.log('b2 text:', b2.toString('utf8'));

const b3 = Buffer.alloc(4, 65); // 'AAAA'
console.log('b3 text:', b3.toString('utf8'));

console.log('slice:', b1.slice(0, 3).toString('utf8'));

const cat = Buffer.concat([b1, b2]);
console.log('concat length:', cat.length);
console.log('concat text:', cat.toString('utf8'));
console.log('isBuffer b1:', Buffer.isBuffer(b1));
console.log('isBuffer [] :', Buffer.isBuffer([]));
