Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace YAML.Syntax

    Public Class MappingEntry : Implements sIdEnumerable

        Public Key As DataItem

        Public Value As DataItem

        Private Property Identifier As String Implements sIdEnumerable.Identifier
            Get
                Return Scripting.ToString(Key)
            End Get
            Set(value As String)
                Throw New ReadOnlyException
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return [String].Format("{{Key:{0}, Value:{1}}}", Key, Value)
        End Function
    End Class
End Namespace
