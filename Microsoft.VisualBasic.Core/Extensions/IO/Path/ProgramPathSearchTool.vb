Imports Microsoft.VisualBasic.Language

Namespace FileIO.Path

    Public Class ProgramPathSearchTool

        Public Property Environments As New List(Of String) From {"", ""}

        Const VERSION As String = "[-_`~.]\d+(\.\d+)*"

        Public Function FindProgram() As String

        End Function

        Public Function FindScript() As String

        End Function

        ''' <summary>
        ''' 商标搜索规则
        ''' </summary>
        ''' <param name="ProgramFiles"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function BranchRule(ProgramFiles As String, Keyword As String) As String()
            Dim ProgramFiles_Directories = FileIO.FileSystem.GetDirectories(
                ProgramFiles,
                FileIO.SearchOption.SearchTopLevelOnly,
                Keyword)
            Dim fsObjs As New List(Of String)

            For Each Dir As String In ProgramFiles_Directories
                fsObjs += FileIO.FileSystem.GetDirectories(
                    Dir, FileIO.SearchOption.SearchTopLevelOnly)
            Next
            Call fsObjs.Add(ProgramFiles_Directories.ToArray)

            If fsObjs.Count = 0 Then
                ' 这个应用程序的安装文件夹可能是带有版本号标记的
                Dim Dirs = FileIO.FileSystem.GetDirectories(ProgramFiles, FileIO.SearchOption.SearchTopLevelOnly)
                Dim version As String = Keyword & ProgramPathSearchTool.VERSION
                Dim Patterns As String() =
                    LinqAPI.Exec(Of String) <= From DIR As String
                                               In Dirs
                                               Let name As String = FileIO.FileSystem.GetDirectoryInfo(DIR).Name
                                               Where Regex.Match(name, version, RegexOptions.IgnoreCase).Success
                                               Select DIR
                Call fsObjs.Add(Patterns)
            End If

            Return fsObjs.ToArray
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="Keyword"></param>
        ''' <param name="withExtension">脚本文件的文件拓展名</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Search.Scripts", Info:="Search for the path of a script file with a specific extension name.")>
        Public Function SearchScriptFile(DIR As String, Keyword As String, Optional withExtension As String = "") As String()
            Dim ScriptFileNameRule As String = String.Format("*{0}*{1}", Keyword, withExtension)
            Dim Files = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, ScriptFileNameRule)
            Dim binDIR As String = String.Format("{0}/bin/", DIR)
            Dim ProgramDIR As String = String.Format("{0}/Program", DIR)
            Dim ScriptsDIR As String = String.Format("{0}/scripts", DIR)
            Dim fileList As New List(Of String)

            If FileIO.FileSystem.DirectoryExists(binDIR) Then fileList += (ls - l - wildcards(ScriptFileNameRule) <= binDIR)
            If FileIO.FileSystem.DirectoryExists(ProgramDIR) Then fileList += (ls - l - wildcards(ScriptFileNameRule) <= ProgramDIR)
            If FileIO.FileSystem.DirectoryExists(ScriptsDIR) Then fileList += (ls - l - wildcards(ScriptFileNameRule) <= ScriptsDIR)

            Call fileList.AddRange(Files)

            If String.IsNullOrEmpty(withExtension) Then
                Return LinqAPI.Exec(Of String) _
 _
                    () <= From strPath As String
                          In fileList
                          Let ext As String = FileIO.FileSystem.GetFileInfo(strPath).Extension
                          Where String.IsNullOrEmpty(ext)
                          Select strPath
            Else
                Return fileList.ToArray
            End If
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

        <Extension>
        Private Function SearchDrive(Drive As DriveInfo, keyword As String) As String()
            If Not Drive.IsReady Then
                Return New String() {}
            End If

            Dim DriveRoot = FileIO.FileSystem.GetDirectories(Drive.RootDirectory.FullName, FileIO.SearchOption.SearchTopLevelOnly, keyword)
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