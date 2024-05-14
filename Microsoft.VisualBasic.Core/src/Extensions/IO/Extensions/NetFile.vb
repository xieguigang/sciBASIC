#Region "Microsoft.VisualBasic::3b0c3112940bb250ccdb1fcf427f7e41, Microsoft.VisualBasic.Core\src\Extensions\IO\Extensions\NetFile.vb"

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

    '   Total Lines: 103
    '    Code Lines: 77
    ' Comment Lines: 11
    '   Blank Lines: 15
    '     File Size: 3.63 KB


    '     Module NetFile
    ' 
    '         Function: GetMapPath, MapGithubRawUrl, MapNetFile, mapToLocalUrl, NetFileExists
    '                   OpenNetStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

Namespace FileIO

    <Package("IO.NetFile")>
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

        <Extension>
        Public Function OpenNetStream(url$, Optional encoding As Encodings = Encodings.Default) As StreamReader
            Dim path$ = url.GetMapPath
            Dim stream As Stream

            If path.FileExists Then
                Dim file As New FileStream(path, FileMode.Open)
                stream = file
            Else ' 网络文件
                Dim WebRequest As HttpWebRequest = HttpWebRequest.Create(url)
                Dim WebResponse As WebResponse = WebRequest.GetResponse
                stream = WebResponse.GetResponseStream
            End If

            Dim out As New StreamReader(stream, encoding.CodePage)
            Return out
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
            If InStr(url, "http://", CompareMethod.Text) + InStr(url, "https://", CompareMethod.Text) > 0 Then
                Return mapToLocalUrl(url)
            ElseIf url.StartsWith("github://") Then
                Return mapToLocalUrl(MapGithubRawUrl(url))
            ElseIf InStr(url, "file://", CompareMethod.Text) = 1 Then
                Return Mid(url, 8)
            Else
                If url.FileExists Then
                    Return url
                Else
                    Throw New Exception(url & " is a unrecognized url path!")
                End If
            End If
        End Function

        Friend Function MapGithubRawUrl(url As String) As String
            If url.StartsWith("github://") Then
                Return url _
                    .Replace("github://", "https://raw.githubusercontent.com/") _
                    .Replace("/blob/master/", "/master/")
            Else
                Return url
            End If
        End Function

        Private Function mapToLocalUrl(url As String) As String
            url = Strings.Split(url, "//").Last

            Dim tokens$() = url.Split("/"c)
            Dim folders As String = tokens.Take(tokens.Length - 1).JoinBy("/")

            url = tokens.Last.NormalizePathString
            url = App.AppSystemTemp & "/" & folders & "/" & url

            Call folders.MakeDir

            Return url
        End Function
    End Module
End Namespace
