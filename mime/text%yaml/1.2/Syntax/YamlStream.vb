#Region "Microsoft.VisualBasic::508c95cdd6edcde824e25c1e276872c0, mime\text%yaml\1.2\Syntax\YamlStream.vb"

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

    '     Class YamlStream
    ' 
    '         Properties: Documents
    ' 
    '         Function: __maps, Enumerative
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Syntax

    Public Class YamlStream

        Public Property Documents As New List(Of YamlDocument)()

        Public Iterator Function Enumerative() As IEnumerable(Of Dictionary(Of MappingEntry))
            For Each doc As YamlDocument In Documents
                Yield __maps(doc)
            Next
        End Function

        Private Function __maps(doc As YamlDocument) As Dictionary(Of MappingEntry)
            Dim root As Mapping = DirectCast(doc.Root, Mapping)
            Return root.GetMaps
        End Function
    End Class
End Namespace
