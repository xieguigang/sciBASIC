#Region "Microsoft.VisualBasic::37420063ea459f8543e5d052146392c4, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ProgressBar.vb"

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

    '   Total Lines: 427
    '    Code Lines: 223 (52.22%)
    ' Comment Lines: 136 (31.85%)
    '    - Xml Docs: 64.71%
    ' 
    '   Blank Lines: 68 (15.93%)
    '     File Size: 16.25 KB


    '     Structure ColorTheme
    ' 
    '         Properties: BackgroundColor, IsEmpty, MessageDetailColor, ProgressBarColor, ProgressMsgColor
    ' 
    '         Function: [Default], DefaultTheme
    ' 
    '     Class ProgressBar
    ' 
    '         Properties: ElapsedMilliseconds, Enable
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: GetCurrentConsoleTop
    ' 
    '         Sub: [Step], ClearPinnedTop, consoleWindowResize, (+2 Overloads) Dispose, PinTop
    '              SetProgress, SetToEchoLine, tick
    ' 
    '     Class ProgressProvider
    ' 
    '         Properties: Current, Elapsed, Target
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Step], (+2 Overloads) ETA, StepProgress, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Terminal.ProgressBar

    ''' <summary>
    ''' The <see cref="ConsoleColor"/> theme for the <see cref="ProgressBar"/>
    ''' </summary>
    Public Structure ColorTheme : Implements IsEmpty

        ' [ERROR 2017/11/5 下午 0704:37] <Print>:System.Exception: Print 
        '  ---> System.Reflection.TargetInvocationException: 调用的目标发生了异常。 
        '  ---> System.Exception: [
        '           "未能从程序集“Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a, Version=3.0.32.34335, Culture=neutral, PublicKeyToken=null”中加载类型“Microsoft.VisualBasic.Terminal.ColorTheme”。",
        '           "未能从程序集“Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a, Version=3.0.32.34335, Culture=neutral, PublicKeyToken=null”中加载类型“Microsoft.VisualBasic.Terminal.ColorTheme”。"
        '       ] 
        '  ---> System.Reflection.ReflectionTypeLoadException: 无法加载一个或多个请求的类型。有关更多信息，请检索 LoaderExceptions 属性。

        ' 在 System.Reflection.RuntimeModule.GetTypes(RuntimeModule Module)
        ' 在 System.Reflection.Assembly.GetTypes()
        ' 在 Microsoft.VisualBasic.CommandLine.Reflection.RunDllEntryPoint.GetTypes(Assembly assm)
        ' --- 内部异常堆栈跟踪的结尾 ---
        ' 在 Microsoft.VisualBasic.CommandLine.Reflection.RunDllEntryPoint.GetTypes(Assembly assm)
        ' 在 Microsoft.VisualBasic.CommandLine.Reflection.RunDllEntryPoint.GetDllMethod(Assembly assembly, String entryPoint)
        ' 在 SMRUCC.WebCloud.httpd.CLI.RunDll(CommandLine args)
        ' --- 内部异常堆栈跟踪的结尾 ---
        ' 在 System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)
        ' 在 System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)
        ' 在 System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
        ' 在 System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
        ' 在 Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke(Object[] callParameters, Object target, Boolean Throw)
        ' --- 内部异常堆栈跟踪的结尾 ---

        Public Property BackgroundColor As ConsoleColor
        Public Property ProgressBarColor As ConsoleColor
        Public Property ProgressMsgColor As ConsoleColor
        Public Property MessageDetailColor As ConsoleColor

        ''' <summary>
        ''' Test if all of the property value is equals to ZERO(<see cref="ConsoleColor.Black"/>).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return BackgroundColor = 0 AndAlso
                    ProgressBarColor = 0 AndAlso
                    ProgressMsgColor = 0 AndAlso
                    MessageDetailColor = 0
            End Get
        End Property

        ''' <summary>
        ''' Default value for optional parameter
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2017-11-5
        ''' This property will cause bug in reflection.
        ''' 
        ''' ###### 2017-11-6
        ''' Change from property to function to avoid bug caused application crashed
        ''' </remarks>
        Public Shared Function DefaultTheme() As [Default](Of ColorTheme)
            Return New [Default](Of ColorTheme) With {
                .value = [Default](),
                .assert = Function(t)
                              Return DirectCast(t, ColorTheme).IsEmpty
                          End Function
            }
        End Function
        ''' <summary>
        ''' The default color theme values
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function [Default]() As ColorTheme
            Return New ColorTheme With {
                .BackgroundColor = ConsoleColor.Cyan,
                .MessageDetailColor = ConsoleColor.White,
                .ProgressBarColor = ConsoleColor.Yellow,
                .ProgressMsgColor = ConsoleColor.Green
            }
        End Function
    End Structure

    ''' <summary>
    ''' Progress bar for the <see cref="Console"/> Screen.
    ''' </summary>
    ''' <remarks>
    ''' http://www.cnblogs.com/masonlu/p/4668232.html
    ''' </remarks>
    ''' 
    <FrameworkConfig(ProgressBar.TerminalProgressBarEnvironmentConfigName)>
    Public Class ProgressBar : Inherits AbstractBar
        Implements IDisposable

        Dim colorBack As ConsoleColor = Console.BackgroundColor
        Dim colorFore As ConsoleColor = Console.ForegroundColor

        ''' <summary>
        ''' The current progress percentage: [0, 100]
        ''' </summary>
        Dim current%
        Dim y%
        Dim theme As ColorTheme

        Shared disabled As Boolean

        ''' <summary>
        ''' If this global value is not null, then set y value in constructor will be disabled.
        ''' </summary>
        Shared pinnedTop As Integer?

        Public Shared Property Enable As Boolean
            Get
                Return Not disabled
            End Get
            Set(value As Boolean)
                disabled = Not value
            End Set
        End Property

        Friend Const TerminalProgressBarEnvironmentConfigName$ = "progress_bar"

        Shared Sub New()
            disabled = App.GetVariable(TerminalProgressBarEnvironmentConfigName).TextEquals(NameOf(disabled))

            ' The progress bar is also be disable in internal pipeline mode
            If App.GetVariable(name:=FlagInternalPipeline).ParseBoolean = True Then
                disabled = True
            End If
        End Sub

        ''' <summary>
        ''' Create a console progress bar object with custom configuration.
        ''' </summary>
        ''' <param name="title">The title of the task which will takes long time for running.</param>
        ''' <param name="Y">The row position number of the progress bar.</param>
        ''' <param name="CLS">Clear the console screen?</param>
        Sub New(title$, Y%, Optional CLS As Boolean = False, Optional theme As ColorTheme = Nothing)
            If CLS AndAlso App.IsConsoleApp AndAlso Not disabled Then
                Call Console.Clear()
            End If

            Call Console.WriteLine(title)

            Me.theme = theme Or ColorTheme.DefaultTheme
            Me.y = Y

            If Not pinnedTop Is Nothing Then
                Me.y = pinnedTop
            End If

            If Not disabled Then
                AddHandler TerminalEvents.Resize, AddressOf consoleWindowResize

                Try
                    Call consoleWindowResize(Nothing, Nothing)
                Catch ex As Exception

                End Try
            End If
        End Sub

        ''' <summary>
        ''' Create a console progress bar with default theme color <see cref="ColorTheme.DefaultTheme"/>
        ''' (在当前位置之后设置进度条，这个构造函数不会清除整个终端屏幕)
        ''' </summary>
        ''' <param name="title$">The task title</param>
        Sub New(title$)
            Call Me.New(title, Y:=Console.CursorTop, CLS:=False)
        End Sub

        Private Sub consoleWindowResize(size As Size, old As Size)
            Console.ResetColor()
            Console.SetCursorPosition(0, y)
            Console.BackgroundColor = ConsoleColor.DarkCyan
            For i = 0 To Console.WindowWidth - 3
                '(0,1) 第二行
                Console.Write(" ")
            Next
            '(0,1) 第二行
            Console.WriteLine(" ")
            Console.BackgroundColor = colorBack

            Call SetToEchoLine()
        End Sub

        ' title
        ' progress bar
        ' details
        '
        ' echo

        ''' <summary>
        ''' 将终端的输出位置放置到详细信息的下一行
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetToEchoLine()
            If Not disabled Then
                Console.SetCursorPosition(0, y + 3)
            End If
        End Sub

        Public Overrides Sub [Step]()
            Call SetProgress(current)
            current += 1
        End Sub

        Dim timer As Stopwatch = Stopwatch.StartNew

        ''' <summary>
        ''' 获取当前实例测量得出的总运行时间（以毫秒为单位）。
        ''' </summary>
        ''' <returns>
        ''' 一个只读长整型，表示当前实例测量得出的总毫秒数。
        ''' </returns>
        Public ReadOnly Property ElapsedMilliseconds As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return timer.ElapsedMilliseconds
            End Get
        End Property

        ''' <summary>
        ''' 更新进度条的当前状态信息
        ''' </summary>
        ''' <param name="p%"></param>
        ''' <param name="details$"></param>
        Private Sub tick(p%, details$)
            Console.BackgroundColor = ConsoleColor.Yellow
            ' /运算返回完整的商，包括余数，SetCursorPosition会自动四舍五入
            Dim cx As Integer = p * (Console.WindowWidth - 2) / 100

            Console.SetCursorPosition(0, y)

            If p < current Then
                Call consoleWindowResize(Nothing, Nothing)
            End If

            For i As Integer = 0 To cx
                Console.Write(" ")
            Next

            Console.BackgroundColor = colorBack
            Console.ForegroundColor = ConsoleColor.Green
            Console.SetCursorPosition(0, y + 1)
            Console.Write("{0}%", p)
            Console.ForegroundColor = colorFore

            If Not String.IsNullOrEmpty(details) Then
                Console.WriteLine(vbTab & details)
            End If

            Call SetToEchoLine()
        End Sub

        ''' <summary>
        ''' <paramref name="percent"/>是进度条的百分比
        ''' </summary>
        ''' <param name="percent">Percentage, 假设是从p到current</param>
        Public Sub SetProgress(percent%, Optional details$ = "")
            current = percent

            If Not disabled Then
                Call tick(current, details)
            End If
        End Sub

        Public Shared Sub ClearPinnedTop()
            pinnedTop = Nothing
        End Sub

        Public Shared Sub PinTop(top As Integer)
            pinnedTop = top
        End Sub

        Public Shared Function GetCurrentConsoleTop() As Integer
            Return Console.CursorTop
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    If Not disabled Then
                        ' TODO: dispose managed state (managed objects).
                        RemoveHandler TerminalEvents.Resize, AddressOf consoleWindowResize
                    End If

                    Call timer.Stop()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

    Public Class ProgressProvider

        ''' <summary>
        ''' 整个工作的总的tick数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Target As Integer
        ''' <summary>
        ''' 当前已经完成的tick数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As Integer

        ReadOnly bindProgress As ProgressBar

        ''' <summary>
        ''' 生成进度条的百分比值
        ''' </summary>
        ''' <param name="total"></param>
        Sub New(bind As ProgressBar, total%)
            Target = total
            bindProgress = bind
        End Sub

        Dim previous#
        Dim previousTime&

        Public ReadOnly Property Elapsed As TimeSpan
            Get
                Return TimeSpan.FromMilliseconds(bindProgress.ElapsedMilliseconds)
            End Get
        End Property

        Public Function ETA(Optional avg As Boolean = True) As TimeSpan
            Dim out As TimeSpan
            Dim elapsed& = bindProgress.ElapsedMilliseconds

            If avg Then
                out = ETA(0R, Current / Target, elapsed)
            Else
                out = ETA(previous, Current / Target, elapsed - previousTime)

                previousTime = elapsed
                previous = Current / Target
            End If

            Return out
        End Function

        ''' <summary>
        ''' 返回来的百分比小数，还需要乘以100才能得到进度
        ''' </summary>
        ''' <returns></returns>
        Public Function [Step]() As Double
            _Current += 1
            Return Current / Target
        End Function

        ''' <summary>
        ''' 百分比进度，不需要再乘以100了
        ''' </summary>
        ''' <returns></returns>
        Public Function StepProgress() As Integer
            Return CInt([Step]() * 100)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Shared ReadOnly NaN As Double = New TimeSpan(
            days:=9999999,' 10658576,
            hours:=23,
            minutes:=59,
            seconds:=59,
            milliseconds:=59
        ).TotalMilliseconds

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="previous#">百分比</param>
        ''' <param name="cur#">百分比</param>
        ''' <param name="Elapsed#">当前的这个百分比差所经历过的时间</param>
        ''' <returns></returns>
        Public Shared Function ETA(previous#, cur#, Elapsed&) As TimeSpan
            Dim d# = cur - previous

            If d# = 0R Then
                d = 0.000000001
            End If

            Dim lefts = (1 - cur) / d   ' lefts = 100% - currents
            Dim time = lefts * Elapsed

            ' System.OverflowException: TimeSpan overflowed because the duration is too long.
            If time > NaN Then
                time = NaN
            End If

            Dim estimates = TimeSpan.FromMilliseconds(time)
            Return estimates
        End Function
    End Class
End Namespace
