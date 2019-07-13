#Region "Microsoft.VisualBasic::ebe014bdbcebb4d7a36a056b1fe7cb2c, mime\text%html\Markups\Markup.vb"

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

    ' Class Markup
    ' 
    '     Properties: nodes
    ' 
    '     Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The markup document(*.html, *.md) its document syntax structure object. 
''' </summary>
Public Class Markup
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
