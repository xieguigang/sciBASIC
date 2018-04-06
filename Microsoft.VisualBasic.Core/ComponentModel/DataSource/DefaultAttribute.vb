Imports Microsoft.VisualBasic.Scripting

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Property default value
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class DefaultAttribute : Inherits Attribute

        Public ReadOnly Property Value As Object

        Sub New(value As Object)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return InputHandler.ToString(Value)
        End Function
    End Class
End Namespace