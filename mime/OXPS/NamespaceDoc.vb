''' <summary>
''' XPS / OXPS 固定版式文档的生成与文本提取。
'''
''' <para>
''' XPS（ECMA-388，Microsoft XPS 2005/06）与 OXPS（OpenXPS，ECMA-388 2nd edition）
''' 都是 OPC(zip) 包 + 固定页面（FixedPage）模型，两者仅命名空间、内容类型与关系
''' 类型不同，因此本包用一枚 <see cref="XpsFlavor"/> 驱动同一套生成与解析代码。
''' </para>
'''
''' <para>
''' 生成链路：<see cref="XpsDocument"/> 收集内容块与样式快照 →
''' <see cref="XpsLayoutEngine"/> 完成折行、分页、表格网格与图片版面 →
''' <see cref="XpsPackager"/> 输出 OPC 包。字体通过 <see cref="XpsFontResource"/>
''' 可选嵌入（ODTTF 混淆），找不到字体时自动回退为不嵌字体的轻量文档。
''' </para>
'''
''' <para>
''' 解析链路：<see cref="XpsTextReader"/> 按 fdseq → fdoc → fpage 的关系链定位页面，
''' 逐页收集 &lt;Glyphs UnicodeString&gt; 并按基线位置重组成行，提供 ExtractText /
''' ExtractPages / ExtractParagraphs / ExtractMetadata 四个入口。
''' </para>
'''
''' <para>
''' 统一报告接口：<see cref="XpsRender"/> 继承
''' <c>Microsoft.VisualBasic.ApplicationServices.Report.ReportRender</c>，
''' 可以直接把 markdown 内容块渲染为 XPS/OXPS 报告文件，调用形态与
''' WordRender / RtfRender / PdfRender 完全一致。
''' </para>
''' </summary>
Public Class NamespaceDoc

End Class
