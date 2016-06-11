Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Syntax
    Public Class Mapping
        Inherits DataItem

        Public Enties As New List(Of MappingEntry)()

        Public Function GetMaps() As Dictionary(Of MappingEntry)
            Return New Dictionary(Of MappingEntry)(Enties)
        End Function
    End Class
End Namespace
