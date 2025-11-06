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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

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
End Namespace
