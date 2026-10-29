Imports System.Diagnostics

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 由 <see cref="DebugAudit.Watch"/> 创建出来的计时作用域对象。
    ''' </summary>
    ''' <remarks>
    ''' 这个对象被设计为在<see cref="IDisposable.Dispose"/>方法之中把作用域内记录的变量值合并到
    ''' <see cref="DebugAudit.GetCaptures"/> 的全局变量快照之中，并且向
    ''' <see cref="DebugAudit.Records"/> 写入一条 <see cref="DebugAuditEventKind.Scope"/> 类型的审计事件，
    ''' 因此推荐的使用方式是<code>
    ''' Using timer = DebugAudit.Watch("loading")
    '''     timer.Capture("size", data.Length)
    '''     ' ... 处理数据 ...
    ''' End Using</code>
    ''' </remarks>
    Public NotInheritable Class DebugAuditScope : Implements IDisposable

        Private ReadOnly _name As String
        Private ReadOnly _logOnExit As Boolean
        Private ReadOnly _timer As Stopwatch
        Private ReadOnly _values As New Dictionary(Of String, Object)
        Private ReadOnly _sourceFile As String
        Private ReadOnly _line As Integer
        Private ReadOnly _caller As String
        Private _disposed As Boolean

        Friend Sub New(name As String, logOnExit As Boolean, sourceFile As String, line As Integer, caller As String)
            _name = If(String.IsNullOrEmpty(name), "<watch>", name)
            _logOnExit = logOnExit
            _sourceFile = sourceFile
            _line = line
            _caller = caller
            _timer = Stopwatch.StartNew()
        End Sub

        ''' <summary>
        ''' 这个计时作用域的名称
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>
        ''' 作用域从创建到现在(或者到被释放的时候为止)所经过的时间，单位为毫秒
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ElapsedMilliseconds As Double
            Get
                Return _timer.Elapsed.TotalMilliseconds
            End Get
        End Property

        ''' <summary>
        ''' 作用域退出的时候是否将耗时信息打印到调试终端
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property LogOnExit As Boolean
            Get
                Return _logOnExit
            End Get
        End Property

        ''' <summary>
        ''' 创建这个作用域的源代码文件路径
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property SourceFile As String
            Get
                Return _sourceFile
            End Get
        End Property

        ''' <summary>
        ''' 创建这个作用域的源代码行号
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Line As Integer
            Get
                Return _line
            End Get
        End Property

        ''' <summary>
        ''' 创建这个作用域的函数名称
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Caller As String
            Get
                Return _caller
            End Get
        End Property

        ''' <summary>
        ''' 在当前的这个计时作用域内记录一个变量的值，这个值将会在作用域退出的时候被合并到全局的变量快照之中
        ''' </summary>
        ''' <param name="name">变量的名称，为空的名字将会被忽略</param>
        ''' <param name="value">变量的值</param>
        Public Sub Capture(name As String, value As Object)
            If String.IsNullOrEmpty(name) Then
                Return
            End If

            SyncLock _values
                _values(name) = value
            End SyncLock
        End Sub

        ''' <summary>
        ''' 读取当前作用域内所记录的变量值，不存在或者名字为空的时候返回<c>Nothing</c>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function GetValue(name As String) As Object
            If String.IsNullOrEmpty(name) Then
                Return Nothing
            End If

            SyncLock _values
                If _values.ContainsKey(name) Then
                    Return _values(name)
                Else
                    Return Nothing
                End If
            End SyncLock
        End Function

        ''' <summary>
        ''' 获取当前作用域内所记录的所有变量值的副本
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Values As Dictionary(Of String, Object)
            Get
                SyncLock _values
                    Return New Dictionary(Of String, Object)(_values)
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 结束计时，合并作用域内的变量值并且写入一条审计事件。重复调用是安全的，只有第一次调用会生效。
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True
            Call _timer.Stop()

            Try
                Call DebugAudit.EndScope(Me)
            Catch ex As Exception
                ' 审计模块自身的问题不应该影响到业务代码的执行
                Call System.Diagnostics.Debug.WriteLine($"DebugAuditScope({_name}) -> {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
