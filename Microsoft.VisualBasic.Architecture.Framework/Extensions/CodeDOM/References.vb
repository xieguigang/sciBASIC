Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Emit.CodeDOM_VBC

    ''' <summary>
    ''' Assembly references solver
    ''' </summary>
    Public Module References

        Public ReadOnly Property AppReferences As String()

        Sub New()
            AppReferences = Assembly _
                .GetExecutingAssembly _
                .EntryPoint _
                .DeclaringType _
                .GetReferences
        End Sub

        ''' <summary>
        ''' 递归的获取该类型所处的模块的所有的依赖关系，返回来的是全路径
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetReferences(Type As Type) As String()
            Dim assembly = Type.Assembly
            Return GetReferences(assembly, False)
        End Function

        ''' <summary>
        ''' 有一些会出现循环引用的情况？？？？？
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="i"></param>
        ''' <param name="refList"></param>
        Private Sub getReferences(assembly As System.Reflection.Assembly, i As Integer, ByRef refList As List(Of String))
            Dim refListBuffer = assembly.GetReferencedAssemblies
            Dim Temp = refList
            Dim LQuery = (From ref In refListBuffer
                          Where Not String.IsNullOrEmpty(ref.FullName)
                          Let entry = ref.FullName
                          Select refListValue = getReferences(url:=entry, i:=i + 1, refList:=Temp)).ToList
            Dim resultBuffer = LQuery.Unlist
            Call resultBuffer.Add(assembly.Location)
            Call refList.AddRange(resultBuffer)
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="removeSystem">是否移除系统引用</param>
        ''' <returns></returns>
        Public Function GetReferences(assembly As System.Reflection.Assembly, removeSystem As Boolean, Optional strict As Boolean = True) As String()
            Dim refList As List(Of String) = New List(Of String)

            Call getReferences(assembly, 0, refList)

            '添加VB_Framework的引用
            Call refList.AddRange((From ref In GetType(Parallel.ParallelLoading).Assembly.GetReferencedAssemblies
                                   Let ass = System.Reflection.Assembly.Load(ref.FullName)
                                   Select ass.Location).ToArray)
            refList = refList.Distinct.ToList

            If removeSystem Then
                refList = (From path As String
                           In refList.AsParallel
                           Where Not IsSystemAssembly(path, strict)
                           Select path).ToList
            End If

            Return refList.ToArray
        End Function

        ''' <summary>
        ''' 放在C:\WINDOWS\Microsoft.Net\这个文件夹下面的所有的引用都是本地编译的，哈希值已经不对了
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Function IsSystemAssembly(url As String, strict As Boolean) As Boolean
            Dim assemblyDir As String = FileIO.FileSystem.GetDirectoryInfo(FileIO.FileSystem.GetParentPath(url)).FullName.Replace("/", "\")

            If Not assemblyDir.Last = "\" Then
                assemblyDir &= "\"
            End If

            If String.Equals(RunTimeDirectory, assemblyDir) OrElse
               assemblyDir.StartsWith("C:\WINDOWS\Microsoft.Net\assembly\GAC_", StringComparison.OrdinalIgnoreCase) OrElse
               assemblyDir.StartsWith("C:\Windows\Microsoft.NET\Framework64", StringComparison.OrdinalIgnoreCase) OrElse
               assemblyDir.StartsWith("C:\Windows\Microsoft.NET\Framework", StringComparison.OrdinalIgnoreCase) Then

                If strict Then
                    Return True
                Else
                    Dim Name As String = IO.Path.GetFileNameWithoutExtension(url)
                    If String.Equals(Name, "mscorlib") OrElse String.Equals(Name, "System") OrElse Name.StartsWith("System.") Then
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="url">+特殊符号存在于这个字符串之中的话，函数会出错</param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Private Function getReferences(url As String, i As Integer, ByRef refList As List(Of String)) As String()
            Dim assembly = System.Reflection.Assembly.Load(url)

            If IsSystemAssembly(assembly.Location, True) OrElse refList.IndexOf(assembly.Location) > -1 Then
                Return New String() {}
            Else
#If DEBUG Then
                Call $"{New String(" "c, i)}{assembly.Location}".__DEBUG_ECHO
#End If
                Call refList.Add(assembly.Location)
            End If

            Call getReferences(assembly, i:=i + 1, refList:=refList)
            Return refList.ToArray
        End Function

        Public ReadOnly Property RunTimeDirectory As String = FileIO.FileSystem.GetDirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory).FullName.Replace("/", "\")
    End Module
End Namespace