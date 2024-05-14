#Region "Microsoft.VisualBasic::4bc7d87b1c2b8387d9a23a207a33af6b, Microsoft.VisualBasic.Core\src\Net\HTTP\sitemap.xml.vb"

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
    '    Code Lines: 66
    ' Comment Lines: 13
    '   Blank Lines: 16
    '     File Size: 3.75 KB


    '     Class sitemap
    ' 
    '         Properties: urls
    ' 
    '         Function: ToString
    '         Structure url
    ' 
    '             Properties: changefreq, lastmod, loc, priority
    ' 
    '             Function: ToString
    ' 
    '         Enum changefreqs
    ' 
    '             always, daily, hourly, monthly, never
    '             weekly, yearly
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: (+2 Overloads) Save, ScanAllFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes

Namespace Net.Http

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' ```
    ''' xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  xsi:schemaLocation="http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"
    ''' ```
    ''' </remarks>
    <XmlRoot("urlset")> Public Class sitemap : Implements ISaveHandle

        <XmlElement("url")> Public Property urls As url()

        Public Structure url

            Public Property loc As String
            Public Property priority As Double
            Public Property lastmod As String
            Public Property changefreq As String

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Enum changefreqs
            always
            hourly
            daily
            weekly
            monthly
            yearly
            never
        End Enum

        ''' <summary>
        ''' ```xml
        ''' &lt;urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd">
        ''' ```
        ''' </summary>
        Const xmlns$ = "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"">"

        Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Dim xml As String = (Me.GetXml)
            xml = Regex.Replace(xml, "<urlset .*?>", xmlns)
            Return xml.SaveTo(path, encoding)
        End Function

        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function

        Shared ReadOnly htmlFiles As [Default](Of String()) = {"*.html", "*.htm"}

        Public Shared Function ScanAllFiles(wwwroot$, host$, Optional fileTypes$() = Nothing, Optional changefreq As changefreqs = changefreqs.monthly) As sitemap
            Dim url As New List(Of url)
            Dim freq$ = changefreq.ToString
            Dim lastmod$ = $"{Now.Year}-{FillDateZero(Now.Month)}-{FillDateZero(Now.Day)}"

            wwwroot = wwwroot.GetDirectoryFullPath
            host = host.TrimDIR

            For Each file$ In ls - l - r - (fileTypes Or htmlFiles) <= wwwroot
                file = file.Replace("\", "/").Replace(wwwroot, "")
                file = host & file
                url += New url With {
                    .changefreq = freq,
                    .lastmod = lastmod,
                    .priority = 1,
                    .loc = file
                }
            Next

            Return New sitemap With {
                .urls = url
            }
        End Function
    End Class
End Namespace
