import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// MNIST UMAP demo — the njs twin of mnist_umap.vb
//
// Run:
//     njs mnist_umap.js --data=<MNIST dir>
// ---------------------------------------------------------------------------

const here = (name) => path.join(__dirname, name);

const argValue = (name) => {
    const prefix = name + '=';

    for (let i = 0; i < process.argv.length; i++) {
        const arg = String(process.argv[i]);

        if (arg.indexOf(prefix) === 0) return arg.substring(prefix.length);
    }

    return null;
};

const repoData = argValue('--data');
const limit = 2000;

let dataset;
let imagesFile = null;
let labelsFile = null;

if (repoData) {
    imagesFile = path.join(repoData, 'train-images-idx3-ubyte');
    labelsFile = path.join(repoData, 'train-labels-idx1-ubyte');
}

if (imagesFile && fs.existsSync(imagesFile) && fs.existsSync(labelsFile)) {
    console.log(`[data] mnist: ${imagesFile}`);
    dataset = stdio.read.mnist(imagesFile, labelsFile, limit);
} else {
    console.log('[data] no mnist dataset given (--data=<dir>), using the synthetic digit set');
    dataset = stdlib.ml.syntheticDigits(30, 2024);
}

// 1. the handwritten digit samples as a unified 2D table
console.log(`mnist table: ${dataset.nsamples} samples x ${dataset.nfeatures} pixels`);

// 2. UMAP: 784 pixel dimensions -> 2 embedding dimensions
const manifold = stdlib.dimreduce.umap(dataset, 2, 64);

console.log(`umap embedding: ${manifold.nsamples} samples x ${manifold.nfeatures} dims`);

// 3. scatter UMAP1 / UMAP2 coloured by the digit class
const classes = stdlib.table.classLabelAt(dataset, 0);
const classId = [];

for (let i = 0; i < classes.length; i++) {
    classId.push(String(classes[i]));
}

const figure = stdlib.plot.scatter(
    stdlib.table.feature(manifold, 'dim_1'), stdlib.table.feature(manifold, 'dim_2'),
    classId,
    'MNIST dataset UMAP embedding',
    'UMAP manifold of 2 dimensions',
    'UMAP1', 'UMAP2', null, '1200,800');

console.log('plot: ' + stdlib.plot.save(figure, here('mnist-umap.png')));

// 4. export the embedding table as csv
console.log('csv : ' + stdio.write.csv(manifold, here('mnist-umap.csv')));

console.log('done: mnist-umap.png');
console.log('done: mnist-umap.csv');
