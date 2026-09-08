Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Emit
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.VisualBasic.Linq

Module DynamicDll

    ' Namespace DynamicDll
    '    Module Program
    '       Public Function Main(args As CommandLine) As Integer
    '          ' run vbs code 
    '       End Function
    '    End Module
    ' End Namespace

    ''' <summary>脚本代码的固定顶层命名空间</summary>
    Public Const NamespaceName As String = "DynamicDll"

    ''' <summary>脚本代码的固定容器类型(Module)名称</summary>
    Public Const ModuleName As String = "Program"

    ''' <summary>脚本入口函数的固定名称</summary>
    Public Const MainName As String = "Main"

    Public Function Compile(vbs As String, referenceDlls As IEnumerable(Of String)) As Assembly
        ' 2. 解析代码为语法树
        Dim syntaxTree As SyntaxTree = VisualBasicSyntaxTree.ParseText(vbs)

        ' 3. 收集依赖项引用
        ' 注意：在现代 .NET 中，收集引用是一个关键点。我们遍历当前已加载的程序集。
        Dim references As New List(Of MetadataReference)()
        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            If Not String.IsNullOrEmpty(asm.Location) Then
                references.Add(MetadataReference.CreateFromFile(asm.Location))
            End If
        Next

        For Each dll As String In referenceDlls.SafeQuery
            ' 如果有缺失的引用，可以手动添加，例如：
            Call references.Add(MetadataReference.CreateFromFile(dll.GetFullPath))
        Next

        ' 4. 配置编译选项 (生成 DLL，且开启优化)
        Dim options As New VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary _
            , optimizationLevel:=OptimizationLevel.Release)

        ' 5. 创建编译对象
        Dim compilation As VisualBasicCompilation = VisualBasicCompilation.Create(
            assemblyName:=NameOf(DynamicDll),
            syntaxTrees:={syntaxTree},
            references:=references,
            options:=options)

        ' 6. 编译并输出到内存流
        Using ms As New MemoryStream()
            Dim emitResult As EmitResult = compilation.Emit(ms)

            If Not emitResult.Success Then
                ' 处理编译错误
                For Each diag As Diagnostic In emitResult.Diagnostics
                    Console.WriteLine($"{diag.Severity}: {diag.GetMessage()}")
                Next
                Return Nothing
            End If

            ' 7. 将流指针重置，并加载到内存中
            ms.Seek(0, SeekOrigin.Begin)
            Dim asmBytes As Byte() = ms.ToArray()

            ' 注意：在现代 .NET 中，Assembly.Load 接受 byte[]
            Return Assembly.Load(asmBytes)
        End Using
    End Function

    ''' <summary>
    ''' Call function Main(args As CommandLine) As integer
    ''' </summary>
    ''' <param name="dynamicAsm"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Run(dynamicAsm As Assembly) As Integer
        ' 8. 通过反射获取目标类型和方法
        Dim targetType As Type = dynamicAsm.GetType($"{NameOf(DynamicDll)}.Program")
        Dim instance As Object = Activator.CreateInstance(targetType)
        Dim methodInfo As MethodInfo = targetType.GetMethod("Main", BindingFlags.Public Or BindingFlags.Instance)

        ' --- 调用方式 A：传统反射调用 ---
        Dim result As Object = methodInfo.Invoke(instance, New Object() {App.CommandLine})
        Dim i32 As Integer = CInt(result)

        Return i32
    End Function
End Module
