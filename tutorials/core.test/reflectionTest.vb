#Region "Microsoft.VisualBasic::29e621c2f7755e88fcd68e59145237cd, sciBASIC#\tutorials\core.test\reflectionTest.vb"

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

    '   Total Lines: 140
    '    Code Lines: 51
    ' Comment Lines: 57
    '   Blank Lines: 32
    '     File Size: 5.27 KB


    ' Module reflectionTest
    ' 
    '     Sub: Main, testDefault
    ' 
    ' Structure AAAA
    ' 
    '     Properties: BugType
    ' 
    ' Structure ColorTheme
    ' 
    '     Properties: BackgroundColor, DefaultTheme, IsEmpty, MessageDetailColor, ProgressBarColor
    '                 ProgressMsgColor
    ' 
    '     Function: [Default]
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::700ceb977ff257a3ac7674e64f6c5d4a, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module reflectionTest
    ' 
    '     Sub: Main, testDefault
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::82a931dc0b83918cb053220a02060b92, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module reflectionTest
    ' 
    '     Sub: Main, testDefault
    ' 
    ' 
    ' 

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module reflectionTest

    Sub Main()

        ' test 1
        Dim dll = "G:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\core.Test\bin\Debug\Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a.dll"

        ' test 2
        dll = App.ExecutablePath

        Dim entry = RunDllEntryPoint.GetDllMethod(Assembly.LoadFile(dll), "test")
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="theme">Default value is <see cref="ColorTheme.DefaultTheme"/> if this optional parameter is omit</param>
    Public Sub testDefault(Optional theme As ColorTheme = Nothing)

        ' value is theme or default theme if it is omit
        Dim value As ColorTheme = theme Or ColorTheme.DefaultTheme

        Call value.GetJson.__DEBUG_ECHO


    End Sub
End Module

Public Structure AAAA

    Public Property BugType As ConsoleColor

End Structure

Public Structure ColorTheme

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

    Public ReadOnly Property IsEmpty As Boolean
        Get
            Return BackgroundColor = 0 AndAlso
                    ProgressBarColor = 0 AndAlso
                    ProgressMsgColor = 0 AndAlso
                    MessageDetailColor = 0
        End Get
    End Property

    Public Shared ReadOnly Property DefaultTheme As New [Default](Of ColorTheme) With {
            .lazy = New Lazy(Of ColorTheme)(Function() [Default]()),
            .assert = Function(t)
                          Return DirectCast(t, ColorTheme).IsEmpty
                      End Function
        }

    Public Shared Function [Default]() As ColorTheme
        Return New ColorTheme With {
                .BackgroundColor = ConsoleColor.Cyan,
                .MessageDetailColor = ConsoleColor.White,
                .ProgressBarColor = ConsoleColor.Yellow,
                .ProgressMsgColor = ConsoleColor.Green
            }
    End Function
End Structure
