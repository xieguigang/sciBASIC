import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// KMeans + PCA demo — the njs twin of kmeans.vb
// ---------------------------------------------------------------------------

const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

const file = path.join(__dirname, '../../data/bezdekIris.csv');
const k = 3;

// 1. load the unified 2D table object (NumericTable) from the csv file
const dataset = stdio.read.csv(file, ['D1', 'D2', 'D3', 'D4']);

console.log(`load table: ${dataset.nsamples} samples x ${dataset.nfeatures} features`);

// 2. run the KMeans clustering (the cluster id is written into the table labels)
const clustered = stdlib.cluster.kmeans(dataset, k);
const labels = stdlib.table.classLabels(clustered);

console.log('kmeans cluster sizes: ' + join(stdlib.cluster.clusterSizes(labels), ', '));

// 3. run PCA on the clustered table and take the score matrix
const score = stdlib.dimreduce.pcaScore(clustered, 2);
const pc1 = stdlib.table.feature(score, 'PC1');
const pc2 = stdlib.table.feature(score, 'PC2');
const classId = [];

for (let i = 0; i < labels.length; i++) {
    classId.push(String(labels[i]));
}

// 4. scatter the PCA scores, coloured by the kmeans cluster id
const figure = stdlib.plot.scatter(
    pc1, pc2, classId,
    'PCA group of bezdek-Iris',
    'PCA score scatter with 3 iris species colors',
    'PC1', 'PC2', null, '1200,800');

console.log('plot: ' + stdlib.plot.save(figure, here('bezdekIris-pca-groups.png')));

// 5. export the clustering result table as a csv file
console.log('csv : ' + stdio.write.csv(clustered, here('bezdekIris-kmeans.csv')));

console.log('done: bezdekIris-pca-groups.png');
console.log('done: bezdekIris-kmeans.csv');
