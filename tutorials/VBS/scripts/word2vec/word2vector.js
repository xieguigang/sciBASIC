import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// Word2Vec + UMAP + KMeans demo — the njs twin of word2vector.vb
// ---------------------------------------------------------------------------

const here = (name) => path.join(__dirname, name);
const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

const textfile = path.join(__dirname, '../../data/Rapunzel.txt');
const text = stdio.read.text(textfile);

// 1. train the word vectors and build them into a unified 2D table
const vectors = stdlib.nlp.word2vec(text, 30, 'cbow', 1, 5, 1);

console.log(`word vectors: ${vectors.nsamples} tokens x ${vectors.nfeatures} dims`);

// 2. reduce the word vectors to 9 dimensions with UMAP
const manifold = stdlib.dimreduce.umap(vectors, 9, 64);

console.log(`umap embedding: ${manifold.nsamples} samples x ${manifold.nfeatures} dims`);

// 3. run KMeans clustering on the reduced table
const clusters = stdlib.cluster.kmeans(manifold, 9);
const labels = stdlib.table.classLabels(clusters, 'cluster');

console.log('kmeans cluster sizes: ' + join(stdlib.cluster.clusterSizes(labels), ', '));

// 4. scatter UMAP1 / UMAP2, coloured by the cluster id
const classId = [];

for (let i = 0; i < labels.length; i++) {
    classId.push(String(labels[i]));
}

const figure = stdlib.plot.scatter(
    stdlib.table.feature(manifold, 'dim_1'), stdlib.table.feature(manifold, 'dim_2'),
    classId,
    'UMAP group of Rapunzel',
    "UMAP scatter of the 'Rapunzel' word2vector embedding result",
    'UMAP1', 'UMAP2', null, '1200,800');

console.log('plot: ' + stdlib.plot.save(figure, here('rapunzel-umap-groups.png')));

// 5. export the clustering result table as a csv file
console.log('csv : ' + stdio.write.csv(clusters, here('rapunzel-umap-groups.csv')));

console.log('done: rapunzel-umap-groups.png');
console.log('done: rapunzel-umap-groups.csv');
