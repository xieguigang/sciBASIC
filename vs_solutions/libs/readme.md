# sciBASIC# Standard Library

A shared standard library api for the **VBS** (`vs_solutions\VBS`) and **NJS**
(`vs_solutions\NJS`) scripting hosts. Both engines drive the very same api, so a
script written against `stdlib` / `stdio` runs unchanged on either of them.

```
libs/
├── stdio/    data file io          (read.* / write.*)
└── stdlib/   data science api      (table / fit / stats / cluster / dimreduce /
                                     plot / net / report / nlp / ml / cuda / lapack)
```

The assemblies are built into `sciBASIC#\.nuget\net10.0\` (the `vbs.exe` folder).

---

## 1. Using the libraries from a script

### VBS (.vb)

```vbnet
#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

dim table = read.csv("iris.csv", columns:={"D1", "D2", "D3", "D4"})
dim model = fit.linear(table, "y")

call console.WriteLine($"R2 = {model.r2}, RMSE = {model.rmse}")
call console.WriteLine(plot.save(plot.scatter(x, y, title:="demo"), here("demo.png")))
call write.csv(table, here("result.csv"))
```

`#include` resolves the dll through the script folder → `vbs.exe` folder →
`libs` folders, so the two names above are enough — no transitive `#include` is
required, because the public api only exposes types of the standard library
itself (`NumericTable` lives in the engine's `Microsoft.VisualBasic.Runtime`
assembly, which every script already references).

### NJS (.js)

```javascript
import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

const table = stdio.read.csv('iris.csv', ['D1', 'D2', 'D3', 'D4']);
const model = stdlib.fit.linear(table, 'y');

console.log(`R2 = ${model.r2}, RMSE = ${model.rmse}`);
console.log(stdlib.plot.save(stdlib.plot.scatter(x, y, null, 'demo'), 'demo.png'));
```

`import <namespace> from '<dll>'` is the LiteJs CLR namespace import: the alias
receives the **public types** of the namespace, so `stdlib.fit` is the `fit`
module and `stdlib.fit.linear(...)` its static members.

Both hosts resolve the dependency tree of an imported dll automatically (managed
assemblies by simple name, native ones through `runtimes/*/native`).

---

## 2. Api rules

The script bridge converts `Double()`, `Double()()`, `String()`, primitives,
`NumericTable` and library-owned classes — **not** `IEnumerable(Of T)`, generics,
`ByRef` or delegates. Every public member therefore follows these rules:

- parameters and return values are arrays / primitives / `NumericTable` /
  `FitModel`, `PlotImage`, `Graph`, `ClusterTree`, `TestResult`, …
- no `Using` / `Dispose` is required: plotting returns a `PlotImage` and
  `plot.save(image, file)` writes and releases it
- functions are silent unless asked otherwise; progress output only comes from
  the wrapped analysis libraries
- module names are flat (`stdlib.fit`, not `stdlib.science.fit`), because the
  LiteJs namespace import does not recurse into sub namespaces

---

## 3. stdio — data file io

All table readers return a `NumericTable` (row names + feature columns +
`label:` prefixed label columns) and all writers accept one.

| Module | Functions |
| --- | --- |
| `read` | `csv(file, columns?, rowHeader?, strict?)`, `tsv(...)`, `table(file, delimiter, ...)`, `xlsx(file, sheet)` / `xlsx(file, index)`, `sheetNames(file)`, `arff(file)`, `json(file)`, `text(file)`, `lines(file)`, `mnist(images, labels, limit?)`, `fieldNames(prefix, n, start?, sep?)` |
| `write` | `csv(table, file)`, `tsv(table, file)`, `table(table, file, delimiter)`, `xlsx(table, file, sheetName)` / `xlsx(tables, file, sheetNames)`, `arff(table, file)`, `json(value, file)`, `text(value, file)`, `lines(values, file)` |

Writers create the target folder, return the absolute path and throw a
`FileNotFoundException` / `IOException` with the path in the message on failure.

```vbnet
dim t = read.csv("bezdekIris.csv", columns:={"D1", "D2", "D3", "D4"})
call write.xlsx(t, here("iris.xlsx"), "iris")
```

---

## 4. stdlib

### 4.1 `table` — the unified 2D table

```
fromRows(rowNames, features, featureNames?)   fromColumns(featureNames, columns, rowNames?)
fromMatrix(matrix, rowNames?, featureNames?)  matrix(t) / feature(t, name) / label(t, name)
labelAt(t, index) / classLabelAt(t, index)    labels(t) / labelNames(t) / featureNames(t) / rowNames(t)
setLabel(t, name, values) / setClassLabel(t, name, values) / classLabels(t, name?)
columns(t, names) / rows(t, indices) / slice(t, start, count) / clone(t)
uniqueRowNames(t, prefix?) / hasUniqueRowNames(t) / names(prefix, n, start?, sep?)
```

`table.names("p", 3)` → `p_1, p_2, p_3` — the standard library replacement of the
VBS `fieldname` magic.

### 4.2 `fit` — regression

```
linear(x, y) / linear(t, y?)      poly(x, y, degree) / poly(t, degree, y?)
linearWeighted(x, y, w)           ridge(t, y?, lambda?)      elasticNet(t, alpha, l1Ratio, y?)
lma(t, y?)                        predict(model, x) / predict(model, features)
predictTable(model, t, name?)     setPrediction(t, model, withResidual?, y?)
```

Every entry returns a `FitModel` with `kind / r2 / rmse / sse / slope /
intercept / coefficients / featureNames / formula / residuals / lambda / alpha /
l1Ratio` plus `predict(x)`, `predictAll(features)` and `GetY(x)`.

### 4.3 `stats` and `lapack`

```
mean / sum / min / max / variance / sd / median / quantile / cor / cov
describe(x) → Dictionary        summary(t) → NumericTable
t_test(x, y, alpha?)            t_test_one(x, mu?, alpha?)      chisq(observed, expected?)
```

`t_test` / `t_test_one` return a `TestResult` (`statistic / pvalue / df / mean /
meanX / meanY / alpha / significant`), `chisq` a `ChiSquareResult`.

`lapack` = `transpose / multiply / solve / inverse / determinant / identity` on
`Double()()` plus the compatibility `t_test` entry.

### 4.4 `cluster` and `dimreduce`

```
kmeans(t, k, threads?, seed?)         distanceMatrix(t)
hca(dist) → ClusterTree               hcut(dist, k) / hcutByDistance(dist, threshold)
leafCount(tree) / leafs(tree) / children(tree) / totalDistance(tree)
clusterSizes(labels)                  crossTable(truth, labels)
```

`hca` / `hcut` take the **distance matrix** produced by `distanceMatrix`
(`kmeans` takes the feature table directly). `kmeans` resets the RNG with
`seed` (default 13), so clustering is reproducible on both engines.

```
pcaScore(t, maxPC?) / pcaLoading(t, maxPC?)      umap(t, dims?, neighbors?)
tsne(t, dims?, perplexity?)                      pacmap(t, dims?, neighbors?)
```

All of them return a **new** coordinate table (`dim_1 .. dim_n`) that inherits
the row names and label columns.

### 4.5 `plot` and `net`

```
scatter(x, y, classId?, title?, subtitle?, xLabel?, yLabel?, theme?, size?, dpi?)
line(x, y, classId?, title?, subtitle?, xLabel?, yLabel?, smooth?, theme?, size?, dpi?)
histogram(values, bins?, density?, title?, xLabel?, yLabel?, theme?, size?, dpi?)
heatmap(matrix, rowLabels?, colLabels?, title?, palette?, showValues?, theme?, size?, dpi?)
save(image, file) → path              register()
```

`theme` is `nature` (default) / `light` / `dark` / `science` / `grayscale`;
`palette` is `plasma` (default) / `viridis` / `inferno` / `magma` / `turbo` / …
The raster driver is registered automatically; `save` picks the format from the
extension (png / jpg / bmp / svg / pdf).

```
net.create() → Graph
net.addNode(g, label, x?, y?, size?)        net.addEdge(g, u, v, weight?)
net.hola(g)                                 net.nodeCount / edgeCount / bendCount
net.coordinates(g) → NumericTable           net.draw(g, file, size?, drawBends?, ...)
```

`Graph` also carries the same operations as methods (`g.addNode(...).addEdge(...)`).

### 4.6 `report` — data reports

The content model is the Core `Block` type, so one document renders to every
supported format:

```vbnet
dim doc = report.document("Linear regression report", "sciBASIC", toc:=False)
call doc.add(report.heading("1. Dataset"))
call doc.add(report.paragraph("100 samples of one feature column."))
call doc.add(report.table(New String() {"model", "R2"}, New String()() {New String() {"linear", "0.9988"}}))
call doc.add(report.tableOf(dataset))
call doc.add(report.image(here("figure.png"), "scatter"))
call doc.add(report.code("y = 2x + 3", "text"))

call doc.save(here("report.md"))     ' .md / .html / .txt
call doc.save(here("report.docx"))   ' .docx / .pdf / .rtf / .xps / .oxps
```

Block factories: `heading / paragraph / code / list / blockquote / hr / raw /
link / image / math / table / tableOf / definitions / tasklist / footnote`;
renderers: `markdown(blocks)`, `html(blocks, title?)`, `plainText(blocks)`,
`render(blocks, file, title?, author?, toc?)`.

> An image block needs a real file on disk — draw the figure first
> (`plot.save`) and pass its path, so the binary formats can embed it.

### 4.7 `nlp`

```
tokenize(text, pattern?)        vocabulary(docs, pattern?)       tfidfMatrix(docs)
tfidfTable(docs)                word2vec(text, vectorSize?, method?, freqThreshold?, window?, threads?, seed?)
similarWords(text, query, vectorSize?, method?, topN?)
```

`tfidfTable` returns the n×n squared-distance matrix as a table (rows/cols =
`doc_i`), `word2vec` returns a table whose row names are the tokens.

### 4.8 `ml` and `cuda`

```
gaussianClusters(perClass?, dims?, seed?)       syntheticDigits(perClass?, seed?)
accuracy(pred, truth)                           confusion(pred, truth)
backendName() / useCuda() / cudaMinGpuElements() / cudaMinGemmElements()
cudaKernelFailures() / cudaLastError()          cnnOperatorTable()
cnnTrain(images, labels, seed?, passes?, samples?, useCuda?)
cnnTrainTable(t, labelName?, seed?, passes?, samples?, useCuda?)
snnTrain(perClass?, epochs?, seed?, hidden?, timeSteps?, batchSize?, testPerClass?)
sparkline(values) / raster(frames, rows)
```

```
cuda.available() / deviceName() / diagnostics()
cuda.kernelNames() / kernelIndexMode(name) / kernelSource(name) / kernelPseudoCode(name)
cuda.selfCheck(m, rows, cols)                   cuda.metrics(m, rows, cols) → CudaMetrics
cuda.pearson / distance / pearsonCpu / distanceCpu / makeMatrix / toMatrix / preview / maxError
```

`cuda` decompiles the `stdlib.PearsonMetrics` functions (ordinary VB.NET maths,
no CUDA concepts) into `.cu` kernels through ILCuda, runs them on the GPU and
offers double precision CPU references for comparison. The kernels live in the
library assembly because only a CLR method can be decompiled — a JS function
cannot.

---

## 5. Tutorials

`tutorials\VBS\scripts\<demo>\` holds a `<demo>.vb` (VBS) and a `<demo>.js`
(NJS) version of every demo; both use only `stdlib` / `stdio`, produce the same
stdout and write the same result files. See
[`tutorials\VBS\readme.md`](../../tutorials/VBS/readme.md).

---

## 6. Build

```bat
cd G:\GCModeller\src\runtime\sciBASIC#
dotnet build vs_solutions\libs\stdio\stdio.vbproj  -c Release
dotnet build vs_solutions\libs\stdlib\stdlib.vbproj -c Release
dotnet build vs_solutions\VBS\VBS.vbproj            -c Release
dotnet build vs_solutions\NJS\NJS.vbproj            -c Release
```
