Imports Microsoft.VisualBasic.Serialization

Public Class ParagraphText : Inherits PlantText

    Public Property Nodes As PlantText()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

''' <summary>
''' 单纯的文本对象
''' </summary>
Public Class PlantText

    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Class

Public Class Hyperlink : Inherits PlantText

    Public Property Links As String
    Public Property Title As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class