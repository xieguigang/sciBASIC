const hasRequire = (typeof require === 'function');
const fs = hasRequire ? require('fs') : fs;

try {
  fs.readFileSync('definitely_missing_file_xyz.txt', 'utf8');
} catch (e) {
  console.log('sync code:', e.code);
  console.log('sync message:', e.message);
  console.log('sync syscall:', e.syscall);
  console.log('sync path:', e.path);
  console.log('sync name:', e.name);
}

fs.readFile('definitely_missing_file_xyz.txt', (err, data) => {
  if (err) {
    console.log('async code:', err.code);
    console.log('async message:', err.message);
  }
});

fs.promises.readFile('definitely_missing_file_xyz.txt')
  .then(d => console.log('should not reach'))
  .catch(err => {
    console.log('promise code:', err.code);
    console.log('promise message:', err.message);
  });
