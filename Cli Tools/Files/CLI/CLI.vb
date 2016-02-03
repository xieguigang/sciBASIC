Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions

Module CLI

    <ExportAPI("Change", Usage:="Change Ext <*.ext> As <*.ext> [In <DIR> /all]")>
    Public Function ChangeExt(args As CommandLine.CommandLine) As Integer
        Dim fromExt As String = args("ext").Split("."c).Last
        Dim toExt As String = "." & args("as").Split("."c).Last
        Dim fromDIR As String = args.GetValue("in", App.CurrentWork)
        Dim method As FileIO.SearchOption =
            If(args.GetBoolean("/all"),
            FileIO.SearchOption.SearchAllSubDirectories,
            FileIO.SearchOption.SearchTopLevelOnly)

        For Each file As String In FileIO.FileSystem.GetFiles(fromDIR, method, $"*.{fromExt}")
            Dim newPath As String = file.TrimFileExt & toExt
            Try
                Call FileIO.FileSystem.MoveFile(file, newPath)
            Catch ex As Exception
                Call App.LogException(New Exception($"{file}  => {newPath}", ex))
            End Try
        Next

        Return 0
    End Function

    <ExportAPI("ReplaceName", Usage:="ReplaceName from <old> to <new> [in <DIR> Limits <extlist> /all]")>
    Public Function Replace(args As CommandLine.CommandLine) As Integer
        Dim from As String = args("from")
        Dim toS As String = args("to")
        Dim fromDIR As String = args.GetValue("in", App.CurrentWork)
        Dim lstExt As String() = args.GetValue("limits", "*.*").Split(";"c).ToArray(Function(x) "*." & x.Split("."c).Last)
        Dim method As FileIO.SearchOption =
            If(args.GetBoolean("/all"),
            FileIO.SearchOption.SearchAllSubDirectories,
            FileIO.SearchOption.SearchTopLevelOnly)

        For Each file As String In FileIO.FileSystem.GetFiles(fromDIR, method, lstExt)
            Dim Name As String = IO.Path.GetFileNameWithoutExtension(file)
            Name = Regex.Replace(Name, from, toS, RegexOptions.IgnoreCase)
            Dim newPath As String = fromDIR & $"/{Name}.{file.Split("."c).Last}"
            Try
                Call FileIO.FileSystem.MoveFile(file, newPath)
            Catch ex As Exception
                Call App.LogException(New Exception($"{file}  => {newPath}", ex))
            End Try
        Next

        Return 0
    End Function

    <ExportAPI("/Merge", Usage:="/Merge /source <sourceDIR> [/ext <extList:=*.txt> /out <outFile.txt>]")>
    <ParameterInfo("/ext", True, Description:="Each file extension should separated by character ';'.")>
    Public Function Merge(args As CommandLine.CommandLine) As Integer
        Dim inDIR As String = args("/source")
        Dim ext As String = args.GetValue("/ext", "*.txt")
        Dim out As String = args.GetValue("/out", inDIR & ".txt")
        Dim lstExt As String() = ext.Split(";"c)

        Try
            Call FileIO.FileSystem.DeleteFile(out, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        Catch ex As Exception

        End Try

        Call $"Merged file was saved at {out.ToFileURL}....".__DEBUG_ECHO

        For Each file As String In FileIO.FileSystem.GetFiles(inDIR, FileIO.SearchOption.SearchTopLevelOnly, lstExt)
            Dim txt As String = IO.File.ReadAllText(file)
            Call FileIO.FileSystem.WriteAllText(file:=out, text:=txt & vbCrLf, append:=True)
        Next

        Call "OK!!!".__DEBUG_ECHO

        Return True
    End Function

    <ExportAPI("Copys", Usage:="Copys From <DIR> to <DIR> NameOf <file_name> /no-name")>
    Public Function Copy(args As CommandLine.CommandLine) As Integer
        Dim fromDIR As String = args("from")
        Dim copyToDIR As String = args("to")
        Dim name As String = args("nameof")
        Dim files = FileIO.FileSystem.GetFiles(fromDIR, FileIO.SearchOption.SearchAllSubDirectories, name)
        Dim noName As Boolean = args.GetBoolean("/no-name")
        Dim ext As String = name.Split("."c).Last

        Call FileIO.FileSystem.CreateDirectory(copyToDIR)

        For Each file As String In files
            Dim dest As String = __copyTo(file, copyToDIR, fromDIR, noName) & ext
            Try
                Call FileIO.FileSystem.DeleteFile(dest)
            Catch ex As Exception

            End Try
            Try
                Call FileIO.FileSystem.CopyFile(file, dest)
            Catch ex As Exception

            End Try

            Call Console.Write(".")
        Next

        Return 0
    End Function

    Private Function __copyTo(source As String,
                              copyToDIR As String,
                              from As String,
                              noName As String) As String

        Dim sourInfo = FileIO.FileSystem.GetFileInfo(source)

        source = Mid(sourInfo.FullName, FileIO.FileSystem.GetDirectoryInfo(from).FullName.Length + 2)
        source = source.Replace("\", "/").Replace("/", ".")

        If noName Then
            source = source.Replace(sourInfo.Name, "")
        End If

        Return copyToDIR & "/" & source
    End Function
End Module
