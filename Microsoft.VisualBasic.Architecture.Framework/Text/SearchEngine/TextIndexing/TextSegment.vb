Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Text.Search

    ''' <summary>
    ''' 文本之中的一个片段
    ''' </summary>
    Public Class TextSegment

        Dim _text As String

        ''' <summary>
        ''' 当前的这个文本片段的字符串值
        ''' </summary>
        ''' <returns></returns>
        Public Property Segment As String
            Get
                Return _text
            End Get
            Set(value As String)
                Dim ascii As Integer() = value.ToArray(Function(c) AscW(c))
                _Array = ascii
                _text = value
            End Set
        End Property

        ''' <summary>
        ''' ASCII值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Array As Integer()
        ''' <summary>
        ''' 在原始文本之中的左端起始位置
        ''' </summary>
        ''' <returns></returns>
        Public Property Index As Integer

        Sub New(Optional value As String = "")
            Segment = value
        End Sub

        Public Overrides Function ToString() As String
            Return Segment
        End Function

        Public Overloads Shared Narrowing Operator CType(segment As TextSegment) As String
            Return segment.Segment
        End Operator
    End Class
End Namespace