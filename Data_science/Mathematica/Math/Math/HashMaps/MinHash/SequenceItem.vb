Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HashMaps.MinHash

    ''' <summary>
    ''' 定义一个简单的数据结构来存储序列
    ''' </summary>
    Public Class SequenceItem

        Public Property ID As Integer
        ''' <summary>
        ''' 最终生成的MinHash签名
        ''' </summary>
        ''' <returns></returns>
        Public Property Signature As UInteger()

        Public Overrides Function ToString() As String
            Return $"[{ID}]_{Signature.GetJson}"
        End Function
    End Class

End Namespace