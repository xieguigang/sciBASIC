Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Xml.Linq

''' <summary>
''' .pptx 文件打包器：将 PresentationDocument 的页面与元素序列化为
''' 符合 OOXML/PresentationML 规范的 OPC/ZIP 包。
''' 
''' 生成的部件：
'''   [Content_Types].xml, _rels/.rels, docProps/*
'''   ppt/presentation.xml, ppt/_rels/presentation.xml.rels
'''   ppt/slideMasters/slideMaster1.xml, ppt/slideLayouts/slideLayout1.xml
'''   ppt/theme/theme1.xml
'''   ppt/slides/slideN.xml, ppt/slides/_rels/slideN.xml.rels
'''   ppt/media/imageN.png
''' </summary>
Public Class PptxPackager

    Private Const NsContentTypes As String = "http://schemas.openxmlformats.org/package/2006/content-types"
    Private Const NsPackageRels As String = "http://schemas.openxmlformats.org/package/2006/relationships"
    Private Const NsOfficeRels As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    Private Const NsCoreProps As String = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
    Private Const NsDc As String = "http://purl.org/dc/elements/1.1/"
    Private Const NsDcTerms As String = "http://purl.org/dc/terms/"
    Private Const NsXsi As String = "http://www.w3.org/2001/XMLSchema-instance"
    Private Const NsAppProps As String = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"
    Private Const NsVt As String = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"

    Private Shared ReadOnly TextEncoding As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

    Private Shared ReadOnly XContentType As XNamespace = NsContentTypes
    Private Shared ReadOnly XRel As XNamespace = NsPackageRels
    Private Shared ReadOnly XP As XNamespace = PptxDrawing.SlideXmlns
    Private Shared ReadOnly XA As XNamespace = PptxDrawing.DrawingXmlns
    Private Shared ReadOnly XR As XNamespace = PptxDrawing.OfficeRelXmlns
    Private Shared ReadOnly XCore As XNamespace = NsCoreProps
    Private Shared ReadOnly XDc As XNamespace = NsDc
    Private Shared ReadOnly XDcTerms As XNamespace = NsDcTerms
    Private Shared ReadOnly XXsi As XNamespace = NsXsi
    Private Shared ReadOnly XApp As XNamespace = NsAppProps
    Private Shared ReadOnly XVt As XNamespace = NsVt

    ''' <summary>保存为 .pptx 文件。</summary>
    Public Sub Save(doc As PresentationDocument, filePath As String)
        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Dim dir As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write)
            Call SaveToStream(doc, fs)
        End Using
    End Sub

    ''' <summary>保存到流。</summary>
    Public Sub SaveAs(doc As PresentationDocument, destination As Stream, Optional leaveOpen As Boolean = False)
        If doc Is Nothing Then
            Throw New ArgumentNullException(NameOf(doc))
        End If

        Call SaveToStream(doc, destination)

        If Not leaveOpen Then
            destination.Dispose()
        End If
    End Sub

    Private Sub SaveToStream(doc As PresentationDocument, destination As Stream)
        Using archive As New ZipArchive(destination, ZipArchiveMode.Create, leaveOpen:=True)
            ' 按页面写出 slide XML 并收集图片部件
            Dim slideXml As New List(Of XDocument)
            Dim slideRels As New List(Of IReadOnlyList(Of PptImagePart))
            Dim imageParts As New List(Of PptImagePart)
            Dim globalImageIndex As Integer = 0

            For Each page As PresentationPageGraphic In doc.Pages
                Dim context As New PptWriteContext(page.Dpi)
                slideXml.Add(BuildSlide(page, context))
                slideRels.Add(context.Images)

                For Each image As PptImagePart In context.Images
                    globalImageIndex += 1
                    image.PartName = $"image{globalImageIndex}.png"
                    imageParts.Add(image)
                Next
            Next

            Call WriteText(archive, "[Content_Types].xml", Serialize(BuildContentTypes(doc.Pages.Count)))
            Call WriteText(archive, "_rels/.rels", Serialize(BuildRootRels()))
            Call WriteText(archive, "docProps/core.xml", Serialize(BuildCoreProps()))
            Call WriteText(archive, "docProps/app.xml", Serialize(BuildAppProps(doc.Pages.Count)))
            Call WriteText(archive, "ppt/presentation.xml", Serialize(BuildPresentation(doc)))
            Call WriteText(archive, "ppt/_rels/presentation.xml.rels", Serialize(BuildPresentationRels(doc.Pages.Count)))
            Call WriteText(archive, "ppt/slideMasters/slideMaster1.xml", Serialize(BuildSlideMaster()))
            Call WriteText(archive, "ppt/slideMasters/_rels/slideMaster1.xml.rels", Serialize(BuildSlideMasterRels()))
            Call WriteText(archive, "ppt/slideLayouts/slideLayout1.xml", Serialize(BuildSlideLayout()))
            Call WriteText(archive, "ppt/slideLayouts/_rels/slideLayout1.xml.rels", Serialize(BuildSlideLayoutRels()))
            Call WriteText(archive, "ppt/theme/theme1.xml", Serialize(BuildTheme()))

            For i As Integer = 0 To slideXml.Count - 1
                Dim number As Integer = i + 1
                Call WriteText(archive, $"ppt/slides/slide{number}.xml", Serialize(slideXml(i)))
                Call WriteText(archive, $"ppt/slides/_rels/slide{number}.xml.rels", Serialize(BuildSlideRels(slideRels(i))))
            Next

            ' 图片按关系名写出（PptImagePart.PartName 与 slide rels 中的 Target 对应）
            For Each image As PptImagePart In imageParts
                Call WriteBinary(archive, $"ppt/media/{image.PartName}", image.Data)
            Next
        End Using
    End Sub

    ' ------------------------------------------------------------------------
    ' ZIP 写入
    ' ------------------------------------------------------------------------

    Private Shared Sub WriteText(archive As ZipArchive, partName As String, content As String)
        Dim entry As ZipArchiveEntry = archive.CreateEntry(partName, CompressionLevel.Optimal)
        Using stream As Stream = entry.Open()
            Dim data As Byte() = TextEncoding.GetBytes(If(content, ""))
            Call stream.Write(data, 0, data.Length)
        End Using
    End Sub

    Private Shared Sub WriteBinary(archive As ZipArchive, partName As String, data As Byte())
        If data Is Nothing Then
            Return
        End If

        Dim entry As ZipArchiveEntry = archive.CreateEntry(partName, CompressionLevel.Optimal)
        Using stream As Stream = entry.Open()
            Call stream.Write(data, 0, data.Length)
        End Using
    End Sub

    Private Shared Function Serialize(document As XDocument) As String
        Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & document.ToString(SaveOptions.DisableFormatting)
    End Function

    ' ------------------------------------------------------------------------
    ' 包部件
    ' ------------------------------------------------------------------------

    Private Shared Function BuildContentTypes(slideCount As Integer) As XDocument
        Dim types As New XElement(XContentType + "Types",
                                  New XElement(XContentType + "Default", New XAttribute("Extension", "rels"), New XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                                  New XElement(XContentType + "Default", New XAttribute("Extension", "xml"), New XAttribute("ContentType", "application/xml")),
                                  New XElement(XContentType + "Default", New XAttribute("Extension", "png"), New XAttribute("ContentType", "image/png")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/ppt/presentation.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/ppt/slideMasters/slideMaster1.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/ppt/slideLayouts/slideLayout1.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/ppt/theme/theme1.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.theme+xml")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/docProps/core.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml")),
                                  New XElement(XContentType + "Override", New XAttribute("PartName", "/docProps/app.xml"), New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml")))

        For i As Integer = 1 To slideCount
            types.Add(New XElement(XContentType + "Override",
                                   New XAttribute("PartName", $"/ppt/slides/slide{i}.xml"),
                                   New XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slide+xml")))
        Next

        Return New XDocument(types)
    End Function

    Private Shared Function BuildRootRels() As XDocument
        Dim rels As New XElement(XRel + "Relationships",
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId1"),
                                              New XAttribute("Type", NsOfficeRels & "/officeDocument"), New XAttribute("Target", "ppt/presentation.xml")),
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId2"),
                                              New XAttribute("Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"), New XAttribute("Target", "docProps/core.xml")),
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId3"),
                                              New XAttribute("Type", NsOfficeRels & "/extended-properties"), New XAttribute("Target", "docProps/app.xml")))
        Return New XDocument(rels)
    End Function

    Private Shared Function BuildCoreProps() As XDocument
        Dim now As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        Dim core As New XElement(XCore + "coreProperties",
                                 New XAttribute(XNamespace.Xmlns + "dc", NsDc),
                                 New XAttribute(XNamespace.Xmlns + "dcterms", NsDcTerms),
                                 New XAttribute(XNamespace.Xmlns + "xsi", NsXsi),
                                 New XElement(XDc + "title", "Presentation"),
                                 New XElement(XDc + "creator", "Microsoft.VisualBasic.MIME.Office.PPTX"),
                                 New XElement(XCore + "lastModifiedBy", "Microsoft.VisualBasic.MIME.Office.PPTX"),
                                 New XElement(XDcTerms + "created", New XAttribute(XXsi + "type", "dcterms:W3CDTF"), now),
                                 New XElement(XDcTerms + "modified", New XAttribute(XXsi + "type", "dcterms:W3CDTF"), now))
        Return New XDocument(core)
    End Function

    Private Shared Function BuildAppProps(slideCount As Integer) As XDocument
        Dim app As New XElement(XApp + "Properties",
                                New XAttribute(XNamespace.Xmlns + "vt", NsVt),
                                New XElement(XApp + "Application", "Microsoft Visual Basic PPTX Writer"),
                                New XElement(XApp + "AppVersion", "1.0"),
                                New XElement(XApp + "Slides", slideCount),
                                New XElement(XApp + "PresentationFormat", "On-screen Show (16:9)"))
        Return New XDocument(app)
    End Function

    Private Shared Function BuildPresentation(doc As PresentationDocument) As XDocument
        Dim presentation As New XElement(XP + "presentation",
                                         New XAttribute(XNamespace.Xmlns + "a", PptxDrawing.DrawingXmlns),
                                         New XAttribute(XNamespace.Xmlns + "r", PptxDrawing.OfficeRelXmlns),
                                         New XAttribute("saveSubsetFonts", 1))

        presentation.Add(New XElement(XP + "sldMasterIdLst",
                                      New XElement(XP + "sldMasterId", New XAttribute("id", 2147483648), New XAttribute(XR + "id", "rId1"))))

        Dim sldIdLst As New XElement(XP + "sldIdLst")
        For i As Integer = 0 To doc.Pages.Count - 1
            sldIdLst.Add(New XElement(XP + "sldId",
                                      New XAttribute("id", 256 + i),
                                      New XAttribute(XR + "id", $"rId{i + 2}")))
        Next
        presentation.Add(sldIdLst)

        Dim size As Drawing.Size = doc.PageSize
        presentation.Add(New XElement(XP + "sldSz",
                                      New XAttribute("cx", PptxDrawing.ToEmu(size.Width, 100)),
                                      New XAttribute("cy", PptxDrawing.ToEmu(size.Height, 100))))
        presentation.Add(New XElement(XP + "notesSz", New XAttribute("cx", 6858000), New XAttribute("cy", 9144000)))
        Return New XDocument(presentation)
    End Function

    Private Shared Function BuildPresentationRels(slideCount As Integer) As XDocument
        Dim rels As New XElement(XRel + "Relationships",
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId1"),
                                              New XAttribute("Type", NsOfficeRels & "/slideMaster"), New XAttribute("Target", "slideMasters/slideMaster1.xml")))

        For i As Integer = 1 To slideCount
            rels.Add(New XElement(XRel + "Relationship", New XAttribute("Id", $"rId{i + 1}"),
                                  New XAttribute("Type", NsOfficeRels & "/slide"), New XAttribute("Target", $"slides/slide{i}.xml")))
        Next

        rels.Add(New XElement(XRel + "Relationship", New XAttribute("Id", $"rId{slideCount + 2}"),
                              New XAttribute("Type", NsOfficeRels & "/theme"), New XAttribute("Target", "theme/theme1.xml")))
        Return New XDocument(rels)
    End Function

    Private Shared Function BuildSlideMaster() As XDocument
        Dim master As New XElement(XP + "sldMaster",
                                   New XAttribute(XNamespace.Xmlns + "a", PptxDrawing.DrawingXmlns),
                                   New XAttribute(XNamespace.Xmlns + "r", PptxDrawing.OfficeRelXmlns),
                                   New XElement(XP + "cSld", BuildEmptyShapeTree()),
                                   New XElement(XP + "clrMap",
                                                New XAttribute("accent1", "accent1"), New XAttribute("accent2", "accent2"),
                                                New XAttribute("accent3", "accent3"), New XAttribute("accent4", "accent4"),
                                                New XAttribute("accent5", "accent5"), New XAttribute("accent6", "accent6"),
                                                New XAttribute("bg1", "lt1"), New XAttribute("bg2", "lt2"),
                                                New XAttribute("folHlink", "folHlink"), New XAttribute("hlink", "hlink"),
                                                New XAttribute("tx1", "dk1"), New XAttribute("tx2", "dk2")),
                                   New XElement(XP + "sldLayoutIdLst",
                                                New XElement(XP + "sldLayoutId", New XAttribute("id", 2147483649), New XAttribute(XR + "id", "rId1"))),
                                   New XElement(XP + "txStyles",
                                                New XElement(XP + "titleStyle"),
                                                New XElement(XP + "bodyStyle"),
                                                New XElement(XP + "otherStyle")))
        Return New XDocument(master)
    End Function

    Private Shared Function BuildSlideMasterRels() As XDocument
        Dim rels As New XElement(XRel + "Relationships",
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId1"),
                                              New XAttribute("Type", NsOfficeRels & "/slideLayout"), New XAttribute("Target", "../slideLayouts/slideLayout1.xml")),
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId2"),
                                              New XAttribute("Type", NsOfficeRels & "/theme"), New XAttribute("Target", "../theme/theme1.xml")))
        Return New XDocument(rels)
    End Function

    Private Shared Function BuildSlideLayout() As XDocument
        Dim layout As New XElement(XP + "sldLayout",
                                   New XAttribute(XNamespace.Xmlns + "a", PptxDrawing.DrawingXmlns),
                                   New XAttribute(XNamespace.Xmlns + "r", PptxDrawing.OfficeRelXmlns),
                                   New XAttribute("type", "blank"),
                                   New XAttribute("preserve", 1),
                                   New XElement(XP + "cSld", New XAttribute("name", "Blank"), BuildEmptyShapeTree()),
                                   New XElement(XP + "clrMapOvr", New XElement(XA + "masterClrMapping")))
        Return New XDocument(layout)
    End Function

    Private Shared Function BuildSlideLayoutRels() As XDocument
        Dim rels As New XElement(XRel + "Relationships",
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId1"),
                                              New XAttribute("Type", NsOfficeRels & "/slideMaster"), New XAttribute("Target", "../slideMasters/slideMaster1.xml")))
        Return New XDocument(rels)
    End Function

    Private Shared Function BuildEmptyShapeTree() As XElement
        Dim nvGrpSpPr As New XElement(XP + "nvGrpSpPr",
                                      New XElement(XP + "cNvPr", New XAttribute("id", 1), New XAttribute("name", "")),
                                      New XElement(XP + "cNvGrpSpPr"),
                                      New XElement(XP + "nvPr"))
        Dim grpSpPr As New XElement(XP + "grpSpPr",
                                    New XElement(XA + "xfrm",
                                                 New XElement(XA + "off", New XAttribute("x", 0), New XAttribute("y", 0)),
                                                 New XElement(XA + "ext", New XAttribute("cx", 0), New XAttribute("cy", 0)),
                                                 New XElement(XA + "chOff", New XAttribute("x", 0), New XAttribute("y", 0)),
                                                 New XElement(XA + "chExt", New XAttribute("cx", 0), New XAttribute("cy", 0))))
        Return New XElement(XP + "spTree", nvGrpSpPr, grpSpPr)
    End Function

    Private Shared Function BuildSlide(page As PresentationPageGraphic, context As PptWriteContext) As XDocument
        Dim spTree As XElement = BuildEmptyShapeTree()

        ' 背景色
        If page.PageBackground.HasValue AndAlso Not page.PageBackground.Value.IsEmpty Then
            Dim bg As New XElement(XP + "bg",
                                   New XElement(XP + "bgPr",
                                                New XElement(XA + "solidFill",
                                                             New XElement(XA + "srgbClr", New XAttribute("val", PptxDrawing.Hex(page.PageBackground.Value)))),
                                                New XElement(XA + "effectLst")))
            Dim cSld As New XElement(XP + "cSld", bg, spTree)
            Dim slideWithBg As New XElement(XP + "sld",
                                            New XAttribute(XNamespace.Xmlns + "a", PptxDrawing.DrawingXmlns),
                                            New XAttribute(XNamespace.Xmlns + "r", PptxDrawing.OfficeRelXmlns),
                                            cSld,
                                            New XElement(XP + "clrMapOvr", New XElement(XA + "masterClrMapping")))
            Call AppendElements(spTree, page, context)
            Return New XDocument(slideWithBg)
        End If

        Dim slide As New XElement(XP + "sld",
                                  New XAttribute(XNamespace.Xmlns + "a", PptxDrawing.DrawingXmlns),
                                  New XAttribute(XNamespace.Xmlns + "r", PptxDrawing.OfficeRelXmlns),
                                  New XElement(XP + "cSld", spTree),
                                  New XElement(XP + "clrMapOvr", New XElement(XA + "masterClrMapping")))
        Call AppendElements(spTree, page, context)
        Return New XDocument(slide)
    End Function

    Private Shared Sub AppendElements(spTree As XElement, page As PresentationPageGraphic, context As PptWriteContext)
        For Each element As PptElement In page.Elements
            Call element.WriteTo(spTree, context)
        Next
    End Sub

    Private Shared Function BuildSlideRels(images As IReadOnlyList(Of PptImagePart)) As XDocument
        Dim rels As New XElement(XRel + "Relationships",
                                 New XElement(XRel + "Relationship", New XAttribute("Id", "rId1"),
                                              New XAttribute("Type", NsOfficeRels & "/slideLayout"), New XAttribute("Target", "../slideLayouts/slideLayout1.xml")))

        If images IsNot Nothing Then
            For Each image As PptImagePart In images
                rels.Add(New XElement(XRel + "Relationship", New XAttribute("Id", image.RelationshipId),
                                      New XAttribute("Type", NsOfficeRels & "/image"), New XAttribute("Target", $"../media/{image.PartName}")))
            Next
        End If

        Return New XDocument(rels)
    End Function

    Private Shared Function BuildTheme() As XDocument
        Dim theme As New XElement(XA + "theme",
                                  New XAttribute("name", "Office Theme"),
                                  New XElement(XA + "themeElements",
                                               BuildColorScheme(),
                                               BuildFontScheme(),
                                               BuildFormatScheme()),
                                  New XElement(XA + "objectDefaults"),
                                  New XElement(XA + "extraClrSchemeLst"))
        Return New XDocument(theme)
    End Function

    Private Shared Function BuildColorScheme() As XElement
        Dim scheme As New XElement(XA + "clrScheme", New XAttribute("name", "Office"))
        Call AddColor(scheme, "dk1", "sysClr", "windowText", "000000")
        Call AddColor(scheme, "lt1", "sysClr", "window", "FFFFFF")
        Call AddColor(scheme, "dk2", "srgbClr", Nothing, "44546A")
        Call AddColor(scheme, "lt2", "srgbClr", Nothing, "E7E6E6")
        Call AddColor(scheme, "accent1", "srgbClr", Nothing, "4472C4")
        Call AddColor(scheme, "accent2", "srgbClr", Nothing, "ED7D31")
        Call AddColor(scheme, "accent3", "srgbClr", Nothing, "A5A5A5")
        Call AddColor(scheme, "accent4", "srgbClr", Nothing, "FFC000")
        Call AddColor(scheme, "accent5", "srgbClr", Nothing, "5B9BD5")
        Call AddColor(scheme, "accent6", "srgbClr", Nothing, "70AD47")
        Call AddColor(scheme, "hlink", "srgbClr", Nothing, "0563C1")
        Call AddColor(scheme, "folHlink", "srgbClr", Nothing, "954F72")
        Return scheme
    End Function

    Private Shared Sub AddColor(scheme As XElement, name As String, tag As String, lastClr As String, value As String)
        Dim color As New XElement(XA + name, New XElement(XA + tag, New XAttribute("val", If(lastClr, value))))
        If lastClr IsNot Nothing Then
            color.Element(XA + tag).Add(New XAttribute("lastClr", value))
        End If
        scheme.Add(color)
    End Sub

    Private Shared Function BuildFontScheme() As XElement
        Dim majorFont As New XElement(XA + "majorFont",
                                      New XElement(XA + "latin", New XAttribute("typeface", "Calibri Light")),
                                      New XElement(XA + "ea", New XAttribute("typeface", "")),
                                      New XElement(XA + "cs", New XAttribute("typeface", "")))
        Dim minorFont As New XElement(XA + "minorFont",
                                      New XElement(XA + "latin", New XAttribute("typeface", "Calibri")),
                                      New XElement(XA + "ea", New XAttribute("typeface", "")),
                                      New XElement(XA + "cs", New XAttribute("typeface", "")))
        Return New XElement(XA + "fontScheme", New XAttribute("name", "Office"), majorFont, minorFont)
    End Function

    Private Shared Function BuildFormatScheme() As XElement
        Dim format As New XElement(XA + "fmtScheme", New XAttribute("name", "Office Style"))

        Dim fillStyles As New XElement(XA + "fillStyleLst")
        For i As Integer = 1 To 3
            fillStyles.Add(New XElement(XA + "solidFill",
                                        New XElement(XA + "schemeClr", New XAttribute("val", "phClr")),
                                        New XElement(XA + "shade", New XAttribute("val", If(i = 1, 0, i * 15000)))))
        Next
        format.Add(fillStyles)

        Dim lineStyles As New XElement(XA + "lnStyleLst")
        For i As Integer = 1 To 3
            lineStyles.Add(New XElement(XA + "ln", New XAttribute("w", 9525), New XAttribute("cap", "flat"), New XAttribute("cmpd", "sng"), New XAttribute("algn", "ctr"),
                                        New XElement(XA + "solidFill", New XElement(XA + "schemeClr", New XAttribute("val", "phClr"))),
                                        New XElement(XA + "prstDash", New XAttribute("val", "solid")),
                                        New XElement(XA + "round")))
        Next
        format.Add(lineStyles)

        Dim effectStyles As New XElement(XA + "effectStyleLst")
        For i As Integer = 1 To 3
            effectStyles.Add(New XElement(XA + "effectStyle", New XElement(XA + "effectLst")))
        Next
        format.Add(effectStyles)

        Dim bgFillStyles As New XElement(XA + "bgFillStyleLst")
        For i As Integer = 1 To 3
            bgFillStyles.Add(New XElement(XA + "solidFill", New XElement(XA + "schemeClr", New XAttribute("val", "phClr"))))
        Next
        format.Add(bgFillStyles)

        Return format
    End Function
End Class
