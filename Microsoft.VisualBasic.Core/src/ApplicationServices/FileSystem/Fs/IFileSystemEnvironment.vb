﻿#Region "Microsoft.VisualBasic::18c7a7220e2c89dfe6b1ce0e62b23f8e, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\Fs\IFileSystemEnvironment.vb"

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

    '   Total Lines: 98
    '    Code Lines: 21 (21.43%)
    ' Comment Lines: 68 (69.39%)
    '    - Xml Docs: 97.06%
    ' 
    '   Blank Lines: 9 (9.18%)
    '     File Size: 3.49 KB


    '     Interface IFileSystemEnvironment
    ' 
    '         Properties: [readonly]
    ' 
    '         Function: DeleteFile, FileExists, FileModifyTime, FileSize, GetFiles
    '                   GetFullPath, OpenFile, ReadAllText, WriteText
    ' 
    '         Sub: Close, Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Directory = Microsoft.VisualBasic.FileIO.Directory

Namespace ApplicationServices

    ''' <summary>
    ''' an abstract interface for union filesystem:
    ''' 
    ''' 1. local filesystem: <see cref="Directory"/>
    ''' 2. zip archive: <see cref="ZipStream"/>
    ''' 3. HDS streampack filesystem
    ''' 
    ''' </summary>
    Public Interface IFileSystemEnvironment

        ''' <summary>
        ''' the file system environment is readonly?
        ''' </summary>
        ReadOnly Property [readonly] As Boolean

        ''' <summary>
        ''' open a specific file for read and write
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="mode"></param>
        ''' <param name="access"></param>
        ''' <returns></returns>
        Function OpenFile(path As String,
                          Optional mode As FileMode = FileMode.OpenOrCreate,
                          Optional access As FileAccess = FileAccess.Read) As Stream
        ''' <summary>
        ''' delete target file which is specific by path
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function DeleteFile(path As String) As Boolean
        ''' <summary>
        ''' check the specific file is exists on current filesystem or not?
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean
        ''' <summary>
        ''' get file size
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns>-1 means file is not exists on the filesystem</returns>
        Function FileSize(path As String) As Long
        ''' <summary>
        ''' get file modified time, usually for the cache expired test
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function FileModifyTime(path As String) As Date

        ''' <summary>
        ''' get the full path of the given file name in current directory model
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        Function GetFullPath(filename As String) As String

        ''' <summary>
        ''' write text content to a specific file
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Function WriteText(text As String, path As String) As Boolean
        Function ReadAllText(path As String) As String

        ''' <summary>
        ''' get all files inside current filesystem object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' be careful when running this on local filesystem object!
        ''' </remarks>
        Function GetFiles() As IEnumerable(Of String)
        ''' <summary>
        ''' get all files inside a specific subfolder recursivly
        ''' </summary>
        ''' <param name="subdir">relative path of the subfolder in current workspace</param>
        ''' <param name="exts">file extension type wildcard filters for the files, default is ``*.*``</param>
        ''' <returns></returns>
        Function GetFiles(subdir As String, ParamArray exts As String()) As IEnumerable(Of String)
        ''' <summary>
        ''' get all files inside a specific subfolder, not recursivly
        ''' </summary>
        ''' <param name="subdir">relative path of the subfolder in current workspace</param>
        ''' <param name="exts">file extension type wildcard filters for the files, default is ``*.*``</param>
        ''' <returns></returns>
        Function EnumerateFiles(subdir As String, ParamArray exts As String()) As IEnumerable(Of String)

        ''' <summary>
        ''' close current filesystem session
        ''' </summary>
        ''' <remarks>
        ''' apply for the zip archive/HDS streampack
        ''' </remarks>
        Sub Close()
        ''' <summary>
        ''' save stream data
        ''' </summary>
        ''' <remarks>
        ''' apply for the zip archive/HDS streampack
        ''' </remarks>
        Sub Flush()

    End Interface
End Namespace
