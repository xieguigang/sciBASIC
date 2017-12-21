#Region "Microsoft.VisualBasic::acad40b6a857804f1d4f050a93348de5, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\Network\MIME\MIME.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Net.Protocols.ContentTypes

    ''' <summary>
    ''' MIME stands for "Multipurpose Internet Mail Extensions. It's a way of identifying files on the Internet according to their nature and format. 
    ''' For example, using the ``Content-type`` header value defined in a HTTP response, the browser can open the file with the proper extension/plugin.
    ''' (http://www.freeformatter.com/mime-types-list.html)
    ''' </summary>
    Public Module MIME

        ''' <summary>
        ''' 枚举出所有已知的文件拓展名列表，Key全部都是小写的 (格式: ``.ext``)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SuffixTable As IReadOnlyDictionary(Of String, ContentType)
        ''' <summary>
        ''' 根据类型来枚举，Key全部都是小写的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ContentTypes As IReadOnlyDictionary(Of String, ContentType)

        ''' <summary>
        ''' .*（ 二进制流，不知道下载文件类型）
        ''' </summary>
        Public Const Unknown As String = "application/octet-stream"
        Public Const MsDownload As String = "application/x-msdownload"
        Public Const Json As String = "application/json"
        Public Const ZIP As String = "application/zip"

        Public ReadOnly Property UnknownType As New ContentType With {
            .FileExt = "*.*",
            .MIMEType = Unknown,
            .Name = NameOf(Unknown)
        }

        Sub New()
            SuffixTable = My.Resources _
                .List_of_MIME_types___Internet_Media_Types_ _
                .lTokens _
                .__loadContents _
                .Where(Function(x) Not x.IsEmpty) _
                .GroupBy(Function(x) x.FileExt.ToLower) _
                .ToDictionary(Function(x) x.Key,
                              Function(x) x.First)
            ContentTypes = SuffixTable.Values _
                .ToDictionary(Function(x) x.MIMEType.ToLower)
        End Sub

        <Extension>
        Private Iterator Function __loadContents(lines As IEnumerable(Of String)) As IEnumerable(Of ContentType)
            lines = From line As String
                    In lines.Skip(1)
                    Where Not String.IsNullOrWhiteSpace(line)
                    Select line

            For Each line$ In lines
                Try
                    ' 2016-11-28
                    ' Not sure why a bugs happed here, there is no bugs here before!
                    Yield ContentType.__createObject(line)
                Catch ex As Exception
#If DEBUG Then
                    Call line.Warning
#End If
                End Try
            Next
        End Function
    End Module
End Namespace
