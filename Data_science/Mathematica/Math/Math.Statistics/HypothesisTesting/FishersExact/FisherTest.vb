#Region "Microsoft.VisualBasic::570ed7a889acca2756219a33ba97faf1, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\FishersExact\FisherTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 215
    '    Code Lines: 131 (60.93%)
    ' Comment Lines: 54 (25.12%)
    '    - Xml Docs: 81.48%
    ' 
    '   Blank Lines: 30 (13.95%)
    '     File Size: 7.43 KB


    '     Module FishersExactTest
    ' 
    '         Function: exact, FishersExact, hyper, hyper_323, hyper0
    '                   lnbico, lnfact
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Distributions.MathGamma
Imports f64 = System.Double
Imports i32 = System.Int32
Imports std = System.Math

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
                Return lngamm(n + 1)
            End If
        End Function

        Public Function lnbico(n As i32, k As i32) As f64
            Return lnfact(n) - lnfact(k) - lnfact(n - k)
        End Function

        Public Function hyper_323(n11 As i32, n1_ As i32, n_1 As i32, n As i32) As f64
            Return std.Exp((lnbico(n1_, n11) + lnbico(n - n1_, n_1 - n11) - lnbico(n, n_1)))
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

        ''' <summary>
        ''' Returns prob,sleft,sright,sless,slarg
        ''' </summary>
        ''' <param name="n11"></param>
        ''' <param name="n1_"></param>
        ''' <param name="n_1"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function exact(n11 As i32, n1_ As i32, n_1 As i32, n As i32) As (f64, f64, f64, f64, f64, HyperState)
            Dim sleft As f64
            Dim sright As f64
            Dim sless As f64
            Dim slarg As f64
            Dim p As f64
            Dim i As i32
            Dim j As i32
            Dim prob As f64
            Dim max = n1_

            If n_1 < max Then
                max = n_1
            End If

            Dim min = n1_ + n_1 - n

            If min < 0 Then
                min = 0
            End If
            If min = max Then
                Return (1.0, 1.0, 1.0, 1.0, 1.0, New HyperState)
            End If

            Dim s As New HyperState

            prob = hyper0(s, n11, n1_, n_1, n)
            sleft = 0.0

            If prob.IsNaNImaginary Then
                Return (1.0, 1.0, 1.0, 1.0, 1.0, New HyperState)
            End If

            p = hyper(s, min)
            i = min + 1

            Do While p <= 0.99999999 * prob
                sleft += p
                p = hyper(s, i)
                i += 1
            Loop

            i -= 1

            If p <= 1.00000001 * prob Then
                sleft += p
            Else
                i += 1
            End If

            sright = 0.0
            p = hyper(s, max)
            j = max - 1

            Do While p <= 0.99999999 * prob
                sright += p
                p = hyper(s, j)
                j -= 1
            Loop

            j += 1

            If p <= 1.00000001 * prob Then
                sright += p
            Else
                j += 1
            End If

            If std.Abs(i - n11) < std.Abs(j - n11) Then
                sless = sleft
                slarg = 1.0 - sleft + prob
            Else
                sless = 1.0 - sright + prob
                slarg = sright
            End If

            Return (prob, sleft, sright, sless, slarg, s)
        End Function

        ''' <summary>
        ''' Computes the Fisher's exact pvales to determine if there are nonrandom associations between two
        ''' categorical variables, in a two by two contingency table.
        '''
        ''' The test Is computed using code ported from Øyvind Langsrud's JavaScript
        ''' implementation at [http://www.langsrud.com/fisher.htm](http://www.langsrud.com/fisher.htm).
        '''
        ''' Use this when sample sizes are small. For large samples, other statistical tests of independence
        ''' are more appropriate.
        '''
        ''' # Examples
        ''' ```
        ''' use fishers_exact:fishers_exact;
        '''
        ''' let p = fishers_exact(&amp;[1,9,11,3]).unwrap();
        '''
        ''' assert!((p.less_pvalue - 0.001346).abs() &lt; 0.0001);
        ''' assert!((p.greater_pvalue - 0.9999663).abs() &lt; 0.0001);
        ''' assert!((p.two_tail_pvalue - 0.0027594).abs() &lt; 0.0001);
        ''' ```
        '''
        ''' # Errors
        ''' Returns `TooLargeValueError` if any member in the `table` array Is too large. Currently
        ''' "too large" Is defined as greater than `std:i32:MAX`.
        ''' </summary>
        ''' <param name="n11"></param>
        ''' <param name="n12"></param>
        ''' <param name="n21"></param>
        ''' <param name="n22"></param>
        ''' <returns></returns>
        Public Function FishersExact(n11 As i32, n12 As i32, n21 As i32, n22 As i32) As FishersExactPvalues
            Dim left, right, twotail
            Dim n1_ = n11 + n12
            Dim n_1 = n11 + n21
            Dim n = n11 + n12 + n21 + n22
            Dim rtvl As (prob#, sleft#, sright#, sless#, slarg#, hyper_stat As HyperState) = exact(n11, n1_, n_1, n)

            left = rtvl.sless
            right = rtvl.slarg
            twotail = rtvl.sleft + rtvl.sright

            If twotail > 1.0 Then
                twotail = 1.0
            End If

            Return New FishersExactPvalues With {
                .two_tail_pvalue = twotail,
                .less_pvalue = left,
                .greater_pvalue = right,
                .hyper_state = rtvl.hyper_stat,
                .matrix = {n11, n12, n21, n22}
            }
        End Function
    End Module
End Namespace
