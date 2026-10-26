import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ============================================================================
//  TF-IDF document distance tutorial — the njs twin of tfidf.vb
// ============================================================================

const here = (name) => path.join(__dirname, name);
const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

// 1) a small in-memory corpus
const docs = [
    'knowledge building needs innovative environments are better at helping their inhabitants explore the adjacent possible',
    'As a basis for evaluating explanations, creative knowledge building weight of evidence is a poor substitute for the first two criteria listed above.',
    'A public idea database makes every passing idea visible to everyone else in the organization and do creative work.',
    'questioning and various disturbances initiate cycles of innovation and creative organization knowledge.',
    'We need some way to ensure knowledge to spread among environments that any notes that are dropped are dropped.'
];

// 2) TF-IDF distance table (rows / columns are doc_1 .. doc_N)
const dist = stdlib.nlp.tfidfTable(docs);
const matrix = stdlib.table.matrix(dist);

console.log(`vocabulary: ${stdlib.nlp.vocabulary(docs).length} terms, documents: ${dist.nsamples}`);

for (let i = 0; i < matrix.length; i++) {
    let line = '';

    for (let j = 0; j < matrix[i].length; j++) {
        line += matrix[i][j].toFixed(4).padStart(9) + ' ';
    }

    console.log(`${dist.rowNames[i]} ${line}`);
}

// 3) render the distance matrix as a heatmap
const heatmap = stdlib.plot.heatmap(
    matrix,
    stdlib.table.rowNames(dist),
    stdlib.table.featureNames(dist),
    'TF-IDF Document Distance Heatmap',
    'plasma', false, 'light', '1200,800');

console.log('plot: ' + stdlib.plot.save(heatmap, here('tfidf-heatmap.png')));

// 4) export the distance matrix through the standard table writer
console.log('csv : ' + stdio.write.csv(dist, here('tfidf-distance.csv')));

console.log('done: tfidf-heatmap.png');
console.log('done: tfidf-distance.csv');
