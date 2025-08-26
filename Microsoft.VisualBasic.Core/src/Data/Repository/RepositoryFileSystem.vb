#Region "Microsoft.VisualBasic::28bd9261a4e8c097332e7c9484557bb6, Microsoft.VisualBasic.Core\src\Data\Repository\RepositoryFileSystem.vb"

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

    '   Total Lines: 221
    '    Code Lines: 139 (62.90%)
    ' Comment Lines: 56 (25.34%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 26 (11.76%)
    '     File Size: 10.20 KB


    '     Module RepositoryFileSystem
    ' 
    '         Function: GetFile, GetMostAppreancePath, LoadEntryList, (+3 Overloads) LoadSourceEntryList, SourceCopy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq.JoinExtensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Data.Repository

    ''' <summary>
    ''' Using file system as repository system provider
    ''' </summary>
    Public Module RepositoryFileSystem

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

            Dim Files As IEnumerable(Of String) = ls - l - wildcards(ext) <= DIR
            Dim matches = (From Path As String
                           In Files.AsParallel
                           Let NameID = BaseName(Path)
                           Where InStr(NameID, keyword, CompareMethod.Text) > 0
                           Let ExtValue = Path.Split("."c).Last
                           Select Path,
                               ExtValue)
            Dim LQuery =
                From extType As String
                In ext
                Select From path
                       In matches
                       Where InStr(extType, path.ExtValue, CompareMethod.Text) > 0
                       Select path.Path

            Return LQuery.IteratesALL.Distinct.ToArray
        End Function

        ''' <summary>
        ''' Load the file from a specific directory from the source parameter as the resource entry list.
        ''' 
        ''' [<see cref="FileIO.SearchOption.SearchAllSubDirectories"/>，这个函数会扫描目标文件夹下面的所有文件。]
        ''' 请注意，本方法是不能够产生具有相同的主文件名的数据的。假若目标GBK是使用本模块之中的方法保存或者导出来的，
        ''' 则可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Load.ResourceEntry")>
        <Extension>
        Public Function LoadSourceEntryList(<Parameter("Dir.Source", "The source directory which will be searchs for file.")> source As String,
                                            <Parameter("List.Ext", "The list of the file extension.")> ext As String(),
                                            Optional topLevel As Boolean = True) As Dictionary(Of String, String)

            If ext.IsNullOrEmpty Then
                ext = {"*.*"}
            End If

            Dim LQuery = (From path As String
                          In If(topLevel, ls - l, ls - l - r) - wildcards(ext) <= source
                          Select ID = BaseName(path),
                              path
                          Group By ID Into Group).ToArray

            ext = LinqAPI.Exec(Of String) <= From value As String
                                             In ext
                                             Select value.Split(CChar(".")).Last.ToLower

            Dim res As Dictionary(Of String, String) = LQuery _
                .ToDictionary(Function(x) x.ID,
                              Function(x)

                                  Return LinqAPI.DefaultFirst(Of String) _
                                                                         _
                                    () <= From path
                                          In x.Group
                                          Let pathValue = path.path
                                          Let extValue As String = pathValue.Split("."c).Last.ToLower
                                          Where Array.IndexOf(ext, extValue) > -1
                                          Select pathValue

                              End Function)

            With From entry
                 In res
                 Where Not String.IsNullOrEmpty(entry.Value)
                 Select entry

                res = .ToDictionary(Function(x) x.Key,
                                    Function(x) x.Value)
            End With

            Call $"{NameOf(ProgramPathSearchTool)} load {res.Count} source entry...".debug

            Return res
        End Function

        ''' <summary>
        ''' 可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
        ''' 请注意，这个函数会搜索目标文件夹下面的所有的文件夹的
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="ext">文件类型的拓展名称</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function LoadSourceEntryList(source$, ParamArray ext$()) As Dictionary(Of String, String)
            If Not FileIO.FileSystem.DirectoryExists(source) Then
                Return New Dictionary(Of String, String)
            End If

            Dim LQuery = From path As String
                         In FileIO.FileSystem.GetFiles(source, FileIO.SearchOption.SearchAllSubDirectories, ext)
                         Select ID = BaseName(path),
                              path
                         Group By ID Into Group
            Dim dict As Dictionary(Of String, String) =
                LQuery.ToDictionary(Function(x) x.ID,
                                    Function(x) x.Group.First.path)
            Return dict
        End Function

        ''' <summary>
        ''' 允许有重复的数据
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <param name="exts"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Load.ResourceEntry")>
        <Extension> Public Function LoadEntryList(<Parameter("Dir.Source")> DIR$, ParamArray exts$()) As NamedValue(Of String)()
            Return LinqAPI.Exec(Of NamedValue(Of String)) _
                                                          _
                () <= From path As String
                      In ls - l - ShellSyntax.r - wildcards(exts) <= DIR
                      Select New NamedValue(Of String) With {
                          .Name = path.BaseName,
                          .Value = path
                      }

        End Function

        ''' <summary>
        ''' Load the file from a specific directory from the source parameter as the resource entry list.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <ExportAPI("Load.ResourceEntry")>
        <Extension>
        Public Function LoadSourceEntryList(source As IEnumerable(Of String)) As Dictionary(Of String, String)
            Dim LQuery = From path As String
                         In source
                         Select ID = BaseName(path),
                             path
                         Group By ID Into Group
            Dim res As Dictionary(Of String, String) =
                LQuery.ToDictionary(Function(x) x.ID,
                                    Function(x) x.Group.First.path)
            Return res
        End Function

        ''' <summary>
        ''' Copy the file in the source list into the copyto directory, function returns the failed operation list.
        ''' (将不同来源<paramref name="source"></paramref>的文件复制到目标文件夹<paramref name="copyto"></paramref>之中)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="copyto"></param>
        ''' <returns>返回失败的文件列表</returns>
        ''' <remarks></remarks>
        <ExportAPI("Source.Copy")>
        Public Function SourceCopy(source As IEnumerable(Of String), CopyTo As String, Optional [Overrides] As Boolean = False) As String()
            Dim failedList As New List(Of String)

            For Each file As String In source
                Try
                    Call FileIO.FileSystem.CopyFile(file, CopyTo & "/" & FileIO.FileSystem.GetFileInfo(file).Name, [Overrides])
                Catch ex As Exception
                    Call failedList.Add(file)
                    Call App.LogException(New Exception(file, ex))
                End Try
            Next

            Return failedList.ToArray
        End Function

        ''' <summary>
        ''' Gets a directory path which is most frequent appeared in the file list.
        ''' </summary>
        ''' <param name="files"></param>
        ''' <returns></returns>
        <ExportAPI("Get.FrequentPath")>
        Public Function GetMostAppreancePath(files As IEnumerable(Of String)) As String
            If files Is Nothing Then
                Return ""
            End If

            Dim LQuery = From strPath As String
                         In files
                         Select FileIO.FileSystem.GetParentPath(strPath)
            Return LQuery _
                .TokenCount(ignoreCase:=True) _
                .OrderByDescending(Function(x) x.Value) _
                .FirstOrDefault _
                .Key
        End Function
    End Module
End Namespace
