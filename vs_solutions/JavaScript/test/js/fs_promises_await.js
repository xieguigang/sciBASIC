const hasRequire = (typeof require === 'function');
const fs = hasRequire ? require('fs') : fs;
const path = hasRequire ? require('path') : path;

const file = path.join(__dirname, 'promise.txt');
fs.writeFileSync(file, 'promise content');

console.log('start');
fs.promises.readFile(file, 'utf8').then(d => {
  console.log('promise data:', d);
});
console.log('after then');

(async () => {
  const d = await fs.promises.readFile(file, 'utf8');
  console.log('await data:', d);
  fs.unlinkSync(file);
})();
