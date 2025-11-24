#Region "Microsoft.VisualBasic::3027b843c983021fe8daff25ac82e847, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\ProgramPathSearchTool.vb"

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

    '   Total Lines: 243
    '    Code Lines: 156 (64.20%)
    ' Comment Lines: 50 (20.58%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 37 (15.23%)
    '     File Size: 10.25 KB


    '     Class ProgramPathSearchTool
    ' 
    '         Properties: CustomDirectories, Directories
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BranchRule, FindProgram, FindScript, safeGetFiles, SearchDirectory
    '                   SearchDrive, searchImpl, SearchProgram, SearchScriptFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports ENV = System.Environment
Imports r = System.Text.RegularExpressions.Regex
Imports SearchOption = Microsoft.VisualBasic.FileIO.SearchOption

Namespace ApplicationServices

    ''' <summary>
    ''' Program helper for search dir like ``C:\Program Files`` or ``C:\Program Files(x86)``
    ''' </summary>
    ''' <remarks>
    ''' Works on windows, have not test on Linux/Mac yet.
    ''' </remarks>
    Public Class ProgramPathSearchTool

        ReadOnly Environments As New List(Of String) From {"ProgramFiles(x86)", "ProgramFiles"}

        Const VERSION As String = "[-_`~.]\d+(\.\d+)*.+"
        Const TopDirectory As SearchOption = SearchOption.SearchTopLevelOnly

        Public ReadOnly Property Directories As IReadOnlyCollection(Of String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Environments _
                    .Select(Function(var) ENV.GetEnvironmentVariable(var)) _
                    .Join(CustomDirectories.SafeQuery) _
                    .Where(Function(dir)
                               Return (Not dir.StringEmpty) AndAlso dir.DirectoryExists
                           End Function) _
                    .ToArray
            End Get
        End Property

        Public Property CustomDirectories As String()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ENV">
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="keyword$">常规的文件名称之类的，不可以包含有通配符</param>
        ''' <param name="includeDll"></param>
        ''' <returns></returns>
        Public Iterator Function FindProgram(keyword$, Optional includeDll As Boolean = True) As IEnumerable(Of String)
            For Each dir As String In Directories
                For Each file As String In SearchProgram(dir, keyword, includeDll)
                    Yield file
                Next
                For Each file As String In SearchProgram($"{dir}/{keyword}", keyword, includeDll)
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
            Dim programFilesDirectories = FileIO.FileSystem.GetDirectories(programFiles, TopDirectory, keyword)
            Dim fsObjs As New List(Of String)

            For Each dir As String In programFilesDirectories
                fsObjs += FileIO.FileSystem.GetDirectories(dir, TopDirectory)
            Next

            Call fsObjs.Add(programFilesDirectories.ToArray)

            If fsObjs = 0 Then
                ' 这个应用程序的安装文件夹可能是带有版本号标记的
                Dim dirs = FileIO.FileSystem.GetDirectories(programFiles, TopDirectory)
                Dim version As String = keyword & ProgramPathSearchTool.VERSION
                Dim patterns$() = LinqAPI.Exec(Of String) _
                                                          _
                    () <= From DIR As String
                          In dirs
                          Let name As String = FileIO.FileSystem.GetDirectoryInfo(DIR).Name
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
        Public Shared Function SearchScriptFile(dir$, keyword$, Optional withExtension$ = Nothing) As IEnumerable(Of String)
            Dim scriptFileNameRule$ = $"*{keyword}*{withExtension}"
            Dim extNameAssert As Predicate(Of String)

            If withExtension.StringEmpty Then
                extNameAssert = Function(path) path.ExtensionSuffix.StringEmpty
            Else
                extNameAssert = Function(path) True
            End If

            Return SearchDirWithRulesImpl(dir, {scriptFileNameRule}).Where(Function(file) extNameAssert(file))
        End Function

        Private Shared Function safeGetFiles(dir$, rules$()) As IEnumerable(Of String)
            If dir.DirectoryExists Then
                Return FileIO.FileSystem.GetFiles(dir, TopDirectory, rules)
            Else
                Return {}
            End If
        End Function

        Private Shared Iterator Function SearchDirWithRulesImpl(dir$, rules$()) As IEnumerable(Of String)
            Dim files As IEnumerable(Of String) = safeGetFiles(dir, rules)
            Dim binDIR As String = $"{dir}/bin/"
            Dim programDIR As String = $"{dir}/Program"
            Dim scriptsDIR As String = $"{dir}/scripts"

            For Each folder As String In {binDIR, programDIR, scriptsDIR}
                If Directory.Exists(folder) Then
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
        Public Shared Iterator Function SearchDirectory(keyword$, Optional specificDrive$ = Nothing) As IEnumerable(Of String)
            Dim drives As ReadOnlyCollection(Of DriveInfo)

            If String.IsNullOrEmpty(specificDrive) Then
                drives = FileIO.FileSystem.Drives
            Else
                drives = New ReadOnlyCollection(Of DriveInfo)({FileIO.FileSystem.GetDriveInfo(specificDrive)})
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
            Dim driveRoot = FileIO.FileSystem.GetDirectories(driveName, SearchOption.SearchTopLevelOnly, keyword).JoinIterates(BranchRule(driveName, keyword)).ToArray
            Dim files As New List(Of String)
            Dim ProgramFiles As String = String.Format("{0}/Program Files", drive.RootDirectory.FullName)

            If FileIO.FileSystem.DirectoryExists(ProgramFiles) Then
                Call files.AddRange(BranchRule(ProgramFiles, keyword))
            End If

            Dim ProgramFilesX86 = String.Format("{0}/Program Files(x86)", drive.RootDirectory.FullName)

            If FileIO.FileSystem.DirectoryExists(ProgramFilesX86) Then
                Call files.AddRange(BranchRule(ProgramFilesX86, keyword))
            End If

            Call files.AddRange(driveRoot)
            Call files.AddRange(driveRoot.Select(Function(rootDir) BranchRule(rootDir, keyword)).Unlist)

            Return files.ToArray
        End Function

        ''' <summary>
        ''' Invoke the search session for the program file using a specific keyword string value.
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="Keyword"></param>
        ''' <returns></returns>
        ''' <remarks>(使用某个关键词来搜索目标应用程序)</remarks>
        Public Shared Function SearchProgram(dir$, keyword$, Optional includeDll As Boolean = True) As IEnumerable(Of String)
            Dim exeNameRule As String = $"*{keyword}*.exe"
            Dim dllNameRule As String = $"*{keyword}*.dll"
            Dim rules$()

            If includeDll Then
                rules = {exeNameRule, dllNameRule}
            Else
                rules = {exeNameRule}
            End If

            Return SearchDirWithRulesImpl(dir, rules)
        End Function
#End Region

    End Class
End Namespace
