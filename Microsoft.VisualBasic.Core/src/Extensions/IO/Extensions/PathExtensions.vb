#Region "Microsoft.VisualBasic::acc7b0307a639e3fb2d5338819f44492, Microsoft.VisualBasic.Core\src\Extensions\IO\Extensions\PathExtensions.vb"

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

    ' Module PathExtensions
    ' 
    '     Function: BaseName, ChangeSuffix, DeleteFile, DIR, DirectoryExists
    '               DirectoryName, EnumerateFiles, (+2 Overloads) ExtensionSuffix, FileCopy, FileExists
    '               FileLength, FileMove, FileName, FileOpened, GetDirectoryFullPath
    '               GetFullPath, ListDirectory, ListFiles, Long2Short, (+2 Overloads) NormalizePathString
    '               ParentDirName, ParentPath, PathCombine, PathIllegal, ReadDirectory
    '               (+2 Overloads) RelativePath, SafeCopyTo, SplitPath, TheFile, ToDIR_URL
    '               ToFileURL, TrimDIR, TrimSuffix, UnixPath
    ' 
    '     Sub: MakeDir
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Math
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Search the path from a specific keyword.(通过关键词来推测路径)
''' </summary>
''' <remarks></remarks>
<Package("Program.Path.Search", Description:="A utility tools for searching a specific file of its path on the file system more easily.")>
Public Module PathExtensions

    ''' <summary>
    ''' 修改路径字符串之中的文件名后缀拓展名为<paramref name="newSuffix"/>指定的值。<paramref name="newSuffix"/>不需要添加小数点
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="newSuffix$">新的文件拓展名，这个拓展名不带有小数点，例如需要修改为*.csv，则直接赋值csv即可</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ChangeSuffix(path$, newSuffix$) As String
        Return path.TrimSuffix & "." & newSuffix
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SplitPath(path As String) As String()
        Return path.Replace("/"c, "\"c) _
                   .StringReplace("\\{2,}", "\") _
                   .Trim("\"c) _
                   .Split("\"c)
    End Function

    ''' <summary>
    ''' Execute file delete
    ''' </summary>
    ''' <param name="path">The file path or the directory path.</param>
    ''' <param name="throwEx"></param>
    ''' <param name="strictFile">
    ''' this function is not allowed for delete a directory by default.
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function DeleteFile(path$,
                               Optional throwEx As Boolean = False,
                               Optional strictFile As Boolean = True) As Boolean
        Try
            If path.FileExists Then
                Call FileIO.FileSystem.DeleteFile(
                   path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently
                )
            ElseIf path.DirectoryExists Then
                If strictFile Then
                    Throw New InvalidOperationException($"the given target is a directory, which the option of this operation is not allowed by default, you could set `strictFile` parameter to FALSE for removes this restriction!")
                Else
                    Call $"All content files in directory '{path}' is removed.".Warning
                    Call FileIO.FileSystem.DeleteDirectory(
                        path, DeleteDirectoryOption.DeleteAllContents
                    )
                End If
            End If

            Return True
        Catch ex As Exception
            If throwEx Then
                Throw New Exception(path, ex)
            Else
                Call App.LogException(ex, path)
                Call ex.PrintException
            End If

            Return False
        Finally
        End Try
    End Function

    ''' <summary>
    ''' 函数返回文件的拓展名后缀，请注意，这里的返回值是不会带有小数点的
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ExtensionSuffix(path As String) As String
        If path.StringEmpty Then
            Return ""
        Else
            Dim fileName = path.Split("\"c).Last.Split("/"c).Last
            Dim suffix = fileName.Split("."c).Last

            If fileName = suffix Then
                Return ""
            Else
                Return suffix
            End If
        End If
    End Function

    ''' <summary>
    ''' Test if is there any given extension name is equals 
    ''' to the extension name of the specific file 
    ''' <paramref name="path"/>.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="isAny">不带小数点的文件拓展名列表</param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ExtensionSuffix(path$, ParamArray isAny As String()) As Boolean
        Return path.ExtensionSuffix.DoCall(Function(ext) isAny.Any(Function(s) s.TextEquals(ext)))
    End Function

    ''' <summary>
    ''' Combine directory path.(这个主要是用于生成文件夹名称)
    ''' 
    ''' ###### Example usage
    ''' 
    ''' ```vbnet
    ''' Dim images As Dictionary(Of String, String) =
    '''     (ls - l - {"*.png", "*.jpg", "*.gif"} &lt;= PlatformEngine.wwwroot.DIR("images")) _
    '''     .ToDictionary(Function(file) file.StripAsId,
    '''                   AddressOf FileName)
    ''' ```
    ''' </summary>
    ''' <param name="d"></param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DIR(d As DirectoryInfo, name$) As String
        Return $"{d.FullName}/{name}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function UnixPath(path As String) As String
        Return FileIO.FileSystem.GetFileInfo(path).FullName.Replace("\", "/")
    End Function

    ''' <summary>
    ''' Make directory
    ''' </summary>
    ''' <param name="DIR"></param>
    <Extension> Public Sub MakeDir(DIR$, Optional throwEx As Boolean = True)
        If DIR.StringEmpty OrElse DIR = "./" OrElse DIR = ".\" Then
            ' 2017-12-25
            ' 当前文件夹
            ' 因为假若能够切换到当前文件夹的话，说明当前的文件夹已经存在了
            ' 则在这里是否应该跳过这个创建过程
            ' 还是不跳过吧
            DIR = App.CurrentDirectory
        End If

        DIR = DIR.Replace("\", "/")

        Try
            Call FileIO.FileSystem.CreateDirectory(DIR)
        Catch ex As Exception
            ex = New Exception("DIR value is: " & DIR, ex)

            If throwEx Then
                Throw ex
            Else
                Call App.LogException(ex)
            End If
        End Try
    End Sub

    <Extension>
    Public Function PathCombine(path As String, addTag As String) As String
        If path.DirectoryExists Then
            Return path.ParentPath & "/" & path.BaseName & addTag
        Else
            Return path.TrimSuffix & addTag
        End If
    End Function

    ReadOnly allKinds As New [Default](Of String())({"*.*"}, Function(o) TryCast(o, String()).IsNullOrEmpty)

    ''' <summary>
    ''' 使用<see cref="FileIO.FileSystem.GetFiles"/>函数枚举
    ''' **当前的**(不是递归的搜索所有的子文件夹)文件夹之中的
    ''' 所有的符合条件的文件路径
    ''' </summary>
    ''' <param name="dir">文件夹路径</param>
    ''' <param name="keyword">
    ''' Default is ``*.*`` for match any kind of files.
    ''' (文件名进行匹配的关键词)
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function EnumerateFiles(dir$, ParamArray keyword$()) As IEnumerable(Of String)
        Const top = FileIO.SearchOption.SearchTopLevelOnly

        If Not dir.DirectoryExists Then
            Call $"Directory {dir} is not valid on your file system!".Warning
            Return New String() {}
        Else
            Return FileIO.FileSystem.GetFiles(dir, top, keyword Or allKinds)
        End If
    End Function

    ''' <summary>
    ''' ```
    ''' ls - l - r - pattern &lt;= DIR
    ''' ```
    ''' 
    ''' 的简化拓展函数模式
    ''' </summary>
    ''' <param name="directory"></param>
    ''' <param name="pattern">
    ''' 如果匹配的模式字符串是带有文件后缀名的，那么文件夹之中所有没有后缀名的文件都可能会被忽略掉
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ListFiles(directory$, Optional pattern$ = "*.*") As IEnumerable(Of String)
        Return ls - l - r - pattern <= directory
    End Function

    ''' <summary>
    ''' 这个函数是会直接枚举出所有的文件路径的
    ''' </summary>
    ''' <param name="DIR$"></param>
    ''' <param name="[option]"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ReadDirectory(DIR$, Optional [option] As FileIO.SearchOption = FileIO.SearchOption.SearchTopLevelOnly) As IEnumerable(Of String)
        Dim current As New DirectoryInfo(DIR)

        ' 20200924 skip invalid directory which have no
        ' access 
        If current.FullName.Trim("\"c, "/"c).IsPattern("[A-Z][:][/\\]System Volume Information") Then
            Call $"Can not access to the {current}, skip enumerate files".Warning
            Return
        End If

        For Each file In current.EnumerateFiles
            Yield file.FullName
        Next

        If [option] = FileIO.SearchOption.SearchAllSubDirectories Then
            For Each folder In current.EnumerateDirectories
                For Each path In folder.FullName.ReadDirectory([option])
                    Yield path
                Next
            Next
        End If
    End Function

    ''' <summary>
    ''' Yield subfolders' FullName
    ''' </summary>
    ''' <param name="DIR">文件夹不存在，则返回空的列表</param>
    ''' <param name="[option]"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ListDirectory(DIR$,
                                           Optional [option] As FileIO.SearchOption = FileIO.SearchOption.SearchTopLevelOnly,
                                           Optional fullName As Boolean = True) As IEnumerable(Of String)

        Dim current As New DirectoryInfo(DIR)

        If Not current.Exists Then
            ' 文件夹不存在，则返回空的列表
            Return
        End If

        For Each folder In current.EnumerateDirectories
            If fullName Then
                Yield folder.FullName
            Else
                Yield folder.Name
            End If

            If [option] = FileIO.SearchOption.SearchAllSubDirectories Then
                For Each path In folder.FullName.ListDirectory([option], fullName)
                    Yield path
                Next
            End If
        Next
    End Function

    ''' <summary>
    ''' 这个函数只会返回文件列表之中的第一个文件，故而需要提取某一个文件夹之中的某一个特定的文件，推荐使用这个函数（这个函数默认只查找第一级文件夹，不会进行递归搜索）
    ''' </summary>
    ''' <param name="DIR$"></param>
    ''' <param name="keyword$"></param>
    ''' <param name="opt"></param>
    ''' <param name="wildcard">The <paramref name="keyword"/> parameter value is a wildcard.</param>
    ''' <returns>当查找不到目标文件或者文件夹不存在的时候会返回空值</returns>
    <Extension>
    Public Function TheFile(DIR$, keyword$,
                            Optional opt As FileIO.SearchOption = FileIO.SearchOption.SearchTopLevelOnly,
                            Optional wildcard As Boolean = True) As String

        If Not DIR.DirectoryExists Then
            Return Nothing
        Else
            Dim check As Func(Of String, Boolean) = Nothing

            If Not wildcard Then
                check = Function(path)
                            Return path.FileName.TextEquals(keyword)
                        End Function
            End If

            If opt = FileIO.SearchOption.SearchAllSubDirectories Then
                If wildcard Then
                    Return (ls - l - r - keyword <= DIR).FirstOrDefault
                Else
                    Return (ls - l - r <= DIR).FirstOrDefault(check)
                End If
            Else
                If wildcard Then
                    Return (ls - l - keyword <= DIR).FirstOrDefault
                Else
                    Return (ls - l <= DIR).FirstOrDefault(check)
                End If
            End If
        End If
    End Function

    ''' <summary>
    ''' Gets the URL type file path.(获取URL类型的文件路径)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Path2Url")>
    <Extension> Public Function ToFileURL(path As String) As String
        If String.IsNullOrEmpty(path) Then
            Return ""
        Else
            path = FileIO.FileSystem.GetFileInfo(path).FullName
            Return String.Format("file:///{0}", path.Replace("\", "/"))
        End If
    End Function

    <ExportAPI("DIR2URL"), ExtensionAttribute>
    Public Function ToDIR_URL(DIR As String) As String
        If String.IsNullOrEmpty(DIR) Then
            Return ""
        Else
            DIR = FileIO.FileSystem.GetDirectoryInfo(DIR).FullName
            Return String.Format("file:///{0}", DIR.Replace("\", "/"))
        End If
    End Function

    ''' <summary>
    ''' 枚举所有非法的路径字符
    ''' </summary>
    ''' <remarks></remarks>
    Public Const ILLEGAL_PATH_CHARACTERS As String = ":*?""<>|&"
    Public Const ILLEGAL_FILENAME_CHARACTERS As String = "\/" & ILLEGAL_PATH_CHARACTERS

    ''' <summary>
    ''' 将目标字符串之中的非法的字符替换为"_"符号以成为正确的文件名字符串。当参数<paramref name="alphabetOnly"/>为真的时候，意味着所有的非字母或者数字的字符都会被替换为下划线，默认为真
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="alphabetOnly">当本参数为真的时候，仅26个字母，0-9数字和下划线_以及小数点可以被保留下来</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("NormalizePathString")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function NormalizePathString(str$, Optional alphabetOnly As Boolean = True) As String
        Return NormalizePathString(str, "_", alphabetOnly)
    End Function

    <ExportAPI("NormalizePathString")>
    <Extension> Public Function NormalizePathString(str$, normAs As String, Optional alphabetOnly As Boolean = True) As String
        Dim sb As New StringBuilder(str)
        For Each ch As Char In ILLEGAL_FILENAME_CHARACTERS
            Call sb.Replace(ch, normAs)
        Next

        If alphabetOnly Then
            For Each ch As Char In "()[]+-~!@#$%^&=;',"
                Call sb.Replace(ch, normAs)
            Next
        End If

        Return sb.ToString
    End Function

    Const PathTooLongException =
        "System.IO.PathTooLongException: The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters."

    ''' <summary>
    ''' 假设文件名过长发生在文件名和最后一个文件夹的路径之上
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' System.IO.PathTooLongException: The specified path, file name, or both are too long.
    ''' The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.
    ''' </remarks>
    <Extension> Public Function Long2Short(path As String, <CallerMemberName> Optional caller As String = "") As String
        Dim parent As String = path.ParentPath
        Dim DIRTokens As String() = parent.Replace("\", "/").Split("/"c)
        ' 请注意，由于path参数可能是相对路径，所以在这里DIRname和name要分开讨论
        Dim DIRname As String = DIRTokens.Last
        ' 因为相对路径最终会出现文件夹名称，但在path里面可能是使用小数点来表示的
        Dim name As String = path.Replace("\", "/").Split("/"c).Last

        If parent.Length + name.Length >= 259 Then
            DIRname = Mid(DIRname, 1, 20) & "~"
            Dim ext As String = name.Split("."c).Last
            name = Mid(name, 1, 20) & "~." & ext
            parent = String.Join("/", DIRTokens.Take(DIRTokens.Length - 1).ToArray)
            parent &= "/" & DIRname
            parent &= "/" & name

            Dim ex As Exception = New PathTooLongException(PathTooLongException)
            ex = New Exception(path, ex)
            ex = New Exception("But the path was corrected as:   " & parent & "  to avoid the crashed problem.", ex)
            Call ex.PrintException
            Call App.LogException(ex, caller & " -> " & MethodBase.GetCurrentMethod.GetFullName)

            Return parent.Replace("\", "/")
        Else
            Return FileIO.FileSystem.GetFileInfo(path).FullName
        End If
    End Function

    ''' <summary>
    ''' + C:\
    ''' + AB:\
    ''' + AB2:\
    ''' + etc...
    ''' </summary>
    Const DriveLabel$ = "[a-zA-Z]([a-zA-Z0-9])*"

    ''' <summary>
    ''' File path illegal?
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <ExportAPI("Path.Illegal?")>
    <Extension> Public Function PathIllegal(path As String) As Boolean
        Dim tokens$() = Strings.Split(path.Replace("\", "/"), ":/")

        If tokens.Length > 2 Then  ' 有多余一个的驱动器符，则肯定是非法的路径格式
            Return False
        ElseIf tokens.Length = 2 Then
            ' 完整路径
            ' 当有很多个驱动器的时候，这里会不止一个字母
            If Not tokens(0).IsPattern(DriveLabel, RegexICSng) Then
                ' 开头的驱动器的符号不正确
                Return False
            Else
                ' 驱动器的符号也正确
            End If
        Else
            ' 只有一个，则是相对路径
        End If

        Dim fileName As String = tokens.Last

        ' 由于这里是判断文件是否合法，所以之判断文件名就行了，即token列表的最后一个元素
        For Each ch As Char In ILLEGAL_PATH_CHARACTERS
            If fileName.IndexOf(ch) > -1 Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' Gets the file length, if the path is not exists, then returns -1.
    ''' (安全的获取文件的大小，如果目标文件不存在的话会返回-1)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FileLength(path As String) As Long
        If path.StringEmpty Then
            Return -1
        ElseIf Not path.FileExists OrElse path.DirectoryExists Then
            Return -1&
        Else
            Return FileIO.FileSystem.GetFileInfo(path).Length
        End If
    End Function

    ''' <summary>
    ''' Safe file copy operation.
    ''' (请注意，<paramref name="copyTo"/>参数的字符串最末尾必须是``/``或者``\``才会被认作为目录路径)
    ''' </summary>
    ''' <param name="source$"></param>
    ''' <param name="copyTo$">
    ''' Can be file name or directory name.
    ''' 
    ''' + If this paramter is a file path, then you can copy the 
    '''   source file to another location with renamed.
    ''' + If this parameter is a directory location, then you can 
    '''   copy the source file to another location with the 
    '''   identical file name.
    ''' 
    ''' Please notice that, the directory path should end with 
    ''' path seperator symbol: ``\`` or ``/``.
    ''' </param>
    ''' <returns></returns>
    <Extension> Public Function FileCopy(source$, copyTo$) As Boolean
        Try
            If copyTo.Last = "/"c OrElse copyTo.Last = "\"c Then
                copyTo = copyTo & source.FileName
            End If

            If copyTo.FileExists Then
                Call FileIO.FileSystem.DeleteFile(copyTo)
            Else
                Call copyTo.ParentPath.MakeDir
            End If

            Call FileIO.FileSystem.CopyFile(source, copyTo)
        Catch ex As Exception
            ex = New Exception({source, copyTo}.GetJson, ex)
            App.LogException(ex)

            Return False
        End Try

        Return True
    End Function

    <Extension>
    Public Function FileMove(source$, target$) As Boolean
        Try
            Call File.Move(source, target)
            Return True
        Catch ex As Exception
            ex = New Exception("source: " & source, ex)
            ex = New Exception("target: " & target, ex)

            Call App.LogException(ex)

            Return False
        End Try
    End Function

    ''' <summary>
    ''' Check if the target file object is exists on your file system or not.
    ''' (这个函数也会自动检查目标<paramref name="path"/>参数是否为空)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="ZERO_Nonexists">将0长度的文件也作为不存在</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function FileExists(path$, Optional ZERO_Nonexists As Boolean = False) As Boolean
        If path.StringEmpty Then
            Return False
        End If
        If path.IndexOf(ASCII.CR) > -1 OrElse path.IndexOf(ASCII.LF) > -1 Then
            ' 包含有回车符或者换行符，则肯定不是文件路径了
            Return False
        End If

        If Not String.IsNullOrEmpty(path) Then
            Try
                ' 文件存在
                If FileIO.FileSystem.FileExists(path) Then
                    If ZERO_Nonexists Then
                        Return FileSystem.FileLen(path) > 0
                    Else
                        Return True
                    End If
                End If
            Catch ex As Exception
                Return False
            End Try
        End If

        Return False
    End Function

    ''' <summary>
    ''' Determine that the target directory is exists on the file system or not?(判断文件夹是否存在)
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns></returns>
    <ExportAPI("DIR.Exists")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DirectoryExists(DIR As String) As Boolean
        Return Not String.IsNullOrEmpty(DIR) AndAlso FileIO.FileSystem.DirectoryExists(DIR)
    End Function

    ''' <summary>
    ''' Get the directory its name of the target <paramref name="dir"/> directory
    ''' </summary>
    ''' <param name="dir$"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function DirectoryName(dir$) As String
        Return dir.TrimDIR _
            .Split("\"c).Last _
            .Split("/"c).Last
    End Function

    ''' <summary>
    ''' Check if the file is opened by other code?(检测文件是否已经被其他程序打开使用之中)
    ''' </summary>
    ''' <param name="FileName">目标文件</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("File.IsOpened")>
    <Extension> Public Function FileOpened(FileName As String) As Boolean
        Try
            Using FileOpenDetect As New FileStream(
                path:=FileName,
                mode:=FileMode.Open,
                access:=FileAccess.Read,
                share:=FileShare.None
            )
                ' Just detects this file is occupied, no more things needs to do....
            End Using

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Gets the name of the target file or directory, if the target is a file, then the name without 
    ''' the extension suffix name.
    ''' (获取目标文件夹的名称或者文件的不包含拓展名的名称)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' ###### 2017-2-14 
    ''' 
    ''' 原先的函数是依靠系统的API来工作的，故而只能够工作于已经存在的文件之上，
    ''' 所以在这里为了更加方便的兼容文件夹或者文件路径，在这使用字符串的方法来
    ''' 进行截取
    ''' </remarks>
    <ExportAPI(NameOf(BaseName))>
    <Extension> Public Function BaseName(fsObj$, Optional allowEmpty As Boolean = False) As String
        If fsObj.StringEmpty Then
            If allowEmpty Then
                Return ""
            Else
                Throw New NullReferenceException(NameOf(fsObj) & " file system object handle is null!")
            End If
        End If

        ' 前面的代码已经处理好了空字符串的情况了，在这里不会出现空字符串的错误
        Dim t$() = fsObj.Trim("\"c, "/"c) _
                        .Replace("\", "/") _
                        .Split("/"c) _
                        .Last _
                        .Split("."c)

        If t.Length = 2 AndAlso t(Scan0) = "" Then
            ' 处理.vs之类的隐藏文件夹
            Return t.JoinBy(".")
        ElseIf t.Length > 1 Then
            ' 文件名之中并没有包含有拓展名后缀，则数组长度为1，则不跳过了
            ' 有后缀拓展名，则split之后肯定会长度大于1的
            t = t.Take(t.Length - 1) _
                 .ToArray
        End If

        Dim name = String.Join(".", t)
        Return name
    End Function

    ''' <summary>
    ''' Gets the name of the file's parent directory, returns value is a name, not path.
    ''' (获取目标文件的父文件夹的文件夹名称，是名称而非路径)
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <Extension> Public Function ParentDirName(file As String) As String
        Dim parentDir As String = FileIO.FileSystem.GetParentPath(file)
        Dim parDirInfo = FileIO.FileSystem.GetDirectoryInfo(parentDir)
        Return parDirInfo.Name
    End Function

    ''' <summary>
    ''' Returns the directory path value of the parent directory.
    ''' (这个函数是返回文件夹的路径而非名称，这个函数不依赖于系统的底层API，
    ''' 因为系统的底层API对于过长的文件名会出错)
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>这个函数不依赖于系统的底层API，因为系统的底层API对于过长的文件名会出错</remarks>
    <ExportAPI(NameOf(ParentPath))>
    <Extension>
    Public Function ParentPath(file$, Optional full As Boolean = True) As String
        Dim UNCprefix As String = file.Match("\\\\\d+(\.\d+)+")
        Dim isUNCpath As Boolean = (Not String.IsNullOrEmpty(UNCprefix)) AndAlso file.StartsWith(UNCprefix)

        ' Console.WriteLine(UNCprefix)

        file = file.Replace("\", "/")

        Dim parent As String = ""
        Dim t As String() = file.Split("/"c)

        If full Then
            If InStr(file, "../") = 1 Then
                parent = FileIO.FileSystem.GetParentPath(App.CurrentDirectory)
                t = t.Skip(1).ToArray
                parent &= "/"
            ElseIf InStr(file, "./") = 1 Then
                parent = App.CurrentDirectory
                t = t.Skip(1).ToArray
                parent &= "/"
            Else

            End If

            If file.Last = "/"c Then ' 是一个文件夹
                parent &= String.Join("/", t.Take(t.Length - 2).ToArray)
            Else
                parent &= String.Join("/", t.Take(t.Length - 1).ToArray)
            End If

            If parent.StringEmpty Then
                ' 用户直接输入了一个文件名，没有包含文件夹部分，则默认是当前的文件夹
                parent = App.CurrentDirectory
            End If
        Else
            parent = String.Join("/", t.Take(t.Length - 1).ToArray)
        End If

        If isUNCpath Then
            Return parent.Replace("/", "\")
        Else
            Return parent
        End If
    End Function

    ''' <summary>
    ''' Get the specific file system object its relative path to the application base directory.
    ''' 
    ''' (获取相对于本应用程序的目标文件的相对路径(请注意，所生成的相对路径之中的字符串最后是没有文件夹的分隔符\或者/的))
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI(NameOf(RelativePath))>
    Public Function RelativePath(path As String) As String
        Return RelativePath(App.HOME, path)
    End Function

    ''' <summary>
    ''' Gets the relative path of file system object <paramref name="pcTo"/> reference to the directory path <paramref name="pcFrom"/>.
    ''' (请注意，所生成的相对路径之中的字符串最后是没有文件夹的分隔符\或者/的)
    ''' </summary>
    ''' <param name="pcFrom">生成相对路径的参考文件夹</param>
    ''' <param name="pcTo">所需要生成相对路径的目标文件系统对象的绝对路径或者相对路径</param>
    ''' <param name="appendParent">是否将父目录的路径也添加进入相对路径之中？默认是</param>
    ''' <returns></returns>
    <ExportAPI(NameOf(RelativePath))>
    Public Function RelativePath(pcFrom$, pcTo$,
                                 Optional appendParent As Boolean = True,
                                 Optional fixZipPath As Boolean = False) As <FunctionReturns("The relative path string of pcTo file object reference to directory pcFrom")> String

        Dim lcRelativePath As String = Nothing
        Dim lcFrom As String = (If(pcFrom Is Nothing, "", pcFrom.Trim().Replace("\", "/")))
        Dim lcTo As String = (If(pcTo Is Nothing, "", pcTo.Trim().Replace("\", "/")))

        If lcFrom.Length = 0 OrElse lcTo.Length = 0 Then
            Throw New InvalidDataException("One of the path string value is null!")
        End If
        If Not IO.Path.GetPathRoot(lcFrom.ToUpper()) _
            .Equals(IO.Path.GetPathRoot(lcTo.ToUpper())) Then
            Return pcTo
        End If

        ' 两个路径都有值并且都在相同的驱动器下才会进行计算

        Dim laDirSep As Char() = {"\"c}
        Dim lcPathFrom As String = (If(IO.Path.GetDirectoryName(lcFrom) Is Nothing, IO.Path.GetPathRoot(lcFrom.ToUpper()), IO.Path.GetDirectoryName(lcFrom)))
        Dim lcPathTo As String = (If(IO.Path.GetDirectoryName(lcTo) Is Nothing, IO.Path.GetPathRoot(lcTo.ToUpper()), IO.Path.GetDirectoryName(lcTo)))
        Dim lcFileTo As String = (If(IO.Path.GetFileName(lcTo) Is Nothing, "", IO.Path.GetFileName(lcTo)))
        Dim laFrom As String() = lcPathFrom.Split(laDirSep)
        Dim laTo As String() = lcPathTo.Split(laDirSep)
        Dim lnFromCnt As Integer = laFrom.Length
        Dim lnToCnt As Integer = laTo.Length
        Dim lnSame As Integer = 0
        Dim lnCount As Integer = 0

        While lnToCnt > 0 AndAlso lnSame < lnToCnt
            If lnCount < lnFromCnt Then
                If laFrom(lnCount).ToUpper().Equals(laTo(lnCount).ToUpper()) Then
                    lnSame += 1
                Else
                    Exit While
                End If
            Else
                Exit While
            End If
            lnCount += 1
        End While

        Dim lcEndPart As String = ""
        For lnEnd As Integer = lnSame To lnToCnt - 1
            If laTo(lnEnd).Length > 0 Then
                lcEndPart += laTo(lnEnd) & "\"
            Else
                Exit For
            End If
        Next

        Dim lnDiff As Integer = Abs(lnFromCnt - lnSame)
        If lnDiff > 0 AndAlso laFrom(lnFromCnt - 1).Length > 0 Then
            While lnDiff > 0
                lnDiff -= 1
                lcEndPart = "..\" & lcEndPart
            End While
        End If

        lcRelativePath = lcEndPart & lcFileTo

        If appendParent Then
            Return "..\" & lcRelativePath
        Else
            If fixZipPath Then
                ' 2017-8-26
                ' 为Xlsx打包模块进行的修复
                Return lcRelativePath _
                    .Split("\"c) _
                    .Skip(1) _
                    .JoinBy("\")
            Else
                Return lcRelativePath
            End If
        End If
    End Function

    ''' <summary>
    ''' Gets the full path of the specific file.(为了兼容Linux，这个函数会自动替换路径之中的\为/符号)
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("File.FullPath")>
    <Extension>
    Public Function GetFullPath(file As String) As String
        Dim fullName As String = FileIO.FileSystem.GetFileInfo(file).FullName
        Dim UNCprefix As String = fullName.Match("\\\\\d+(\.\d+)+")

        If (Not UNCprefix.StringEmpty) AndAlso fullName.StartsWith(UNCprefix) Then
            ' is a network location on NAS server
            Return fullName
        Else
            Return fullName.Replace("\", "/")
        End If
    End Function

    ''' <summary>
    ''' Gets the full path of the specific directory. 
    ''' (这个函数为了兼容linux的文件系统，也会自动的将所有的``\``替换为``/``)
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="stack">当程序出错误的时候记录进入日志的一个追踪目标参数，调试用</param>
    ''' <returns></returns>
    <ExportAPI("Dir.FullPath")>
    <Extension>
    Public Function GetDirectoryFullPath(dir$, <CallerMemberName> Optional stack$ = Nothing) As String
        Try
            Dim dirInfo As New DirectoryInfo(dir)
            Dim UNCprefix As String = dirInfo.FullName.Match("\\\\\d+(\.\d+)+")
            Dim fullName As String = dirInfo.FullName.Replace("\", "/")

            If (Not UNCprefix.StringEmpty) AndAlso dirInfo.FullName.StartsWith(UNCprefix) Then
                Return dirInfo.FullName
            Else
                Return fullName
            End If
        Catch ex As Exception
            stack = stack & " --> " & NameOf(GetDirectoryFullPath)

            If dir = "/" AndAlso Not App.IsMicrosoftPlatform Then
                Return "/"  ' Linux上面已经是全路径了，root
            Else
                ex = New Exception(stack & ": " & dir, ex)
                Call App.LogException(ex)
                Call ex.PrintException
                Return dir
            End If
        End Try
    End Function

    ''' <summary>
    ''' Removes the file extension name from the file path.(去除掉文件的拓展名)
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("File.Ext.Trim")>
    <Extension> Public Function TrimSuffix(file As String) As String
        Dim tokens$() = file.Replace("\"c, "/"c).Split("/"c)
        Dim fileNameTokens = tokens.Last.Split("."c)
        Dim directory = tokens.Take(tokens.Length - 1).JoinBy("/")
        Dim fileName$ = fileNameTokens.Take(fileNameTokens.Length - 1).JoinBy(".")

        Return $"{directory}/{fileName}"
    End Function

    ''' <summary>
    ''' Removes the last \ and / character in a directory path string.
    ''' (使用这个函数修剪文件夹路径之中的最后一个分隔符，以方便生成文件名)
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function TrimDIR(DIR As String) As String
        Return DIR.TrimEnd("/"c, "\"c)
    End Function

    ''' <summary>
    ''' 返回``文件名称.拓展名``
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数为单纯的字符串解析函数，不依赖于文件系统的API
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("File.Name")>
    <Extension>
    Public Function FileName(path As String) As String
        If path.StringEmpty Then
            Return ""
        Else
            Return path.StringSplit("(\\|/)").Last
        End If
    End Function

    ''' <summary>
    ''' 进行安全的复制，出现错误不会导致应用程序崩溃，大文件不推荐使用这个函数进行复制
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="copyTo"></param>
    ''' <returns></returns>
    <ExportAPI("SafeCopyTo")>
    Public Function SafeCopyTo(source As String, copyTo As String) As Boolean
        Try
            Dim buf As Byte() = IO.File.ReadAllBytes(source)
            Call buf.FlushStream(copyTo)
        Catch ex As Exception
            Dim pt As String = $"{source.ToFileURL} ===> {copyTo.ToFileURL}"
            Call App.LogException(New Exception(pt, ex))
            Return False
        End Try

        Return True
    End Function
End Module
