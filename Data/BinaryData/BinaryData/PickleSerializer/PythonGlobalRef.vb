Namespace Pickle

    ''' <summary>
    ''' 内部类型：表示 Python 全局引用（module.classname）。
    ''' 由 GLOBAL 操作码产生，供 REDUCE / NEWOBJ 消费。
    ''' 不对外公开，因为用户应通过 PythonObject 访问反序列化结果。
    ''' </summary>
    Friend Class PythonGlobalRef
        Public Property ModuleName As String
        Public Property ClassName As String

        Public Sub New(moduleName As String, className As String)
            Me.ModuleName = moduleName
            Me.ClassName = className
        End Sub

        Public ReadOnly Property FullName As String
            Get
                Return $"{ModuleName}.{ClassName}"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return FullName
        End Function
    End Class

End Namespace