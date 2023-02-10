#Region "Microsoft.VisualBasic::8db7bb7bf80dda2ac73b5f8a076f13a4, sciBASIC#\Microsoft.VisualBasic.Core\src\Net\MIME\MIME.vb"

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

    '   Total Lines: 88
    '    Code Lines: 54
    ' Comment Lines: 25
    '   Blank Lines: 9
    '     File Size: 3.65 KB


    '     Module MIME
    ' 
    '         Properties: ContentTypes, SuffixTable, UnknownType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: loadContents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

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
        ''' <remarks>
        ''' recommended use <see cref="Utils.FileMimeType"/> to get this mime type data
        ''' </remarks>
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
        Public Const Png As String = "image/png"
        Public Const Xml As String = "text/xml"
        Public Const Html As String = "text/html"

        ''' <summary>
        ''' ``application/octet-stream``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UnknownType As New ContentType With {
            .FileExt = ".*",
            .MIMEType = Unknown,
            .Name = NameOf(Unknown)
        }

        Sub New()
            SuffixTable = My.Resources _
                .List_of_MIME_types___Internet_Media_Types_ _
                .LineTokens _
                .loadContents _
                .Where(Function(x) Not x.IsEmpty) _
                .GroupBy(Function(x) x.FileExt.ToLower) _
                .ToDictionary(Function(x) x.Key,
                              Function(x)
                                  Return x.First
                              End Function)
            ContentTypes = SuffixTable _
                .Values _
                .ToDictionary(Function(x)
                                  Return x.MIMEType.ToLower
                              End Function)

            Call DirectCast(SuffixTable, Dictionary(Of String, ContentType)).Add(".dzi", New ContentType With {.Details = "Deep Zoom Image", .FileExt = ".dzi", .MIMEType = "application/xml", .Name = "Deep Zoom Image"})
        End Sub

        <Extension>
        Private Iterator Function loadContents(lines As IEnumerable(Of String)) As IEnumerable(Of ContentType)
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
