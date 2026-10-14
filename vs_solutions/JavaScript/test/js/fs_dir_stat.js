const hasRequire = (typeof require === 'function');
const fs = hasRequire ? require('fs') : fs;
const path = hasRequire ? require('path') : path;

const dir = path.join(__dirname, 'sandbox');
fs.mkdirSync(dir, { recursive: true });
fs.writeFileSync(path.join(dir, 'a.txt'), 'A');
fs.writeFileSync(path.join(dir, 'b.txt'), 'B');
fs.mkdirSync(path.join(dir, 'sub'));

console.log('readdir:', JSON.stringify(fs.readdirSync(dir).sort()));
const st = fs.statSync(path.join(dir, 'a.txt'));
console.log('file size:', st.size);
console.log('isFile:', st.isFile());
console.log('isDirectory:', st.isDirectory());
const dst = fs.statSync(dir);
console.log('dir isDirectory:', dst.isDirectory());
console.log('dir isFile:', dst.isFile());

fs.renameSync(path.join(dir, 'a.txt'), path.join(dir, 'a2.txt'));
console.log('after rename:', JSON.stringify(fs.readdirSync(dir).sort()));

fs.rmdirSync(path.join(dir, 'sub'));
fs.unlinkSync(path.join(dir, 'b.txt'));
fs.unlinkSync(path.join(dir, 'a2.txt'));
fs.rmdirSync(dir);
console.log('cleanup done');
