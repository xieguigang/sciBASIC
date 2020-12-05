Imports Microsoft.VisualBasic.Language.Python

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
                Return buffer.SpanSlice(start, span_size)
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