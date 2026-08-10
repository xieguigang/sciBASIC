#Region "Microsoft.VisualBasic::9f3c1a7b2e5d4c8a9b0c1d2e3f4a5b6c, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\docx\IDocumentWriter.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2026 GPL3 Licensed
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

    '   Total Lines: 1
    '    Code Lines: 1 (100.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 0.00 KB


    ' Interface IDocumentWriter
    ' 
    '     Properties: Author, Title, Subject, Description, Tags, ApplicationName
    ' 
    '     Function: HeadingStyle, ParagraphStyle, DefaultStyle, TableStyle, CodeStyle,
    '               BlockquoteStyle, TitleStyle, PageSetup, PageSetupA4, PageSetupLetter,
    '               DocTitle, H1, H2, H3, H4, H5, H6, Heading, Paragraph, CodeBlock,
    '               Blockquote, List, TaskList, DefinitionList, Hr, PageBreak, Toc,
    '               Table, TableAutoFitWindow, TableAutoFitContents, Image, WriteBlocks
    ' 
    '     Sub: Save
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' IDocumentWriter.vb - 统一文档写入接口
'
' 抽出 WordDocument(docx) 的全部公开写入能力，使 docx 与 pdf 两种实现
' 共享同一套编程接口。所有写入方法返回 IDocumentWriter 以支持流式链式调用。
' 调用方面向该接口编程，仅通过传入不同实例即可产出不同格式。
' ============================================================================

Imports Microsoft.VisualBasic.MIME.text.markdown

''' <summary>
''' 统一文档写入接口。
''' <see cref="WordDocument"/>（生成 docx）与 <c>Microsoft.VisualBasic.MIME.application.pdf.PdfDocument</c>
''' （生成 pdf）均实现该接口，使同一份文档写入代码可通过传入不同实例生成不同格式。
''' 所有写入方法返回 <see cref="IDocumentWriter"/> 自身以支持流式链式调用。
''' </summary>
Public Interface IDocumentWriter

    ' === 元数据（与 WordDocument 同形同义） ===

    ''' <summary>文档作者。</summary>
    Property Author As String
    ''' <summary>文档标题。</summary>
    Property Title As String
    ''' <summary>文档主题。</summary>
    Property Subject As String
    ''' <summary>文档描述。</summary>
    Property Description As String
    ''' <summary>文档标签。</summary>
    Property Tags As String()
    ''' <summary>生成该文档的应用程序名称。</summary>
    Property ApplicationName As String

    ' === 样式设置（流式 API，均返回接口自身） ===

    ''' <summary>设置指定级别的标题样式 (level 1-6)。</summary>
    Function HeadingStyle(level As Integer, style As WordStyle) As IDocumentWriter
    ''' <summary>设置正文段落样式。</summary>
    Function ParagraphStyle(style As WordStyle) As IDocumentWriter
    ''' <summary>设置文档默认样式。</summary>
    Function DefaultStyle(style As WordStyle) As IDocumentWriter
    ''' <summary>设置表格样式。</summary>
    Function TableStyle(style As TableStyle) As IDocumentWriter
    ''' <summary>设置代码块样式。</summary>
    Function CodeStyle(style As WordStyle) As IDocumentWriter
    ''' <summary>设置引用块样式。</summary>
    Function BlockquoteStyle(style As WordStyle) As IDocumentWriter
    ''' <summary>设置文档标题样式。</summary>
    Function TitleStyle(style As WordStyle) As IDocumentWriter

    ' === 页面设置（尺寸单位 twips，与 WordDocument 一致） ===

    ''' <summary>设置页面尺寸和边距 (twips)。</summary>
    Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                       marginTop As Integer, marginRight As Integer,
                       marginBottom As Integer, marginLeft As Integer) As IDocumentWriter
    ''' <summary>A4 纸张，1 英寸边距。</summary>
    Function PageSetupA4() As IDocumentWriter
    ''' <summary>Letter 纸张，1 英寸边距。</summary>
    Function PageSetupLetter() As IDocumentWriter

    ' === 内容写入（流式 API，均返回接口自身） ===

    ''' <summary>写入文档标题（居中大字号，非 heading 样式）。</summary>
    Function DocTitle(text As String) As IDocumentWriter
    ''' <summary>写入一级标题。</summary>
    Function H1(text As String) As IDocumentWriter
    ''' <summary>写入二级标题。</summary>
    Function H2(text As String) As IDocumentWriter
    ''' <summary>写入三级标题。</summary>
    Function H3(text As String) As IDocumentWriter
    ''' <summary>写入四级标题。</summary>
    Function H4(text As String) As IDocumentWriter
    ''' <summary>写入五级标题。</summary>
    Function H5(text As String) As IDocumentWriter
    ''' <summary>写入六级标题。</summary>
    Function H6(text As String) As IDocumentWriter
    ''' <summary>写入指定级别的标题 (level 1-6)。</summary>
    Function Heading(level As Integer, text As String) As IDocumentWriter
    ''' <summary>写入正文段落。</summary>
    Function Paragraph(text As String) As IDocumentWriter
    ''' <summary>写入正文段落（指定样式）。</summary>
    Function Paragraph(text As String, style As WordStyle) As IDocumentWriter
    ''' <summary>写入代码块（等宽字体，灰色背景）。</summary>
    Function CodeBlock(code As String, Optional language As String = "") As IDocumentWriter
    ''' <summary>写入引用块。</summary>
    Function Blockquote(text As String) As IDocumentWriter
    ''' <summary>写入列表（有序或无序）。</summary>
    Function List(items As String(), Optional ordered As Boolean = False) As IDocumentWriter
    ''' <summary>写入任务列表。</summary>
    Function TaskList(items As String(), checked As Boolean()) As IDocumentWriter
    ''' <summary>写入定义列表。</summary>
    Function DefinitionList(terms As String(), definitions As String()) As IDocumentWriter
    ''' <summary>写入水平分割线。</summary>
    Function Hr() As IDocumentWriter
    ''' <summary>插入分页符。</summary>
    Function PageBreak() As IDocumentWriter
    ''' <summary>插入目录 (TOC)。</summary>
    Function Toc(Optional maxLevel As Integer = 3) As IDocumentWriter

    ' === 表格（流式 API，均返回接口自身） ===

    ''' <summary>写入等宽表格（二维数组形式）。</summary>
    Function Table(headers As String(), data As String(,)) As IDocumentWriter
    ''' <summary>写入表格（二维数组形式，支持对齐方式）。</summary>
    Function Table(headers As String(), data As String(,), alignments As String()) As IDocumentWriter
    ''' <summary>写入等宽表格（交错数组形式）。</summary>
    Function Table(headers As String(), rows As String()(),
                   Optional alignments As String() = Nothing) As IDocumentWriter
    ''' <summary>写入表格，按窗口宽度自适应。</summary>
    Function TableAutoFitWindow(headers As String(), rows As String()(),
                                Optional alignments As String() = Nothing,
                                Optional center As Boolean = False,
                                Optional threeLine As Boolean = False) As IDocumentWriter
    ''' <summary>写入表格，按内容宽度自适应。</summary>
    Function TableAutoFitContents(headers As String(), rows As String()(),
                                  Optional alignments As String() = Nothing,
                                  Optional center As Boolean = False,
                                  Optional threeLine As Boolean = False) As IDocumentWriter
    ''' <summary>写入表格，按窗口宽度自适应（二维数组形式）。</summary>
    Function TableAutoFitWindow(headers As String(,), rows As String(,),
                                Optional alignments As String() = Nothing,
                                Optional center As Boolean = False,
                                Optional threeLine As Boolean = False) As IDocumentWriter
    ''' <summary>写入表格，按内容宽度自适应（二维数组形式）。</summary>
    Function TableAutoFitContents(headers As String(,), rows As String(,),
                                 Optional alignments As String() = Nothing,
                                 Optional center As Boolean = False,
                                 Optional threeLine As Boolean = False) As IDocumentWriter

    ' === 图片与 Markdown 块 ===

    ''' <summary>插入图片。</summary>
    Function Image(file As String,
                   Optional width As Double = 0,
                   Optional height As Double = 0,
                   Optional caption As String = "") As IDocumentWriter

    ''' <summary>将一组 Markdown 内容块写入文档。</summary>
    Function WriteBlocks(blocks As IEnumerable(Of JSONSchema.Block)) As IDocumentWriter

    ' === 保存 ===

    ''' <summary>将文档保存到指定文件路径。</summary>
    Sub Save(filePath As String)

End Interface
