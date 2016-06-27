Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=True)>
    Public Class Author : Inherits Attribute

        Public ReadOnly Property value As NamedValue(Of String)

        Sub New(name As String, email As String)
            value = New NamedValue(Of String)(name, email)
        End Sub

        Public Sub EMail()
            Call Diagnostics.Process.Start($"mailto://{value.x}")
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace