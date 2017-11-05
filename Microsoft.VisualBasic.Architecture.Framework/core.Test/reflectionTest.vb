Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language

Public Module reflectionTest

    Sub Main()

        ' test 1
        Dim dll = "G:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\core.Test\bin\Debug\Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a.dll"

        ' test 2
        dll = App.ExecutablePath

        Dim entry = RunDllEntryPoint.GetDllMethod(Assembly.LoadFile(dll), "test")
    End Sub
End Module

Public Structure AAAA

    Public Property BugType  As ConsoleColor 

End Structure

''' <summary>
''' The <see cref="ConsoleColor"/> theme for the <see cref="ProgressBar"/>
''' </summary>
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

    Public Shared ReadOnly Property DefaultTheme As New DefaultValue(Of ColorTheme) With {
            .LazyValue = Function() [Default](),
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
