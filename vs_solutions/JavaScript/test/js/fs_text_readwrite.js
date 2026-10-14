const hasRequire = (typeof require === 'function');
const fs = hasRequire ? require('fs') : fs;
const path = hasRequire ? require('path') : path;

const file = path.join(__dirname, 'sample.txt');
fs.writeFileSync(file, 'Hello, LiteJs!');
const txt = fs.readFileSync(file, 'utf8');
console.log('read utf8:', txt);

const buf = fs.readFileSync(file);
console.log('buf length:', buf.length);
console.log('buf text:', buf.toString('utf8'));
console.log('buf raw:', buf);

fs.appendFileSync(file, ' appended');
console.log('after append:', fs.readFileSync(file, 'utf8'));

console.log('exists:', fs.existsSync(file));
const copy = path.join(__dirname, 'sample.copy.txt');
fs.copyFileSync(file, copy);
console.log('copy exists:', fs.existsSync(copy));
console.log('copy content:', fs.readFileSync(copy, 'utf8'));

fs.unlinkSync(copy);
console.log('copy removed:', !fs.existsSync(copy));
fs.unlinkSync(file);
console.log('orig removed:', !fs.existsSync(file));
