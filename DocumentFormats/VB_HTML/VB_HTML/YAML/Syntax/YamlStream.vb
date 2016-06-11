Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Scripting

Namespace YAML.Syntax

    Public Class YamlStream

        Public Property Documents As New List(Of YamlDocument)()

        Public Iterator Function Enumerative() As IEnumerable(Of Dictionary(Of MappingEntry))
            For Each doc As YamlDocument In Documents
                Yield __maps(doc)
            Next
        End Function

        Private Function __maps(doc As YamlDocument) As Dictionary(Of MappingEntry)
            Dim root As Mapping = doc.Root.As(Of Mapping)
            Return root.GetMaps
        End Function
    End Class
End Namespace
