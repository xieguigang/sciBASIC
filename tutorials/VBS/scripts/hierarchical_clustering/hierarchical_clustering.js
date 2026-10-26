import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// Hierarchical clustering demo — the njs twin of hierarchical_clustering.vb
// ---------------------------------------------------------------------------

const here = (name) => path.join(__dirname, name);
const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

const file = path.join(__dirname, '../../data/bezdekIris.csv');
const k = 3;

// 1. load the iris dataset
let dataset = stdio.read.csv(file, ['D1', 'D2', 'D3', 'D4']);

// the real species is mapped to a numeric label column, and the duplicated
// row names are rewritten to unique sample ids
const species = stdlib.table.rowNames(dataset);
const speciesNames = [];
const sp = [];

for (let i = 0; i < species.length; i++) {
    const index = speciesNames.indexOf(species[i]);

    if (index < 0) {
        speciesNames.push(species[i]);
        sp.push(speciesNames.length);
    } else {
        sp.push(index + 1);
    }
}

stdlib.table.setLabel(dataset, 'species', sp);
stdlib.table.uniqueRowNames(dataset, 'sample_');

console.log(`dataset: ${dataset.nsamples} samples x ${dataset.nfeatures} features, ${speciesNames.length} species`);

// 2. feature table -> symmetric distance matrix
const dist = stdlib.cluster.distanceMatrix(dataset);

console.log(`distance matrix: ${dist.nsamples} x ${dist.nfeatures} (square matrix)`);

// 3. hierarchical clustering: dendrogram + cut into k clusters
const tree = stdlib.cluster.hca(dist);
const flat = stdlib.cluster.hcut(dist, k);
const labels = stdlib.table.classLabels(flat);

console.log(`dendrogram: leafs = ${stdlib.cluster.leafCount(tree)}`);
console.log(`hcut(k := ${k}) cluster sizes: ` + join(stdlib.cluster.clusterSizes(labels), ', '));

const byThreshold = stdlib.cluster.hcutByDistance(dist, 3.0);
const thresholdNumber = stdlib.cluster.clusterSizes(stdlib.table.classLabels(byThreshold)).length;

console.log(`hcut(threshold := 3.0) -> ${thresholdNumber} clusters`);

// 4. compare the clusters with the real species (cross tabulation)
const cross = stdlib.cluster.crossTable(stdlib.table.classLabelAt(dataset, 0), labels);
const crossValues = stdlib.table.matrix(cross);

for (let i = 0; i < cross.nsamples; i++) {
    console.log(`  ${speciesNames[i]}: ` + join(crossValues[i], ', '));
}

// 5. PCA scatter coloured by the hierarchical cluster id
const score = stdlib.dimreduce.pcaScore(dataset, 2);
const classId = [];

for (let i = 0; i < labels.length; i++) {
    classId.push('cluster ' + labels[i]);
}

const figure = stdlib.plot.scatter(
    stdlib.table.feature(score, 'PC1'), stdlib.table.feature(score, 'PC2'),
    classId,
    'Hierarchical clustering of bezdek-Iris',
    `average linkage + hcut(k := ${k})`,
    'PC1', 'PC2', null, '1200,800');

console.log('plot: ' + stdlib.plot.save(figure, here('bezdekIris-hclust.png')));

// 6. export the cluster ids together with the original table
stdlib.table.setClassLabel(dataset, 'cluster', labels);
console.log('csv : ' + stdio.write.csv(dataset, here('bezdekIris-hclust.csv')));

console.log('done: bezdekIris-hclust.png');
console.log('done: bezdekIris-hclust.csv');
