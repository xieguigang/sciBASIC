#Region "Microsoft.VisualBasic::e84c85dea4dbf01f8365effc80bd10b0, Data_science\Mathematica\Math\Math\Quantile\FastRankQuantile.vb"

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

    '   Total Lines: 48
    '    Code Lines: 34
    ' Comment Lines: 5
    '   Blank Lines: 9
    '     File Size: 1.40 KB


    '     Class FastRankQuantile
    ' 
    '         Properties: sample
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Query, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Distributions

Namespace Quantile

    Public Class FastRankQuantile : Implements QuantileQuery

        ReadOnly data As Double()
        ReadOnly max As Double
        ReadOnly min As Double

        Public ReadOnly Property sample As SampleDistribution
            Get
                Return New SampleDistribution(data, estimateQuantile:=False)
            End Get
        End Property

        Sub New(vector As IEnumerable(Of Double))
            Me.data = vector.OrderBy(Function(a) a).ToArray
            Me.max = data.Last
            Me.min = data.First
        End Sub

        Sub New(data As IEnumerable(Of Integer))
            Call Me.New(data.Select(Function(i) CDbl(i)))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="q">[0, 1]</param>
        ''' <returns></returns>
        Public Function Query(q As Double) As Double Implements QuantileQuery.Query
            Dim i As Integer = CInt(data.Length * q)

            If i >= data.Length Then
                Return max
            ElseIf i < 0 Then
                Return min
            Else
                Return data(i)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.debugView
        End Function
    End Class
End Namespace
