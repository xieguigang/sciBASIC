#Region "Microsoft.VisualBasic::42d82a3fc8faaee112903d583eea786c, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\File\FileSaveHandle.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace ComponentModel

    ''' <summary>
    ''' This is a file object which have a handle to save its data to the filesystem.(这是一个带有文件数据保存方法的文件模型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface ISaveHandle

        ''' <summary>
        ''' Handle for saving the file data.(保存文件的方法)
        ''' </summary>
        ''' <param name="Path">The file path that will save data to.(进行文件数据保存的文件路径)</param>
        ''' <param name="encoding">The text encoding value for the text document.(文本文档的编码格式)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Save(Optional Path As String = "", Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Function Save(Optional Path As String = "", Optional encoding As TextEncodings.Encodings = Encodings.UTF8) As Boolean
    End Interface

    Public Interface IDocumentEditor : Inherits ISaveHandle
        Property DocumentPath As String
        Function LoadDocument(Path As String) As Boolean
    End Interface
End Namespace
