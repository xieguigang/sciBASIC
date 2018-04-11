
''' <summary>
''' ### Fisher's exact test
''' 
''' > https://en.wikipedia.org/wiki/Fisher's_exact_test
''' </summary>
Public Module FisherTest

    Public Function FisherPvalue(a#, b#, c#, d#) As Double
        Dim X = VBMath.Factorial(a + b) *
                VBMath.Factorial(c + d) *
                VBMath.Factorial(a + c) *
                VBMath.Factorial(b + d)
        Dim N = a + b + c + d
        Dim Y = VBMath.Factorial(a) *
                VBMath.Factorial(b) *
                VBMath.Factorial(c) *
                VBMath.Factorial(d) *
                VBMath.Factorial(N)
        Dim p# = X / Y

        Return p
    End Function
End Module
