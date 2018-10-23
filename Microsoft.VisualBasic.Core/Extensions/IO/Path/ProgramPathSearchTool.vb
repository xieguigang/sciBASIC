Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports ENV = System.Environment
Imports r = System.Text.RegularExpressions.Regex

Namespace FileIO.Path

    Public Class ProgramPathSearchTool

        ReadOnly Environments As New List(Of String) From {"ProgramFiles(x86)", "ProgramFiles"}

        Const VERSION As String = "[-_`~.]\d+(\.\d+)*"
        Const TopDirectory As SearchOption = SearchOption.SearchTopLevelOnly

        Public ReadOnly Property Directories As IReadOnlyCollection(Of String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Environments _
                    .Select(Function(var) ENV.GetEnvironmentVariable(var)) _
                    .Join(CustomDirectories.SafeQuery) _
                    .Where(Function(dir) dir.DirectoryExists) _
                    .ToArray
            End Get
        End Property

        Public Property CustomDirectories As String()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ENV$">
        ''' 环境变量名称列表，如果不赋值这个参数，则会默认使用Program Files和Program Files(x86)这两个文件夹
        ''' </param>
        Sub New(ParamArray ENV$())
            Environments += ENV
        End Sub

        Public Iterator Function FindScript(keyword$, Optional withExtension$ = Nothing) As IEnumerable(Of String)
            For Each dir As String In Directories
                For Each file As String In SearchScriptFile(dir, keyword, withExtension)
                    Yield file
                Next
            Next
        End Function

        Public Iterator Function FindProgram(keyword$, Optional includeDll As Boolean = True) As IEnumerable(Of String)
            For Each dir As String In Directories
                For Each file As String In SearchProgram(dir, keyword, includeDll)
                    Yield file
                Next
            Next
        End Function

#Region "Search Implementation Internal"

        ''' <summary>
        ''' 商标搜索规则
        ''' </summary>
        ''' <param name="ProgramFiles"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function BranchRule(programFiles$, keyword$) As IEnumerable(Of String)
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
        Public Shared Function SearchScriptFile(dir$, keyword$, Optional withExtension$ = Nothing) As IEnumerable(Of String)
            Dim scriptFileNameRule$ = $"*{keyword}*{withExtension}"
            Dim extNameAssert As Assert(Of String)

            If withExtension.StringEmpty Then
                extNameAssert = Function(path) path.ExtensionSuffix.StringEmpty
            Else
                extNameAssert = Function(path) True
            End If

            Return searchImpl(dir, {scriptFileNameRule}).Where(Function(file) extNameAssert(file))
        End Function

        Private Shared Iterator Function searchImpl(dir$, rules$()) As IEnumerable(Of String)
            Dim files = FileSystem.GetFiles(dir, TopDirectory, rules)
            Dim binDIR As String = $"{dir}/bin/"
            Dim programDIR As String = $"{dir}/Program"
            Dim scriptsDIR As String = $"{dir}/scripts"

            For Each folder As String In {binDIR, programDIR, scriptsDIR}
                If FileSystem.DirectoryExists(folder) Then
                    For Each file As String In ls - l - rules <= folder
                        Yield file
                    Next
                End If
            Next

            For Each file As String In files
                Yield file
            Next
        End Function

        ''' <summary>
        ''' Search for the directories which its name was matched the keyword pattern.
        ''' </summary>
        ''' <param name="SpecificDrive">所指定进行搜索的驱动器，假若希望搜索整个硬盘，请留空字符串</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("DIR.Search.Program_Directory")>
        <Description("Search for the directories which its name was matched the keyword pattern.")>
        Public Shared Iterator Function SearchDirectory(keyword$, Optional specificDrive$ = Nothing) As IEnumerable(Of String)
            Dim drives As ReadOnlyCollection(Of DriveInfo)

            If String.IsNullOrEmpty(specificDrive) Then
                drives = FileSystem.Drives
            Else
                drives = New ReadOnlyCollection(Of DriveInfo)({FileSystem.GetDriveInfo(specificDrive)})
            End If

            For Each drive As DriveInfo In drives
                For Each dir As String In SearchDrive(drive, keyword)
                    Yield dir
                Next
            Next
        End Function

        Private Shared Function SearchDrive(drive As DriveInfo, keyword As String) As String()
            If Not drive.IsReady Then
                Return New String() {}
            End If

            Dim driveName$ = drive.RootDirectory.FullName
            Dim driveRoot = FileSystem.GetDirectories(driveName, SearchOption.SearchTopLevelOnly, keyword)
            Dim files As New List(Of String)
            Dim ProgramFiles As String = String.Format("{0}/Program Files", drive.RootDirectory.FullName)

            If FileSystem.DirectoryExists(ProgramFiles) Then
                Call files.AddRange(BranchRule(ProgramFiles, keyword))
            End If

            Dim ProgramFilesX86 = String.Format("{0}/Program Files(x86)", drive.RootDirectory.FullName)

            If FileSystem.DirectoryExists(ProgramFilesX86) Then
                Call files.AddRange(BranchRule(ProgramFilesX86, keyword))
            End If

            Call files.AddRange(driveRoot)
            Call files.AddRange(driveRoot.Select(Function(rootDir) BranchRule(rootDir, keyword)).Unlist)

            Return files.ToArray
        End Function

        ''' <summary>
        ''' Invoke the search session for the program file using a specific keyword string value.
        ''' (使用某个关键词来搜索目标应用程序)
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("File.Search.Program")>
        <Description("Invoke the search session for the program file using a specific keyword string value.")>
        Public Shared Function SearchProgram(dir$, keyword$, Optional includeDll As Boolean = True) As IEnumerable(Of String)
            Dim exeNameRule As String = $"*{keyword}*.exe"
            Dim dllNameRule As String = $"*{keyword}*.dll"
            Dim rules$()

            If includeDll Then
                rules = {exeNameRule, dllNameRule}
            Else
                rules = {exeNameRule}
            End If

            Return searchImpl(dir, rules)
        End Function
#End Region

    End Class
End Namespace