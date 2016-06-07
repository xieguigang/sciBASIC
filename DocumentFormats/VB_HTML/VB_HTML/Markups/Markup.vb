Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The markup document(*.html, *.md) its document syntax structure object. 
''' </summary>
Public Class Markup : Inherits ClassObject
    Implements IEnumerable(Of PlantText)

    Public Property nodes As List(Of PlantText)

    Public Iterator Function GetEnumerator() As IEnumerator(Of PlantText) Implements IEnumerable(Of PlantText).GetEnumerator
        If nodes.IsNullOrEmpty Then
            Return
        End If

        For Each node As PlantText In nodes
            Yield node
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class