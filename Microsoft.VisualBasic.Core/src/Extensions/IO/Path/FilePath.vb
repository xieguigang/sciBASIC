#Region "Microsoft.VisualBasic::ef4776a88a99e3c598855493b2821bf6, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\IO\Path\FilePath.vb"

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

    '   Total Lines: 95
    '    Code Lines: 73
    ' Comment Lines: 8
    '   Blank Lines: 14
    '     File Size: 3.21 KB


    '     Class FilePath
    ' 
    '         Properties: Components, DirectoryPath, FileName, IsAbsolutePath, IsDirectory
    '                     ParentDirectory
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: combineDirectory, Parse, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace FileIO.Path

    Public Class FilePath

        Public ReadOnly Property Components As String()
        Public ReadOnly Property IsDirectory As Boolean = False
        Public ReadOnly Property IsAbsolutePath As Boolean = False

        Public ReadOnly Property DirectoryPath As String
            Get
                Return If(IsAbsolutePath, "/", "") & combineDirectory()
            End Get
        End Property

        ''' <summary>
        ''' the file basename, not file path
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FileName As String
            Get
                Return Components.Last
            End Get
        End Property

        Public ReadOnly Property ParentDirectory As FilePath
            Get
                If Components.Length = 1 Then
                    Return New FilePath("/")
                Else
                    Return New FilePath(Components.Take(Components.Length - 1), True, IsAbsolutePath)
                End If
            End Get
        End Property

        Public Sub New(tokens As IEnumerable(Of String), isDir As Boolean, isAbs As Boolean)
            IsDirectory = isDir
            IsAbsolutePath = isAbs
            Components = (From name As String
                          In tokens
                          Where Not name.StringEmpty).ToArray
        End Sub

        Sub New(filepath As String)
            If filepath.EndsWith("/"c) OrElse filepath.EndsWith("\"c) Then
                IsDirectory = True
            End If
            If filepath.StartsWith("/"c) OrElse filepath.StartsWith("\"c) Then
                IsAbsolutePath = True
            End If

            Components = filepath _
                .Replace("\", "/") _
                .Split("/"c) _
                .Where(Function(t) Not t.StringEmpty) _
                .ToArray
        End Sub

        Private Function combineDirectory() As String
            If IsDirectory Then
                Return Components.JoinBy("/")
            Else
                Return Components.Take(Components.Length - 1).JoinBy("/")
            End If
        End Function

        ''' <summary>
        ''' get full path string
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            If IsDirectory Then
                Return DirectoryPath.StringReplace("[/]{2,}", "/")
            Else
                Return $"{DirectoryPath}/{FileName}".StringReplace("[/]{2,}", "/")
            End If
        End Function

        Public Shared Function Parse(path As String) As FilePath
            Return New FilePath(path)
        End Function

        Public Shared Operator =(file1 As FilePath, file2 As FilePath) As Boolean
            Dim path1 As String = file1.ToString
            Dim path2 As String = file2.ToString

            Return path1 = path2 AndAlso
                file1.IsAbsolutePath = file2.IsAbsolutePath AndAlso
                file1.IsDirectory = file2.IsDirectory
        End Operator

        Public Shared Operator <>(file1 As FilePath, file2 As FilePath) As Boolean
            Return Not file1 = file2
        End Operator
    End Class
End Namespace
