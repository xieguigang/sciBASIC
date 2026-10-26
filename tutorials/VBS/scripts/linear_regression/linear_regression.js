import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// Linear regression demo — the njs twin of linear_regression.vb
//
// The very same stdlib / stdio api is used here: the two scripts only differ in
// their host language syntax (template literals, positional optional arguments
// and the JS array helpers below), so their stdout and their output files match.
// ---------------------------------------------------------------------------

// the njs host publishes the node style 'path' / 'fs' / 'process' globals, no require()
const here = (name) => path.join(__dirname, name);
const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

// ---------------------------------------------------------------------------
// 1. Build a noisy linear dataset: y = 2x + 3 + noise (deterministic sawtooth)
// ---------------------------------------------------------------------------
const n = 100;
const features = [];
const y = [];

for (let i = 0; i < n; i++) {
    const x = i * 0.1;
    const noise = ((i % 7) - 3) * 0.1;

    features.push([x]);
    y.push(2.0 * x + 3.0 + noise);
}

const dataset = stdlib.table.fromRows(stdlib.table.names('sample', n), features, ['x']);
stdlib.table.setLabel(dataset, 'y', y);

console.log(`dataset: ${dataset.nsamples} samples x ${dataset.nfeatures} feature`);

// ---------------------------------------------------------------------------
// 2. Fit a linear model and a quadratic model for comparison
// ---------------------------------------------------------------------------
const model = stdlib.fit.linear(dataset, 'y');

console.log(`linear fit  : y = ${model.slope} * x + ${model.intercept}`);
console.log(`R2 = ${model.r2}, RMSE = ${model.rmse}`);

const quad = stdlib.fit.poly(dataset, 2, 'y');
console.log(`poly fit(2) : R2 = ${quad.r2}, RMSE = ${quad.rmse}`);

// ---------------------------------------------------------------------------
// 3. Write the prediction and the residual back into the label matrix
// ---------------------------------------------------------------------------
const result = stdlib.fit.setPrediction(stdlib.table.clone(dataset), model, true, 'y');

console.log('prediction labels: ' + join(result.labelNames, ', '));
console.log(`first row: y = ${stdlib.table.label(result, 'y')[0]}, ` +
            `prediction = ${stdlib.table.label(result, 'prediction')[0]}, ` +
            `residual = ${stdlib.table.label(result, 'residual')[0]}`);

// ---------------------------------------------------------------------------
// 4. Draw the observed scatter points together with the fitted line
// ---------------------------------------------------------------------------
const lineSize = 50;
const total = dataset.nsamples + lineSize;
const xs = [];
const ys = [];
const classId = [];
const xValues = stdlib.table.feature(dataset, 'x');

for (let i = 0; i < dataset.nsamples; i++) {
    xs.push(xValues[i]);
    ys.push(y[i]);
    classId.push('observed');
}

for (let i = 0; i < lineSize; i++) {
    const px = i * 0.2;

    xs.push(px);
    ys.push(model.GetY(px));
    classId.push('fitted');
}

const figure = stdlib.plot.scatter(
    xs, ys, classId,
    'Linear regression of a synthetic dataset',
    `y = ${model.slope} * x + ${model.intercept}, R2 = ${model.r2}`,
    'x', 'y', null, '1200,800');

console.log('plot: ' + stdlib.plot.save(figure, here('linear-regression.png')));

// ---------------------------------------------------------------------------
// 5. Export the result table and a formatted data report
// ---------------------------------------------------------------------------
console.log('csv : ' + stdio.write.csv(result, here('linear-regression.csv')));

const doc = stdlib.report.document('Linear regression report', 'sciBASIC standard library');

doc.add(stdlib.report.heading('1. Synthetic dataset'));
doc.add(stdlib.report.paragraph(
    `The dataset contains ${dataset.nsamples} samples of one feature column x and the label y = 2x + 3 + noise.`));
doc.add(stdlib.report.heading('2. Fitted models'));
doc.add(stdlib.report.table(
    ['model', 'R2', 'RMSE'],
    [
        ['linear', model.r2.toFixed(6), model.rmse.toFixed(6)],
        ['poly(2)', quad.r2.toFixed(6), quad.rmse.toFixed(6)]
    ]));
doc.add(stdlib.report.heading('3. Observation and fitted line'));
doc.add(stdlib.report.image(here('linear-regression.png'), 'linear regression'));
doc.add(stdlib.report.heading('4. First 5 prediction rows'));
doc.add(stdlib.report.tableOf(stdlib.table.slice(result, 0, 5)));

console.log('md  : ' + doc.save(here('linear-regression.md')));
console.log('html: ' + doc.save(here('linear-regression.html')));
console.log('docx: ' + doc.save(here('linear-regression.docx')));
console.log('pdf : ' + doc.save(here('linear-regression.pdf')));
console.log('rtf : ' + doc.save(here('linear-regression.rtf')));
console.log('xps : ' + doc.save(here('linear-regression.oxps')));

console.log('done: linear-regression');
