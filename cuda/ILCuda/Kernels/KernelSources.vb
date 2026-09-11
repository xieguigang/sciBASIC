#Region "Microsoft.VisualBasic::81cebd80e2d212717ab8c1f5af793e26, cuda\ILCuda\Kernels\KernelSources.vb"

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

    '   Total Lines: 187
    '    Code Lines: 115 (61.50%)
    ' Comment Lines: 35 (18.72%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 37 (19.79%)
    '     File Size: 8.00 KB


    '     Module KernelSources
    ' 
    '         Function: All, Builtin, BuiltinAssembly, CombinedSource, ExportTo
    '                   LoadFrom, LoadFromAssembly, ReadResources, Registered
    ' 
    '         Sub: Register, RegisterFile, RegisterSource, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 内嵌 CUDA C 内核源码的读取与外部注册
'
' 框架自带的内核（basic / reduce / elementwise / blas）以内嵌资源的形式随
' Enigma.ILCuda 程序集分发；使用方（例如 demo 工程）也可以把属于自己的
' .cu 资源注册进来，与框架内核一起并入同一个 NVRTC 编译单元：
'
'     Enigma.ILCuda.Kernels.KernelSources.Register(Assembly.GetExecutingAssembly())
'
' 注册必须在 CudaEngine.TryCreate 之前完成，否则该内核不会参与编译。
' ------------------------------------------------------------------------

Imports System.IO
Imports System.Reflection
Imports System.Text

Namespace Kernels

    ''' <summary>内核源码仓库：框架自带的内嵌 .cu + 使用方注册的外部 .cu</summary>
    Public Module KernelSources

        ''' <summary>框架自带内核的来源标识</summary>
        Public Const BuiltinOrigin As String = "ILCuda"
        ''' <summary>手工注册内联源码时使用的默认来源标识</summary>
        Public Const ExternalOrigin As String = "external"

        Private ReadOnly _sync As New Object()
        Private ReadOnly _external As New List(Of KernelSourceFile)()
        Private ReadOnly _registeredKeys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Private _builtin As IReadOnlyList(Of KernelSourceFile)

        ''' <summary>框架自身的程序集（不随调用方所在的程序集变化）</summary>
        Public Function BuiltinAssembly() As Assembly
            Return GetType(KernelSources).Assembly
        End Function

        ''' <summary>读取框架自身内嵌的全部 *.cu 资源</summary>
        Public Function LoadFromAssembly() As IReadOnlyList(Of KernelSourceFile)
            Return ReadResources(BuiltinAssembly(), BuiltinOrigin)
        End Function

        ''' <summary>读取指定程序集内嵌的全部 *.cu 资源</summary>
        Public Function LoadFrom(asm As Assembly, Optional origin As String = Nothing) As IReadOnlyList(Of KernelSourceFile)
            If asm Is Nothing Then Throw New ArgumentNullException(NameOf(asm))

            If String.IsNullOrWhiteSpace(origin) Then
                origin = If(asm.GetName().Name, ExternalOrigin)
            End If

            Return ReadResources(asm, origin)
        End Function

        Private Function ReadResources(asm As Assembly, origin As String) As IReadOnlyList(Of KernelSourceFile)
            Dim result As New List(Of KernelSourceFile)()

            For Each name In asm.GetManifestResourceNames() _
                .Where(Function(n) n.EndsWith(".cu", StringComparison.OrdinalIgnoreCase)) _
                .OrderBy(Function(n) n, StringComparer.Ordinal)

                Using stream = asm.GetManifestResourceStream(name)
                    If stream Is Nothing Then Continue For

                    Using reader As New StreamReader(stream, Encoding.UTF8)
                        result.Add(New KernelSourceFile With {
                            .Name = name,
                            .Origin = origin,
                            .Text = reader.ReadToEnd()
                        })
                    End Using
                End Using
            Next

            Return result
        End Function

        Private Function Builtin() As IReadOnlyList(Of KernelSourceFile)
            If _builtin Is Nothing Then
                SyncLock _sync
                    If _builtin Is Nothing Then _builtin = LoadFromAssembly()
                End SyncLock
            End If

            Return _builtin
        End Function

        ' ------------------------------------------------------------------
        ' 外部源码注册
        ' ------------------------------------------------------------------

        ''' <summary>注册一个程序集内嵌的全部 *.cu 资源（来源标识取程序集名）</summary>
        Public Sub Register(asm As Assembly)
            If asm Is Nothing Then Throw New ArgumentNullException(NameOf(asm))

            For Each file In LoadFrom(asm)
                RegisterFile(file)
            Next
        End Sub

        ''' <summary>注册一段内联的内核源码</summary>
        Public Sub RegisterSource(name As String, text As String, Optional origin As String = Nothing)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("内核源码名称不能为空", NameOf(name))
            End If

            RegisterFile(New KernelSourceFile With {
                .Name = name,
                .Origin = If(String.IsNullOrWhiteSpace(origin), ExternalOrigin, origin),
                .Text = If(text, String.Empty)
            })
        End Sub

        ''' <summary>注册一个内核源码文件（按"来源::资源名"去重）</summary>
        Public Sub RegisterFile(file As KernelSourceFile)
            If file Is Nothing Then Throw New ArgumentNullException(NameOf(file))
            If String.IsNullOrWhiteSpace(file.Origin) Then file.Origin = ExternalOrigin

            SyncLock _sync
                If _registeredKeys.Add(file.Key) Then _external.Add(file)
            End SyncLock
        End Sub

        ''' <summary>清空所有外部注册的源码（框架自带的部分保留）</summary>
        Public Sub Reset()
            SyncLock _sync
                _external.Clear()
                _registeredKeys.Clear()
            End SyncLock
        End Sub

        ''' <summary>外部已注册的源码快照</summary>
        Public Function Registered() As IReadOnlyList(Of KernelSourceFile)
            SyncLock _sync
                Return _external.ToArray()
            End SyncLock
        End Function

        ''' <summary>内置 + 已注册的全部源码（顺序稳定，可直接拼成单个编译单元）</summary>
        Public Function All() As IReadOnlyList(Of KernelSourceFile)
            Dim list As New List(Of KernelSourceFile)(Builtin())
            list.AddRange(Registered())
            Return list
        End Function

        ''' <summary>
        ''' 把所有内核源码合并成一个编译单元（NVRTC 一次编译即可，
        ''' 模块内同时包含框架内核与外部注册的内核）
        ''' </summary>
        Public Function CombinedSource() As String
            Dim files = All()
            If files.Count = 0 Then Return String.Empty

            Dim code As New StringBuilder()

            For Each file In files
                code.AppendLine($"// ===== 来源: {file.Origin}/{file.FileName} =====")
                code.AppendLine(file.Text)
                code.AppendLine()
            Next

            Return code.ToString()
        End Function

        ''' <summary>
        ''' 把全部内核源码导出到指定目录（配合 emit-kernels 使用，
        ''' 便于用 nvcc 离线编译成 .ptx / .cubin 作为兜底镜像）
        ''' </summary>
        Public Function ExportTo(folder As String) As IReadOnlyList(Of String)
            Dim written As New List(Of String)()
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            Directory.CreateDirectory(folder)

            For Each source As KernelSourceFile In All()
                Dim fileName = source.FileName

                ' 不同来源可能出现同名文件，冲突时加上来源前缀
                If Not used.Add(fileName) Then fileName = $"{source.Origin}.{fileName}"

                Dim outputPath = Path.Combine(folder, fileName)
                File.WriteAllText(outputPath, source.Text, New UTF8Encoding(False))
                written.Add(outputPath)
            Next

            Return written
        End Function
    End Module
End Namespace

