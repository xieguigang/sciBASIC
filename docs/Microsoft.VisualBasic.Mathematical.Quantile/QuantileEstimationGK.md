# QuantileEstimationGK
_namespace: [Microsoft.VisualBasic.Mathematical.Quantile](./index.md)_

Implementation of the Greenwald and Khanna algorithm for streaming
 calculation of epsilon-approximate quantiles.
 
 See: 
 
 > Greenwald and Khanna, "Space-efficient online computation of quantile summaries" in SIGMOD 2001



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Quantile.QuantileEstimationGK.#ctor(System.Double,System.Int32)
```
Implementation of the Greenwald and Khanna algorithm for streaming
 calculation of epsilon-approximate quantiles.

|Parameter Name|Remarks|
|--------------|-------|
|epsilon|Acceptable % error in percentile estimate|
|compact_size|Threshold to trigger a compaction|



### Properties

#### compact_size
Threshold to trigger a compaction
#### count
Total number of items in stream
#### epsilon
Acceptable % error in percentile estimate
