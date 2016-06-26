Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace CommandLine.Reflection

    Public Class RunDllEntryPoint : Inherits [Namespace]

        Sub New(Name As String)
            Call MyBase.New(Name, "")
        End Sub
    End Class

    ''' <summary>
    ''' A very basically type in the <see cref="CommandLine"/>
    ''' </summary>
    Public MustInherit Class CLIToken : Inherits Attribute
        Implements IReadOnlyId

        ''' <summary>
        ''' Name of this token object, this can be parameter name or api name.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Name As String Implements IReadOnlyId.Identity

        ''' <summary>
        ''' Init this token by using <see cref="name"/> value.
        ''' </summary>
        ''' <param name="name">Token name</param>
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