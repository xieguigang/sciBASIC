#Region "Microsoft.VisualBasic::bb606b10c10866998fcbdd7c77fe46e1, ..\sciBASIC#\www\Microsoft.VisualBasic.NETProtocol\Test\Module1.vb"

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

Module Module1

    Sub Main()

        Call Microsoft.VisualBasic.Net.HTTP.WebSaveAs.SaveAs("http://blog.xieguigang.me/about/", "x:\test2\")

        Pause()

        For Each s In Microsoft.VisualBasic.Net.HTTP.WebCrawling.DownloadAllLinks("http://120.76.195.65/index.html", "x:\")
            Call s.Warning
        Next
    End Sub
End Module

