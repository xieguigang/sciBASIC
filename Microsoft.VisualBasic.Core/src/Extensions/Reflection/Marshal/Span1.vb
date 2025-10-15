#Region "Microsoft.VisualBasic::df001eecc22389649d2e0afe308f5f02, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Marshal\Span1.vb"

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

    '   Total Lines: 90
    '    Code Lines: 60 (66.67%)
    ' Comment Lines: 16 (17.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (15.56%)
    '     File Size: 2.71 KB


    '     Class Span
    ' 
    '         Properties: ArrayLength, Length, OffsetEnds, SpanView
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Slice, SpanCopy, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Emit.Marshal

    ''' <summary>
    ''' A simulation of system.span in .NET 5
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Span(Of T)

        ReadOnly buffer As T()
        ReadOnly start As Integer
        ReadOnly span_size As Integer

        ''' <summary>
        ''' current span view size
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            Get
                Return span_size
            End Get
        End Property

        Public ReadOnly Property SpanView As T()
            Get
                Return SpanCopy()
            End Get
        End Property

        ''' <summary>
        ''' the length of the entire array
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ArrayLength As Integer
            Get
                Return buffer.Length
            End Get
        End Property

        Default Public Property Item(i As Integer) As T
            Get
                Return buffer(start + i)
            End Get
            Set(value As T)
                buffer(start + i) = value
            End Set
        End Property

        ''' <summary>
        ''' the offset ends in the raw input buffer
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OffsetEnds As Integer
            Get
                Return start + Length
            End Get
        End Property

        Sub New(ByRef raw As T())
            buffer = raw
        End Sub

        Public Sub New(ByRef raw As T(), start As Integer, length As Integer)
            Me.buffer = raw
            Me.start = start
            Me.span_size = length
        End Sub

        Public Function SpanCopy() As T()
            Dim v As T() = New T(span_size - 1) {}
            Call Array.ConstrainedCopy(buffer, start, v, Scan0, span_size)
            Return v
        End Function

        Public Function Slice(start As Integer, length As Integer) As Span(Of T)
            Return New Span(Of T)(buffer, start, length)
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim Span As {GetType(T).Name}[] = new {GetType(T).Name}[{ArrayLength - 1}][ span_view={start}:{OffsetEnds}, span_size={Length} ]"
        End Function

        Public Shared Widening Operator CType(raw As T()) As Span(Of T)
            Return New Span(Of T)(raw)
        End Operator

        Public Shared Narrowing Operator CType(span As Span(Of T)) As T()
            Return span.buffer
        End Operator
    End Class
End Namespace
