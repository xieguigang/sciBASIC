Imports Microsoft.VisualBasic.Math.Distributions.MathGamma
Imports f64 = System.Double
Imports i32 = System.Int32
Imports stdNum = System.Math

Namespace Hypothesis.FishersExact

    ''' <summary>
    ''' Fisher's exact test.
    '''
    ''' Implements a 2×2 Fishers exact test. Use this to test the independence of two
    ''' categorical variables when the sample sizes are small.
    '''
    ''' For an approachable explanation of Fisher's exact test, see
    ''' [Fisher's exact test of independence](http://www.biostathandbook.com/fishers.html) by
    ''' John H. McDonald in the [Handbook of Biological Statistics](http://www.biostathandbook.com/).
    '''
    ''' The test Is computed using code ported from Øyvind Langsrud's JavaScript
    ''' implementation at [http://www.langsrud.com/fisher.htm](http://www.langsrud.com/fisher.htm),
    ''' used with permission.
    '''
    ''' https//github.com/cpearce/fishers_exact
    ''' </summary>
    Public Module FishersExactTest

        Public Function lnfact(n As i32) As f64
            If n <= 1 Then
                Return 0.0
            Else
                Return lngamma(n + 1)
            End If
        End Function

        Public Function lnbico(n As i32, k As i32) As f64
            Return lnfact(n) - lnfact(k) - lnfact(n - k)
        End Function

        Public Function hyper_323(n11 As i32, n1_ As i32, n_1 As i32, n As i32) As f64
            Return stdNum.Exp((lnbico(n1_, n11) + lnbico(n - n1_, n_1 - n11) - lnbico(n, n_1)))
        End Function

        Public Function hyper(s As HyperState, n11 As i32) As f64
            Return hyper0(s, n11, 0, 0, 0)
        End Function

        Public Function hyper0(s As HyperState, n11i As i32, n1_i As i32, n_1i As i32, ni As i32) As f64
            If s.valid AndAlso (n1_i Or n_1i Or ni) = 0 Then
                If Not (n11i Mod 10 = 0) Then
                    If n11i = s.n11 + 1 Then
                        s.prob *= ((s.n1_ - s.n11) / n11i) * ((s.n_1 - s.n11) / (n11i + s.n - s.n1_ - s.n_1))
                        s.n11 = n11i
                        Return s.prob
                    End If
                    If n11i = s.n11 - 1 Then
                        s.prob *= ((s.n11) / (s.n1_ - n11i)) * ((s.n11 + s.n - s.n1_ - s.n_1) / (s.n_1 - n11i))
                        s.n11 = n11i
                        Return s.prob
                    End If
                End If
                s.n11 = n11i
            Else
                s.n11 = n11i
                s.n1_ = n1_i
                s.n_1 = n_1i
                s.n = ni
                s.valid = True
            End If
            s.prob = hyper_323(s.n11, s.n1_, s.n_1, s.n)
            Return s.prob
        End Function
    End Module
End Namespace