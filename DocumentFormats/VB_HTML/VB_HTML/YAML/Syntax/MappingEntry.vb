Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Syntax
    Public Class MappingEntry
        Public Key As DataItem

        Public Value As DataItem

        Public Overrides Function ToString() As String
            Return [String].Format("{{Key:{0}, Value:{1}}}", Key, Value)
        End Function
    End Class
End Namespace
