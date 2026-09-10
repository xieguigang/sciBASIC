# Data_science/DataMining/FeatureFrame/FeatureFrame.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.FeatureFrame
- AssemblyName  : Microsoft.VisualBasic.DataMining.FeatureFrame
- TargetFramework: net10.0
- Source files  : 6
- Existing Title: Feature Vector Encoders for Numeric Machine Learning Data
- Existing Desc : Encodes raw feature vectors of a data frame into numeric columns for model training: one-hot style encoding for enums and flag bit fields, numeric binning and discretisation, and automatic per-field encoder selection.
- Existing Tags : scibasic;feature-engineering;encoder;dataframe;machine-learning

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DataMining.FeatureFrame)

## Public types
- Class Encoder (Encoder.vb) - A helper module for encode the feature vector into numeric feature
- Class EnumEncoder (Encoder\EnumEncoder.vb) - the feature type should be the string type
- Class FlagEncoder (Encoder\FlagEncoder.vb) - the feature type should be the boolean type
- Class NumericBinsEncoder (Encoder\NumericBinsEncoder.vb)
- Class NumericEncoder (Encoder\NumericEncoder.vb)

## Notable public members
- Public Sub AddEncodingRule(field As String, encoder As FeatureEncoder)
- Public Function Encoding(data As DataFrame) As DataFrame
- Public Shared Function AutoEncoding(data As DataFrame) As DataFrame
- Public Shared Function Encode(feature As FeatureVector) As DataFrame
- Public Overrides Function Encode(feature As FeatureVector) As DataFrame
- Public Overrides Function Encode(feature As FeatureVector) As DataFrame
- Public Overrides Function Encode(feature As FeatureVector) As DataFrame
- Public Shared Function NumericBinsEncoder(feature As FeatureVector, nbins As Integer, Optional format As String = "G3") As DataFrame
- Public Overrides Function Encode(feature As FeatureVector) As DataFrame
- Public MustOverride Function Encode(feature As FeatureVector) As DataFrame
- Protected Shared Function IndexNames(feature As FeatureVector) As String()

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.Data.Framework
- Microsoft.VisualBasic.DataMining.ComponentModel.Discretion
- Microsoft.VisualBasic.Linq
- System.Runtime.CompilerServices

## File tree
- Encoder.vb
- Encoder\EnumEncoder.vb
- Encoder\FlagEncoder.vb
- Encoder\NumericBinsEncoder.vb
- Encoder\NumericEncoder.vb
- FeatureEncoder.vb

