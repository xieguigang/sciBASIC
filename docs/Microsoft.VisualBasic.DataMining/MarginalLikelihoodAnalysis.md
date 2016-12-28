# MarginalLikelihoodAnalysis
_namespace: [Microsoft.VisualBasic.DataMining](./index.md)_

@author Marc Suchard
 @author Alexei Drummond
 
 Source translated from ``model_P.c`` (a component of BAli-Phy by Benjamin Redelings and Marc Suchard



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.MarginalLikelihoodAnalysis.#ctor(System.Collections.Generic.IList{System.Double},System.Int32,System.String,System.Int32)
```
Constructor

|Parameter Name|Remarks|
|--------------|-------|
|sample|-|
|burnin|          used for 'toString' display purposes only |
|analysisType|-|
|bootstrapLength| a value of zero will turn off bootstrapping |


#### logMarginalLikelihoodAICM
```csharp
Microsoft.VisualBasic.DataMining.MarginalLikelihoodAnalysis.logMarginalLikelihoodAICM(System.Collections.Generic.IList{System.Double})
```
Calculates the AICM of a model using method-of-moments from Raftery et al. (2007)

|Parameter Name|Remarks|
|--------------|-------|
|v| a posterior sample of logLikelihoods |


_returns:  the AICM (lower values are better) _

#### logMarginalLikelihoodArithmetic
```csharp
Microsoft.VisualBasic.DataMining.MarginalLikelihoodAnalysis.logMarginalLikelihoodArithmetic(System.Collections.Generic.IList{System.Nullable{System.Double}})
```
Calculates the log marginal likelihood of a model using the arithmetic mean estimator

|Parameter Name|Remarks|
|--------------|-------|
|v| a posterior sample of logLikelihoods |


_returns:  the log marginal likelihood _

#### logMarginalLikelihoodHarmonic
```csharp
Microsoft.VisualBasic.DataMining.MarginalLikelihoodAnalysis.logMarginalLikelihoodHarmonic(System.Collections.Generic.IList{System.Double})
```
Calculates the log marginal likelihood of a model using Newton and Raftery's harmonic mean estimator

|Parameter Name|Remarks|
|--------------|-------|
|v| a posterior sample of logLikelihoods |


_returns:  the log marginal likelihood _

#### logMarginalLikelihoodSmoothed
```csharp
Microsoft.VisualBasic.DataMining.MarginalLikelihoodAnalysis.logMarginalLikelihoodSmoothed(System.Collections.Generic.IList{System.Double},System.Double,System.Double)
```
Calculates the log marginal likelihood of a model using Newton and Raftery's smoothed estimator

|Parameter Name|Remarks|
|--------------|-------|
|v|     a posterior sample of logLikelihood |
|delta| proportion of pseudo-samples from the prior |
|Pdata| current estimate of the log marginal likelihood |


_returns:  the log marginal likelihood _


