Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct Or AttributeTargets.Enum, AllowMultiple:=False, Inherited:=True)>
    Public Class ActiveViews : Inherits Attribute

        Public ReadOnly Property Views As String
        ''' <summary>
        ''' Code type name in the markdown, default is ``json``
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String = "json"

        Sub New(view As String)
            Views = view
        End Sub

        Public Overrides Function ToString() As String
            Return Views
        End Function
    End Class
End Namespace