// Shared shim: Node uses require('fs'/'path'); LiteJs exposes them as globals.
const hasRequire = (typeof require === 'function');
const path = hasRequire ? require('path') : path;

console.log('sep:', JSON.stringify(path.sep));
console.log('join:', path.join('a', 'b', 'c'));
console.log('join dir:', path.join(__dirname, 'x', 'y'));
console.log('basename:', path.basename('foo/bar/baz.txt'));
console.log('basename ext:', path.basename('foo/bar/baz.txt', '.txt'));
console.log('dirname:', path.dirname('foo/bar/baz.txt'));
console.log('extname:', path.extname('foo/bar/baz.txt'));
console.log('isAbsolute abs:', path.isAbsolute(__dirname));
console.log('isAbsolute rel:', path.isAbsolute('relative/path'));
console.log('normalize:', path.normalize('foo/./bar/../baz'));
console.log('resolve:', path.resolve(__dirname, 'x'));
const p = path.parse(__dirname + '/baz.txt');
console.log('parse:', JSON.stringify({ root: p.root, dir: p.dir, base: p.base, ext: p.ext, name: p.name }));
