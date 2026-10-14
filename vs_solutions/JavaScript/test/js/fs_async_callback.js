const hasRequire = (typeof require === 'function');
const fs = hasRequire ? require('fs') : fs;
const path = hasRequire ? require('path') : path;

const file = path.join(__dirname, 'async.txt');
fs.writeFileSync(file, 'async content');

console.log('start');
fs.readFile(file, 'utf8', (err, data) => {
  console.log('callback data:', data);
});
console.log('after readFile call');

fs.readFile(file, (err, data) => {
  console.log('callback buffer length:', data.length);
  console.log('callback buffer text:', data.toString('utf8'));
});

fs.unlinkSync(file);
