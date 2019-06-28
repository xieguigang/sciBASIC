#Region "Microsoft.VisualBasic::41217b8f69c2d501c649a7825b10f729, Data_science\DataMining\DataMining\ComponentModel\Discretizer\NormalRangeDiscretizer.vb"

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

    '     Class NormalRangeDiscretizer
    ' 
    '         Properties: max, mean, min, std
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: doCreateRanges, Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Distributions

Namespace ComponentModel.Discretion

    ''' <summary>
    ''' 对原始数据进行区间离散化
    ''' </summary>
    ''' <remarks>
    ''' 这种离散化处理方法比较适用于人工神经网络, 如果你的数据在归一化之后任然无法收敛
    ''' 可以使用区间离散化来进行归一化, 通过减少数据特征来消除无法收敛的问题
    ''' </remarks>
    Public Class NormalRangeDiscretizer

        Public Property min As Double
        Public Property max As Double
        Public Property mean As Double
        Public Property std As Double

        ReadOnly ranges As DoubleRange()

        ''' <summary>
        ''' 创建一个简易的正态分布区间
        ''' </summary>
        ''' <param name="sample"></param>
        Sub New(sample As IEnumerable(Of Double))
            With sample.ToArray
                min = .Min
                max = .Max
                mean = .Average
                std = .StdError
            End With

            ranges = doCreateRanges()
        End Sub

        Sub New(dist As SampleDistribution)
            min = dist.min
            max = dist.max
            mean = dist.average
            std = dist.stdErr
            ranges = doCreateRanges()
        End Sub

        Private Function doCreateRanges() As DoubleRange()
            Dim normalLow As New DoubleRange(mean - std, mean)
            Dim normalHigh As New DoubleRange(mean + std, mean)
            Dim moderateLow As New DoubleRange(normalLow.Min, mean - 2 * std)
            Dim moderateHigh As New DoubleRange(normalHigh.Max, mean + 2 * std)
            Dim criticalLow As New DoubleRange(moderateLow.Min, mean - 3 * std)
            Dim criticalHigh As New DoubleRange(moderateHigh.Max, mean + 3 * std)

            Return {criticalLow, moderateLow, normalLow, normalHigh, moderateHigh, criticalHigh}
        End Function

        Public Function Normalize(x As Double) As Double
            If x < ranges(0).Min Then
                Return 0
            ElseIf x > ranges.Last.Max Then
                Return 1
            End If

            For i As Integer = 0 To ranges.Length - 1
                If ranges(i).IsInside(x) Then
                    Return (i + 1) * 0.15
                End If
            Next

            Throw New Exception("This exception will never happens!")
        End Function
    End Class
End Namespace
