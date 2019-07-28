Namespace ApplicationServices.Plugin

    <AttributeUsage(AttributeTargets.Method Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class PluginAttribute : Inherits Attribute

        Public ReadOnly Property UniqueKey As String

        Sub New(guid As String)
            UniqueKey = guid
        End Sub

        Public Overrides Function ToString() As String
            Return UniqueKey
        End Function
    End Class
End Namespace