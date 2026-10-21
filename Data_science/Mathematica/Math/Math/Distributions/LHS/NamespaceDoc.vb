Namespace Distributions.LHS

    ''' <summary>
    ''' Latin hypercube sampling (LHS) in the unit hypercube: stratified
    ''' sampling where every one dimensional projection of the design owns
    ''' exactly one sample point inside each of the N equal probability
    ''' strata, while the dimensions are paired by independent random
    ''' permutations.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The sample matrix layout used by all of the members inside this
    ''' namespace is:
    ''' </para>
    ''' <code>
    ''' samples(i)(j)  i = 0 .. N - 1  (the sample row index)
    '''                j = 0 .. d - 1  (the dimension/column index)
    ''' </code>
    ''' <para>
    ''' so that a design of N samples and d dimensions is a `N x d` matrix
    ''' where the rows are the samples. Such a row orientated matrix can be
    ''' used to construct a <see cref="Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix"/>
    ''' directly, and each column of the design is a perfect stratification
    ''' of the interval `[0,1)`.
    ''' </para>
    ''' <para>
    ''' Notes from the literature:
    ''' </para>
    ''' <list type="bullet">
    ''' <item>LHS reduces the variance of an integral estimate from
    ''' `O(N^-1/2)` (plain Monte Carlo) to nearly `O(N^-1)`.</item>
    ''' <item>LHS does not remove the spurious correlation between the
    ''' dimensions that a random pairing creates; the Iman-Conover
    ''' rearrangement is required if a target correlation structure
    ''' must be honoured.</item>
    ''' <item>Never append extra samples to an existing LHS design: the
    ''' stratification of the whole design gets broken. Generate a
    ''' complete new design instead.</item>
    ''' </list>
    ''' </remarks>
    Module NamespaceDoc
    End Module

End Namespace
