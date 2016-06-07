Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The markup document(*.html, *.md) its document syntax structure object. 
''' </summary>
Public Class Markup : Inherits ClassObject
    Implements IEnumerable(Of PlantText)

    Public Property Document As List(Of PlantText)

    Public Iterator Function GetEnumerator() As IEnumerator(Of PlantText) Implements IEnumerable(Of PlantText).GetEnumerator
        If Document.IsNullOrEmpty Then
            Return
        End If

        For Each node As PlantText In Document
            Yield node
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class

Public Class PlantText

    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Class

Public Class Header : Inherits PlantText

    Public Property Level As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class