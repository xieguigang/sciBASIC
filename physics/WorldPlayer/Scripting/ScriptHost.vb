#Region "Microsoft.VisualBasic::3945ef461a4a047ec0b33eed8739574f, physics\WorldPlayer\Scripting\ScriptHost.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 179
    '    Code Lines: 91 (50.84%)
    ' Comment Lines: 50 (27.93%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 38 (21.23%)
    '     File Size: 6.86 KB


    ' Class ScriptHost
    ' 
    '     Properties: BeforeRun, IsReady, LastError, ScriptPath
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Fire, FirstFrame, Has, Load
    ' 
    '     Sub: DefineEventHook, DefineGlobal, Log, LogError
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
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

    ''' <summary>
    ''' 脚本执行前调用一次的回调（宿主用它注册 world / camera / sim 等全局 API）。
    ''' 必须在 <c>Run</c> 之前注册，脚本的顶层语句才能直接引用这些 API。
    ''' </summary>
    Public Property BeforeRun As Action = Nothing

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
    ''' <remarks>
    ''' 必须先把 lambda 显式赋给 <see cref="Func(Of Object(), Object)"/> 变量再传出去。
    ''' 若直接写成 <c>DefineGlobal("on", Function(...) ... End Function)</c>，
    ''' 由于参数类型是 Object，VB 会生成合成委托类型
    ''' <c>VB$AnonymousDelegate_0(Of Object(), Object)</c>，
    ''' LiteJs 的 <c>TypeOf ... Is Func(Of Object(), Object)</c> 判定会失败并抛 InvalidCastException。
    ''' </remarks>
    Public Sub DefineEventHook()
        Dim handler As Func(Of Object(), Object) =
            Function(args As Object()) As Object
                If args Is Nothing OrElse args.Length < 2 Then Return JsRuntime.Undef

                Dim name As String = JsRuntime.JsStr(args(0))
                Dim fn As Object = args(1)

                If Not TypeOf fn Is Func(Of Object(), Object) Then
                    Call io.WriteError($"on('{name}') 的第二个参数必须是函数")
                    Return JsRuntime.Undef
                End If

                Handlers(name) = DirectCast(fn, Func(Of Object(), Object))

                Return JsRuntime.Undef
            End Function

        Call DefineGlobal("on", handler)
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

            ' API 全局必须在 Run 之前注册
            If BeforeRun IsNot Nothing Then
                Call BeforeRun.Invoke()
            End If

            Dim program As Program = Parser.Parse(source)

            Call engine.Run(program)

            Return True
        Catch ex As ParseException
            LastError = $"语法错误: {ex.Message}"
        Catch ex As JsRuntimeException
            LastError = $"运行时错误: {ex.Message}"
        Catch ex As Exception
            LastError = $"宿主错误: {ex.GetType().Name}: {ex.Message} @ {FirstFrame(ex)}"
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

    ''' <summary>取异常栈里第一条不属于本文件的帧，便于定位脚本错误来源。</summary>
    Private Shared Function FirstFrame(ex As Exception) As String
        If ex.StackTrace Is Nothing Then Return "?"

        For Each line As String In ex.StackTrace.Split(ControlChars.Lf)
            If line.Contains("JavaScript") Then Return line.Trim()
        Next

        Return ex.StackTrace.Split(ControlChars.Lf)(0).Trim()
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

