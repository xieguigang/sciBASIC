#Region "Microsoft.VisualBasic::1832a06e22224b31f8fa10e2ca2102b9, ..\sciBASIC#\mime\text%html\HTML\CSS\Parser\enums.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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

Namespace HTML.CSS

    ''' <summary>
    ''' Define Tag type. 
    ''' </summary>
    Public Enum CSSSelectorTypes As Byte

        ''' <summary>
        ''' 表达式作为选择器
        ''' </summary>
        expression = 0

        ''' <summary>
        ''' Normal HTMl Tag.
        ''' </summary>
        tag
        ''' <summary>
        ''' Class in HTML Code.
        ''' </summary>
        [class]
        ''' <summary>
        ''' Tag in HTML Code.
        ''' </summary>
        id
    End Enum

    ''' <summary>
    ''' HTML Tags. 
    ''' </summary>
    Public Enum HtmlTags As Integer
        h1
        h2
        h3
        h4
        h5
        h6
        body
        a
        img
        ol
        ul
        li
        table
        tr
        th
        nav
        heder
        footer
        form
        [option]
        [select]
        button
        textarea
        input
        audio
        video
        iframe
        hr
        em
        div
        pre
        p
        span
        svg
    End Enum
End Namespace