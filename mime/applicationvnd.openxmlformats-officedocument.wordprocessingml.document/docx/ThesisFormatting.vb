' ============================================================================
' ThesisFormatting.vb - 学位论文主题格式
'
' 默认样式严格遵循《江南大学研究生学位论文要求及格式规范（2025年修订）》
' （江大校办〔2025〕50 号）"三、学位论文格式规范" 一节：
'
'   - 全文：中文使用宋体，英文字母、阿拉伯数字和半角标点使用 Times New Roman；
'   - 页面：A4 纸，上下左右页边距各 2.5cm；
'   - 章标题(Heading 1)：三号黑体，居中对齐，段前段后分别空 1 行；
'   - 节标题(Heading 2)：四号宋体，加粗，对齐方式靠左，段前段后分别空 0.5 行；
'   - 小节标题(Heading 3)：小四号宋体，加粗，对齐方式靠左，段前段后分别空 0.5 行；
'   - 正文内容：小四号宋体，首行缩进两个中文字符，1.25 倍行距；
'   - 图题和表题：五号，居中对齐；表格内容：五号字；
'   - 表格：采用国际通行的三线表。
'
' 字号-磅值对照（国标字号）：小二=18pt，三号=16pt，四号=14pt，小四=12pt，五号=10.5pt。
' "空 N 行"间距按标题自身字号折算为磅值（如三号标题空 1 行 = 16pt，四号标题空 0.5 行 = 7pt）。
'
' 所有样式属性均公开可写：修改属性后调用 ApplyTo 即可得到自定义版式；
' 也可以继承本类并重写 ApplyTo 实现其他学校的论文格式。
'
' 使用示例：
'   Dim doc As New WordDocument(...)
'   Dim theme As New ThesisFormatting()
'   theme.ApplyTo(doc)
'   doc.WriteBlocks(blocks)
'   doc.Save("thesis.docx")
' ============================================================================

''' <summary>
''' 学位论文主题格式对象。默认内置《江南大学研究生学位论文要求及格式规范（2025年修订）》
''' 的版式规则，通过 <see cref="ApplyTo"/> 一次性应用到 <see cref="WordDocument"/>
''' 的样式体系，使 <see cref="WordDocument.WriteBlocks"/> 写入的文档内容自动获得论文格式。
''' </summary>
Public Class ThesisFormatting

    ' ========================================================================
    ' 字体（规范：中文宋体，西文 Times New Roman；章标题等大标题使用黑体）
    ' ========================================================================

    ''' <summary>西文字体（英文字母、阿拉伯数字、半角标点）。</summary>
    Public Property LatinFont As String = "Times New Roman"

    ''' <summary>中文正文字体（规范：宋体）。</summary>
    Public Property ChineseBodyFont As String = "宋体"

    ''' <summary>中文大标题字体（规范：章标题使用黑体）。</summary>
    Public Property ChineseHeadingFont As String = "黑体"

    ' ========================================================================
    ' 字号常量（磅值，国标字号）
    ' ========================================================================

    ''' <summary>小二号 = 18pt（论文题目）。</summary>
    Public Const SizeXiaoEr As Double = 18
    ''' <summary>三号 = 16pt（章标题）。</summary>
    Public Const SizeSanHao As Double = 16
    ''' <summary>四号 = 14pt（节标题）。</summary>
    Public Const SizeSiHao As Double = 14
    ''' <summary>小四号 = 12pt（正文、小节标题）。</summary>
    Public Const SizeXiaoSi As Double = 12
    ''' <summary>五号 = 10.5pt（图题、表题、表格内容、参考文献）。</summary>
    Public Const SizeWuHao As Double = 10.5

    ' ========================================================================
    ' 页面设置（twips；规范：A4，上下左右页边距各 2.5cm）
    ' 1 cm = 1440 / 2.54 ≈ 566.93 twips，2.5cm ≈ 1417 twips
    ' ========================================================================

    Public Property PageWidth As Integer = 11906       ' A4 宽 210mm
    Public Property PageHeight As Integer = 16838      ' A4 高 297mm
    Public Property MarginTop As Integer = 1417        ' 2.5cm
    Public Property MarginRight As Integer = 1417      ' 2.5cm
    Public Property MarginBottom As Integer = 1417     ' 2.5cm
    Public Property MarginLeft As Integer = 1417       ' 2.5cm

    ''' <summary>Table 方法是否采用三线表（规范：表的编排宜采用国际通行的三线表）。</summary>
    Public Property UseThreeLineTable As Boolean = True

    ' ========================================================================
    ' 样式属性（公开可自定义；默认值 = 江南大学 2025 规范）
    ' ========================================================================

    ''' <summary>章标题（Heading 1）：三号黑体，居中，段前段后各空 1 行（16pt）。</summary>
    Public Property Heading1Style As WordStyle

    ''' <summary>节标题（Heading 2）：四号宋体加粗，左对齐，段前段后各空 0.5 行（7pt）。</summary>
    Public Property Heading2Style As WordStyle

    ''' <summary>小节标题（Heading 3）：小四号宋体加粗，左对齐，段前段后各空 0.5 行（6pt）。</summary>
    Public Property Heading3Style As WordStyle

    ''' <summary>四级标题（Heading 4，规范未定义，按小节标题降级处理）：小四号宋体加粗，左对齐。</summary>
    Public Property Heading4Style As WordStyle

    ''' <summary>五级标题（Heading 5，规范未定义，降级为五号宋体加粗）：左对齐。</summary>
    Public Property Heading5Style As WordStyle

    ''' <summary>六级标题（Heading 6，规范未定义，降级为五号宋体加粗）：左对齐。</summary>
    Public Property Heading6Style As WordStyle

    ''' <summary>正文段落：小四号宋体，首行缩进 2 字符（24pt），1.25 倍行距，两端对齐。</summary>
    Public Property BodyStyle As WordStyle

    ''' <summary>论文题目（DocTitle）：黑体小二号，居中（封面规范：中文题目黑体小二号）。</summary>
    Public Property TitleStyle As WordStyle

    ''' <summary>代码块：五号 Consolas（西文等宽）/宋体（中文），黑色，无底纹。</summary>
    Public Property CodeStyle As WordStyle

    ''' <summary>引用块：小四号宋体，首行缩进 2 字符，1.25 倍行距，无底纹、不倾斜。</summary>
    Public Property BlockquoteStyle As WordStyle

    ''' <summary>表格文字（表头字体/字号与单元格内容）：五号字（规范：表格内容用五号字）。</summary>
    Public Property TableTextStyle As WordStyle

    ''' <summary>图题/题注（Image 的 caption）：五号字，居中对齐（规范：图题字号五号，居中对齐）。</summary>
    Public Property CaptionStyle As WordStyle

    ''' <summary>表格边框/底纹样式：黑色细边框，表头白底黑字，无隔行底纹。</summary>
    Public Property Table As TableStyle

    ' ========================================================================
    ' 构造：按江南大学 2025 规范初始化默认样式
    ' ========================================================================

    Public Sub New()
        ' -- 章标题：三号(16pt)黑体，居中，段前段后各空 1 行(16pt) --
        Heading1Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseHeadingFont,
            .Size = SizeSanHao,
            .Bold = False,
            .ForeColor = WordColors.Black,
            .Alignment = "center",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeSanHao,
            .SpaceAfter = SizeSanHao
        }

        ' -- 节标题：四号(14pt)宋体加粗，左对齐，段前段后各空 0.5 行(7pt) --
        Heading2Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeSiHao,
            .Bold = True,
            .ForeColor = WordColors.Black,
            .Alignment = "left",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeSiHao / 2,
            .SpaceAfter = SizeSiHao / 2
        }

        ' -- 小节标题：小四号(12pt)宋体加粗，左对齐，段前段后各空 0.5 行(6pt) --
        Heading3Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeXiaoSi,
            .Bold = True,
            .ForeColor = WordColors.Black,
            .Alignment = "left",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeXiaoSi / 2,
            .SpaceAfter = SizeXiaoSi / 2
        }

        ' -- 四级标题（规范未定义，按小节标题样式处理） --
        Heading4Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeXiaoSi,
            .Bold = True,
            .ForeColor = WordColors.Black,
            .Alignment = "left",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeXiaoSi / 2,
            .SpaceAfter = SizeXiaoSi / 2
        }

        ' -- 五级标题（规范未定义，降级为五号宋体加粗） --
        Heading5Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeWuHao,
            .Bold = True,
            .ForeColor = WordColors.Black,
            .Alignment = "left",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeWuHao / 2,
            .SpaceAfter = SizeWuHao / 2
        }

        ' -- 六级标题（规范未定义，降级为五号宋体加粗） --
        Heading6Style = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeWuHao,
            .Bold = True,
            .ForeColor = WordColors.Black,
            .Alignment = "left",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeWuHao / 2,
            .SpaceAfter = SizeWuHao / 2
        }

        ' -- 正文：小四号(12pt)宋体，首行缩进 2 字符(24pt)，1.25 倍行距 --
        BodyStyle = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeXiaoSi,
            .Bold = False,
            .ForeColor = WordColors.Black,
            .Alignment = "justify",
            .LineSpacing = 1.25,
            .FirstLineIndent = SizeXiaoSi * 2,
            .SpaceBefore = 0,
            .SpaceAfter = 0
        }

        ' -- 论文题目：黑体小二号(18pt)，居中（封面规范：中文题目黑体小二号） --
        TitleStyle = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseHeadingFont,
            .Size = SizeXiaoEr,
            .Bold = False,
            .ForeColor = WordColors.Black,
            .Alignment = "center",
            .LineSpacing = 1.25,
            .SpaceBefore = SizeXiaoEr,
            .SpaceAfter = SizeXiaoEr
        }

        ' -- 代码块：五号 Consolas/宋体，黑色，无底纹 --
        CodeStyle = New WordStyle With {
            .FontName = "Consolas",
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeWuHao,
            .ForeColor = WordColors.Black,
            .BackColor = "",
            .SpaceBefore = 6,
            .SpaceAfter = 6
        }

        ' -- 引用块：小四号宋体，首行缩进 2 字符，1.25 倍行距，无底纹、不倾斜 --
        BlockquoteStyle = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeXiaoSi,
            .Italic = False,
            .ForeColor = WordColors.Black,
            .BackColor = "",
            .Alignment = "justify",
            .LineSpacing = 1.25,
            .FirstLineIndent = SizeXiaoSi * 2,
            .SpaceBefore = 6,
            .SpaceAfter = 6
        }

        ' -- 表格文字：五号字（规范：表格内容用五号字） --
        TableTextStyle = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeWuHao,
            .Bold = False,
            .ForeColor = WordColors.Black,
            .Alignment = "left"
        }

        ' -- 图题/表题：五号(10.5pt)，居中对齐（规范：图题和表题字号为五号，居中对齐） --
        CaptionStyle = New WordStyle With {
            .FontName = LatinFont,
            .FontNameEastAsia = ChineseBodyFont,
            .Size = SizeWuHao,
            .Bold = False,
            .Italic = False,
            .ForeColor = WordColors.Black,
            .Alignment = "center",
            .LineSpacing = 1.25,
            .SpaceBefore = 6,
            .SpaceAfter = 6
        }

        ' -- 表格边框：黑色细线；表头白底黑字；无隔行底纹（三线表模式下仅三线起作用） --
        Table = New TableStyle With {
            .HeaderBackColor = WordColors.White,
            .HeaderForeColor = WordColors.Black,
            .HeaderBold = True,
            .BorderColor = WordColors.Black,
            .BorderSize = 4,
            .AltRowBackColor = "",
            .CellPadding = 120
        }
    End Sub

    ' ========================================================================
    ' 应用主题
    ' ========================================================================

    ''' <summary>
    ''' 将本主题的全部样式应用到 <see cref="WordDocument"/>（页面设置 + 各级标题/正文/
    ''' 表格/代码/引用/题目/表格文字/题注样式 + 三线表开关）。
    ''' 应在调用 <see cref="WordDocument.WriteBlocks"/> 等内容写入方法之前调用。
    ''' </summary>
    Public Overridable Function ApplyTo(doc As WordDocument) As WordDocument
        If doc Is Nothing Then Return doc

        Return doc _
            .PageSetup(PageWidth, PageHeight, MarginTop, MarginRight, MarginBottom, MarginLeft) _
            .HeadingStyle(1, Heading1Style.Clone()) _
            .HeadingStyle(2, Heading2Style.Clone()) _
            .HeadingStyle(3, Heading3Style.Clone()) _
            .HeadingStyle(4, Heading4Style.Clone()) _
            .HeadingStyle(5, Heading5Style.Clone()) _
            .HeadingStyle(6, Heading6Style.Clone()) _
            .ParagraphStyle(BodyStyle.Clone()) _
            .TitleStyle(TitleStyle.Clone()) _
            .CodeStyle(CodeStyle.Clone()) _
            .BlockquoteStyle(BlockquoteStyle.Clone()) _
            .TableTextStyle(TableTextStyle.Clone()) _
            .CaptionStyle(CaptionStyle.Clone()) _
            .TableStyle(New TableStyle With {
                .HeaderBackColor = Table.HeaderBackColor,
                .HeaderForeColor = Table.HeaderForeColor,
                .HeaderBold = Table.HeaderBold,
                .BorderColor = Table.BorderColor,
                .BorderSize = Table.BorderSize,
                .AltRowBackColor = Table.AltRowBackColor,
                .CellPadding = Table.CellPadding
            }) _
            .ThreeLineTable(UseThreeLineTable)
    End Function

End Class
