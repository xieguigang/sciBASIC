#Region "Microsoft.VisualBasic::e0edd21a496943541d9f5950948fa921, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Marshal\Span1.vb"

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

    '   Total Lines: 80
    '    Code Lines: 55
    ' Comment Lines: 12
    '   Blank Lines: 13
    '     File Size: 2.39 KB


    '     Class Span
    ' 
    '         Properties: ArrayLength, Length, SpanView
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
        ''' the span size
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

        Sub New(ByRef raw As T())
            buffer = raw
        End Sub

        Private Sub New(ByRef raw As T(), start As Integer, length As Integer)
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
            Return $"Dim Span As {GetType(T).Name}[] = new {GetType(T).Name}[{ArrayLength - 1}][&{start}:&{start + span_size}]"
        End Function

        Public Shared Widening Operator CType(raw As T()) As Span(Of T)
            Return New Span(Of T)(raw)
        End Operator

        Public Shared Narrowing Operator CType(span As Span(Of T)) As T()
            Return span.buffer
        End Operator
    End Class
End Namespace
