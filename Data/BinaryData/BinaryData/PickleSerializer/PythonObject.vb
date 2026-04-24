Namespace Pickle

    ''' <summary>
    ''' 表示从 Pickle 反序列化的未知 Python 对象。
    ''' 当遇到无法映射到 .NET 类型的 Python 类实例时，
    ''' 使用此包装器保存模块名、类名、构造参数和对象状态，
    ''' 以便后续代码能够识别和处理这些对象。
    ''' </summary>
    Public Class PythonObject
        ''' <summary>Python 模块名（如 "collections"）</summary>
        Public Property ModuleName As String

        ''' <summary>Python 类名（如 "OrderedDict"）</summary>
        Public Property ClassName As String

        ''' <summary>传递给 __init__ 的构造参数</summary>
        Public Property ConstructorArgs As Object()

        ''' <summary>对象的 __dict__ 状态（由 BUILD 操作码设置）</summary>
        Public Property State As Dictionary(Of Object, Object)

        Public Sub New(moduleName As String, className As String, args As Object())
            Me.ModuleName = moduleName
            Me.ClassName = className
            Me.ConstructorArgs = args
            Me.State = New Dictionary(Of Object, Object)()
        End Sub

        ''' <summary>获取状态的完整限定类名</summary>
        Public ReadOnly Property FullName As String
            Get
                Return $"{ModuleName}.{ClassName}"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"<{FullName}>"
        End Function
    End Class

End Namespace