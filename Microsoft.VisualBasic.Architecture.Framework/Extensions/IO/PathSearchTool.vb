Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Reflection
Imports System.Collections.ObjectModel

''' <summary>
''' Search the path from a specific keyword.(通过关键词来推测路径)
''' </summary>
''' <remarks></remarks>
'''
<[Namespace]("Program.Path.Search", Description:="A utility tools for searching a specific file of its path on the file system more easily.")>
Public Module ProgramPathSearchTool

    <Extension>
    Public Iterator Function EnumerateFiles(DIR As String, ParamArray keyword As String()) As IEnumerable(Of String)
        Dim files = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, keyword)

        For Each file As String In files
            Yield file
        Next
    End Function

    ''' <summary>
    ''' Gets the URL type file path.(获取URL类型的文件路径)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Path2Url", Info:="Gets the URL type file path.")>
    <Extension> Public Function ToFileURL(Path As String) As String
        If String.IsNullOrEmpty(Path) Then
            Return ""
        Else
            Path = FileIO.FileSystem.GetFileInfo(Path).FullName
            Return String.Format("file:///{0}", Path.Replace("\", "/"))
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
    Public Const ILLEGAL_PATH_CHARACTERS_ENUMERATION As String = ":*?""<>|"
    Public Const ILLEGAL_FILENAME_CHARACTERS As String = "\/" & ILLEGAL_PATH_CHARACTERS_ENUMERATION

    ''' <summary>
    ''' 将目标字符串之中的非法的字符替换为"_"符号以成为正确的文件名字符串
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="OnlyASCII">当本参数为真的时候，仅26个字母，0-9数字和下划线_以及小数点可以被保留下来</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("NormalizePathString")>
    <Extension> Public Function NormalizePathString(str As String, Optional OnlyASCII As Boolean = True) As String
        Return NormalizePathString(str, "_", OnlyASCII)
    End Function

    <ExportAPI("NormalizePathString")>
    <Extension> Public Function NormalizePathString(str As String, normAs As String, Optional onlyASCII As Boolean = True) As String
        Dim sBuilder As StringBuilder = New StringBuilder(str)
        For Each ch As Char In ILLEGAL_FILENAME_CHARACTERS
            Call sBuilder.Replace(ch, normAs)
        Next

        If onlyASCII Then
            For Each ch As Char In "()[]+-~!@#$%^&=;',"
                Call sBuilder.Replace(ch, normAs)
            Next
        End If

        Return sBuilder.ToString
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
        Dim DIRname As String = DIRTokens.Last  ' 请注意，由于path参数可能是相对路径，所以在这里DIRname和name要分开讨论
        Dim name As String = path.Replace("\", "/").Split("/"c).Last  ' 因为相对路径最终会出现文件夹名称，但在path里面可能是使用小数点来表示的
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

            Return parent
        Else
            Return FileIO.FileSystem.GetFileInfo(path).FullName
        End If
    End Function

    ''' <summary>
    ''' File path illegal?
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <ExportAPI("Path.Illegal?")>
    <Extension> Public Function PathIllegal(path As String) As Boolean
        Dim Tokens As String() = path.Replace("\", "/").Split("/"c)
        Dim fileName As String = Tokens.Last

        For Each ch As Char In ILLEGAL_PATH_CHARACTERS_ENUMERATION
            If fileName.IndexOf(ch) > -1 Then
                Return True
            End If
        Next

        For Each DIRBase As String In Tokens.Takes(Tokens.Length - 1)
            For Each ch As Char In ILLEGAL_PATH_CHARACTERS_ENUMERATION
                If fileName.IndexOf(ch) > -1 Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function

    ''' <summary>
    ''' Check if the target file object is exists on your file system or not.(这个函数也会自动检查目标<paramref name="path"/>参数是否为空)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
#If FRAMEWORD_CORE Then
    <ExportAPI("File.Exists", Info:="Check if the target file object is exists on your file system or not.")>
    <Extension> Public Function FileExists(path As String) As Boolean
#Else
    <Extension> Public Function FileExists(path As String) As Boolean
#End If
        Return Not String.IsNullOrEmpty(path) AndAlso FileIO.FileSystem.FileExists(path)
    End Function

    ''' <summary>
    ''' Determine that the target directory is exists on the file system or not?(判断文件夹是否存在)
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns></returns>
    <ExportAPI("DIR.Exists", Info:="Determine that the target directory is exists on the file system or not?")>
    <Extension>
    Public Function DirectoryExists(DIR As String) As Boolean
        Return Not String.IsNullOrEmpty(DIR) AndAlso
            FileIO.FileSystem.DirectoryExists(DIR)
    End Function

    ''' <summary>
    ''' Check if the file is opened by other code?(检测文件是否已经被其他程序打开使用之中)
    ''' </summary>
    ''' <param name="FileName">目标文件</param>
    ''' <returns></returns>
    <ExportAPI("File.IsOpened", Info:="Detect while the target file is opened by someone process.")>
    <Extension> Public Function FileOpened(FileName As String) As Boolean
        Try
            Using FileOpenDetect As New FileStream(FileName,
                                                   IO.FileMode.Open,
                                                   IO.FileAccess.Read,
                                                   IO.FileShare.None)
                ' Just detects this file is occupied, no more things needs to do....
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Gets the name of the target file or directory, if the target is a file, then the name without the extension name.
    ''' (获取目标文件夹的名称或者文件的不包含拓展名的名称)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI(NameOf(BaseName), Info:="Gets the name of the target directory/file object.")>
    <Extension> Public Function BaseName(fsObj As String) As String
        If fsObj.FileExists Then
            Return IO.Path.GetFileNameWithoutExtension(fsObj)
        Else
            Return FileIO.FileSystem.GetDirectoryInfo(fsObj).Name
        End If
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
    ''' 这个函数是返回文件夹的路径而非名称，这个函数不依赖于系统的底层API，因为系统的底层API对于过长的文件名会出错
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>这个函数不依赖于系统的底层API，因为系统的底层API对于过长的文件名会出错</remarks>
    <ExportAPI(NameOf(ParentPath))>
    <Extension> Public Function ParentPath(file As String) As String
        Dim Parent As String = ""
        Dim Tokens As String() = file.Replace("\", "/").ShadowCopy(file).Split("/"c)

        If InStr(file, "../") = 1 Then
            Parent = FileIO.FileSystem.GetParentPath(App.CurrentWork)
            Tokens = Tokens.Skip(1).ToArray
            Parent &= "/"
        ElseIf InStr(file, "./") = 1 Then
            Parent = App.CurrentWork
            Tokens = Tokens.Skip(1).ToArray
            Parent &= "/"
        Else

        End If

        If file.Last = "/"c Then ' 是一个文件夹
            Parent &= String.Join("/", Tokens.Take(Tokens.Length - 2).ToArray)
        Else
            Parent &= String.Join("/", Tokens.Take(Tokens.Length - 1).ToArray)
        End If

        Return Parent
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <param name="keyword"></param>
    ''' <param name="ext">元素的排布是有顺序的</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Get.File.Path")>
    <Extension> Public Function GetFile(DIR As String,
                                       <Parameter("Using.Keyword")> keyword As String,
                                       <Parameter("List.Ext")> ParamArray ext As String()) _
                                    As <FunctionReturns("A list of file path which match with the keyword and the file extension name.")> String()

        Dim Files As IEnumerable(Of String) = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, ext)
        Dim matches = (From Path As String
                       In Files.AsParallel
                       Let NameID = IO.Path.GetFileNameWithoutExtension(Path)
                       Where InStr(NameID, keyword, CompareMethod.Text) > 0
                       Let ExtValue = Path.Split("."c).Last
                       Select Path,
                           ExtValue)
        Dim LQuery = (From ExtType As String
                      In ext
                      Select (From path In matches
                              Where InStr(ExtType, path.ExtValue, CompareMethod.Text) > 0
                              Select path.Path).ToArray).MatrixAsIterator.Distinct.ToArray
        Return LQuery
    End Function

    <ExportAPI("Md5.Renamed")>
    Public Function BatchMd5Renamed(dir As String) As Boolean
        dir = FileIO.FileSystem.GetDirectoryInfo(dir).FullName

        For Each path As String In FileIO.FileSystem.GetFiles(dir)
            On Error Resume Next

            Dim Md5 As String = SecurityString.GetMd5Hash(path)
            Dim ext As String = IO.Path.GetExtension(path)
            Dim FileName As String = dir & "/" & Md5 & ext

            Call IO.File.Move(path, FileName)
        Next

        Return True
    End Function

    ''' <summary>
    ''' [<see cref="FileIO.SearchOption.SearchAllSubDirectories"/>，这个函数会扫描目标文件夹下面的所有文件。]
    ''' 请注意，本方法是不能够产生具有相同的主文件名的数据的。假若目标GBK是使用本模块之中的方法保存或者导出来的，
    ''' 则可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Load.ResourceEntry",
               Info:="Load the file from a specific directory from the source parameter as the resource entry list.")>
    <Extension>
    Public Function LoadSourceEntryList(<Parameter("Dir.Source", "The source directory which will be searchs for file.")> source As String,
                                        <Parameter("List.Ext", "The list of the file extension.")> ext As String(),
                                        Optional TopLevel As Boolean = True) As Dictionary(Of String, String)

        If ext.IsNullOrEmpty Then
            ext = {"*.*"}
        End If

        Dim LQuery = (From path As String
                      In FileIO.FileSystem.GetFiles(source, If(TopLevel, FileIO.SearchOption.SearchTopLevelOnly, FileIO.SearchOption.SearchAllSubDirectories), ext).AsParallel
                      Select ID = IO.Path.GetFileNameWithoutExtension(path), path
                      Group By ID Into Group).ToArray
        ext = (From value As String In ext Select value.Split(CChar(".")).Last.ToLower).ToArray
        Dim Dict As Dictionary(Of String, String) = LQuery.ToDictionary(Function(Entry) Entry.ID,
                                                                        Function(Entry) (From path In Entry.Group.ToArray
                                                                                         Let extValue As String = path.path.Split("."c).Last.ToLower
                                                                                         Where Array.IndexOf(ext, extValue) > -1
                                                                                         Select path.path).FirstOrDefault)
        Dict = (From Entry
                In Dict
                Where Not String.IsNullOrEmpty(Entry.Value)
                Select Entry).ToDictionary(Function(obj) obj.Key, elementSelector:=Function(obj) obj.Value)

        Call Console.WriteLine($"[DEBUG {Now.ToString}] {NameOf(ProgramPathSearchTool)} load {Dict.Count} source entry...")

        Return Dict
    End Function

    ''' <summary>
    ''' 可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
    ''' 请注意，这个函数会搜索目标文件夹下面的所有的文件夹的
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="ext">文件类型的拓展名称</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function LoadSourceEntryList(source As String, ParamArray ext As String()) As Dictionary(Of String, String)
        If Not FileIO.FileSystem.DirectoryExists(source) Then
            Return New Dictionary(Of String, String)
        End If

        Dim LQuery = (From path As String
                      In FileIO.FileSystem.GetFiles(source, FileIO.SearchOption.SearchAllSubDirectories, ext)
                      Select ID = IO.Path.GetFileNameWithoutExtension(path),
                          path
                      Group By ID Into Group)
        Dim dict As Dictionary(Of String, String) =
            LQuery.ToDictionary(Function(item) item.ID,
                                Function(item) item.Group.First.path)
        Return dict
    End Function

    ''' <summary>
    ''' 允许有重复的数据
    ''' </summary>
    ''' <param name="Source"></param>
    ''' <param name="Ext"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Load.ResourceEntry")>
    <Extension> Public Function LoadEntryList(<Parameter("Dir.Source")> Source As String, ParamArray Ext As String()) As KeyValuePair(Of String, String)()
        Dim LQuery = (From path As String
                      In FileIO.FileSystem.GetFiles(Source, FileIO.SearchOption.SearchAllSubDirectories, Ext)
                      Select New KeyValuePair(Of String, String)(IO.Path.GetFileNameWithoutExtension(path), path)).ToArray
        Return LQuery
    End Function

    <ExportAPI("Load.ResourceEntry", Info:="Load the file from a specific directory from the source parameter as the resource entry list.")>
    <Extension>
    Public Function LoadSourceEntryList(source As IEnumerable(Of String)) As Dictionary(Of String, String)
        Dim LQuery = (From path As String
                      In source
                      Select ID = IO.Path.GetFileNameWithoutExtension(path), path
                      Group By ID Into Group)
        Dim PathDict As Dictionary(Of String, String) = LQuery.ToDictionary(Function(item) item.ID, Function(item) item.Group.First.path)
        Return PathDict
    End Function

    ''' <summary>
    ''' 将不同来源<paramref name="source"></paramref>的文件复制到目标文件夹<paramref name="copyto"></paramref>之中
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="copyto"></param>
    ''' <returns>返回失败的文件列表</returns>
    ''' <remarks></remarks>
    <ExportAPI("Source.Copy", Info:="Copy the file in the source list into the copyto directory, function returns the failed operation list.")>
    Public Function SourceCopy(Source As Generic.IEnumerable(Of String), CopyTo As String, Optional [Overrides] As Boolean = False) As String()
        Dim FailedList As List(Of String) = New List(Of String)

        For Each File As String In Source
            Try
                Call FileIO.FileSystem.CopyFile(File, CopyTo & "/" & FileIO.FileSystem.GetFileInfo(File).Name, [Overrides])
            Catch ex As Exception
                Call FailedList.Add(File)
            End Try
        Next

        Return FailedList.ToArray
    End Function

    <ExportAPI("Get.FrequentPath", Info:="Gets a directory path which is most frequent appeared in the file list.")>
    Public Function GetMostAppreancePath(FileList As Generic.IEnumerable(Of String)) As String
        If FileList.IsNullOrEmpty Then
            Return ""
        End If

        Dim LQuery = (From strPath As String In FileList Select FileIO.FileSystem.GetParentPath(strPath)).ToArray
        Return LQuery.CountStringTokens(IgnoreCase:=True).First.Key
    End Function

    ''' <summary>
    ''' Invoke the search session for the program file using a specific keyword string value.(使用某个关键词来搜索目标应用程序)
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <param name="Keyword"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("File.Search.Program", Info:="Invoke the search session for the program file using a specific keyword string value.")>
    Public Function SearchProgram(DIR As String, Keyword As String) As String()
        Dim ExeNameRule As String = String.Format("*{0}*.exe", Keyword)
        Dim DllNameRule As String = String.Format("*{0}*.dll", Keyword)

        Dim Files = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly, ExeNameRule, DllNameRule)
        Dim binDIR As String = String.Format("{0}/bin/", DIR)
        Dim ProgramDIR As String = String.Format("{0}/Program", DIR)
        Dim buffer As List(Of String) = New List(Of String)

        If FileIO.FileSystem.DirectoryExists(binDIR) Then
            buffer += FileIO.FileSystem.GetFiles(binDIR, FileIO.SearchOption.SearchTopLevelOnly, ExeNameRule, DllNameRule)
        End If
        If FileIO.FileSystem.DirectoryExists(ProgramDIR) Then
            buffer += FileIO.FileSystem.GetFiles(ProgramDIR, FileIO.SearchOption.SearchTopLevelOnly, ExeNameRule, DllNameRule)
        End If

        buffer += Files

        Return buffer.ToArray
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
        Dim ChunkList As List(Of String) = New List(Of String)

        If FileIO.FileSystem.DirectoryExists(binDIR) Then Call ChunkList.AddRange(FileIO.FileSystem.GetFiles(binDIR, FileIO.SearchOption.SearchTopLevelOnly, ScriptFileNameRule))
        If FileIO.FileSystem.DirectoryExists(ProgramDIR) Then Call ChunkList.AddRange(FileIO.FileSystem.GetFiles(ProgramDIR, FileIO.SearchOption.SearchTopLevelOnly, ScriptFileNameRule))
        If FileIO.FileSystem.DirectoryExists(ScriptsDIR) Then Call ChunkList.AddRange(FileIO.FileSystem.GetFiles(ScriptsDIR, FileIO.SearchOption.SearchTopLevelOnly, ScriptFileNameRule))

        Call ChunkList.AddRange(Files)

        If String.IsNullOrEmpty(withExtension) Then
            Return (From strPath As String In ChunkList Where String.IsNullOrEmpty(FileIO.FileSystem.GetFileInfo(strPath).Extension) Select strPath).ToArray
        Else
            Return ChunkList.ToArray
        End If
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="SpecificDrive">所制定进行搜索的驱动器，假若希望搜索整个硬盘，请留空字符串</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Dir.Search.Program_Directory", Info:="Search for the directories which its name was matched the keyword pattern.")>
    Public Function SearchDirectory(Keyword As String, SpecificDrive As String) As String()
        Dim Drives As ObjectModel.ReadOnlyCollection(Of DriveInfo) =
            If(String.IsNullOrEmpty(SpecificDrive),
               FileIO.FileSystem.Drives,
               New ReadOnlyCollection(Of IO.DriveInfo)(
                   {FileIO.FileSystem.GetDriveInfo(SpecificDrive)}))
        Dim DIRs As List(Of String) = New List(Of String)

        For Each Drive As DriveInfo In Drives
            Call DIRs.AddRange(SearchDrive(Drive, Keyword))
        Next

        Return DIRs.ToArray
    End Function

    Private Function SearchDrive(Drive As IO.DriveInfo, Keyword As String) As String()
        If Not Drive.IsReady Then
            Return New String() {}
        End If

        Dim DriveRoot = FileIO.FileSystem.GetDirectories(Drive.RootDirectory.FullName, FileIO.SearchOption.SearchTopLevelOnly, Keyword)
        Dim ChunkList As List(Of String) = New List(Of String)

        Dim ProgramFiles As String = String.Format("{0}/Program Files", Drive.RootDirectory.FullName)
        If FileIO.FileSystem.DirectoryExists(ProgramFiles) Then
            Call ChunkList.AddRange(BranchRule(ProgramFiles, Keyword))
        End If

        Dim ProgramFilesX86 = String.Format("{0}/Program Files(x86)", Drive.RootDirectory.FullName)
        If FileIO.FileSystem.DirectoryExists(ProgramFilesX86) Then
            Call ChunkList.AddRange(BranchRule(ProgramFilesX86, Keyword))
        End If
        Call ChunkList.AddRange(DriveRoot)
        Call ChunkList.AddRange(DriveRoot.ToArray(Function(rootDir) BranchRule(rootDir, Keyword)).MatrixToList)

        Return ChunkList.ToArray
    End Function

    ''' <summary>
    ''' 商标搜索规则
    ''' </summary>
    ''' <param name="ProgramFiles"></param>
    ''' <param name="Keyword"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function BranchRule(ProgramFiles As String, Keyword As String) As String()
        Dim ProgramFiles_Directories = FileIO.FileSystem.GetDirectories(ProgramFiles, FileIO.SearchOption.SearchTopLevelOnly, Keyword)
        Dim ChunkList As List(Of String) = New List(Of String)

        For Each Dir As String In ProgramFiles_Directories
            Call ChunkList.AddRange(FileIO.FileSystem.GetDirectories(Dir, FileIO.SearchOption.SearchTopLevelOnly))
        Next
        Call ChunkList.AddRange(ProgramFiles_Directories)

        If ChunkList.Count = 0 Then
            ' 这个应用程序的安装文件夹可能是带有版本号标记的
            Dim Dirs = FileIO.FileSystem.GetDirectories(ProgramFiles, FileIO.SearchOption.SearchTopLevelOnly)
            Dim version As String = Keyword & ProgramPathSearchTool.VERSION
            Dim Patterns = (From dir As String
                            In Dirs
                            Let name As String = FileIO.FileSystem.GetDirectoryInfo(dir).Name
                            Where Regex.Match(name, version, RegexOptions.IgnoreCase).Success
                            Select dir).ToArray
            Call ChunkList.Add(Patterns)
        End If

        Return ChunkList.ToArray
    End Function

    Const VERSION As String = "[-_`~.]\d+(\.\d+)*"

    ''' <summary>
    ''' 获取相对于本应用程序的目标文件的相对路径(请注意，所生成的相对路径之中的字符串最后是没有文件夹的分隔符\或者/的)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    '''
    <ExportAPI(NameOf(RelativePath),
               Info:="Get the specific file system object its relative path to the application base directory.")>
    Public Function RelativePath(path As String) As String
        Return RelativePath(App.HOME, path)
    End Function

    ''' <summary>
    ''' Gets the relative path of file system object <paramref name="pcTo"/> reference to the directory path <paramref name="pcFrom"/>.
    ''' (请注意，所生成的相对路径之中的字符串最后是没有文件夹的分隔符\或者/的)
    ''' </summary>
    ''' <param name="pcFrom">生成相对路径的参考文件夹</param>
    ''' <param name="pcTo">所需要生成相对路径的目标文件系统对象的绝对路径或者相对路径</param>
    ''' <returns></returns>
    '''
    <ExportAPI(NameOf(RelativePath),
               Info:="Gets the relative path value of pcTo file system object relative to a reference directory pcFrom")>
    Public Function RelativePath(pcFrom As String, pcTo As String) _
        As <FunctionReturns("The relative path string of pcTo file object reference to directory pcFrom")> String

        Dim lcRelativePath As String = Nothing
        Dim lcFrom As String = (If(pcFrom Is Nothing, "", pcFrom.Trim()))
        Dim lcTo As String = (If(pcTo Is Nothing, "", pcTo.Trim()))

        If lcFrom.Length > 0 AndAlso lcTo.Length > 0 AndAlso Path.GetPathRoot(lcFrom.ToUpper()).Equals(Path.GetPathRoot(lcTo.ToUpper())) Then
            Dim laDirSep As Char() = {"\"c}
            Dim lcPathFrom As String = (If(Path.GetDirectoryName(lcFrom) Is Nothing, Path.GetPathRoot(lcFrom.ToUpper()), Path.GetDirectoryName(lcFrom)))
            Dim lcPathTo As String = (If(Path.GetDirectoryName(lcTo) Is Nothing, Path.GetPathRoot(lcTo.ToUpper()), Path.GetDirectoryName(lcTo)))
            Dim lcFileTo As String = (If(Path.GetFileName(lcTo) Is Nothing, "", Path.GetFileName(lcTo)))
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

            Dim lnDiff As Integer = Math.Abs(lnFromCnt - lnSame)
            If lnDiff > 0 AndAlso laFrom(lnFromCnt - 1).Length > 0 Then
                While lnDiff > 0
                    lnDiff -= 1
                    lcEndPart = "..\" & lcEndPart
                End While
            End If
            lcRelativePath = lcEndPart & lcFileTo
        End If

        Return "..\" & lcRelativePath
    End Function

    ''' <summary>
    ''' Gets the full path of the specific file.
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("File.FullPath", Info:="Gets the full path of the file.")>
    <Extension> Public Function GetFullPath(file As String) As String
        Return FileIO.FileSystem.GetFileInfo(file).FullName.Replace("\", "/")
    End Function

    ''' <summary>
    ''' Gets the full path of the specific directory.
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Dir.FullPath", Info:="Gets the full path of the directory.")>
    <Extension> Public Function GetDirectoryFullPath(dir As String) As String
        Return FileIO.FileSystem.GetDirectoryInfo(dir).FullName.Replace("\", "/")
    End Function

    ''' <summary>
    ''' 去除掉文件的拓展名
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("File.Ext.Trim")>
    <Extension> Public Function TrimFileExt(file As String) As String
        Dim fileInfo = FileIO.FileSystem.GetFileInfo(file)
        Dim Name As String = IO.Path.GetFileNameWithoutExtension(file)
        Return $"{fileInfo.Directory.FullName}/{Name}"
    End Function

    ''' <summary>
    ''' 只有文件名称，没有拓展名
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <ExportAPI("File.BaseName")>
    <Extension>
    Public Function GetJustFileName(path As String) As String
        Return IO.Path.GetFileNameWithoutExtension(path)
    End Function

    ''' <summary>
    ''' 进行安全的复制，出现错误不会导致应用程序崩溃
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="copyTo"></param>
    ''' <returns></returns>
    <ExportAPI("SafeCopyTo")>
    Public Function SafeCopyTo(source As String, copyTo As String) As Boolean
        Try
            Dim buf As Byte() = File.ReadAllBytes(source)
            Call buf.FlushStream(copyTo)
        Catch ex As Exception
            Call App.LogException(New Exception($"{source.ToFileURL} ===> {copyTo.ToFileURL}", ex))
            Return False
        End Try

        Return True
    End Function
End Module
