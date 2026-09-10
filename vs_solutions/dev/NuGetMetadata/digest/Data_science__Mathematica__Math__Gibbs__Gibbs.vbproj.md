# Data_science/Mathematica/Math/Gibbs/Gibbs.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.GibbsSampling
- AssemblyName  : Microsoft.VisualBasic.Math.GibbsSampling
- TargetFramework: net10.0
- Source files  : 3
- Existing Title: Gibbs Sampling and Metropolis-Hastings Samplers
- Existing Desc : Gibbs sampling for repeated motif discovery in biological sequences, plus a Metropolis-Hastings sampler over Markov bases for contingency table and chi-square analysis. Part of the sciBASIC# statistics stack.
- Existing Tags : scibasic;gibbs-sampling;metropolis-hastings;motif-finding;markov-chain;mcmc

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math.GibbsSampling)

## Public types
- Class Gibbs (Gibbs.vb) - Gibb's Sampling Steps:<br/> 1. Set every variable to a random value.<br/> 2. Choose a variable to update. <br/>
- Class MetropolisHastings (MetropolisHastings.vb)
- Class Score (Score.vb)

## Notable public members
- Public Sub New(seqArray As String(), motifLength As Integer)
- Public Function sample(Optional MAXIT As Integer = 1999) As Score()
- Public Function PQ(chosenSeqIndex As Integer) As (p As Double(), q As Double())
- Public Function calculateQ(tempMotif As String,
- Public Function calculateP(tempMotif As String, chosenSeqIndex As Integer) As Double
- Public Shared Sub Main(dim1 As Integer, dim2 As Integer, observed As Integer(), dim3 As Integer, dim4 As Integer, markovBasis As Integer()())
- Public Shared Sub RunSampling(dim3 As Integer, dim4 As Integer, observed As Integer(), expected As Double(), markovBasis As Integer()())
- Public Shared Function calcTransProb(x As Integer(), markov As Integer()) As Double
- Public Shared Function calcChiSquare(observed As Integer(), expected As Double()) As Double
- Public ReadOnly Property pwm As String
- Public Property seq As String
- Public Property start As Integer
- Public Property len As Integer
- Public Overrides Function ToString() As String

## Imports
- randf2 = Microsoft.VisualBasic.Math.RandomExtensions
- System.IO

## File tree
- Gibbs.vb
- MetropolisHastings.vb
- Score.vb

