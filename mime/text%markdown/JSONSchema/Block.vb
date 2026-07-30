#Region "Microsoft.VisualBasic::1525078201f2cf921b0ce48e9d298cd8, mime\text%markdown\JSONSchema\Block.vb"

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

    '   Total Lines: 101
    '    Code Lines: 20 (19.80%)
    ' Comment Lines: 73 (72.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (7.92%)
    '     File Size: 3.76 KB


    '     Class Block
    ' 
    '         Properties: alignments, alt, checked, content, definitions
    '                     headers, id, items, language, level
    '                     ordered, rows, terms, title, type
    '                     url
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace JSONSchema

    ''' <summary>
    ''' 针对markdown格式有限的支持
    ''' </summary>
    Public Class Block

        ''' <summary>
        ''' 块级语法类型，统一小写。当前支持的完整集合：
        ''' heading(h) / paragraph(p) / code / list(li) / blockquote /
        ''' table / hr(horizontal-rule) / image(img) / html(raw) /
        ''' math / link / tasklist / footnote / deflist
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        ''' <summary>
        ''' heading level if type = heading
        ''' </summary>
        ''' <returns></returns>
        Public Property level As Integer
        ''' <summary>
        ''' the text content of heading/paragraph/code/blockquote
        ''' </summary>
        ''' <returns></returns>
        Public Property content As String
        ''' <summary>
        ''' the language code if type = code, example as bash/r/vbnet/c-sharp/python/php
        ''' </summary>
        ''' <returns></returns>
        Public Property language As String
        ''' <summary>
        ''' is ordered list if type = list
        ''' </summary>
        ''' <returns></returns>
        Public Property ordered As Boolean
        ''' <summary>
        ''' the list items for type = list
        ''' </summary>
        ''' <returns></returns>
        Public Property items As String()
        ''' <summary>
        ''' the table headers for type = table
        ''' </summary>
        ''' <returns></returns>
        Public Property headers As String()
        ''' <summary>
        ''' the table header alignments, value could be left|right|center
        ''' </summary>
        ''' <returns></returns>
        Public Property alignments As String()
        ''' <summary>
        ''' the table rows for type = table, each block elements inside this array should be list type, list items will be used as table row cells
        ''' </summary>
        ''' <returns></returns>
        Public Property rows As String()()

        ''' <summary>
        ''' the image source url if type = image/img
        ''' </summary>
        ''' <returns></returns>
        Public Property url As String
        ''' <summary>
        ''' the alternative text if type = image/img
        ''' </summary>
        ''' <returns></returns>
        Public Property alt As String
        ''' <summary>
        ''' the optional title (hover tip) if type = image/img or link
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String

        ''' <summary>
        ''' tasklist only: 与 <see cref="items"/> 平行排列，标记每一项是否被勾选。
        ''' 当该字段为 Nothing 时，渲染时默认所有项均未勾选。
        ''' </summary>
        ''' <returns></returns>
        Public Property checked As Boolean()

        ''' <summary>
        ''' footnote only: 脚注的唯一标识，例如 "1" / "note"。
        ''' 在 markdown 中表现为 [^id]: content，在 html 中表现为 id="fn-id"。
        ''' </summary>
        ''' <returns></returns>
        Public Property id As String

        ''' <summary>
        ''' deflist only: 术语列表（对应 html 的 &lt;dt&gt;）。
        ''' 与 <see cref="definitions"/> 平行排列。
        ''' </summary>
        ''' <returns></returns>
        Public Property terms As String()

        ''' <summary>
        ''' deflist only: 定义列表（对应 html 的 &lt;dd&gt;），与 <see cref="terms"/> 平行排列。
        ''' </summary>
        ''' <returns></returns>
        Public Property definitions As String()

    End Class
End Namespace
