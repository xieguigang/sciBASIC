// process is a global in both Node and LiteJs.
console.log('platform:', process.platform);
console.log('arch:', process.arch);
console.log('cwd:', process.cwd());
console.log('__dirname:', __dirname);
console.log('__filename:', __filename);
console.log('argv user args:', JSON.stringify(process.argv.slice(2)));
const hasPath = (typeof process.env.PATH === 'string') || (typeof process.env.Path === 'string');
console.log('env PATH present:', hasPath);
