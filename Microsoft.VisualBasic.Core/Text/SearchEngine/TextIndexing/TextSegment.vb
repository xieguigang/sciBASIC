#Region "Microsoft.VisualBasic::f4d500803df92a3b5f6a6b9bd64382f3, Microsoft.VisualBasic.Core\Text\SearchEngine\TextIndexing\TextSegment.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class TextSegment
    ' 
    '         Properties: Array, Index, Segment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Text.Levenshtein

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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _text
            End Get
            Set(value As String)
                Dim ascii%() = value _
                    .Select(AddressOf AscW) _
                    .ToArray

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(segment As TextSegment) As String
            Return segment.Segment
        End Operator

        Public Shared Operator Like(segment As TextSegment, text$) As DistResult
            With LevenshteinDistance.ComputeDistance(segment.Array, text)
                .Reference = segment._text
                Return .ByRef
            End With
        End Operator
    End Class
End Namespace
