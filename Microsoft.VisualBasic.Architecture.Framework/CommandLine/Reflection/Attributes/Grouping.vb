Namespace CommandLine.Reflection

    ''' <summary>
    ''' 应用于方法之上的，标注当前的这个方法的功能分组
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class GroupAttribute : Inherits CLIToken

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Sub New(name As System.Enum)
            MyBase.New(name.Description)
        End Sub
    End Class

    ''' <summary>
    ''' 应用于命令行类型容器之上的，用于功能分组的详细描述信息
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class GroupingDefineAttribute : Inherits GroupAttribute

        ''' <summary>
        ''' 当前的这一功能分组的详细描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Property Description As String

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Sub New(name As System.Enum)
            MyBase.New(name)
        End Sub
    End Class
End Namespace