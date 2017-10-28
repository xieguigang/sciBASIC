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
