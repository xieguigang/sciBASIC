''' <summary>
''' 字典树（Trie）
''' </summary>
Public Class Trie

    Public ReadOnly Property Root As CharacterNode

End Class

Public Class CharacterNode : Inherits Tree(Of Char, Char)

    ''' <summary>
    ''' 以这个字符结束的单词的数目
    ''' </summary>
    ''' <returns></returns>
    Public Property Ends As Integer

    Sub New()
        Call MyBase.New(qualDeli:="")
    End Sub
End Class