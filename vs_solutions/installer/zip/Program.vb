Imports System.IO.Compression
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/compress")>
    <Usage("/compress [/directory] /out <out.zip> <directory/filelist>")>
    Public Function Compress(args As CommandLine) As Integer
        Dim isDirectory As Boolean = args.IsTrue("/directory")
        Dim out$ = args <= "/out"

        If isDirectory Then
            Call GZip.DirectoryArchive(
                args.Tokens(4),
                saveZip:=out,
                action:=ArchiveAction.Replace,
                compression:=CompressionLevel.Fastest,
                fileOverwrite:=Overwrite.Always)
        Else
            Call args.Tokens _
                .Skip(3) _
                .AddToArchive(
                    out,
                    action:=ArchiveAction.Replace,
                    compression:=CompressionLevel.Fastest,
                    fileOverwrite:=Overwrite.Always)
        End If

        Return 0
    End Function
End Module
