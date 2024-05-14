#Region "Microsoft.VisualBasic::14aa13c87d6e5ec8a37d0e2600c00890, Data_science\Mathematica\Math\Math\Distributions\ECDF.vb"

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

    '   Total Lines: 111
    '    Code Lines: 50
    ' Comment Lines: 47
    '   Blank Lines: 14
    '     File Size: 4.32 KB


    '     Class ECDF
    ' 
    '         Properties: X
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CDF, FindThreshold
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' #### Empirical distribution function
    ''' 
    ''' In statistics, an empirical distribution function (commonly also called 
    ''' an empirical cumulative distribution function, eCDF) is the distribution
    ''' function associated with the empirical measure of a sample. This cumulative
    ''' distribution function is a step function that jumps up by 1/n at each of
    ''' the n data points. Its value at any specified value of the measured 
    ''' variable is the fraction of observations of the measured variable that 
    ''' are less than or equal to the specified value.
    '''
    ''' The empirical distribution Function Is an estimate Of the cumulative 
    ''' distribution Function that generated the points In the sample. It converges 
    ''' With probability 1 To that underlying distribution, according To the 
    ''' Glivenko–Cantelli theorem. A number Of results exist To quantify the rate 
    ''' Of convergence Of the empirical distribution Function To the underlying
    ''' cumulative distribution Function.
    ''' </summary>
    Public Class ECDF

        ReadOnly data As DataBinBox(Of Double)()

        ''' <summary>
        ''' the input x vector data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property X As IEnumerable(Of Double)
            Get
                Return data.Select(Function(bi) bi.Raw).IteratesALL
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(x As IEnumerable(Of Double), Optional k As Integer = 100)
            data = CutBins.FixedWidthBins(x, k, Function(xi) xi).ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="q"></param>
        ''' <param name="eps"></param>
        ''' <returns>
        ''' the upper bound raw value of the threshold
        ''' </returns>
        Public Function FindThreshold(q As Double, Optional eps As Double = 0.1) As Double
            Dim sample As DataBinBox(Of Double)() = data _
                .OrderBy(Function(b) b.Boundary.Min) _
                .ToArray
            Dim N As Integer = Aggregate point As DataBinBox(Of Double)
                               In sample
                               Into Sum(point.Count)
            Dim minK As Integer = 1
            Dim minD As Double = Double.MaxValue

            If sample.Length = 0 Then
                Return 0
            End If

            For k As Integer = 1 To sample.Length - 1
                Dim cdf As Double = ECDF.CDF(sample.Take(k), N)

                If cdf > q Then
                    Exit For
                End If

                Dim d As Double = std.Abs(cdf - q)

                If d <= eps Then
                    Return sample(k).Boundary.Min
                ElseIf d < minD Then
                    minD = d
                    minK = k
                End If
            Next

            Return sample(minK - 1).Boundary.Max
        End Function

        ''' <summary>
        ''' T computation involves the cumulative distributive
        ''' function p(k)(CDF), defined As
        ''' 
        ''' ```
        ''' q ~ p(k) = sum(h(i)) / N
        ''' ```
        '''
        ''' h(i) stands For the i bin's frequency within an image 
        ''' histogram, N is the image’s pixel count. Given a target 
        ''' probability q it Is possible to find the bin k whose 
        ''' CDF closely resembles q. Then, the upper limit Of the 
        ''' bin k In h will be used As the threshold value T.
        ''' </summary>
        ''' <param name="bin"></param>
        ''' <param name="N"></param>
        ''' <returns></returns>
        Public Shared Function CDF(bin As IEnumerable(Of DataBinBox(Of Double)), N As Integer) As Double
            Dim sumHk As Double = Aggregate hi As DataBinBox(Of Double) In bin Into Sum(hi.Count)
            Dim p As Double = sumHk / N

            Return p
        End Function
    End Class
End Namespace
