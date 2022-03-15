#Region "Microsoft.VisualBasic::00c626cd8267d1857d768b0f2f6869a3, sciBASIC#\vs_solutions\installer\zip\Program.vb"

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

    '   Total Lines: 76
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.88 KB


    ' Module Program
    ' 
    '     Function: Compress, CompressGzip, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.ZipLib
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http

<CLI> Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/compress.gzip")>
    <Usage("/compress.gzip /file <path> [/out <out.gzip>]")>
    <Description("Compress target file in gzip format.")>
    Public Function CompressGzip(args As CommandLine) As Integer
        Dim in$ = args <= "/file"
        Dim out$ = args("/out") Or [in].ChangeSuffix("gzip")
        Dim file As Stream = [in].Open(FileMode.Open, False)
        Dim gzip = file.GZipStream

        Return gzip.FlushStream(out).CLICode
    End Function

    <ExportAPI("/compress")>
    <Usage("/compress [/directory /tree] /out <out.zip> <directory/filelist>")>
    <Description("Zip compress target file or directory.")>
    <Argument("/directory", True, CLITypes.Boolean,
              Description:="Zip compress target is a directory?")>
    <Argument("/tree", True, CLITypes.Boolean,
              Description:="The directory content should be in tree style or flat style? This option only works when the ``/directory`` option was presented.")>
    <Argument("/out", False, CLITypes.File, PipelineTypes.std_out,
              Extensions:="*.zip",
              Description:="The output file path of the zip package file.")>
    Public Function Compress(args As CommandLine) As Integer
        Dim isDirectory As Boolean = args.IsTrue("/directory")
        Dim out$ = args <= "/out"
        Dim temp$ = App.GetAppSysTempFile(".zip", App.PID)
        Dim tree As Boolean = args("/tree")

        If isDirectory Then
            Call ZipLib.DirectoryArchive(
                args.Tokens.Last,
                saveZip:=temp,
                action:=ArchiveAction.Replace,
                compression:=CompressionLevel.Fastest,
                fileOverwrite:=Overwrite.Always,
                flatDirectory:=Not tree
            )
        Else
            Call args.Tokens _
                .Skip(3) _
                .AddToArchive(
                    temp,
                    action:=ArchiveAction.Replace,
                    compression:=CompressionLevel.Fastest,
                    fileOverwrite:=Overwrite.Always
                )
        End If

        Try
            Call File.Delete(out)
        Catch ex As Exception

        End Try

        Call out.ParentPath.MkDIR
        Call File.Copy(temp, out)

        Return 0
    End Function
End Module
