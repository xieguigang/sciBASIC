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

            ' 1. 精确命中 #imports 文件映射
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
    End Class

End Namespace