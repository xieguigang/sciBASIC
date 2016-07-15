#Region "Microsoft.VisualBasic::668261abffc6e8e6e1c8fe3e238d35db, ..\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\MIME\MIME.vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Net.Protocols.ContentTypes

    ''' <summary>
    ''' MIME stands for "Multipurpose Internet Mail Extensions. It's a way of identifying files on the Internet according to their nature and format. 
    ''' For example, using the "Content-type" header value defined in a HTTP response, the browser can open the file with the proper extension/plugin.
    ''' (http://www.freeformatter.com/mime-types-list.html)
    ''' </summary>
    Public Module MIME

        ''' <summary>
        ''' 枚举出所有已知的文件拓展名列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExtDict As IReadOnlyDictionary(Of String, ContentType)
        ''' <summary>
        ''' 根据类型来枚举
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ContentTypes As IReadOnlyDictionary(Of String, ContentType)

        ''' <summary>
        ''' .*（ 二进制流，不知道下载文件类型）
        ''' </summary>
        Public Const Unknown As String = "application/octet-stream"
        Public Const MsDownload As String = "application/x-msdownload"

        Sub New()
            Dim lines As String() = My.Resources.List_of_MIME_types___Internet_Media_Types_.lTokens
            lines = (From line As String In lines.Skip(1).AsParallel
                     Where Not String.IsNullOrWhiteSpace(line)
                     Select line).ToArray
            Dim array As ContentType() = lines.ToArray(Function(line) ContentType.__createObject(line))
            ExtDict = (From x In array Select x Group x By x.FileExt.ToLower Into Group).ToDictionary(Function(x) x.ToLower, Function(x) x.Group.First)
            ContentTypes = array.ToDictionary(Function(x) x.MIMEType.ToLower)
        End Sub
    End Module
End Namespace
