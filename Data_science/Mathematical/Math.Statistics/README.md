# statistics

> [USACE-MMC/statistics](https://github.com/USACE-MMC/statistics): a simple statistics library to support data analysis and monte carlo

statistics is a library, original written in java and was translated into VB.NET, indended to allow data to be fitted to analytical distributions using MOM or Linear moments.

The resulting distributions can then be accessed using ``GetPDF``, ``GetCDF``, and ``GetInvCDF``.  This allows programmers to create simple monte carlo programs without having a math library that solves all of the problems in the world.

To produce MOM statistics there are a few options, BasicProductMoments utilizes an inline algorithm for calculating count, min, max, mean, and variance.  BasicProductMomentsHistogram extends BasicProductMoments to include a histogram that is created based on a bin width. This histogram is also developed using an inline algorithm.  ProductMoments requires storing the data or passing the entire array into the object at one time, but gives the programmer access to skew and kurtosis.

The library is split into two major packages ``Distributions`` and ``MomentFunctions``.

## Distributions
Distributions is a repository for implementations of the abstract class ContinuousDistribution. Each continuous distribution produces the CDF, PDF, and Inverse CDF functions. There are two packages under distributions, LinearMoments, and MethodOfMoments.  These packages have similar distributions, but the fitting methods when using the constructors that consume arrays of double data are based on the package they are in.

Under LinearMoments the following distributions appear:

  * Exponential
  * Generalized Extreme Value
  * Gumbel
  * LogPearson type III
  * Logistic
  * Pareto

Under MethodOfMoments the following distributions appear:

  * Beta
  * Exponential
  * Generalized Extreme Value
  * Gamma
  * Gumbel
  * LogNormal
  * LogPearson type III
  * Normal
  * Rayleigh
  * Triangular
  * Uniform

## MomentFunctions
MomentFunctions is a repository for classes that perform operations on data arrays or streams that describe the data with various typical statistics like mean standard deviation etc.  Not only are method of moments utilized but also Linear moments.  BasicProductMoments (and BasicProductMomentsHistogram) utilize an inline algorithm for mean and standard deviation for method of moments (we use the moniker product moments to refer to traditional method of moments, to differentiate between MOM and linear moments...). ProductMoments requires the entire dataset to be supplied to the constructor, it loops through the data twice, but it also calculates Skew and Kurtosis.

## SpecialFunctions
Special functions is a static class that computes functions that are necessary for various operations in the special extreme distributions included in this package. Most of the methods are based off of existing algorithims from Cephes or other similar sources.

## Example Code

The following codeblock is an example of how to create a monte carlo with the standard normal distribution

```vbnet
Public Module ExampleMonteCarlo

    Sub Main()
        Call MonteCarlo()
        Call Pause()
    End Sub

    Public Sub MonteCarlo()
        ' this Is a very trivial example of creating a monte carlo using a
        ' standard normal distribution
        Dim sn As New Distributions.MethodOfMoments.Normal()

        ' output now contains 10000 random normally distributed values.
        Dim output As Vector = sn.GetInvCDF(rand(10000))

        ' to evaluate the mean And standard deviation of the output
        ' you can use Basic Product Moment Stats
        Dim BPM As New MomentFunctions.BasicProductMoments(output)

        Call println("Mean: %s", BPM.Mean())
        Call println("StDev: %s", BPM.StDev())
        Call println("Sample Size: %s", BPM.SampleSize())
        Call println("Minimum: %s", BPM.Min())
        Call println("Maximum: %s", BPM.Max())
    End Sub
End Module
```

Exchanging:

```vbnet
Dim SN As New Distributions.MethodOfMoments.Normal()
```
with the following:
```vbnet
Dim SN As New Distributions.MethodOfMoments.Normal(5,3)
```
will create a normal distribution with a mean of 5 and a standard deviation of 3. The result will be seen in the output to the debug window.

As you can see in the previous example, we used the normal distribution from the MethodofMoments package. The major difference between the MethodOfMoments package and the LinearMoments package is in the way that the distributions are fitted to data.

To fit a distribution to data use the constructor that accepts an argument of ``Double()`` or ``Vector`` type in VisualBasic.
```vbnet
Dim data#() = {1.0, 3.2, 4.9, 7.4, 2.4, 2.2}
Dim Norm As New Distributions.MethodOfMoments.Normal(data)
```
This example will calculate the mean and standard of deviation for the resulting normal distribution Norm, based on the input data, and the output of the monte carlo will be based on that computed mean and standard of deviation.
