Namespace CommandLine.Reflection

    Public Class RunDllEntryPoint : Inherits [Namespace]

        Sub New(Name As String)
            Call MyBase.New(Name, "")
        End Sub
    End Class

    Public MustInherit Class CLIToken : Inherits Attribute

        Public Overridable ReadOnly Property Name As String

        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class CLIParameter : Inherits CLIToken

        Sub New(name As String)
            Call MyBase.New(name)
        End Sub
    End Class
End Namespace