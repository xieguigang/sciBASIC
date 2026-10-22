' ============================================================================
' XpsPackager.vb - OPC 打包器
'
' 把排版结果（XpsPage 列表）与资源（字体 / 图片）写成符合 OPC 规范的 zip 包：
'
'   [Content_Types].xml                          部件内容类型清单
'   _rels/.rels                                  包级关系（固定表征 + 核心属性）
'   FixedDocumentSequence.fdseq                  固定文档序列
'   Documents/1/FixedDocument.fdoc               固定文档
'   Documents/1/_rels/FixedDocument.fdoc.rels    页面关系
'   Documents/1/Pages/N.fpage                    固定页面（Glyphs / Path / ImageBrush）
'   Documents/1/Pages/_rels/N.fpage.rels         字体与图片资源关系
'   Resources/Fonts/{guid}.odttf                 ODTTF 混淆字体（可选）
'   Resources/Images/N.png|jpg                   图片资源（可选）
'   docProps/core.xml                            文档属性
'
' XPS 与 OXPS 的差异（命名空间 / 内容类型 / 关系类型）全部由 XpsSpec 提供。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>写入 XPS/OXPS 包时携带的文档元数据。</summary>
Public Class XpsWriteMeta

    ''' <summary>文档标题。</summary>
    Public Property Title As String = ""
    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""
    ''' <summary>关键字（逗号分隔）。</summary>
    Public Property Keywords As String = ""
    ''' <summary>生成程序名称。</summary>
    Public Property Creator As String = "VB.NET XpsDocument Generator"

End Class

''' <summary>
''' XPS / OXPS 包的序列化器：把页面与资源写成 OPC(zip) 字节流。
''' </summary>
Public Class XpsPackager

    ''' <summary>UTF-8（无 BOM）编码，OPC 部件统一使用。</summary>
    Private Shared ReadOnly TextEncoding As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

    ''' <summary>
    ''' 把页面与资源打包为完整的 XPS/OXPS 包字节。
    ''' </summary>
    ''' <param name="pages">排版后的页面列表（至少一页）。</param>
    ''' <param name="images">文档中用到的图片资源。</param>
    ''' <param name="font">字体资源（回退模式下不写字体部件）。</param>
    ''' <param name="meta">文档元数据。</param>
    ''' <param name="flavor">输出方言。</param>
    ''' <returns>包字节。</returns>
    Public Shared Function Build(pages As List(Of XpsPage), images As List(Of XpsImageResource),
                                 font As XpsFontResource, meta As XpsWriteMeta,
                                 flavor As XpsFlavor) As Byte()

        Dim pageList As List(Of XpsPage) = If(pages, New List(Of XpsPage)())

        If pageList.Count = 0 Then
            Call pageList.Add(New XpsPage() With {
                .Width = XpsSpec.DefaultPageWidth / XpsSpec.TwipsPerPoint,
                .Height = XpsSpec.DefaultPageHeight / XpsSpec.TwipsPerPoint
            })
        End If

        Dim imageList As List(Of XpsImageResource) = If(images, New List(Of XpsImageResource)())
        Dim fontResource As XpsFontResource = If(font, New XpsFontResource())

        Using ms As New MemoryStream()
            Using archive As New ZipArchive(ms, ZipArchiveMode.Create, leaveOpen:=True)
                Call WriteText(archive, XpsSpec.ContentTypesPart, BuildContentTypes(imageList, fontResource, flavor))
                Call WriteText(archive, XpsSpec.PackageRelsPart, BuildPackageRels(flavor))
                Call WriteText(archive, XpsSpec.FixedDocumentSequencePart, BuildFixedDocumentSequence(flavor))
                Call WriteText(archive, XpsSpec.FixedDocumentPart, BuildFixedDocument(pageList.Count, flavor))
                Call WriteText(archive, XpsSpec.FixedDocumentRelsPart, BuildFixedDocumentRels(pageList.Count, flavor))

                For i As Integer = 0 To pageList.Count - 1
                    Dim pageNumber As Integer = i + 1

                    Call WriteText(archive, XpsSpec.PagePart(pageNumber),
                                   BuildFixedPage(pageList(i), fontResource, flavor))
                    Call WriteText(archive, XpsSpec.PageRelsPart(pageNumber),
                                   BuildPageRels(fontResource, imageList, flavor))
                Next

                If fontResource.IsEmbedded AndAlso fontResource.Data IsNot Nothing Then
                    Call WriteBinary(archive, fontResource.PartName, fontResource.Data)
                End If

                For Each image As XpsImageResource In imageList
                    If image IsNot Nothing AndAlso image.Data IsNot Nothing Then
                        Call WriteBinary(archive, image.PartName, image.Data)
                    End If
                Next

                Call WriteText(archive, XpsSpec.CorePropertiesPart, BuildCoreProperties(meta))
            End Using

            Return ms.ToArray()
        End Using
    End Function

    ' ========================================================================
    ' 包级部件
    ' ========================================================================

    Private Shared Function BuildContentTypes(images As List(Of XpsImageResource),
                                              font As XpsFontResource,
                                              flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append("<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">").Append(vbCrLf)
        Call AppendDefault(sb, "rels", XpsSpec.RelationshipsContentType)
        Call AppendDefault(sb, "fdseq", XpsSpec.FixedDocumentSequenceContentType(flavor))
        Call AppendDefault(sb, "fdoc", XpsSpec.FixedDocumentContentType(flavor))
        Call AppendDefault(sb, "fpage", XpsSpec.FixedPageContentType(flavor))

        If font IsNot Nothing AndAlso font.IsEmbedded Then
            Call AppendDefault(sb, "odttf", XpsSpec.ObfuscatedFontContentType(flavor))
        End If

        Dim hasPng As Boolean = False
        Dim hasJpeg As Boolean = False

        For Each image As XpsImageResource In images
            If image Is Nothing Then
                Continue For
            End If
            If image.ContentType = "image/png" Then
                hasPng = True
            Else
                hasJpeg = True
            End If
        Next

        If hasPng Then
            Call AppendDefault(sb, "png", "image/png")
        End If
        If hasJpeg Then
            Call AppendDefault(sb, "jpg", "image/jpeg")
            Call AppendDefault(sb, "jpeg", "image/jpeg")
        End If

        Call sb.Append($"  <Override PartName=""/{XpsSpec.CorePropertiesPart}"" ContentType=""{XpsSpec.CorePropertiesContentType}"" />").Append(vbCrLf)
        Call sb.Append("</Types>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Sub AppendDefault(sb As StringBuilder, extension As String, contentType As String)
        Call sb.Append($"  <Default Extension=""{extension}"" ContentType=""{contentType}"" />").Append(vbCrLf)
    End Sub

    Private Shared Function BuildPackageRels(flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<Relationships xmlns=""{XpsSpec.PackageRelationshipsNamespace}"">").Append(vbCrLf)
        Call sb.Append($"  <Relationship Id=""rId1"" Type=""{XpsSpec.FixedRepresentationRelationship(flavor)}"" Target=""{XpsSpec.FixedDocumentSequencePart}"" />").Append(vbCrLf)
        Call sb.Append($"  <Relationship Id=""rId2"" Type=""{XpsSpec.CorePropertiesRelationship}"" Target=""{XpsSpec.CorePropertiesPart}"" />").Append(vbCrLf)
        Call sb.Append("</Relationships>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Function BuildFixedDocumentSequence(flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<FixedDocumentSequence xmlns=""{XpsSpec.PageNamespace(flavor)}"">").Append(vbCrLf)
        Call sb.Append($"  <DocumentReference Source=""/{XpsSpec.FixedDocumentPart}"" />").Append(vbCrLf)
        Call sb.Append("</FixedDocumentSequence>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Function BuildFixedDocument(pageCount As Integer, flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<FixedDocument xmlns=""{XpsSpec.PageNamespace(flavor)}"">").Append(vbCrLf)

        For i As Integer = 1 To pageCount
            Call sb.Append($"  <PageContent Source=""/{XpsSpec.PagePart(i)}"" />").Append(vbCrLf)
        Next

        Call sb.Append("</FixedDocument>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Function BuildFixedDocumentRels(pageCount As Integer, flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<Relationships xmlns=""{XpsSpec.PackageRelationshipsNamespace}"">").Append(vbCrLf)

        For i As Integer = 1 To pageCount
            Call sb.Append($"  <Relationship Id=""rId{i}"" Type=""{XpsSpec.FixedPageRelationship(flavor)}"" Target=""Pages/{i}.fpage"" />").Append(vbCrLf)
        Next

        Call sb.Append("</Relationships>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Function BuildPageRels(font As XpsFontResource, images As List(Of XpsImageResource),
                                          flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()
        Dim id As Integer = 1

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<Relationships xmlns=""{XpsSpec.PackageRelationshipsNamespace}"">").Append(vbCrLf)

        If font IsNot Nothing AndAlso font.IsEmbedded Then
            ' 关系目标相对于 Documents/1/Pages/，需要回退两级到包根
            Call sb.Append($"  <Relationship Id=""rId{id}"" Type=""{XpsSpec.FontRelationship(flavor)}"" Target=""../../{font.PartName}"" />").Append(vbCrLf)
            id += 1
        End If

        For Each image As XpsImageResource In images
            If image Is Nothing Then
                Continue For
            End If

            Call sb.Append($"  <Relationship Id=""rId{id}"" Type=""{XpsSpec.ImageRelationship(flavor)}"" Target=""../../{image.PartName}"" />").Append(vbCrLf)
            id += 1
        Next

        Call sb.Append("</Relationships>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    ' ========================================================================
    ' 页面
    ' ========================================================================

    Private Shared Function BuildFixedPage(page As XpsPage, font As XpsFontResource,
                                           flavor As XpsFlavor) As String
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append($"<FixedPage xmlns=""{XpsSpec.PageNamespace(flavor)}"" Width=""{XpsSpec.Num(page.Width)}"" Height=""{XpsSpec.Num(page.Height)}"">").Append(vbCrLf)

        ' 底纹矩形
        For Each rect As XpsRectangle In page.Rectangles
            Call sb.Append($"  <Path Fill=""{rect.Fill}"" Data=""{RectangleData(rect)}"" />").Append(vbCrLf)
        Next

        ' 直线
        For Each line As XpsLineSegment In page.Lines
            Call sb.Append($"  <Path Stroke=""{line.Stroke}"" StrokeThickness=""{XpsSpec.Num(line.Thickness)}"" Data=""M {XpsSpec.Num(line.X1)},{XpsSpec.Num(line.Y1)} L {XpsSpec.Num(line.X2)},{XpsSpec.Num(line.Y2)}"" />").Append(vbCrLf)
        Next

        ' 图片
        For Each image As XpsImagePlacement In page.Images
            Call sb.Append("  <Path ").Append($"Data=""M {XpsSpec.Num(image.X)},{XpsSpec.Num(image.Y)} L {XpsSpec.Num(image.X + image.Width)},{XpsSpec.Num(image.Y)} " &
                                              $"L {XpsSpec.Num(image.X + image.Width)},{XpsSpec.Num(image.Y + image.Height)} " &
                                              $"L {XpsSpec.Num(image.X)},{XpsSpec.Num(image.Y + image.Height)} Z"">").Append(vbCrLf)
            Call sb.Append("    <Path.Fill>").Append(vbCrLf)
            Call sb.Append($"      <ImageBrush ImageSource=""{image.ResourceUri}"" ") _
                .Append($"Viewbox=""0,0,{image.PixelWidth},{image.PixelHeight}"" ViewboxUnits=""Absolute"" ") _
                .Append($"Viewport=""{XpsSpec.Num(image.X)},{XpsSpec.Num(image.Y)},{XpsSpec.Num(image.Width)},{XpsSpec.Num(image.Height)}"" ViewportUnits=""Absolute"" />").Append(vbCrLf)
            Call sb.Append("    </Path.Fill>").Append(vbCrLf)
            Call sb.Append("  </Path>").Append(vbCrLf)
        Next

        ' 文本
        For Each glyph As XpsGlyphs In page.Glyphs
            Call AppendGlyphs(sb, glyph, font)
        Next

        Call sb.Append("</FixedPage>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Sub AppendGlyphs(sb As StringBuilder, glyph As XpsGlyphs, font As XpsFontResource)
        If String.IsNullOrEmpty(glyph.Text) Then
            Return
        End If

        Call sb.Append("  <Glyphs")

        If font IsNot Nothing AndAlso font.IsEmbedded Then
            Call sb.Append($" FontUri=""{font.FontUri}""")
        End If

        Call sb.Append($" FontRenderingEmSize=""{XpsSpec.Num(glyph.FontSize)}""")
        Call sb.Append($" OriginX=""{XpsSpec.Num(glyph.OriginX)}"" OriginY=""{XpsSpec.Num(glyph.OriginY)}""")
        Call sb.Append($" Fill=""{glyph.Fill}""")
        Call sb.Append($" UnicodeString=""{GlyphText(glyph.Text)}""")

        If glyph.Bold AndAlso glyph.Italic Then
            Call sb.Append(" StyleSimulations=""BoldItalicSimulation""")
        ElseIf glyph.Bold Then
            Call sb.Append(" StyleSimulations=""BoldSimulation""")
        ElseIf glyph.Italic Then
            Call sb.Append(" StyleSimulations=""ItalicSimulation""")
        End If

        Call sb.Append(" />").Append(vbCrLf)
    End Sub

    Private Shared Function RectangleData(rect As XpsRectangle) As String
        Dim right As Double = rect.X + rect.Width
        Dim bottom As Double = rect.Y + rect.Height

        Return $"M {XpsSpec.Num(rect.X)},{XpsSpec.Num(rect.Y)} L {XpsSpec.Num(right)},{XpsSpec.Num(rect.Y)} " &
               $"L {XpsSpec.Num(right)},{XpsSpec.Num(bottom)} L {XpsSpec.Num(rect.X)},{XpsSpec.Num(bottom)} Z"
    End Function

    ' ========================================================================
    ' 元数据
    ' ========================================================================

    Private Shared Function BuildCoreProperties(meta As XpsWriteMeta) As String
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ",
                                                     System.Globalization.CultureInfo.InvariantCulture)
        Dim sb As New StringBuilder()

        Call sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>").Append(vbCrLf)
        Call sb.Append("<cp:coreProperties" &
                       " xmlns:cp=""http://schemas.openxmlformats.org/package/2006/metadata/core-properties""" &
                       " xmlns:dc=""http://purl.org/dc/elements/1.1/""" &
                       " xmlns:dcterms=""http://purl.org/dc/terms/""" &
                       " xmlns:dcmitype=""http://purl.org/dc/dcmitype/""" &
                       " xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">").Append(vbCrLf)

        If meta IsNot Nothing Then
            Call AppendElement(sb, "dc:title", meta.Title)
            Call AppendElement(sb, "dc:creator", meta.Author)
            Call AppendElement(sb, "dc:subject", meta.Subject)
            Call AppendElement(sb, "dc:description", meta.Description)
            Call AppendElement(sb, "cp:keywords", meta.Keywords)
            Call AppendElement(sb, "cp:lastModifiedBy", meta.Creator)
        End If

        Call sb.Append($"  <dcterms:created xsi:type=""dcterms:W3CDTF"">{now}</dcterms:created>").Append(vbCrLf)
        Call sb.Append($"  <dcterms:modified xsi:type=""dcterms:W3CDTF"">{now}</dcterms:modified>").Append(vbCrLf)
        Call sb.Append("</cp:coreProperties>").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Sub AppendElement(sb As StringBuilder, name As String, value As String)
        If String.IsNullOrEmpty(value) Then
            Return
        End If

        Call sb.Append($"  <{name}>{XpsSpec.Escape(value)}</{name}>").Append(vbCrLf)
    End Sub

    ' ========================================================================
    ' 写入辅助
    ' ========================================================================

    ''' <summary>
    ''' Glyphs/@UnicodeString 的文本转义：控制字符（制表符 / 换行）按 XPS 惯例写成
    ''' ``_xHHHH_``，其余按 XML 规则转义。
    ''' </summary>
    Private Shared Function GlyphText(text As String) As String
        Dim sb As New StringBuilder(text.Length + 16)

        For Each c As Char In text
            Select Case AscW(c)
                Case 9, 10, 13
                    Call sb.Append($"_x{AscW(c):X4}_")
                Case Else
                    Call sb.Append(c)
            End Select
        Next

        Return XpsSpec.Escape(sb.ToString())
    End Function

    Private Shared Sub WriteText(archive As ZipArchive, partName As String, content As String)
        Dim entry As ZipArchiveEntry = archive.CreateEntry(partName, CompressionLevel.Optimal)

        Using es As Stream = entry.Open()
            Dim data As Byte() = TextEncoding.GetBytes(If(content, ""))

            Call es.Write(data, 0, data.Length)
        End Using
    End Sub

    Private Shared Sub WriteBinary(archive As ZipArchive, partName As String, data As Byte())
        If String.IsNullOrEmpty(partName) OrElse data Is Nothing Then
            Return
        End If

        Dim entry As ZipArchiveEntry = archive.CreateEntry(partName, CompressionLevel.Optimal)

        Using es As Stream = entry.Open()
            Call es.Write(data, 0, data.Length)
        End Using
    End Sub

End Class
