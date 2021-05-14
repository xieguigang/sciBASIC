#Region "Microsoft.VisualBasic::d04e821c6beb24a5aa6bb3f5d9ec50dc, Microsoft.VisualBasic.Core\src\Language\Language\UnixBash\FileSystem\File.vb"

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

    '     Class File
    ' 
    '         Function: Save
    '         Operators: <, >, >>
    ' 
    '     Structure FileHandle
    ' 
    '         Properties: IsDirectory, IsFile, IsHTTP
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Language.UnixBash.FileSystem

    ''' <summary>
    ''' Asbtract file IO model
    ''' </summary>
    Public MustInherit Class File
        Implements ISaveHandle

        Public Function Save(Path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.CodePage)
        End Function

        Public MustOverride Function Save(Path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save

        Public Shared Operator >(file As File, path As String) As Boolean
            Return file.Save(path, Encodings.UTF8)
        End Operator

        Public Shared Operator <(file As File, path As String) As Boolean
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >>(file As File, path As Integer) As Boolean
            Dim handle As FileHandle = My.File.GetHandle(path)
            Return file.Save(handle.FileName, handle.encoding)
        End Operator
    End Class

    ''' <summary>
    ''' 文件系统对象的句柄
    ''' </summary>
    Public Structure FileHandle
        Dim FileName As String
        Dim handle As Integer
        Dim encoding As Encoding

        ''' <summary>
        ''' Determined that is this filename is a network location.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsHTTP As Boolean
            Get
                Return FileName.isURL
            End Get
        End Property

        Public ReadOnly Property IsFile As Boolean
            Get
                Return FileName.FileExists
            End Get
        End Property

        Public ReadOnly Property IsDirectory As Boolean
            Get
                Return FileName.DirectoryExists
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
