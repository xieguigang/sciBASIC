#Region "Microsoft.VisualBasic::fb99b408254ffd2f867e273ffd2b583f, ..\sciBASIC#\vs_solutions\installer\zip\Program.vb"

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

Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.GZip
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/compress")>
    <Usage("/compress [/directory] /out <out.zip> <directory/filelist>")>
    <Argument("/directory", True, CLITypes.Boolean,
              Description:="Zip compress target is a directory?")>
    <Argument("/out", False, CLITypes.File, PipelineTypes.std_out,
              Extensions:="*.zip",
              Description:="The output file path of the zip package file.")>
    Public Function Compress(args As CommandLine) As Integer
        Dim isDirectory As Boolean = args.IsTrue("/directory")
        Dim out$ = args <= "/out"
        Dim temp$ = App.GetAppSysTempFile(".zip", App.PID)

        If isDirectory Then
            Call GZip.DirectoryArchive(
                args.Tokens(4),
                saveZip:=temp,
                action:=ArchiveAction.Replace,
                compression:=CompressionLevel.Fastest,
                fileOverwrite:=Overwrite.Always,
                flatDirectory:=True)
        Else
            Call args.Tokens _
                .Skip(3) _
                .AddToArchive(
                    temp,
                    action:=ArchiveAction.Replace,
                    compression:=CompressionLevel.Fastest,
                    fileOverwrite:=Overwrite.Always)
        End If

        Try
            Call System.IO.File.Delete(out)
        Catch ex As Exception

        End Try

        Call out.ParentPath.MkDIR
        Call System.IO.File.Copy(temp, out)

        Return 0
    End Function
End Module

