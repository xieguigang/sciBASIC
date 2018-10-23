Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports r = System.Text.RegularExpressions.Regex

Namespace FileIO.Path

    Public Class ProgramPathSearchTool

        Public Property Environments As New List(Of String) From {"", ""}

        Const VERSION As String = "[-_`~.]\d+(\.\d+)*"
        Const TopDirectory As SearchOption = SearchOption.SearchTopLevelOnly

        ''' <summary>
        ''' 商标搜索规则
        ''' </summary>
        ''' <param name="ProgramFiles"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function BranchRule(programFiles$, keyword$) As IEnumerable(Of String)
            Dim programFilesDirectories = FileSystem.GetDirectories(programFiles, TopDirectory, keyword)
            Dim fsObjs As New List(Of String)

            For Each dir As String In programFilesDirectories
                fsObjs += FileSystem.GetDirectories(dir, TopDirectory)
            Next

            Call fsObjs.Add(programFilesDirectories.ToArray)

            If fsObjs = 0 Then
                ' 这个应用程序的安装文件夹可能是带有版本号标记的
                Dim dirs = FileSystem.GetDirectories(programFiles, TopDirectory)
                Dim version As String = keyword & ProgramPathSearchTool.VERSION
                Dim patterns$() = LinqAPI.Exec(Of String) _
 _
                    () <= From DIR As String
                          In dirs
                          Let name As String = FileSystem.GetDirectoryInfo(DIR).Name
                          Let match = r.Match(name, version, RegexOptions.IgnoreCase)
                          Where match.Success
                          Select DIR

                Call fsObjs.Add(patterns)
            End If

            Return fsObjs
        End Function

        ''' <summary>
        ''' Search for the path of a script file with a specific extension name.
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="Keyword"></param>
        ''' <param name="withExtension">脚本文件的文件拓展名</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Search.Scripts", Info:="Search for the path of a script file with a specific extension name.")>
        Public Iterator Function SearchScriptFile(dir$, keyword$, Optional withExtension$ = "") As IEnumerable(Of String)
            Dim scriptFileNameRule$ = $"*{keyword}*{withExtension}"
            Dim files = FileSystem.GetFiles(dir, TopDirectory, scriptFileNameRule)
            Dim binDIR As String = $"{dir}/bin/"
            Dim programDIR As String = $"{dir}/Program"
            Dim scriptsDIR As String = $"{dir}/scripts"
            Dim testExtensionName As Assert(Of String)

            If withExtension.StringEmpty Then
                testExtensionName = Function(path) path.ExtensionSuffix.StringEmpty
            Else
                testExtensionName = Function(path) True
            End If

            For Each folder As String In {binDIR, programDIR, scriptsDIR}
                If FileSystem.DirectoryExists(folder) Then
                    For Each file As String In ls - l - scriptFileNameRule <= folder
                        If testExtensionName(file) Then
                            Yield file
                        End If
                    Next
                End If
            Next

            For Each file As String In files
                If testExtensionName(file) Then
                    Yield file
                End If
            Next
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="SpecificDrive">所制定进行搜索的驱动器，假若希望搜索整个硬盘，请留空字符串</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("DIR.Search.Program_Directory",
                   Info:="Search for the directories which its name was matched the keyword pattern.")>
        Public Function SearchDirectory(Keyword As String, SpecificDrive As String) As String()
            Dim Drives As ReadOnlyCollection(Of DriveInfo) =
                If(String.IsNullOrEmpty(SpecificDrive),
                   FileIO.FileSystem.Drives,
                   New ReadOnlyCollection(Of IO.DriveInfo)(
                       {FileIO.FileSystem.GetDriveInfo(SpecificDrive)}))
            Dim DIRs As New List(Of String)

            For Each Drive As DriveInfo In Drives
                DIRs += SearchDrive(Drive, Keyword)
            Next

            Return DIRs.ToArray
        End Function

        Private Function SearchDrive(Drive As DriveInfo, keyword As String) As String()
            If Not Drive.IsReady Then
                Return New String() {}
            End If

            Dim driveName$ = Drive.RootDirectory.FullName
            Dim DriveRoot = FileSystem.GetDirectories(driveName, SearchOption.SearchTopLevelOnly, keyword)
            Dim files As New List(Of String)
            Dim ProgramFiles As String = String.Format("{0}/Program Files", Drive.RootDirectory.FullName)
            If FileIO.FileSystem.DirectoryExists(ProgramFiles) Then
                Call files.AddRange(BranchRule(ProgramFiles, keyword))
            End If

            Dim ProgramFilesX86 = String.Format("{0}/Program Files(x86)", Drive.RootDirectory.FullName)
            If FileIO.FileSystem.DirectoryExists(ProgramFilesX86) Then
                Call files.AddRange(BranchRule(ProgramFilesX86, keyword))
            End If
            Call files.AddRange(DriveRoot)
            Call files.AddRange(DriveRoot.Select(Function(rootDir) BranchRule(rootDir, keyword)).Unlist)

            Return files.ToArray
        End Function

        ''' <summary>
        ''' Invoke the search session for the program file using a specific keyword string value.(使用某个关键词来搜索目标应用程序)
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("File.Search.Program",
                   Info:="Invoke the search session for the program file using a specific keyword string value.")>
        Public Function SearchProgram(DIR As String, Keyword As String) As String()
            Dim ExeNameRule As String = String.Format("*{0}*.exe", Keyword)
            Dim DllNameRule As String = String.Format("*{0}*.dll", Keyword)

            Dim Files = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, ExeNameRule, DllNameRule)
            Dim binDIR As String = String.Format("{0}/bin/", DIR)
            Dim ProgramDIR As String = String.Format("{0}/Program", DIR)
            Dim buffer As New List(Of String)

            If FileIO.FileSystem.DirectoryExists(binDIR) Then
                buffer += FileIO.FileSystem.GetFiles(
                    binDIR,
                    FileIO.SearchOption.SearchTopLevelOnly,
                    ExeNameRule, DllNameRule)
            End If
            If FileIO.FileSystem.DirectoryExists(ProgramDIR) Then
                buffer += FileIO.FileSystem.GetFiles(
                    ProgramDIR,
                    FileIO.SearchOption.SearchTopLevelOnly,
                    ExeNameRule, DllNameRule)
            End If

            buffer += Files

            Return buffer.ToArray
        End Function
    End Class
End Namespace