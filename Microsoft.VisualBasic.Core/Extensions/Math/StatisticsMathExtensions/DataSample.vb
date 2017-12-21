#Region "Microsoft.VisualBasic::e5e2b6f2a8202b2d24840418b8c8a98b, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\StatisticsMathExtensions\DataSample.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Math.Statistics

    ''' <summary>
    ''' Numeric value statics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DataSample(Of T As {IComparable, Structure})
        Implements IEnumerable(Of T)

        Protected Friend ranges As IRanges(Of T)
        Protected Friend buffer As List(Of T)
        Protected Friend means As Double

        Public Overridable ReadOnly Property Min As T
            Get
                Return ranges.Min
            End Get
        End Property

        Public Overridable ReadOnly Property Max As T
            Get
                Return ranges.Max
            End Get
        End Property

        ''' <summary>
        ''' The sample average
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Mean As Double
            Get
                Return means
            End Get
        End Property

        ''' <summary>
        ''' The sample size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SampleSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Count
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(sample As IEnumerable(Of T))
            buffer = New List(Of T)(sample)
        End Sub

        Public Iterator Function Split(partSize As Integer) As IEnumerable(Of T())
            For Each chunk As T() In buffer.SplitIterator(partSize)
                Yield chunk
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return buffer.ToArray.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In buffer
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    Public Module DataSampleExtensions

        <Extension>
        Public Function IntegerSample(data As IEnumerable(Of Integer)) As DataSample(Of Integer)
            Dim buf As Integer() = data.ToArray
            Return New DataSample(Of Integer)(buf) With {
                .means = buf.Average,
                .ranges = New IntRange(buf.Min, buf.Max)
            }
        End Function

        <Extension>
        Public Function DoubleSample(data As IEnumerable(Of Double)) As DataSample(Of Double)
            Dim buf As Double() = data.ToArray
            Return New DataSample(Of Double)(buf) With {
                .means = buf.Average,
                .ranges = New DoubleRange(buf.Min, buf.Max)
            }
        End Function
    End Module
End Namespace
