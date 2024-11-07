#Region "Microsoft.VisualBasic::1d29d822507b18e368473917fe944414, Microsoft.VisualBasic.Core\src\CommandLine\Stream.vb"

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

    '   Total Lines: 93
    '    Code Lines: 50 (53.76%)
    ' Comment Lines: 31 (33.33%)
    '    - Xml Docs: 54.84%
    ' 
    '   Blank Lines: 12 (12.90%)
    '     File Size: 3.95 KB


    '     Enum FileTypes
    ' 
    '         DiskFile, MemoryFile, PipelineFile
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module StreamExtensions
    ' 
    '         Function: FileType, OpenForRead, OpenForWrite
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.MemoryMappedFiles

Namespace CommandLine

    Public Enum FileTypes
        DiskFile
        PipelineFile
        MemoryFile
    End Enum

    ''' <summary>
    ''' + file path: C://path/to/file
    ''' + standard input: std_in://
    ''' + standard output: std_out://
    ''' + memory mapping file: memory://file/uri
    ''' </summary>
    <HideModuleName> Public Module StreamExtensions

        Public Function FileType(reference As String) As FileTypes
            If reference.TextEquals("std_in://") OrElse reference.TextEquals("std_out://") Then
                Return FileTypes.PipelineFile
            ElseIf reference.ToLower.StartsWith("memory://") Then
                Return FileTypes.MemoryFile
            Else
                Return FileTypes.DiskFile
            End If
        End Function

        ''' <summary>
        ''' 一个用于通用化的打开数据流读取对象的函数
        ''' </summary>
        ''' <param name="reference"></param>
        ''' <returns></returns>
        Public Function OpenForRead(reference As String) As Stream
            If reference.TextEquals("std_in://") Then
                Return Console.OpenStandardInput
            ElseIf reference.TextEquals("std_out://") Then
                Throw New InvalidProgramException()
            ElseIf reference.ToLower.StartsWith("memory://") Then
#If WINDOWS Then
                Dim view As Stream

                reference = reference.GetTagValue(":/").Value
                view = MemoryMappedFile.OpenExisting(reference).CreateViewStream
                view.Seek(Scan0, SeekOrigin.Begin)

                Return view
#Else
                Throw New NotSupportedException("MemoryMappedFile is not supported on unix system")
#End If
            Else
                Return New FileStream(reference, FileMode.Open, access:=FileAccess.Read, share:=FileShare.ReadWrite)
            End If
        End Function

        ''' <summary>
        ''' 请注意,这个函数在创建内存映射文件的时候,默认是0.5GB大小的
        ''' </summary>
        ''' <param name="reference"></param>
        ''' <param name="size"></param>
        ''' <returns></returns>
        Public Function OpenForWrite(reference As String, Optional size& = 512 * 1024 * 1024) As Stream
            If reference.TextEquals("std_in://") Then
                Throw New InvalidProgramException
            ElseIf reference.TextEquals("std_out://") Then
                Return Console.OpenStandardOutput
            ElseIf reference.ToLower.StartsWith("memory://") Then
#If WINDOWS Then
                Dim view As Stream
                'Dim security As New MemoryMappedFileSecurity()
                'Dim userName$ = "everyone"

                'If Not App.IsMicrosoftPlatform Then
                '    ' 20190724 linux平台上很有可能不存在"everyone"这个账户角色
                '    userName = Environment.UserName
                'End If

                ' Security.AddAccessRule(New AccessRule(Of MemoryMappedFileRights)(userName, MemoryMappedFileRights.FullControl, AccessControlType.Allow))
                reference = reference.GetTagValue(":/").Value
                view = MemoryMappedFile.CreateOrOpen(reference, size, MemoryMappedFileAccess.ReadWrite).CreateViewStream
                'view = MemoryMappedFile.CreateOrOpen(
                '    reference, size,
                '    access:=MemoryMappedFileAccess.ReadWrite,
                '    options:=MemoryMappedFileOptions.DelayAllocatePages,
                '    memoryMappedFileSecurity:=security,
                '    inheritability:=HandleInheritability.Inheritable
                ').CreateViewStream

                Call view.Seek(Scan0, SeekOrigin.Begin)

                Return view
#Else
                Throw New NotSupportedException("MemoryMappedFile is not working on unix system.")
#End If
            Else
                Return New FileStream(reference, FileMode.OpenOrCreate, access:=FileAccess.Write, share:=FileShare.Read)
            End If
        End Function
    End Module
End Namespace
