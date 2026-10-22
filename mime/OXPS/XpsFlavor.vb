' ============================================================================
' XpsFlavor.vb - XPS / OXPS 包方言的规格常量
'
' XPS（ECMA-388，Microsoft XPS 2005/06）与 OXPS（OpenXPS，ECMA-388 2nd edition）
' 使用完全相同的 OPC(zip) 包结构，差异只在于：
'   - XML 命名空间（page / resource）
'   - 部件内容类型（fixeddocumentsequence / fixeddocument / fixedpage / 混淆字体）
'   - 关系类型（fixedrepresentation）
' 因此用一枚 XpsFlavor 枚举驱动同一套生成与解析代码，差异全部集中在本文件。
' ============================================================================

''' <summary>
''' 输出的 XPS 包方言：OpenXPS（ECMA-388 2nd edition）或 Microsoft XPS（2005/06）。
''' </summary>
Public Enum XpsFlavor
    ''' <summary>OpenXPS，.oxps 包。</summary>
    Oxps
    ''' <summary>Microsoft XPS，.xps 包。</summary>
    Xps
End Enum

''' <summary>
''' XPS / OXPS 规格常量：两种方言的命名空间、内容类型、关系类型，以及页面与
''' XML 输出相关的公共辅助函数。
''' </summary>
Public Module XpsSpec

    ''' <summary>1 磅 = 20 twips（页面尺寸入参单位）。</summary>
    Public Const TwipsPerPoint As Double = 20.0

    ''' <summary>默认页面宽度（twips，A4 210mm）。</summary>
    Public Const DefaultPageWidth As Integer = 11906

    ''' <summary>默认页面高度（twips，A4 297mm）。</summary>
    Public Const DefaultPageHeight As Integer = 16838

    ''' <summary>默认页边距（twips，1 英寸）。</summary>
    Public Const DefaultMargin As Integer = 1440

    ' ==== OPC 包固定部件名 ====

    ''' <summary>包内容类型清单。</summary>
    Public Const ContentTypesPart As String = "[Content_Types].xml"
    ''' <summary>包级关系。</summary>
    Public Const PackageRelsPart As String = "_rels/.rels"
    ''' <summary>固定文档序列部件。</summary>
    Public Const FixedDocumentSequencePart As String = "FixedDocumentSequence.fdseq"
    ''' <summary>固定文档部件。</summary>
    Public Const FixedDocumentPart As String = "Documents/1/FixedDocument.fdoc"
    ''' <summary>固定文档关系部件。</summary>
    Public Const FixedDocumentRelsPart As String = "Documents/1/_rels/FixedDocument.fdoc.rels"
    ''' <summary>文档属性部件。</summary>
    Public Const CorePropertiesPart As String = "docProps/core.xml"

    ''' <summary>OpenXML 包关系命名空间。</summary>
    Public Const PackageRelationshipsNamespace As String = "http://schemas.openxmlformats.org/package/2006/relationships"
    ''' <summary>OpenXML 核心属性内容类型。</summary>
    Public Const CorePropertiesContentType As String = "application/vnd.openxmlformats-package.core-properties+xml"
    ''' <summary>关系部件内容类型。</summary>
    Public Const RelationshipsContentType As String = "application/vnd.openxmlformats-package.relationships+xml"
    ''' <summary>核心属性关系类型。</summary>
    Public Const CorePropertiesRelationship As String = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"

    ''' <summary>第 <paramref name="pageNumber"/> 页（从 1 开始）的页面部件名。</summary>
    Public Function PagePart(pageNumber As Integer) As String
        Return $"Documents/1/Pages/{pageNumber}.fpage"
    End Function

    ''' <summary>第 <paramref name="pageNumber"/> 页（从 1 开始）的页面关系部件名。</summary>
    Public Function PageRelsPart(pageNumber As Integer) As String
        Return $"Documents/1/Pages/_rels/{pageNumber}.fpage.rels"
    End Function

    ' ==== XML 命名空间 ====

    ''' <summary>FixedPage / FixedDocument / FixedDocumentSequence 的默认命名空间。</summary>
    Public Function PageNamespace(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "http://schemas.microsoft.com/xps/2005/06"
        Else
            Return "http://schemas.openxps.org/oxps/v1.0"
        End If
    End Function

    ''' <summary>资源字典命名空间（XAML 的 x: 前缀）。</summary>
    Public Function ResourceNamespace(flavor As XpsFlavor) As String
        Return PageNamespace(flavor) & "/resource"
    End Function

    ' ==== 内容类型 ====

    ''' <summary>固定文档序列部件的内容类型。</summary>
    Public Function FixedDocumentSequenceContentType(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "application/vnd.ms-package.xps-fixeddocumentsequence+xml"
        Else
            Return "application/oxps.fixeddocumentsequence+xml"
        End If
    End Function

    ''' <summary>固定文档部件的内容类型。</summary>
    Public Function FixedDocumentContentType(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "application/vnd.ms-package.xps-fixeddocument+xml"
        Else
            Return "application/oxps.fixeddocument+xml"
        End If
    End Function

    ''' <summary>固定页面部件的内容类型。</summary>
    Public Function FixedPageContentType(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "application/vnd.ms-package.xps-fixedpage+xml"
        Else
            Return "application/oxps.fixedpage+xml"
        End If
    End Function

    ''' <summary>ODTTF（混淆 OpenType 字体）部件的内容类型。</summary>
    Public Function ObfuscatedFontContentType(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "application/vnd.ms-package.obfuscated-opentype"
        Else
            Return "application/oxps.obfuscatedopentype"
        End If
    End Function

    ' ==== 关系类型 ====

    ''' <summary>包级关系：固定表征（FixedDocumentSequence）。</summary>
    Public Function FixedRepresentationRelationship(flavor As XpsFlavor) As String
        If flavor = XpsFlavor.Xps Then
            Return "http://schemas.microsoft.com/xps/2005/06/fixedrepresentation"
        Else
            Return "http://schemas.openxps.org/oxps/v1.0/fixedrepresentation"
        End If
    End Function

    ''' <summary>固定文档关系：页面。</summary>
    Public Function FixedPageRelationship(flavor As XpsFlavor) As String
        Return PageNamespace(flavor) & "/fixedpage"
    End Function

    ''' <summary>固定页面关系：字体资源。</summary>
    Public Function FontRelationship(flavor As XpsFlavor) As String
        Return PageNamespace(flavor) & "/font"
    End Function

    ''' <summary>固定页面关系：图片资源。</summary>
    Public Function ImageRelationship(flavor As XpsFlavor) As String
        Return PageNamespace(flavor) & "/image"
    End Function

    ' ==== 输出辅助 ====

    ''' <summary>
    ''' 按不变文化格式化数值：自动去掉多余的尾随 0，保证同一输入在同一台机器上
    ''' 产生完全一致的 XML（不随区域设置变化）。
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function Num(value As Double) As String
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return "0"
        End If

        Dim text As String = value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)

        If text = "-0" Then
            Return "0"
        End If

        Return text
    End Function

    ''' <summary>
    ''' 把 twips 换算为 XPS 使用的磅值并格式化。
    ''' </summary>
    ''' <param name="value">twips 数值。</param>
    ''' <returns></returns>
    Public Function Twips(value As Integer) As String
        Return Num(value / TwipsPerPoint)
    End Function

    ''' <summary>
    ''' 转义 XML 文本节点与属性值中的特殊字符。
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Escape(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return ""
        End If

        Dim sb As New System.Text.StringBuilder(text.Length + 16)

        For Each c As Char In text
            Select Case c
                Case "&"c : Call sb.Append("&amp;")
                Case "<"c : Call sb.Append("&lt;")
                Case ">"c : Call sb.Append("&gt;")
                Case """"c : Call sb.Append("&quot;")
                Case Else : Call sb.Append(c)
            End Select
        Next

        Return sb.ToString()
    End Function

End Module
