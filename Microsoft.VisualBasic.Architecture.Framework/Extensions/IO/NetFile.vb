#Region "Microsoft.VisualBasic::fdf26f8c70dbbd9c7462ddc702d48142, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\IO\NetFile.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace FileIO

    <PackageNamespace("IO.NetFile")>
    Public Module NetFile

        ''' <summary>
        ''' 将网络文件映射为本地文件，这个可以同时兼容http或者本地文件路径
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        <ExportAPI("MapNetFile")>
        <Extension>
        Public Function MapNetFile(url As String) As String
            Dim path As String = url.GetMapPath

            If path.FileExists Then
                Return path
            Else  ' 下载数据然后缓存
                Call DownloadFile(url, path)
                Return path
            End If
        End Function

        <ExportAPI("NetFile.FileExists")>
        <Extension>
        Public Function NetFileExists(url As String) As Boolean
            Return url.GetMapPath.FileExists
        End Function

        ''' <summary>
        ''' 网络文件转换为本地文件路径
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Map.Path")>
        <Extension>
        Public Function GetMapPath(url As String) As String
            If InStr(url, "http://", CompareMethod.Text) +
                InStr(url, "https://", CompareMethod.Text) > 0 Then

                url = Strings.Split(url, "//").Last
                url = App.AppSystemTemp & "/" & url.NormalizePathString

                Dim folders As String = FileSystem.GetParentPath(url)
                Call FileSystem.CreateDirectory(folders)

                Return url
            Else
                If url.FileExists Then
                    Return url
                Else
                    Throw New Exception(url & " is a unrecognized url path!")
                End If
            End If
        End Function
    End Module
End Namespace
