#Region "Microsoft.VisualBasic::d3104f06108de8e179a7bd1fbcaf3818, mime\text%html\Parser\CSS\Enums\Enums.vb"

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

    '   Total Lines: 124
    '    Code Lines: 47
    ' Comment Lines: 71
    '   Blank Lines: 6
    '     File Size: 7.47 KB


    '     Enum CSSSelectorTypes
    ' 
    '         [class], id, tag
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum HtmlTags
    ' 
    '         [option], [select], a, audio, body
    '         button, div, em, footer, form
    '         h1, h2, h3, h4, h5
    '         h6, heder, hr, iframe, img
    '         input, li, nav, ol, p
    '         pre, span, svg, table, tbody
    '         textarea, th, thead, tr, ul
    '         video
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language.CSS

    ''' <summary>
    ''' Define Tag type. 
    ''' </summary>
    ''' <remarks>
    ''' |CSS version|选择器              |例子                  |例子描述                                          |         
    ''' |-----------|--------------------|---------------------|-------------------------------------------------|
    ''' | 1         |.class              |.intro               |选择 class="intro" 的所有元素。                   |
    ''' | 1         |#id                 |#firstname           |选择 id="firstname" 的所有元素。                  |
    ''' | 2         |*                   |*                    |选择所有元素。	                                 |
    ''' | 1         |element             |p                    |选择所有 &lt;p> 元素。	                         |
    ''' | 1         |element,element     |div,p	               |选择所有 &lt;div> 元素和所有 &lt;p> 元素。         |
    ''' | 1         |element element     |div p	               |选择 &lt;div> 元素内部的所有 &lt;p> 元素。         |
    ''' | 2         |element>element     |div>p                |选择父元素为 &lt;div> 元素的所有 &lt;p> 元素。      |
    ''' | 2         |element+element     |div+p                |选择紧接在 &lt;div> 元素之后的所有 &lt;p> 元素。    |
    ''' | 2         |[attribute]         |[target]             |选择带有 target 属性所有元素。                     |
    ''' | 2         |[attribute=value]   |[target=_blank]      |选择 target="_blank" 的所有元素。	                 |
    ''' | 2         |[attribute~=value]  |[title~=flower]      |选择 title 属性包含单词 "flower" 的所有元素。       |
    ''' | 2         |[attribute\|=value] |[lang|=en]	       |选择 lang 属性值以 "en" 开头的所有元素。            |
    ''' | 1         |:link               |a:link	           |选择所有未被访问的链接。                            |
    ''' | 1         |:visited            |a:visited	           |选择所有已被访问的链接。                            |
    ''' | 1         |:active             |a:active	           |选择活动链接。                                     |
    ''' | 1         |:hover              |a:hover	           |选择鼠标指针位于其上的链接。                        |
    ''' | 2         |:focus              |input:focus          |选择获得焦点的 input 元素。	                      |
    ''' | 1         |:first-letter       |p:first-letter       |选择每个 &lt;p> 元素的首字母。                      |
    ''' | 1         |:first-line         |p:first-line	       |选择每个 &lt;p> 元素的首行。                        |
    ''' | 2         |:first-child        |p:first-child        |选择属于父元素的第一个子元素的每个 &lt;p> 元素。      |
    ''' | 2         |:before             |p:before	           |在每个 &lt;p> 元素的内容之前插入内容。               |
    ''' | 2         |:after              |p:after              |在每个 &lt;p> 元素的内容之后插入内容。               |
    ''' | 2         |:lang(language)     |p:lang(it)	       |选择带有以 "it" 开头的 lang 属性值的每个 &lt;p> 元素。|
    ''' | 3         |element1~element2   |p~ul	               |选择前面有 &lt;p> 元素的每个 &lt;ul> 元素。	       |
    ''' | 3         |[attribute^=value]  |a[src^="https"]      |选择其 src 属性值以 "https" 开头的每个 &lt;a> 元素。 |
    ''' | 3         |[attribute$=value]  |a[src$=".pdf"]       |选择其 src 属性以 ".pdf" 结尾的所有 &lt;a> 元素。    |
    ''' | 3         |[attribute*=value]  |a[src*="abc"]	       |选择其 src 属性中包含 "abc" 子串的每个 &lt;a> 元素。 |
    ''' | 3         |:first-of-type      |p:first-of-type      |选择属于其父元素的首个 &lt;p> 元素的每个 &lt;p> 元素。|
    ''' | 3         |:last-of-type       |p:last-of-type       |选择属于其父元素的最后 &lt;p> 元素的每个 &lt;p> 元素。|
    ''' | 3         |:only-of-type       |p:only-of-type       |选择属于其父元素唯一的 &lt;p> 元素的每个 &lt;p> 元素。|
    ''' | 3         |:only-child         |p:only-child	       |选择属于其父元素的唯一子元素的每个 &lt;p> 元素。	   |
    ''' | 3         |:nth-child(n)       |p:nth-child(2)       |选择属于其父元素的第二个子元素的每个 &lt;p> 元素。	   |
    ''' | 3         |:nth-last-child(n)  |p:nth-last-child(2)  |同上，从最后一个子元素开始计数。	                   |
    ''' | 3         |:nth-of-type(n)     |p:nth-of-type(2)     |选择属于其父元素第二个 &lt;p> 元素的每个 &lt;p> 元素。|
    ''' | 3         |:nth-last-of-type(n)|p:nth-last-of-type(2)|同上，但是从最后一个子元素开始计数。	               |
    ''' | 3         |:last-child         |p:last-child	       |选择属于其父元素最后一个子元素每个 &lt;p> 元素。	   |
    ''' | 3         |:root               |:root                |选择文档的根元素。	                               |
    ''' | 3         |:empty              |p:empty	           |选择没有子元素的每个 &lt;p> 元素（包括文本节点）。	   |
    ''' | 3         |:target             |#news:target         |选择当前活动的 #news 元素。	                       |
    ''' | 3         |:enabled            |input:enabled	       |选择每个启用的 &lt;input> 元素。                     |
    ''' | 3         |:disabled           |input:disabled       |选择每个禁用的 &lt;input> 元素	                   |
    ''' | 3         |:checked            |input:checked	       |选择每个被选中的 &lt;input> 元素。	               |
    ''' | 3         |:Not(selector)      |:Not(p)	           |选择非 &lt;p> 元素的每个元素。	                   |
    ''' | 3         |:selection          |:selection	       |选择被用户选取的元素部分。                           |
    ''' 
    ''' > "CSS version" 列指示该属性是在哪个 CSS 版本中定义的。（CSS1、CSS2 还是 CSS3。）
    ''' </remarks>
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

        ''' <summary>
        ''' Invalid tag name
        ''' </summary>
        NA = 0

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
        thead
        tbody
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
