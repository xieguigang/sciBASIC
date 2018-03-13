
Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 单词翻译的结果
''' </summary>
Public Class WordTranslation

    ''' <summary>
    ''' 输入的目标单词
    ''' </summary>
    ''' <returns></returns>
    Public Property Word As String
    ''' <summary>
    ''' 该单词所产生的翻译结果列表
    ''' </summary>
    ''' <returns></returns>
    Public Property Translations As Word()
    Public Property Pronunciation As String()

    Public Overrides Function ToString() As String
        Return $"{Word} -> {Translations.GetJson}"
    End Function
End Class
