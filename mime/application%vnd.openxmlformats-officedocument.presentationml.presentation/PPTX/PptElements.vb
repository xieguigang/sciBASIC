Imports System.IO
Imports System.Linq
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

''' <summary>
''' DrawingML 生成时使用的单位换算、颜色与样式转换工具。
''' 画布内部坐标单位为 100 dpi 像素，写出时统一换算为 EMU（1 inch = 914400 EMU）。
''' </summary>
Public Module PptxDrawing

    Public Const EmusPerInch As Long = 914400

    Public Const SlideXmlns As String = "http://schemas.openxmlformats.org/presentationml/2006/main"
    Public Const DrawingXmlns As String = "http://schemas.openxmlformats.org/drawingml/2006/main"
    Public Const OfficeRelXmlns As String = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"

    Public ReadOnly NS_P As XNamespace = SlideXmlns
    Public ReadOnly NS_A As XNamespace = DrawingXmlns
    Public ReadOnly NS_R As XNamespace = OfficeRelXmlns

    ''' <summary>像素转 EMU。</summary>
    Public Function ToEmu(pixels As Single, dpi As Single) As Long
        If dpi <= 0 OrElse Single.IsNaN(pixels) OrElse Single.IsInfinity(pixels) Then
            Return 0
        End If

        Return CLng(std.Round(pixels / dpi * EmusPerInch))
    End Function

    ''' <summary>整数像素转 EMU。</summary>
    Public Function ToEmu(pixels As Integer, dpi As Single) As Long
        Return ToEmu(CSng(pixels), dpi)
    End Function

    ''' <summary>颜色转 DrawingML 使用的 RRGGBB 十六进制字符串。</summary>
    Public Function Hex(color As Drawing.Color) As String
        Return $"{color.R:X2}{color.G:X2}{color.B:X2}"
    End Function

    ''' <summary>从 Brush 中提取纯色；非纯色或空 Brush 返回 fallback。</summary>
    Public Function SolidColor(brush As Brush, Optional fallback As Drawing.Color = Nothing) As Drawing.Color
        If brush Is Nothing Then
            Return fallback
        End If

        Dim color As Drawing.Color = Brush.SolidColor(brush)
        Return If(color.IsEmpty, fallback, color)
    End Function

    ''' <summary>字体大小转 DrawingML 的百分之一磅。</summary>
    Public Function FontSize(font As Font) As Integer
        If font Is Nothing Then
            Return 1800
        End If

        Dim points As Single = font.SizeInPoints
        If points <= 0 Then
            points = font.Size * 0.75F
        End If

        Return CInt(std.Max(100, std.Round(points * 100)))
    End Function

    ''' <summary>Pen 宽度转 EMU。</summary>
    Public Function LineWidth(pen As Pen, dpi As Single) As Long
        If pen Is Nothing Then
            Return 12700
        End If

        Return std.Max(ToEmu(pen.Width, dpi), 1L)
    End Function

    ''' <summary>虚线样式映射为 DrawingML prstDash。</summary>
    Public Function DashPreset(pen As Pen) As String
        If pen Is Nothing Then
            Return Nothing
        End If

        Select Case pen.DashStyle
            Case DashStyle.Dash : Return "dash"
            Case DashStyle.Dot : Return "dot"
            Case DashStyle.DashDot : Return "dashDot"
            Case DashStyle.DashDotDot : Return "lgDashDotDot"
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>旋转角度转 DrawingML 的 1/60000 度。</summary>
    Public Function RotationValue(degrees As Single) As Integer
        Return CInt(std.Round(degrees * 6000))
    End Function

    ''' <summary>将图像编码为 PNG 字节。</summary>
    Public Function ImageBytes(image As Image) As Byte()
        If image Is Nothing Then
            Return Array.Empty(Of Byte)()
        End If

        Using stream As MemoryStream = image.ToStream()
            Return stream.ToArray()
        End Using
    End Function
End Module

''' <summary>图片部件信息。</summary>
Public Class PptImagePart

    Public Property PartName As String

    Public Property RelationshipId As String

    Public Property Data As Byte()
End Class

''' <summary>
''' 单张幻灯片的写出上下文：分配形状 id、注册图片关系并携带画布 dpi。
''' </summary>
Public Class PptWriteContext

    Private _nextShapeId As Integer = 2
    Private _nextImageId As Integer = 1
    Private ReadOnly _images As New List(Of PptImagePart)

    ''' <summary>rId1 固定给 slideLayout，图片关系从 rId2 开始。</summary>
    Private _nextRelId As Integer = 2

    Public Sub New(dpi As Single)
        Me.Dpi = dpi
    End Sub

    Public ReadOnly Property Dpi As Single

    Public Function NextShapeId() As Integer
        Dim id As Integer = _nextShapeId
        _nextShapeId += 1
        Return id
    End Function

    ''' <summary>注册图片并返回其在 slide rels 中的关系 id。</summary>
    Public Function RegisterImage(image As Image) As String
        Dim index As Integer = _nextImageId
        _nextImageId += 1

        Dim part As New PptImagePart With {
            .PartName = $"image{index}.png",
            .RelationshipId = $"rId{_nextRelId}",
            .Data = PptxDrawing.ImageBytes(image)
        }
        _nextRelId += 1
        _images.Add(part)
        Return part.RelationshipId
    End Function

    Public ReadOnly Property Images As IReadOnlyList(Of PptImagePart)
        Get
            Return _images
        End Get
    End Property
End Class

''' <summary>幻灯片元素基类。</summary>
Public MustInherit Class PptElement

    ''' <summary>将元素序列化为一个或多个 DrawingML 节点并追加到 spTree。</summary>
    Public MustOverride Sub WriteTo(spTree As XElement, context As PptWriteContext)
End Class

''' <summary>几何类型。</summary>
Public Enum PptShapeGeometry
    Rectangle
    RoundedRectangle
    Ellipse
    Line
    Freeform
End Enum

''' <summary>
''' 形状元素：矩形、圆角矩形、椭圆、直线或采样后的自由形状。
''' </summary>
Public Class PptShapeElement : Inherits PptElement

    Public Property Geometry As PptShapeGeometry

    ''' <summary>未旋转状态下的左上角坐标（像素）。</summary>
    Public Property Position As Drawing.PointF

    ''' <summary>未旋转状态下的尺寸（像素）。</summary>
    Public Property Extent As Drawing.SizeF

    ''' <summary>绕形状中心旋转的角度（度）。</summary>
    Public Property Rotation As Single

    ''' <summary>Nothing 表示无填充。</summary>
    Public Property Fill As Drawing.Color?

    ''' <summary>Nothing 表示无描边。</summary>
    Public Property Stroke As Pen

    ''' <summary>Freeform 使用的采样点（像素，未旋转）。</summary>
    Public Property Points As List(Of Drawing.PointF)

    ''' <summary>Freeform 是否闭合。</summary>
    Public Property IsClosed As Boolean

    Public Overrides Sub WriteTo(spTree As XElement, context As PptWriteContext)
        Dim id As Integer = context.NextShapeId()
        Dim sp As New XElement(PptxDrawing.NS_P + "sp")

        sp.Add(New XElement(PptxDrawing.NS_P + "nvSpPr",
                            New XElement(PptxDrawing.NS_P + "cNvPr", New XAttribute("id", id), New XAttribute("name", $"Shape {id}")),
                            New XElement(PptxDrawing.NS_P + "cNvSpPr"),
                            New XElement(PptxDrawing.NS_P + "nvPr")))

        Dim spPr As New XElement(PptxDrawing.NS_P + "spPr")
        spPr.Add(BuildTransform(context.Dpi))
        spPr.Add(BuildGeometry(context.Dpi))

        If Geometry = PptShapeGeometry.Line Then
            spPr.Add(New XElement(PptxDrawing.NS_A + "noFill"))
        Else
            spPr.Add(BuildFill())
        End If

        Dim line As XElement = BuildLine(context.Dpi)
        If line IsNot Nothing Then
            spPr.Add(line)
        End If

        sp.Add(spPr)
        spTree.Add(sp)
    End Sub

    Private Function BuildTransform(dpi As Single) As XElement
        Dim xfrm As New XElement(PptxDrawing.NS_A + "xfrm")

        If Rotation <> 0 Then
            xfrm.Add(New XAttribute("rot", PptxDrawing.RotationValue(Rotation)))
        End If

        xfrm.Add(New XElement(PptxDrawing.NS_A + "off",
                              New XAttribute("x", PptxDrawing.ToEmu(Position.X, dpi)),
                              New XAttribute("y", PptxDrawing.ToEmu(Position.Y, dpi))))
        xfrm.Add(New XElement(PptxDrawing.NS_A + "ext",
                              New XAttribute("cx", PptxDrawing.ToEmu(std.Max(Extent.Width, 0), dpi)),
                              New XAttribute("cy", PptxDrawing.ToEmu(std.Max(Extent.Height, 0), dpi))))
        Return xfrm
    End Function

    Private Function BuildGeometry(dpi As Single) As XElement
        Select Case Geometry
            Case PptShapeGeometry.Rectangle
                Return New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "rect"), New XElement(PptxDrawing.NS_A + "avLst"))
            Case PptShapeGeometry.RoundedRectangle
                Return New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "roundRect"), New XElement(PptxDrawing.NS_A + "avLst"))
            Case PptShapeGeometry.Ellipse
                Return New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "ellipse"), New XElement(PptxDrawing.NS_A + "avLst"))
            Case PptShapeGeometry.Line
                Return New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "line"), New XElement(PptxDrawing.NS_A + "avLst"))
            Case Else
                Return BuildFreeformGeometry(dpi)
        End Select
    End Function

    Private Function BuildFreeformGeometry(dpi As Single) As XElement
        Dim custGeom As New XElement(PptxDrawing.NS_A + "custGeom",
                                     New XElement(PptxDrawing.NS_A + "avLst"),
                                     New XElement(PptxDrawing.NS_A + "gdLst"),
                                     New XElement(PptxDrawing.NS_A + "ahLst"),
                                     New XElement(PptxDrawing.NS_A + "cxnLst"))

        Dim widthEmu As Long = PptxDrawing.ToEmu(std.Max(Extent.Width, 0), dpi)
        Dim heightEmu As Long = PptxDrawing.ToEmu(std.Max(Extent.Height, 0), dpi)

        custGeom.Add(New XElement(PptxDrawing.NS_A + "rect",
                                  New XAttribute("l", 0), New XAttribute("t", 0),
                                  New XAttribute("r", widthEmu), New XAttribute("b", heightEmu)))

        Dim path As New XElement(PptxDrawing.NS_A + "path",
                                 New XAttribute("w", widthEmu), New XAttribute("h", heightEmu))

        If Points IsNot Nothing AndAlso Points.Count > 0 Then
            For i As Integer = 0 To Points.Count - 1
                Dim x As Long = PptxDrawing.ToEmu(Points(i).X - Position.X, dpi)
                Dim y As Long = PptxDrawing.ToEmu(Points(i).Y - Position.Y, dpi)
                Dim point As New XElement(PptxDrawing.NS_A + "pt", New XAttribute("x", x), New XAttribute("y", y))

                If i = 0 Then
                    path.Add(New XElement(PptxDrawing.NS_A + "moveTo", point))
                Else
                    path.Add(New XElement(PptxDrawing.NS_A + "lnTo", point))
                End If
            Next

            If IsClosed Then
                path.Add(New XElement(PptxDrawing.NS_A + "close"))
            End If
        End If

        custGeom.Add(New XElement(PptxDrawing.NS_A + "pathLst", path))
        Return custGeom
    End Function

    Private Function BuildFill() As XElement
        If Not Fill.HasValue Then
            Return New XElement(PptxDrawing.NS_A + "noFill")
        End If

        Return New XElement(PptxDrawing.NS_A + "solidFill",
                            New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(Fill.Value))))
    End Function

    Private Function BuildLine(dpi As Single) As XElement
        If Stroke Is Nothing Then
            Return Nothing
        End If

        Dim color As Drawing.Color = PptxDrawing.SolidColor(Stroke.Brush, Stroke.Color)
        If color.IsEmpty Then
            color = Drawing.Color.Black
        End If

        Dim line As New XElement(PptxDrawing.NS_A + "ln", New XAttribute("w", PptxDrawing.LineWidth(Stroke, dpi)))
        line.Add(New XElement(PptxDrawing.NS_A + "solidFill",
                              New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(color)))))

        Dim dash As String = PptxDrawing.DashPreset(Stroke)
        If dash IsNot Nothing Then
            line.Add(New XElement(PptxDrawing.NS_A + "prstDash", New XAttribute("val", dash)))
        End If

        Return line
    End Function
End Class

''' <summary>文本对齐方式。</summary>
Public Enum PptTextAlign
    Left
    Center
    Right
End Enum

''' <summary>文本元素：可编辑的 PPT 文本框。</summary>
Public Class PptTextElement : Inherits PptElement

    Public Property Position As Drawing.PointF

    Public Property Extent As Drawing.SizeF

    Public Property Rotation As Single

    Public Property Text As String

    Public Property Font As Font

    Public Property Color As Drawing.Color

    Public Property Alignment As PptTextAlign

    Public Overrides Sub WriteTo(spTree As XElement, context As PptWriteContext)
        Dim id As Integer = context.NextShapeId()
        Dim sp As New XElement(PptxDrawing.NS_P + "sp")

        sp.Add(New XElement(PptxDrawing.NS_P + "nvSpPr",
                            New XElement(PptxDrawing.NS_P + "cNvPr", New XAttribute("id", id), New XAttribute("name", $"TextBox {id}")),
                            New XElement(PptxDrawing.NS_P + "cNvSpPr", New XAttribute("txBox", 1)),
                            New XElement(PptxDrawing.NS_P + "nvPr")))

        Dim xfrm As New XElement(PptxDrawing.NS_A + "xfrm")
        If Rotation <> 0 Then
            xfrm.Add(New XAttribute("rot", PptxDrawing.RotationValue(Rotation)))
        End If
        xfrm.Add(New XElement(PptxDrawing.NS_A + "off",
                              New XAttribute("x", PptxDrawing.ToEmu(Position.X, context.Dpi)),
                              New XAttribute("y", PptxDrawing.ToEmu(Position.Y, context.Dpi))))
        xfrm.Add(New XElement(PptxDrawing.NS_A + "ext",
                              New XAttribute("cx", PptxDrawing.ToEmu(std.Max(Extent.Width, 0), context.Dpi)),
                              New XAttribute("cy", PptxDrawing.ToEmu(std.Max(Extent.Height, 0), context.Dpi))))

        sp.Add(New XElement(PptxDrawing.NS_P + "spPr",
                            xfrm,
                            New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "rect"), New XElement(PptxDrawing.NS_A + "avLst")),
                            New XElement(PptxDrawing.NS_A + "noFill")))

        Dim bodyPr As New XElement(PptxDrawing.NS_A + "bodyPr",
                                   New XAttribute("wrap", "square"),
                                   New XAttribute("lIns", 0), New XAttribute("tIns", 0),
                                   New XAttribute("rIns", 0), New XAttribute("bIns", 0))

        Dim anchor As String = If(Alignment = PptTextAlign.Center, "ctr", If(Alignment = PptTextAlign.Right, "r", "l"))
        bodyPr.Add(New XAttribute("anchorCtr", 0))

        Dim txBody As New XElement(PptxDrawing.NS_P + "txBody", bodyPr, New XElement(PptxDrawing.NS_A + "lstStyle"))

        Dim lines As String() = If(String.IsNullOrEmpty(Text), New String() {""}, Text.Replace(vbCrLf, vbLf).Split(New Char() {vbLf}, StringSplitOptions.None))
        For Each line As String In lines
            Dim paragraph As New XElement(PptxDrawing.NS_A + "p")
            Dim pPr As New XElement(PptxDrawing.NS_A + "pPr", New XAttribute("algn", anchor))
            paragraph.Add(pPr)

            Dim run As New XElement(PptxDrawing.NS_A + "r")
            Dim rPr As New XElement(PptxDrawing.NS_A + "rPr",
                                    New XAttribute("lang", "en-US"),
                                    New XAttribute("sz", PptxDrawing.FontSize(Font)))

            If Font IsNot Nothing AndAlso Font.Bold Then
                rPr.Add(New XAttribute("b", 1))
            End If
            If Font IsNot Nothing AndAlso Font.Italic Then
                rPr.Add(New XAttribute("i", 1))
            End If

            rPr.Add(New XElement(PptxDrawing.NS_A + "solidFill",
                                 New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(Color)))))

            If Font IsNot Nothing AndAlso Not String.IsNullOrEmpty(Font.Name) Then
                rPr.Add(New XElement(PptxDrawing.NS_A + "latin", New XAttribute("typeface", Font.Name)))
            End If

            run.Add(rPr)
            run.Add(New XElement(PptxDrawing.NS_A + "t", line))
            paragraph.Add(run)
            txBody.Add(paragraph)
        Next

        sp.Add(txBody)
        spTree.Add(sp)
    End Sub
End Class

''' <summary>图片元素。</summary>
Public Class PptImageElement : Inherits PptElement

    Public Property Position As Drawing.PointF

    Public Property Extent As Drawing.SizeF

    Public Property Rotation As Single

    Public Property Image As Image

    Public Overrides Sub WriteTo(spTree As XElement, context As PptWriteContext)
        If Image Is Nothing Then
            Return
        End If

        Dim id As Integer = context.NextShapeId()
        Dim relId As String = context.RegisterImage(Image)

        Dim pic As New XElement(PptxDrawing.NS_P + "pic")
        pic.Add(New XElement(PptxDrawing.NS_P + "nvPicPr",
                             New XElement(PptxDrawing.NS_P + "cNvPr", New XAttribute("id", id), New XAttribute("name", $"Picture {id}")),
                             New XElement(PptxDrawing.NS_P + "cNvPicPr", New XElement(PptxDrawing.NS_A + "picLocks", New XAttribute("noChangeAspect", 1))),
                             New XElement(PptxDrawing.NS_P + "nvPr")))

        pic.Add(New XElement(PptxDrawing.NS_P + "blipFill",
                             New XElement(PptxDrawing.NS_A + "blip", New XAttribute(PptxDrawing.NS_R + "embed", relId)),
                             New XElement(PptxDrawing.NS_A + "stretch", New XElement(PptxDrawing.NS_A + "fillRect"))))

        Dim xfrm As New XElement(PptxDrawing.NS_A + "xfrm")
        If Rotation <> 0 Then
            xfrm.Add(New XAttribute("rot", PptxDrawing.RotationValue(Rotation)))
        End If
        xfrm.Add(New XElement(PptxDrawing.NS_A + "off",
                              New XAttribute("x", PptxDrawing.ToEmu(Position.X, context.Dpi)),
                              New XAttribute("y", PptxDrawing.ToEmu(Position.Y, context.Dpi))))
        xfrm.Add(New XElement(PptxDrawing.NS_A + "ext",
                              New XAttribute("cx", PptxDrawing.ToEmu(std.Max(Extent.Width, 0), context.Dpi)),
                              New XAttribute("cy", PptxDrawing.ToEmu(std.Max(Extent.Height, 0), context.Dpi))))

        pic.Add(New XElement(PptxDrawing.NS_P + "spPr",
                             xfrm,
                             New XElement(PptxDrawing.NS_A + "prstGeom", New XAttribute("prst", "rect"), New XElement(PptxDrawing.NS_A + "avLst"))))

        spTree.Add(pic)
    End Sub
End Class

''' <summary>表格样式。</summary>
Public Class PptTableStyle

    Public Property ShowHeader As Boolean = True

    Public Property HeaderFill As Drawing.Color = Drawing.Color.FromArgb(45, 90, 135)

    Public Property BodyFill As Drawing.Color = Drawing.Color.White

    Public Property AlternateFill As Drawing.Color = Drawing.Color.FromArgb(240, 244, 248)

    Public Property BorderColor As Drawing.Color = Drawing.Color.FromArgb(210, 215, 220)

    Public Property HeaderFont As Font = New Font("Calibri", 14, FontStyle.Bold)

    Public Property BodyFont As Font = New Font("Calibri", 12)

    Public Property HeaderColor As Drawing.Color = Drawing.Color.White

    Public Property BodyColor As Drawing.Color = Drawing.Color.FromArgb(40, 40, 40)
End Class

''' <summary>表格元素：生成 PPT 原生 graphicFrame/a:tbl。</summary>
Public Class PptTableElement : Inherits PptElement

    Public Property Position As Drawing.PointF

    Public Property Extent As Drawing.SizeF

    Public Property Rotation As Single

    Public Property Data As String(,)

    Public Property Style As PptTableStyle

    Public Overrides Sub WriteTo(spTree As XElement, context As PptWriteContext)
        If Data Is Nothing Then
            Return
        End If

        Dim rows As Integer = Data.GetLength(0)
        Dim columns As Integer = Data.GetLength(1)
        If rows = 0 OrElse columns = 0 Then
            Return
        End If

        Dim style As PptTableStyle = If(Style, New PptTableStyle())
        Dim id As Integer = context.NextShapeId()

        Dim frame As New XElement(PptxDrawing.NS_P + "graphicFrame")
        frame.Add(New XElement(PptxDrawing.NS_P + "nvGraphicFramePr",
                               New XElement(PptxDrawing.NS_P + "cNvPr", New XAttribute("id", id), New XAttribute("name", $"Table {id}")),
                               New XElement(PptxDrawing.NS_P + "cNvGraphicFramePr", New XElement(PptxDrawing.NS_A + "graphicFrameLocks", New XAttribute("noGrp", 1))),
                               New XElement(PptxDrawing.NS_P + "nvPr")))

        Dim xfrm As New XElement(PptxDrawing.NS_P + "xfrm")
        If Rotation <> 0 Then
            xfrm.Add(New XAttribute("rot", PptxDrawing.RotationValue(Rotation)))
        End If
        xfrm.Add(New XElement(PptxDrawing.NS_A + "off",
                              New XAttribute("x", PptxDrawing.ToEmu(Position.X, context.Dpi)),
                              New XAttribute("y", PptxDrawing.ToEmu(Position.Y, context.Dpi))))
        xfrm.Add(New XElement(PptxDrawing.NS_A + "ext",
                              New XAttribute("cx", PptxDrawing.ToEmu(std.Max(Extent.Width, 0), context.Dpi)),
                              New XAttribute("cy", PptxDrawing.ToEmu(std.Max(Extent.Height, 0), context.Dpi))))
        frame.Add(xfrm)

        Dim table As New XElement(PptxDrawing.NS_A + "tbl")
        Dim tblPr As New XElement(PptxDrawing.NS_A + "tblPr", New XAttribute("firstRow", If(style.ShowHeader, 1, 0)), New XAttribute("bandRow", 1))
        tblPr.Add(New XElement(PptxDrawing.NS_A + "tableStyleId", "{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}"))
        table.Add(tblPr)

        Dim grid As New XElement(PptxDrawing.NS_A + "tblGrid")
        Dim columnWidth As Long = PptxDrawing.ToEmu(std.Max(Extent.Width, 0) / columns, context.Dpi)
        For i As Integer = 1 To columns
            grid.Add(New XElement(PptxDrawing.NS_A + "gridCol", New XAttribute("w", columnWidth)))
        Next
        table.Add(grid)

        Dim rowHeight As Long = PptxDrawing.ToEmu(std.Max(Extent.Height, 0) / rows, context.Dpi)

        For r As Integer = 0 To rows - 1
            Dim isHeader As Boolean = style.ShowHeader AndAlso r = 0
            Dim row As New XElement(PptxDrawing.NS_A + "tr", New XAttribute("h", rowHeight))

            For c As Integer = 0 To columns - 1
                Dim cell As New XElement(PptxDrawing.NS_A + "tc")
                Dim cellColor As Drawing.Color = If(isHeader, style.HeaderFill,
                                                    If(r Mod 2 = 0, style.BodyFill, style.AlternateFill))

                Dim cellFont As Font = If(isHeader, style.HeaderFont, style.BodyFont)
                Dim textColor As Drawing.Color = If(isHeader, style.HeaderColor, style.BodyColor)

                Dim txBody As New XElement(PptxDrawing.NS_A + "txBody",
                                           New XElement(PptxDrawing.NS_A + "bodyPr",
                                                        New XAttribute("wrap", "square"),
                                                        New XAttribute("lIns", 91440), New XAttribute("tIns", 45720),
                                                        New XAttribute("rIns", 91440), New XAttribute("bIns", 45720)),
                                           New XElement(PptxDrawing.NS_A + "lstStyle"))

                Dim paragraph As New XElement(PptxDrawing.NS_A + "p",
                                              New XElement(PptxDrawing.NS_A + "pPr", New XAttribute("algn", "l")))
                Dim run As New XElement(PptxDrawing.NS_A + "r")
                Dim rPr As New XElement(PptxDrawing.NS_A + "rPr",
                                        New XAttribute("lang", "en-US"),
                                        New XAttribute("sz", PptxDrawing.FontSize(cellFont)))

                If cellFont IsNot Nothing AndAlso cellFont.Bold Then
                    rPr.Add(New XAttribute("b", 1))
                End If
                If cellFont IsNot Nothing AndAlso cellFont.Italic Then
                    rPr.Add(New XAttribute("i", 1))
                End If

                rPr.Add(New XElement(PptxDrawing.NS_A + "solidFill",
                                     New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(textColor)))))
                If cellFont IsNot Nothing AndAlso Not String.IsNullOrEmpty(cellFont.Name) Then
                    rPr.Add(New XElement(PptxDrawing.NS_A + "latin", New XAttribute("typeface", cellFont.Name)))
                End If

                run.Add(rPr)
                run.Add(New XElement(PptxDrawing.NS_A + "t", If(Data(r, c), "")))
                paragraph.Add(run)
                txBody.Add(paragraph)
                cell.Add(txBody)

                Dim tcPr As New XElement(PptxDrawing.NS_A + "tcPr")
                tcPr.Add(New XElement(PptxDrawing.NS_A + "solidFill",
                                      New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(cellColor)))))
                For Each edge As String In New String() {"lnL", "lnR", "lnT", "lnB"}
                    tcPr.Add(New XElement(PptxDrawing.NS_A + edge, New XAttribute("w", 6350),
                                          New XElement(PptxDrawing.NS_A + "solidFill",
                                                       New XElement(PptxDrawing.NS_A + "srgbClr", New XAttribute("val", PptxDrawing.Hex(style.BorderColor))))))
                Next
                cell.Add(tcPr)
                row.Add(cell)
            Next

            table.Add(row)
        Next

        frame.Add(New XElement(PptxDrawing.NS_A + "graphic",
                               New XElement(PptxDrawing.NS_A + "graphicData",
                                            New XAttribute("uri", PptxDrawing.DrawingXmlns + "/table"),
                                            table)))
        spTree.Add(frame)
    End Sub
End Class
