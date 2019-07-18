#Region "Microsoft.VisualBasic::f7090379a93984ecee32ecbba064de75, CommandLine\Stream.vb"

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
    Public Module StreamExtensions

        Public Function FileType(reference As String) As FileTypes
            If reference.TextEquals("std_in://") OrElse reference.TextEquals("std_out://") Then
                Return FileTypes.PipelineFile
            ElseIf reference.ToLower.StartsWith("memory://") Then
                Return FileTypes.MemoryFile
            Else
                Return FileTypes.DiskFile
            End If
        End Function

        Public Function OpenForRead(reference As String) As Stream
            If reference.TextEquals("std_in://") Then
                Return Console.OpenStandardInput
            ElseIf reference.TextEquals("std_out://") Then
                Throw New InvalidProgramException()
            ElseIf reference.ToLower.StartsWith("memory://") Then
                reference = reference.GetTagValue(":/").Value
                Return MemoryMappedFile.OpenExisting(reference).CreateViewStream
            Else
                Return New FileStream(reference, FileMode.Open, access:=FileAccess.Read, share:=FileShare.ReadWrite)
            End If
        End Function

        Public Function OpenForWrite(reference As String, Optional size& = 1024 * 1024 * 1024) As Stream
            If reference.TextEquals("std_in://") Then
                Throw New InvalidProgramException
            ElseIf reference.TextEquals("std_out://") Then
                Return Console.OpenStandardOutput
            ElseIf reference.ToLower.StartsWith("memory://") Then
                reference = reference.GetTagValue(":/").Value
                Return MemoryMappedFile.CreateOrOpen(reference, size).CreateViewStream
            Else
                Return New FileStream(reference, FileMode.OpenOrCreate, access:=FileAccess.Write, share:=FileShare.Read)
            End If
        End Function
    End Module
End Namespace
