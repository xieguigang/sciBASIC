Namespace CommandLine.InteropService

    ''' <summary>
    ''' The class object which can interact with the target commandline program.
    ''' (与目标命令行程序进行命令行交互的编程接口，本类型的对象的作用主要是生成命令行参数)
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class InteropService : Inherits CLIBuilder

        ''' <summary>
        ''' Assembly path for the target invoked program.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend _executableAssembly As String

        Public Function RunDotNetApp(args$) As IIORedirectAbstract
            Return App.Shell(_executableAssembly, args, CLR:=True)
        End Function

        Public Function RunProgram(args$) As IIORedirectAbstract
            Return App.Shell(_executableAssembly, args, CLR:=False)
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(_executableAssembly) Then
                Return MyBase.ToString()
            Else
                Return _executableAssembly
            End If
        End Function
    End Class

    ''' <summary>
    ''' Default value
    ''' </summary>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class NullOrDefault : Inherits Attribute

        Public ReadOnly Property value As String

        Sub New(Optional _default As String = "")
            value = _default
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class

    Public MustInherit Class CLIBuilder

        Public Overrides Function ToString() As String
            Return Me.GetCLI
        End Function
    End Class
End Namespace