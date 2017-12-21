Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 为了构建GCModeller的脚本自动化而构建的源代码生成器，生成``perl module(*.pm)``
    ''' </summary>
    Public Class Perl : Inherits CodeGenerator

        Dim namespace$

        Public Sub New(CLI As Type, namespace$)
            MyBase.New(CLI)
            Me.namespace = [namespace]
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace