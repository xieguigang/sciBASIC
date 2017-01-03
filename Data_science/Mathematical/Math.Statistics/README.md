# statistics

> [USACE-MMC/statistics](https://github.com/USACE-MMC/statistics): a simple statistics library to support data analysis and monte carlo

statistics is a library, written in java, indended to allow data to be fitted to analytical distributions
using MOM or Linear moments.

The resulting distributions can then be accessed using GetPDF, GetCDF, and GetInvCDF.  This allows programmers 
to create simple monte carlo programs without having a math library that solves all of the problems in the world.

To produce MOM statistics there are a few options, BasicProductMoments utilizes an inline algorithm for calculating count, min,
max, mean, and variance.  BasicProductMomentsHistogram extends BasicProductMoments to include a histogram that is created based on
a bin width. This histogram is also developed using an inline algorithm.  ProductMoments requires storing the data or passing the
entire array into the object at one time, but gives the programmer access to skew and kurtosis.

The library is split into two major packages Distributions and MomentFunctions.

##Distributions
Distributions is a repository for implementations of the abstract class ContinuousDistribution.  Each continuous distribution produces the CDF, PDF, and Inverse CDF functions. There are two packages under distributions, LinearMoments, and MethodOfMoments.  These packages have similar distributions, but the fitting methods when using the constructors that consume arrays of double data are based on the package they are in.  

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

##MomentFunctions
MomentFunctions is a repository for classes that perform operations on data arrays or streams that describe the data with various typical statistics like mean standard deviation etc.  Not only are method of moments utilized but also Linear moments.  BasicProductMoments (and BasicProductMomentsHistogram) utilize an inline algorithm for mean and standard deviation for method of moments (we use the moniker product moments to refer to traditional method of moments, to differentiate between MOM and linear moments...). ProductMoments requires the entire dataset to be supplied to the constructor, it loops through the data twice, but it also calculates Skew and Kurtosis.

##SpecialFunctions
Special functions is a static class that computes functions that are necessary for various operations in the special extreme distributions included in this package. Most of the methods are based off of existing algorithims from Cephes or other similar sources.

##Example Code
The following codeblock is an example of how to create a monte carlo with the standard normal distribution
```java
public class ExampleMonteCarlo {
    public static void main(String args[]){
        MonteCarlo();
    }
    public static void MonteCarlo(){
        //this is a very trivial example of creating a monte carlo using a
        //standard normal distribution
        Distributions.MethodOfMoments.Normal SN = new Distributions.MethodOfMoments.Normal();
        double[] output = new double[10000];
        java.util.Random r = new java.util.Random();
        for(int i = 0; i < output.length; i++){
            output[i] =SN.GetInvCDF(r.nextDouble());
        }
        //output now contains 10000 random normally distributed values.
        
        //to evaluate the mean and standard deviation of the output
        //you can use Basic Product Moment Stats
        MomentFunctions.BasicProductMoments BPM = new MomentFunctions.BasicProductMoments(output);
        System.out.println("Mean: " + BPM.GetMean());
        System.out.println("StDev:" + BPM.GetStDev());
        System.out.println("Sample Size: " + BPM.GetSampleSize());
        System.out.println("Minimum: " + BPM.GetMin());
        System.out.println("Maximum: " + BPM.GetMax());   
    }
}
```
Exchanging: 
```java
Distributions.MethodOfMoments.Normal SN = new Distributions.MethodOfMoments.Normal();
```
with the following:
```java
Distributions.MethodOfMoments.Normal SN = new Distributions.MethodOfMoments.Normal(5,3);
```
will create a normal distribution with a mean of 5 and a standard deviation of 3.  The result will be seen
in the output to the debug window.

As you can see in the previous example, we used the normal distribution from the MethodofMoments package.  The major difference between the MethodOfMoments package and the LinearMoments package is in the way that the distributions are fitted to data. 

To fit a distribution to data use the constructor that accepts an argument of double[].
```java
double[] data = {1.0,3.2,4.9,7.4,2.4,2.2};
Distributions.MethodOfMoments.Normal Norm = new Distributions.MethodOfMoments.Normal(data);
```
This example will calculate the mean and standard of deviation for the resulting normal distribution Norm, based on the input data, and the output of the monte carlo will be based on that computed mean and standard of deviation.


