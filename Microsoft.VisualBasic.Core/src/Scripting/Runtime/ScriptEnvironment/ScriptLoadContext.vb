Imports System.IO
Imports System.Reflection
Imports System.Runtime.Loader

Namespace Scripting.Runtime

    ''' <summary>
    ''' 脚本专用AssemblyLoadContext: 在运行时解析动态assembly的#imports依赖
    ''' </summary>
    Public Class ScriptLoadContext : Inherits AssemblyLoadContext

        ' 简单程序集名 -> dll绝对路径
        Private ReadOnly _fileMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        ' 探测目录(#imports的所在文件夹, 用于解析依赖的依赖)
        Private ReadOnly _probeDirs As New List(Of String)
        Private ReadOnly _loaded As New Dictionary(Of String, Assembly)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>宿主引擎程序集(类型同一性绝对必要, 必须共享)</summary>
        Private Shared ReadOnly _hostAsm As Assembly = Assembly.GetExecutingAssembly

        ''' <summary>
        ''' Default ALC 已加载程序集的名称索引。
        ''' 宿主启动时其全部依赖(含CommandLine所在库)已加载至此,
        ''' 缓存为字典避免每次Load回调都做LINQ扫描
        ''' </summary>
        Private Shared ReadOnly _defaultAsmIndex As Dictionary(Of String, Assembly) =
            AssemblyLoadContext.Default.Assemblies _
                .GroupBy(Function(a) a.GetName().Name, StringComparer.OrdinalIgnoreCase) _
                .ToDictionary(Function(g) g.Key,
                              Function(g)
                                  Return g.First()
                              End Function, StringComparer.OrdinalIgnoreCase)

        Public Sub New(name As String, [imports] As IEnumerable(Of String))
            MyBase.New(name, isCollectible:=True)

            For Each dll As String In [imports].Distinct()
                If File.Exists(dll) Then
                    _fileMap(Path.GetFileNameWithoutExtension(dll)) = dll

                    Dim dir As String = Path.GetDirectoryName(dll)
                    If Not _probeDirs.Contains(dir) Then
                        _probeDirs.Add(dir)
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' 运行时依赖解析核心: 当脚本代码第一次触碰某个外部类型时,
        ''' CLR会调用本函数来定位对应的程序集
        ''' </summary>
        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
            Dim simple As String = assemblyName.Name

            If _loaded.ContainsKey(simple) Then
                Return _loaded(simple)
            End If

            Dim asm As Assembly = Nothing

            ' 1. 引擎宿主自身: 强制共享, 否则RunMain传参必然类型分裂
            If String.Equals(simple, _hostAsm.GetName().Name, StringComparison.OrdinalIgnoreCase) Then
                Return _hostAsm
            End If

            ' 2. Default ALC 已加载的程序集: 优先共享返回
            '    (CommandLine所在的sciBASIC库正属此类 => 修复本错误的关键)
            If _defaultAsmIndex.ContainsKey(simple) Then
                Return _defaultAsmIndex(simple)
            End If

            ' 3. 精确命中 #imports 文件映射
            If _fileMap.ContainsKey(simple) Then
                asm = LoadFromFile(_fileMap(simple))
            Else
                ' 2. 在#imports所在文件夹中按名称探测(解决传递依赖)
                For Each dir As String In _probeDirs
                    Dim candidate As String = Path.Combine(dir, simple & ".dll")

                    If File.Exists(candidate) Then
                        asm = LoadFromFile(candidate)
                        Exit For
                    End If
                Next
            End If

            If Not asm Is Nothing Then
                Call _loaded.Add(simple, asm)
                Return asm
            End If

            ' 3. 返回Nothing => 回退到Default ALC解析BCL共享框架
            '    (System.*, Microsoft.VisualBasic.Core, 以及宿主assembly)
            Return Nothing
        End Function

        ''' <summary>
        ''' 使用LoadFromStream而非LoadFromAssemblyPath:
        ''' 1) 不对磁盘dll产生文件锁  2) collectible卸载更干净
        ''' </summary>
        Private Function LoadFromFile(path As String) As Assembly
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Return LoadFromStream(fs)
            End Using
        End Function

        ''' <summary>
        ''' 在本脚本ALC的作用域内查找类型。
        ''' 查找顺序: 主脚本assembly => 已加载的依赖assembly => #imports惰性加载
        ''' 
        ''' 注意: 若返回的类型来自被隔离加载的assembly(本ALC重新加载的dll),
        ''' 它与宿主Default ALC中的同名类型**不是同一个Type对象**,
        ''' 此时不应将此类型的实例传回宿主的强类型API
        ''' </summary>
        Public Overloads Function [GetType](mainAsm As Assembly, typeFullName As String) As Type

            ' 1. 优先查主脚本assembly(脚本自定义类型都在这里)
            If Not mainAsm Is Nothing Then
                Dim t As Type = mainAsm.GetType(typeFullName)
                If Not t Is Nothing Then
                    Return t
                End If
            End If

            ' 2. 遍历本ALC已加载的所有assembly(含#imports加载的依赖)
            For Each asm As Assembly In Me.Assemblies
                Dim t As Type = asm.GetType(typeFullName)
                If Not t Is Nothing Then
                    Return t
                End If
            Next

            ' 3. #imports文件映射中尚未加载的, 惰性加载后再查找
            For Each dll As String In _fileMap.Values
                Dim name As String = Path.GetFileNameWithoutExtension(dll)

                If Not _loaded.ContainsKey(name) Then
                    Dim a As Assembly = LoadFromFile(dll)
                    Call _loaded.Add(name, a)

                    Dim t As Type = a.GetType(typeFullName)
                    If Not t Is Nothing Then
                        Return t
                    End If
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 在脚本ALC作用域内实例化目标类型
        ''' </summary>
        Public Function CreateInstance(mainAsm As Assembly, typeFullName As String, ParamArray args() As Object) As Object
            Dim t As Type = [GetType](mainAsm, typeFullName)

            If t Is Nothing Then
                Throw New TypeLoadException(
                    $"无法在脚本assembly及#imports依赖中定位类型: {typeFullName}")
            End If

            Return Activator.CreateInstance(t, args)
        End Function
    End Class

End Namespace