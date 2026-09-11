# Feature Vector Encoders for Numeric Machine Learning Data

Encodes raw feature vectors of a data frame into numeric columns so that a data frame can be fed directly to model training.

## Overview
- Per-column encoder rules: register a `FeatureEncoder` for any named field and expand that column into one or more numeric indicator columns.
- String columns become one indicator column per distinct level (`EnumEncoder`); boolean columns become a flag column (`FlagEncoder`).
- Numeric columns pass through as-is (`NumericEncoder`) or are discretised into N equal-width bins with formatted bin labels (`NumericBinsEncoder`, built on the `Discretizer`).
- Automatic mode inspects each column's data type and picks the right encoder, replacing the original column with the generated numeric block.

## Key Types
- `Microsoft.VisualBasic.DataMining.FeatureFrame.Encoder` — the orchestrator: `AddEncodingRule`, `Encoding(data As DataFrame)`, shared `AutoEncoding` and shared `Encode(feature As FeatureVector)`.
- `Microsoft.VisualBasic.DataMining.FeatureFrame.FeatureEncoder` — abstract base defining `Encode(feature As FeatureVector) As DataFrame` and the shared `IndexNames` helper.
- `Microsoft.VisualBasic.DataMining.FeatureFrame.EnumEncoder` — expands a string feature into one 0/1 column per distinct factor level.
- `Microsoft.VisualBasic.DataMining.FeatureFrame.FlagEncoder` — encodes a boolean feature as a numeric flag column.
- `Microsoft.VisualBasic.DataMining.FeatureFrame.NumericEncoder` — passes numeric features (Single/Double/Short/Integer/Long) through unchanged.
- `Microsoft.VisualBasic.DataMining.FeatureFrame.NumericBinsEncoder` — bins a numeric feature into `nbins` levels and emits one indicator column per bin.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataMining.FeatureFrame

' explicit per-field rules
Dim enc As New Encoder()
Call enc.AddEncodingRule("species", New EnumEncoder())
Call enc.AddEncodingRule("isControl", New FlagEncoder())
Call enc.AddEncodingRule("weight", New NumericBinsEncoder(nbins:=5))

Dim numeric As DataFrame = enc.Encoding(frame)

' or let the auto encoder pick by column data type
Dim auto As DataFrame = Encoder.AutoEncoding(frame)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.FeatureFrame`
- TargetFramework: `net10.0`
- Tags: `scibasic;feature-engineering;encoder;dataframe;machine-learning`

## License
GPL-3.0-or-later
