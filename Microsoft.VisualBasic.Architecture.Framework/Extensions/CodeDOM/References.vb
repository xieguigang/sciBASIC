#Region "Microsoft.VisualBasic::deff9e489bf4adab17b3389cd7440d0a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\CodeDOM\References.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Language

Namespace Emit.CodeDOM_VBC

    ''' <summary>
    ''' Assembly references solver
    ''' </summary>
    Public Module ReferenceSolver

        ''' <summary>
        ''' 获取当前所执行的应用程序的所有引用dll模块的文件路径列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExecutingReferences As String()

        Sub New()
            Dim assm As Assembly = Assembly.GetEntryAssembly
            Dim main As MethodInfo = assm.EntryPoint
            Dim [module] As Type = main.DeclaringType

            ReferenceSolver.ExecutingReferences =
                [module].GetReferences
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
        ''' 
        <Extension>
        Private Sub __getReferences(assembly As Assembly, i As Integer, ByRef refList As List(Of String))
            Dim myRefs = assembly.GetReferencedAssemblies
            Dim tmp As List(Of String) = refList
            Dim LQuery = LinqAPI.MakeList(Of String) <=
 _
                From ref As AssemblyName
                In myRefs
                Where Not String.IsNullOrEmpty(ref.FullName)
                Let entry = ref.FullName
                Select refListValue =
                    getReferences(url:=entry, i:=i + 1, refList:=tmp)

            Call LQuery.Add(assembly.Location)
            Call refList.AddRange(LQuery.AsEnumerable)
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="removeSystem">是否移除系统引用</param>
        ''' <returns></returns>
        Public Function GetReferences(assembly As Assembly, removeSystem As Boolean, Optional strict As Boolean = True) As String()
            Dim refList As New List(Of String)

            assembly.__getReferences(0, refList)
            refList += From ref As AssemblyName
                       In GetType(App).Assembly.GetReferencedAssemblies   ' 添加VB_Framework的引用
                       Let ass As Assembly =
                           Assembly.Load(ref.FullName)
                       Select ass.Location
            refList = refList.Distinct.ToList

            If removeSystem Then
                refList = LinqAPI.MakeList(Of String) <=
 _
                    From path As String
                    In refList
                    Where Not IsSystemAssembly(path, strict)
                    Select path

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
                    Dim Name As String = basename(url)
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

            Call __getReferences(assembly, i:=i + 1, refList:=refList)

            Return refList.ToArray
        End Function

        Public ReadOnly Property RunTimeDirectory As String = FileIO.FileSystem.GetDirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory).FullName.Replace("/", "\")
    End Module
End Namespace
