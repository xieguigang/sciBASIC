#Region "Microsoft.VisualBasic::78187d75887816f837d8cbb8943492bd, Microsoft.VisualBasic.Core\src\ComponentModel\File\FileSaveHandle.vb"

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

    '   Total Lines: 45
    '    Code Lines: 15 (33.33%)
    ' Comment Lines: 22 (48.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (17.78%)
    '     File Size: 1.61 KB


    '     Interface ISaveHandle
    ' 
    '         Function: (+3 Overloads) Save
    ' 
    '     Interface IFileReference
    ' 
    '         Properties: FilePath, MimeType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel

    ''' <summary>
    ''' This is a file object which have a handle to save its data to the filesystem.
    ''' </summary>
    ''' <remarks>(这是一个带有文件数据保存方法的文件模型)</remarks>
    Public Interface ISaveHandle

        ''' <summary>
        ''' Handle for saving the file data.
        ''' </summary>
        ''' <param name="path">The file path that will save data to.(进行文件数据保存的文件路径)</param>
        ''' <param name="encoding">The text encoding value for the text document.(文本文档的编码格式)</param>
        ''' <returns></returns>
        ''' <remarks>(保存文件的方法)</remarks>
        Function Save(path$, encoding As Encoding) As Boolean
        Function Save(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Function Save(file As Stream, encoding As Encoding) As Boolean

    End Interface

    ''' <summary>
    ''' 表示一个对文件的引用接口
    ''' </summary>
    Public Interface IFileReference

        ''' <summary>
        ''' 进行文件引用的路径
        ''' </summary>
        ''' <returns></returns>
        Property FilePath As String

        ''' <summary>
        ''' 一个可选的只读属性用来标记文件所符合的类型
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property MimeType As ContentType()

    End Interface
End Namespace
