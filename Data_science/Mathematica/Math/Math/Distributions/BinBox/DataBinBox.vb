#Region "Microsoft.VisualBasic::8f3032d89098953626b8bc3fea2b9077, Data_science\Mathematica\Math\Math\Distributions\BinBox\DataBinBox.vb"

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

    '     Class DataBinBox
    ' 
    '         Properties: BinMaps, Count, Raw, Sample
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBinMaps, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Statistics.Linq

Namespace Distributions.BinBox

    ''' <summary>
    ''' 数据分箱之中的一个bucket
    ''' </summary>
    Public Class DataBinBox(Of T)

        Public ReadOnly Property Raw As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return bin.ToArray
            End Get
        End Property

        Public ReadOnly Property BinMaps(Optional method As BinMaps = BinBox.BinMaps.Mean) As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetBinMaps(bin, method, eval)
            End Get
        End Property

        Public ReadOnly Property Sample As SampleDistribution
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New SampleDistribution(bin.Select(Function(x) eval(x)))
            End Get
        End Property

        ReadOnly bin As New List(Of T)
        ReadOnly eval As Evaluate(Of T)

        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return bin.Count
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of T), eval As Evaluate(Of T))
            Me.bin = data.ToList
            Me.eval = eval
        End Sub

        Public Overrides Function ToString() As String
            With Sample
                Return $"[{ .min}, { .max}] count={Count}"
            End With
        End Function

        ''' <summary>
        ''' 将当前的区间内的对象序列映射为一段实数序列
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="method"></param>
        ''' <param name="eval"></param>
        ''' <returns></returns>
        Public Shared Function GetBinMaps(data As IEnumerable(Of T), method As BinMaps, eval As Evaluate(Of T)) As Double()
            Dim v As Double() = data.Select(Function(x) eval(x)).ToArray

            Select Case method
                Case BinBox.BinMaps.Mean
                    Return v.Average.Replicate(v.Length).ToArray

                Case BinBox.BinMaps.Median
                    Return v.Median.Replicate(v.Length).ToArray

                Case BinBox.BinMaps.Boundary
                    Dim min = v.Min
                    Dim max = v.Max

                    Return (Iterator Function() As IEnumerable(Of Double)
                                For Each x As Double In v
                                    If (x - min) < (max - x) Then
                                        Yield min
                                    Else
                                        Yield max
                                    End If
                                Next
                            End Function)().ToArray

                Case Else
                    ' mean by default
                    Return v.Average.Replicate(v.Length).ToArray
            End Select
        End Function
    End Class
End Namespace
