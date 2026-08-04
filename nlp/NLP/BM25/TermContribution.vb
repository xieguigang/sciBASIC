Namespace BM25

    ''' <summary>
    ''' 单个查询词的贡献明细。
    ''' </summary>
    Public Class TermContribution

        ''' <summary>查询词。</summary>
        Public Property Term As String

        ''' <summary>IDF 值。</summary>
        Public Property Idf As Double

        ''' <summary>词频 TF。</summary>
        Public Property Tf As Integer

        ''' <summary>文档长度因子。</summary>
        Public Property LengthFactor As Double

        ''' <summary>该词的 TF 饱和部分得分。</summary>
        Public Property TfSaturation As Double

        ''' <summary>该词的最终贡献分。</summary>
        Public Property Contribution As Double

    End Class
End Namespace