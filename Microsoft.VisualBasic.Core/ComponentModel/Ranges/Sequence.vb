#Region "Microsoft.VisualBasic::585af9b6e34291ded89ba898745a1541, Microsoft.VisualBasic.Core\ComponentModel\Ranges\Sequence.vb"

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

    '     Class Sequence
    ' 
    '         Properties: Max, Min, n, Range, Steps
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) IsInside, IsOverlapping, ToArray, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace ComponentModel.Ranges

    <XmlType("numeric-sequence")>
    Public Class Sequence : Implements IRanges(Of Double)

        <XmlElement("range")>
        Public Property Range As DoubleRange
        <XmlAttribute>
        Public Property n As Integer

        Public ReadOnly Property Steps As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Length / n
            End Get
        End Property

        Public ReadOnly Property Min As Double Implements IRange(Of Double).Min
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Min
            End Get
        End Property
        Public ReadOnly Property Max As Double Implements IRange(Of Double).Max
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Range.Max
            End Get
        End Property

        Sub New(a#, b#, n%)
            Me.Range = New DoubleRange(a, b)
            Me.n = n
        End Sub

        Sub New()

        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsInside(x As Double) As Boolean Implements IRanges(Of Double).IsInside
            Return Range.IsInside(x)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsInside(range As IRanges(Of Double)) As Boolean Implements IRanges(Of Double).IsInside
            Return range.IsInside(range)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsOverlapping(range As IRanges(Of Double)) As Boolean Implements IRanges(Of Double).IsOverlapping
            Return range.IsOverlapping(range)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As Double()
            Return Range.Enumerate(n)
        End Function

        Public Overrides Function ToString() As String
            Return Range.ToString
        End Function
    End Class
End Namespace
