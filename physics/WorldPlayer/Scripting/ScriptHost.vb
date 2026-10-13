Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Language
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' LiteJs 宿主封装：负责引擎生命周期、脚本装载、事件注册与回调驱动。
''' </summary>
''' <remarks>
''' 线程约定：LiteJs 的 <c>JsRuntime.Io</c> 是模块级静态属性，引擎本身无锁，
''' 因此全部调用都必须在 UI 线程串行执行。
''' </remarks>
Public Class ScriptHost

    Private ReadOnly onLog As Action(Of String, Boolean)
    Private ReadOnly io As UiScriptIO
    Private engine As Interpreter

    ''' <summary>已注册的事件回调表。</summary>
    Public ReadOnly Handlers As New Dictionary(Of String, Func(Of Object(), Object))()

    ''' <summary>当前脚本路径（仅用于显示）。</summary>
    Public Property ScriptPath As String = ""

    ''' <summary>最近一次装载错误；Nothing 表示正常。</summary>
    Public Property LastError As String = Nothing

    Public Sub New(onLog As Action(Of String, Boolean))
        Me.onLog = onLog
        Me.io = New UiScriptIO(onLog)
    End Sub

    ''' <summary>引擎是否可用。</summary>
    Public ReadOnly Property IsReady As Boolean
        Get
            Return engine IsNot Nothing
        End Get
    End Property

    ' /********************************************************************************/
    '  全局注册
    ' /********************************************************************************/

    ''' <summary>
    ''' 注册一个宿主侧全局值。可注册：
    ''' Double / String / Boolean、<see cref="List(Of Object)"/>（JS 数组）、
    ''' <see cref="Dictionary(Of String, Object)"/>（JS 对象）、
    ''' <see cref="Func(Of Object(), Object)"/>（JS 函数）。
    ''' </summary>
    Public Sub DefineGlobal(name As String, value As Object)
        If engine Is Nothing Then Return

        Call engine.DefineGlobal(name, value)
    End Sub

    ''' <summary>注册 <c>on(event, fn)</c>：JS 用它把回调交给宿主。</summary>
    Public Sub DefineEventHook()
        DefineGlobal("on", Function(args As Object()) As Object
                                If args Is Nothing OrElse args.Length < 2 Then Return JsRuntime.Undef

                                Dim name As String = JsRuntime.JsStr(args(0))
                                Dim fn As Object = args(1)

                                If Not TypeOf fn Is Func(Of Object(), Object) Then
                                    Call io.WriteError($"on('{name}') 的第二个参数必须是函数")
                                    Return JsRuntime.Undef
                                End If

                                Handlers(name) = DirectCast(fn, Func(Of Object(), Object))

                                Return JsRuntime.Undef
                            End Function)
    End Sub

    ' /********************************************************************************/
    '  装载与执行
    ' /********************************************************************************/

    ''' <summary>
    ''' 装载并运行一段脚本。成功返回 True；语法错误与运行时错误都会
    ''' 转成错误文本，不会抛给 UI。
    ''' </summary>
    Public Function Load(source As String) As Boolean
        LastError = Nothing
        Handlers.Clear()

        Try
            engine = New Interpreter(io)

            Call DefineEventHook()

            Dim program As Program = Parser.Parse(source)

            Call engine.Run(program)

            Return True
        Catch ex As ParseException
            LastError = $"语法错误: {ex.Message}"
        Catch ex As JsRuntimeException
            LastError = $"运行时错误: {ex.Message}"
        Catch ex As Exception
            LastError = $"宿主错误: {ex.Message}"
        End Try

        engine = Nothing
        Call io.WriteError(LastError)

        Return False
    End Function

    ' /********************************************************************************/
    '  回调驱动
    ' /********************************************************************************/

    ''' <summary>
    ''' 触发一个 JS 事件。返回 True 表示存在回调且执行成功。
    ''' </summary>
    Public Function Fire(name As String, ParamArray args As Object()) As Boolean
        Dim fn As Func(Of Object(), Object) = Nothing

        If Not Handlers.TryGetValue(name, fn) Then Return False

        Try
            Call JsRuntime.JsInvoke(fn, args)
            Return True
        Catch ex As JsRuntimeException
            Call io.WriteError($"回调 '{name}' 出错: {ex.Message}")
        Catch ex As Exception
            Call io.WriteError($"回调 '{name}' 宿主异常: {ex.Message}")
        End Try

        Return False
    End Function

    ''' <summary>是否存在某个事件的回调。</summary>
    Public Function Has(name As String) As Boolean
        Return Handlers.ContainsKey(name)
    End Function

    ''' <summary>写一行日志到宿主控制台。</summary>
    Public Sub Log(text As String)
        io.WriteLine(text)
    End Sub

    ''' <summary>写一行错误到宿主控制台。</summary>
    Public Sub LogError(text As String)
        io.WriteError(text)
    End Sub
End Class
