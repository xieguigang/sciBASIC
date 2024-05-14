#Region "Microsoft.VisualBasic::2ee7f8ed5f4d818ea7071fb14d915976, Data_science\Mathematica\Math\Math.Statistics\MomentFunctions\ProductMoments.vb"

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

    '   Total Lines: 58
    '    Code Lines: 37
    ' Comment Lines: 11
    '   Blank Lines: 10
    '     File Size: 2.08 KB


    '     Class ProductMoments
    ' 
    '         Properties: Kurtosis, Max, Mean, Median, Min
    '                     SampleSize, Skew, StandardDeviation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions

    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class ProductMoments

        Public ReadOnly Property Median As Double
        Public ReadOnly Property Skew As Double
        Public ReadOnly Property Kurtosis As Double
        Public ReadOnly Property Min As Double
        Public ReadOnly Property Max As Double
        Public ReadOnly Property Mean As Double
        Public ReadOnly Property StandardDeviation As Double
        Public ReadOnly Property SampleSize As Integer

        Public Sub New(data As Double())
            Dim count = data.Length
            Dim BPM As New BasicProductMoments(data)

            _Min = BPM.Min()
            _Max = BPM.Max()
            _Mean = BPM.Mean()
            _StandardDeviation = BPM.StDev()
            _SampleSize = count

            Dim skewsums As Double = 0
            Dim ksums As Double = 0

            For i As Integer = 0 To data.Length - 1
                skewsums += stdNum.Pow((data(i) - _Mean), 3)
                ksums += stdNum.Pow(((data(i) - _Mean) / _StandardDeviation), 4)
            Next

            'just alittle more stdNum...
            ksums *= (count * (count + 1)) \ ((count - 1) * (count - 2) * (count - 3))
            _Skew = (count * skewsums) / ((count - 1) * (count - 2) * stdNum.Pow(_StandardDeviation, 3))
            _Kurtosis = ksums - ((3 * (stdNum.Pow(count - 1, 2))) / ((count - 2) * (count - 3)))

            'figure out an efficent algorithm for median...
            Median = data.Median
        End Sub

        Public Overrides Function ToString() As String
            Return GetJson
        End Function
    End Class
End Namespace
