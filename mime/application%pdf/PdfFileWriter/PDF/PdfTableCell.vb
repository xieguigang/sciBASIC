#Region "Microsoft.VisualBasic::3fa488f123930a78ffc4303ee8a85956, mime\application%pdf\PdfFileWriter\PDF\PdfTableCell.vb"

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

    '   Total Lines: 781
    '    Code Lines: 338 (43.28%)
    ' Comment Lines: 353 (45.20%)
    '    - Xml Docs: 77.90%
    ' 
    '   Blank Lines: 90 (11.52%)
    '     File Size: 28.71 KB


    ' Enum CellType
    ' 
    '     Barcode, Empty, Image, Text, TextBox
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfTableCell
    ' 
    '     Properties: AnnotAction, Barcode, ClientBottom, ClientLeft, ClientRight
    '                 ClientTop, ClientWidth, FormattedText, FrameLeft, FrameWidth
    '                 Header, Image, ImageHeight, ImageWidth, Index
    '                 Parent, Style, TextBox, TextBoxHeight, Type
    '                 Value, WebLink
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CreateTextBox, LeftPos, TextHorPos, TextVerPos, TopPos
    ' 
    '     Sub: DrawCell, DrawCellInitialization, Reset, TextBoxInitialization
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfTableCell
'	Data table cell support.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System.Drawing
Imports System.Runtime.InteropServices

''' <summary>
''' Cell type enumeration
''' </summary>
Public Enum CellType
    ''' <summary>
    ''' Cell's value is null.
    ''' </summary>
    Empty

    ''' <summary>
    ''' Cell's value is String and Style.MultiLineText is false.
    ''' </summary>
    Text

    ''' <summary>
    ''' Cell's value is TextBox or String with Style.MultiLineText is true.
    ''' </summary>
    TextBox

    ''' <summary>
    ''' Cell's value is image.
    ''' </summary>
    Image

    ''' <summary>
    ''' Cell's value is barcode.
    ''' </summary>
    Barcode
End Enum


''' <summary>
''' PDF table cell class
''' </summary>
''' <remarks>
''' <para>
''' The PDF table cell class represent one cell in the table.
''' </para>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DataTableSupport">
''' 2.12 Data Table Support</a>
''' </para>
''' </remarks>

Public Class PdfTableCell

    ''' <summary>
    ''' Gets parent PdfTable.
    ''' </summary>
    Private _Index As Integer, _Header As Boolean, _Type As CellType, _FormattedText As String, _TextBox As TextBox, _TextBoxHeight As Double, _Image As PdfImage, _Barcode As Barcode, _FrameLeft As Double, _FrameWidth As Double, _ClientLeft As Double, _ClientBottom As Double, _ClientRight As Double, _ClientTop As Double, _Parent As PdfTable

    ''' <summary>
    ''' Gets cell's index position within Table.Cell array.
    ''' </summary>
    ''' <remarks>
    ''' It is the cell's column number starting with zero.
    ''' </remarks>
    Public Property Index As Integer
        Get
            Return _Index
        End Get
        Friend Set(value As Integer)
            _Index = value
        End Set
    End Property

    ''' <summary>
    ''' Cell is a header.
    ''' </summary>
    ''' <remarks>
    ''' If this property is true, the PdfTableCell is a header otherwise it is a cell.
    ''' </remarks>
    Public Property Header As Boolean
        Get
            Return _Header
        End Get
        Friend Set(value As Boolean)
            _Header = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets cell's style.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' If Style was not set by the caller, this value is the default cell style.
    ''' Any change to the properties will affect all cells without cell style.
    ''' </para>
    ''' <para>
    ''' If Style was set by the caller to a private style, this value is the private cell style.
    ''' Any change to the properties will affect all other cells sharing this private cell style.
    ''' </para>
    ''' </remarks>
    Public Property Style As PdfTableStyle

    ''' <summary>
    ''' Gets cell's enumeration type.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' CellType will be Text for String and MultiLineText set to false plus all basic numeric values.
    ''' </para>
    ''' <para>
    ''' CallType will be TextBox for String and MultiLineText set to true or Value set to TextBox.
    ''' </para>
    ''' <para>
    ''' CallType will be set ti Image or Barcode if Value is set accordingly.
    ''' </para>
    ''' </remarks>
    Public Property Type As CellType
        Get
            Return _Type
        End Get
        Friend Set(value As CellType)
            _Type = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets cell's value
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Value can be set to String, basic numeric values, bool, Char, TextBox, PdfImage or Barcode.
    ''' </para>
    ''' <para>
    ''' If value is set to String and MultiLineText is set to true, 
    ''' the String will be converted to TextBox.
    ''' </para>
    ''' <para>
    ''' All basic numeric values will be converted to String.
    ''' </para>
    ''' <para>
    ''' Value will be reset to null after each row drawing.
    ''' </para>
    ''' </remarks>
    Public Property Value As Object

    ''' <summary>
    ''' Gets cell's formatted value.
    ''' </summary>
    ''' <remarks>
    ''' If Value is a numeric type, it is converted to formatted text
    ''' using Value.ToString(Format, NumberFormat) method.
    ''' </remarks>
    Public Property FormattedText As String
        Get
            Return _FormattedText
        End Get
        Friend Set(value As String)
            _FormattedText = value
        End Set
    End Property

    ''' <summary>
    ''' Gets TextBox if Type is TextBox.
    ''' </summary>
    ''' <remarks>
    ''' TextBox will be set if Value is a String and Style.MultiLine is true,
    ''' or Value is a TextBox.
    ''' </remarks>
    Public Property TextBox As TextBox
        Get
            Return _TextBox
        End Get
        Friend Set(value As TextBox)
            _TextBox = value
        End Set
    End Property

    ''' <summary>
    ''' Text box height including extra space
    ''' </summary>
    ''' <remarks>
    ''' TextBoxHeight Value is calculated within DrawRow method. 
    ''' It is valid for CustomDrawCellEvent.
    ''' </remarks>
    Public Property TextBoxHeight As Double
        Get
            Return _TextBoxHeight
        End Get
        Friend Set(value As Double)
            _TextBoxHeight = value
        End Set
    End Property

    ''' <summary>
    ''' Gets Image if Type is Image.
    ''' </summary>
    ''' <seealso cref="PdfTableCell.ImageWidth"/>
    ''' <seealso cref="PdfTableCell.ImageHeight"/>
    ''' <remarks>
    ''' <para>
    ''' 	If ImageWidth and ImageHeight were not set by the user,
    ''' 	the image width will be set to ClientWidth and the height
    ''' 	will be calculated to preserve image's aspect ratio.
    ''' 	</para>
    ''' 	<para>
    ''' 	If ImageWidth was not set by the user and ImageHeight
    ''' 	was set by the user. ImageWidth will be calculated to 
    ''' 	preserve image's aspect ratio.
    ''' 	</para>
    ''' 	<para>
    ''' 	If ImageWidth was set by the user and ImageHeight was
    ''' 	not set by the user, ImageHeight will be calculated to 
    ''' 	preserve image's aspect ratio.
    ''' 	</para>
    ''' <para>
    ''' 	If both ImageWidth and ImageHeight were set by the user,
    ''' 	the aspect ratio of the image will be ignored.
    ''' 	</para>
    ''' <para>
    ''' If ImageWidth is wider than Client width, both ImageWidth
    ''' and ImageHeight will be adjusted to fit the available width.
    ''' </para>
    ''' <para>
    ''' ImageWidth and ImageHeight will be reset to zero after each row drawing.
    ''' </para>
    ''' </remarks>
    Public Property Image As PdfImage
        Get
            Return _Image
        End Get
        Friend Set(value As PdfImage)
            _Image = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets image width in user units.
    ''' </summary>
    ''' <seealso cref="PdfTableCell.Image"/>
    ''' <remarks>
    ''' Please note "Remarks" in Image property for description
    ''' of ImageWidth and ImageHeight.
    ''' </remarks>
    Public Property ImageWidth As Double

    ''' <summary>
    ''' Gets or sets image height in user units.
    ''' </summary>
    ''' <seealso cref="PdfTableCell.Image"/>
    ''' <remarks>
    ''' Please note "Remarks" in Image property for description
    ''' of ImageWidth and ImageHeight.
    ''' </remarks>
    Public Property ImageHeight As Double

    ''' <summary>
    ''' Gets barcode if type is Barcode
    ''' </summary>
    Public Property Barcode As Barcode
        Get
            Return _Barcode
        End Get
        Friend Set(value As Barcode)
            _Barcode = value
        End Set
    End Property

    ' total barcode plus text height
    Private BarcodeBox As BarcodeBox

    ''' <summary>
    ''' Sets a web link for this cell.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The web ling string is converted to Annot Action object.
    ''' </para>
    ''' </remarks>
    Public WriteOnly Property WebLink As String
        Set(value As String)
            AnnotAction = New AnnotWebLink(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets annotation action derived classes
    ''' </summary>
    ''' <remarks>
    ''' <para>The user can activate the annotation action by clicking anywhere in the cell area.
    ''' Right click for attached file.</para>
    ''' <list type="table">
    ''' <item><description>Weblink action to activate web browser.</description></item>
    ''' <item><description>Go to action to jump to another page in the document.</description></item>
    ''' <item><description>Display media action to isplay video or play sound.</description></item>
    ''' <item><description>File attachment to save or view embedded file.</description></item>
    ''' </list>
    ''' </remarks>
    Public Property AnnotAction As AnnotAction

    ''' <summary>
    ''' Gets cell's frame left side (grid line).
    ''' </summary>
    Public Property FrameLeft As Double
        Get
            Return _FrameLeft
        End Get
        Friend Set(value As Double)
            _FrameLeft = value
        End Set
    End Property

    ''' <summary>
    ''' Gets cell's frame width (grid line to grid line).
    ''' </summary>
    Public Property FrameWidth As Double
        Get
            Return _FrameWidth
        End Get
        Friend Set(value As Double)
            _FrameWidth = value
        End Set
    End Property

    ''' <summary>
    ''' Gets client area left side.
    ''' </summary>
    ''' <remarks>
    ''' ClientLeft is FrameLeft + Margin.Left.
    ''' </remarks>
    Public Property ClientLeft As Double
        Get
            Return _ClientLeft
        End Get
        Friend Set(value As Double)
            _ClientLeft = value
        End Set
    End Property

    ''' <summary>
    ''' Gets client area bottom side.
    ''' </summary>
    ''' <remarks>
    ''' ClientBottom is Table.RowBottomPosition + Margin.Bottom
    ''' </remarks>
    Public Property ClientBottom As Double
        Get
            Return _ClientBottom
        End Get
        Friend Set(value As Double)
            _ClientBottom = value
        End Set
    End Property

    ''' <summary>
    ''' Gets client area right side.
    ''' </summary>
    ''' <remarks>
    ''' ClientRight is FrameLeft + FrameWidth - Margin.Right.
    ''' </remarks>
    Public Property ClientRight As Double
        Get
            Return _ClientRight
        End Get
        Friend Set(value As Double)
            _ClientRight = value
        End Set
    End Property

    ''' <summary>
    ''' Gets client area top side.
    ''' </summary>
    ''' <remarks>
    ''' ClientTop is Table.RowTopPosition - Margin.Top.
    ''' </remarks>
    Public Property ClientTop As Double
        Get
            Return _ClientTop
        End Get
        Friend Set(value As Double)
            _ClientTop = value
        End Set
    End Property

    ''' <summary>
    ''' Gets Client area width.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' ClientWidth is FrameWidth - Margin.Left - Margin.Right.
    ''' </para>
    ''' <para>
    ''' Calling client width before initialization will force initialization.
    ''' Table.PdfTableInitialization() method will be called.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property ClientWidth As Double
        Get
            If FrameWidth = 0.0 Then Parent.PdfTableInitialization()
            Return FrameWidth - Style.Margin.Left - Style.Margin.Right
        End Get
    End Property

    Public Property Parent As PdfTable
        Get
            Return _Parent
        End Get
        Friend Set(value As PdfTable)
            _Parent = value
        End Set
    End Property

    Friend CellHeight As Double
    Friend TextBoxCellHeight As Double
    Friend TextBoxLineNo As Integer

    ''' <summary>
    ''' internal constructor
    ''' PdfTable creates two PdfTableCell arrays.
    ''' </summary>
    ''' <param name="Parent"></param>
    ''' <param name="Index"></param>
    ''' <param name="Header"></param>
    Friend Sub New(Parent As PdfTable, Index As Integer, Header As Boolean)
        Me.Parent = Parent
        Me.Index = Index
        Me.Header = Header
        Style = If(Header, Parent.DefaultHeaderStyle, Parent.DefaultCellStyle)
    End Sub

    ''' <summary>
    ''' Creates an empty text box with client width.
    ''' </summary>
    ''' <returns>Empty text box with client width.</returns>
    ''' <remarks>
    ''' <para>
    ''' The newly created TextBox will have the correct client width.
    ''' First line indent and line break factor will be taken from cell's style.
    ''' </para>
    ''' <para>
    ''' CreateTextBox() method sets the Value property of this cell
    ''' to the returned TextBox value;
    ''' </para>
    ''' </remarks>
    Public Function CreateTextBox() As TextBox
        Value = New TextBox(ClientWidth, Style.TextBoxFirstLineIndent, Style.TextBoxLineBreakFactor)
        Return CType(Value, TextBox)
    End Function

    ''' <summary>
    ''' Draw Cell Initialization
    ''' </summary>
    Friend Sub DrawCellInitialization()
        ' calculate left and right client space
        ClientLeft = FrameLeft + Style.Margin.Left
        ClientRight = FrameLeft + FrameWidth - Style.Margin.Right

        ' initialize cell height to top and bottom margins
        CellHeight = Style.Margin.Top + Style.Margin.Bottom
        Dim LineEnd As Integer = Nothing

        ' we have something to draw
        If Value IsNot Nothing Then
            ' assume cell type to be text
            Type = CellType.Text

            ' get object type
            Dim ValueType As Type = Value.GetType()

            ' value is string
            If ValueType Is GetType(String) Then
                ' multi line text
                If Style.MultiLineText Then
                    ' convert string to TextBox
                    TextBox = New TextBox(ClientRight - ClientLeft, Style.TextBoxFirstLineIndent, Style.TextBoxLineBreakFactor)
                    TextBox.AddText(Style.Font, Style.FontSize, Style.ForegroundColor, CStr(Value))

                    ' textbox initialization

                    ' single line text
                    TextBoxInitialization()
                Else
                    ' save value as string
                    FormattedText = CStr(Value)

                    ' add line spacing
                    CellHeight += Style.FontLineSpacing
                End If

                ' value is text box
            ElseIf ValueType Is GetType(TextBox) Then
                ' set TextBox
                TextBox = CType(Value, TextBox)

                ' test width
                If TextBox.BoxWidth - (ClientRight - ClientLeft) > Parent.Epsilon Then Throw New ApplicationException("PdfTableCell: TextBox width is greater than column width")

                ' textbox initialization
                TextBoxInitialization()

                ' value is PdfImage
            ElseIf ValueType Is GetType(PdfImage) Then
                ' set image
                Image = CType(Value, PdfImage)

                ' calculate client width
                Dim Width = ClientWidth

                ' calculate image width and height
                If ImageWidth = 0.0 Then
                    If ImageHeight = 0.0 Then
                        ImageWidth = Width
                        ImageHeight = ImageWidth * Image.HeightPix / Image.WidthPix
                    Else
                        ImageWidth = ImageHeight * Image.WidthPix / Image.HeightPix
                    End If
                ElseIf ImageHeight = 0.0 Then
                    ImageHeight = ImageWidth * Image.HeightPix / Image.WidthPix
                End If

                ' image width is too wide
                If ImageWidth > Width Then
                    ImageHeight = Width * ImageHeight / ImageWidth
                    ImageWidth = Width
                End If

                ' adjust cell's height
                CellHeight += ImageHeight

                ' set type to image
                Type = CellType.Image

                ' value is a derived class of barcode
            ElseIf ValueType.BaseType Is GetType(Barcode) Then
                ' set barcode
                Barcode = CType(Value, Barcode)

                ' test barcode height
                If Style.BarcodeHeight <= 0.0 Then Throw New ApplicationException("PdfTableStyle: BarcodeHeight must be defined.")

                ' calculate total barcode height
                BarcodeBox = Barcode.GetBarcodeBox(Style.BarcodeBarWidth, Style.BarcodeHeight, Style.Font, Style.FontSize)

                ' adjust cell's height
                CellHeight += BarcodeBox.TotalHeight

                ' set type to barcode

                ' value is basic mostly numeric object
                Type = CellType.Barcode
            Else
                Dim Format = Style.Format
                Dim NumberFormat = Style.NumberFormatInfo

                If ValueType Is GetType(Integer) Then
                    FormattedText = CInt(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Single) Then
                    FormattedText = CSng(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Double) Then
                    FormattedText = CDbl(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Boolean) Then
                    FormattedText = CBool(Value).ToString()
                ElseIf ValueType Is GetType(Char) Then
                    FormattedText = CChar(Value).ToString()
                ElseIf ValueType Is GetType(Byte) Then
                    FormattedText = CByte(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(SByte) Then
                    FormattedText = CSByte(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Short) Then
                    FormattedText = CShort(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(UShort) Then
                    FormattedText = CUShort(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(UInteger) Then
                    FormattedText = CUInt(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Long) Then
                    FormattedText = CLng(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(ULong) Then
                    FormattedText = CULng(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(Decimal) Then
                    FormattedText = CDec(Value).ToString(Format, NumberFormat)
                ElseIf ValueType Is GetType(DBNull) Then
                    FormattedText = String.Empty
                Else
                    Throw New ApplicationException("PdfTableCell: Unknown object type")
                End If

                ' add line spacing
                CellHeight += Style.FontLineSpacing
            End If

            ' value is null and textbox continuation is required
        ElseIf Type = CellType.TextBox AndAlso TextBoxLineNo <> 0 Then
            CellHeight += TextBox.BoxHeightExtra(TextBoxLineNo, LineEnd, Parent._RowTopPosition - Parent.TableBottomLimit - (Style.Margin.Top + Style.Margin.Bottom), Style.TextBoxLineExtraSpace, Style.TextBoxParagraphExtraSpace)
            TextBoxCellHeight = CellHeight
        Else
            ' reset cell type
            Type = CellType.Empty
        End If

        ' test for minimum height requirement
        If CellHeight < Style.MinHeight Then CellHeight = Style.MinHeight

        ' cell minimum height for all types but textbox
        If Type <> CellType.TextBox Then TextBoxCellHeight = CellHeight
    End Sub

    Friend Sub TextBoxInitialization()
        ' terminate TextBox
        TextBox.Terminate()

        ' calculate overall TextBox height
        TextBoxHeight = TextBox.BoxHeightExtra(Style.TextBoxLineExtraSpace, Style.TextBoxParagraphExtraSpace)
        Dim TextBoxHeightPageBreak = TextBoxHeight

        ' textbox minimum height for page break calculations
        If Not Header AndAlso Style.TextBoxPageBreakLines <> 0 AndAlso Style.TextBoxPageBreakLines < TextBox.LineCount Then
            ' calculate TextBox height and add to cell height
            TextBoxHeightPageBreak = TextBox.BoxHeightExtra(Style.TextBoxPageBreakLines, Style.TextBoxLineExtraSpace, Style.TextBoxParagraphExtraSpace)
        End If

        ' required cell height for page break calculations and for full textbox height
        TextBoxCellHeight = CellHeight + TextBoxHeightPageBreak
        CellHeight += TextBoxHeight

        ' reset textbox line number
        TextBoxLineNo = 0

        ' set type to text box
        Type = CellType.TextBox
    End Sub

    ''' <summary>
    ''' Draw Cell
    ''' </summary>
    Friend Sub DrawCell()
        ' draw background color
        If Style.BackgroundColor <> Color.Empty Then
            Parent.Contents.SaveGraphicsState()
            Parent.Contents.SetColorNonStroking(Style.BackgroundColor)
            Parent.Contents.DrawRectangle(Parent._ColumnPosition(Index), Parent.RowBottomPosition, Parent._ColumnWidth(Index), If(Header, Parent.HeaderHeight, Parent.RowHeight), PaintOp.Fill)
            Parent.Contents.RestoreGraphicsState()
        End If

        ' switch based on cell type
        Select Case Type
            ' one line of text
            Case CellType.Text
                Dim TextJustify As TextJustify
                Parent.Contents.DrawText(Style.Font, Style.FontSize, TextHorPos(TextJustify), TextVerPos(), TextJustify, Style.TextDrawStyle, Style.ForegroundColor, FormattedText)

            ' text box
            Case CellType.TextBox
                ' calculate textbox size and position
                Dim LineEnd As Integer
                If TextBoxLineNo <> 0 OrElse TextBoxHeight > ClientTop - ClientBottom + Parent.Epsilon Then TextBoxHeight = TextBox.BoxHeightExtra(TextBoxLineNo, LineEnd, ClientTop - ClientBottom + Parent.Epsilon, Style.TextBoxLineExtraSpace, Style.TextBoxParagraphExtraSpace)
                Dim YPos = TopPos(TextBoxHeight)

                ' draw textbox
                Parent.Contents.SaveGraphicsState()
                LineEnd = Parent.Contents.DrawText(LeftPos(TextBox.BoxWidth), YPos, ClientBottom - Parent.Epsilon, TextBoxLineNo, Style.TextBoxLineExtraSpace, Style.TextBoxParagraphExtraSpace, Style.TextBoxTextJustify, TextBox)
                Parent.Contents.RestoreGraphicsState()

                ' textbox did not fit in current page
                If LineEnd < TextBox.LineCount Then
                    TextBoxLineNo = LineEnd
                    ' textbox drawing is done
                    Parent.TextBoxContinue = True
                Else
                    TextBoxLineNo = 0
                End If

            ' image
            Case CellType.Image
                Parent.Contents.DrawImage(Image, LeftPos(ImageWidth), TopPos(ImageHeight) - ImageHeight, ImageWidth, ImageHeight)

            ' barcode
            Case CellType.Barcode
                Parent.Contents.DrawBarcode(LeftPos(BarcodeBox.TotalWidth) + BarcodeBox.OriginX, TopPos(BarcodeBox.TotalHeight) - BarcodeBox.TotalHeight + BarcodeBox.OriginY, TextJustify.Left, Style.BarcodeBarWidth, Style.BarcodeHeight, Style.ForegroundColor, Barcode, Style.Font, Style.FontSize)
        End Select

        ' cell has a web link
        If AnnotAction IsNot Nothing Then
            Parent.Page.AddAnnotation(New PdfRectangle(ClientLeft, ClientBottom, ClientRight, ClientTop), AnnotAction)
        End If
    End Sub

    ''' <summary>
    ''' Calculate text horizontal position
    ''' </summary>
    ''' <param name="Justify"></param>
    ''' <returns></returns>
    Private Function TextHorPos(<Out> ByRef Justify As TextJustify) As Double
        If (Style.Alignment And (ContentAlignment.TopLeft Or ContentAlignment.MiddleLeft Or ContentAlignment.BottomLeft)) <> 0 Then
            Justify = TextJustify.Left
            Return ClientLeft
        End If

        If (Style.Alignment And (ContentAlignment.TopRight Or ContentAlignment.MiddleRight Or ContentAlignment.BottomRight)) <> 0 Then
            Justify = TextJustify.Right
            Return ClientRight
        End If

        If (Style.Alignment And (ContentAlignment.TopCenter Or ContentAlignment.MiddleCenter Or ContentAlignment.BottomCenter)) <> 0 Then
            Justify = TextJustify.Center
            Return 0.5 * (ClientLeft + ClientRight)
        End If

        Justify = TextJustify.Left
        Return ClientLeft
    End Function


    ''' <summary>
    ''' Calculate left side
    ''' </summary>
    ''' <param name="Width"></param>
    ''' <returns></returns>
    Private Function LeftPos(Width As Double) As Double
        If (Style.Alignment And (ContentAlignment.TopLeft Or ContentAlignment.MiddleLeft Or ContentAlignment.BottomLeft)) <> 0 Then Return ClientLeft
        If (Style.Alignment And (ContentAlignment.TopCenter Or ContentAlignment.MiddleCenter Or ContentAlignment.BottomCenter)) <> 0 Then Return 0.5 * (ClientLeft + ClientRight - Width)
        If (Style.Alignment And (ContentAlignment.TopRight Or ContentAlignment.MiddleRight Or ContentAlignment.BottomRight)) <> 0 Then Return ClientRight - Width
        Return ClientLeft
    End Function


    ''' <summary>
    ''' Calculate text vertical position
    ''' </summary>
    ''' <returns></returns>
    Private Function TextVerPos() As Double
        If (Style.Alignment And (ContentAlignment.TopLeft Or ContentAlignment.TopCenter Or ContentAlignment.TopRight)) <> 0 Then Return ClientTop - Style.FontAscent
        If (Style.Alignment And (ContentAlignment.BottomLeft Or ContentAlignment.BottomCenter Or ContentAlignment.BottomRight)) <> 0 Then Return ClientBottom + Style.FontDescent
        If (Style.Alignment And (ContentAlignment.MiddleLeft Or ContentAlignment.MiddleCenter Or ContentAlignment.MiddleRight)) <> 0 Then Return 0.5 * (ClientTop + ClientBottom - Style.FontAscent + Style.FontDescent)
        Return ClientTop - Style.FontAscent
    End Function


    ''' <summary>
    ''' Calculate top side
    ''' </summary>
    ''' <param name="Height"></param>
    ''' <returns></returns>
    Private Function TopPos(Height As Double) As Double
        If (Style.Alignment And (ContentAlignment.TopLeft Or ContentAlignment.TopCenter Or ContentAlignment.TopRight)) <> 0 Then Return ClientTop
        If (Style.Alignment And (ContentAlignment.MiddleLeft Or ContentAlignment.MiddleCenter Or ContentAlignment.MiddleRight)) <> 0 Then Return 0.5 * (ClientTop + ClientBottom + Height)
        If (Style.Alignment And (ContentAlignment.BottomLeft Or ContentAlignment.BottomCenter Or ContentAlignment.BottomRight)) <> 0 Then Return ClientBottom + Height
        Return ClientTop
    End Function


    ''' <summary>
    ''' Reset cell after the current row was drawn
    ''' </summary>
    Friend Sub Reset()
        Value = Nothing
        AnnotAction = Nothing
        ImageWidth = 0.0
        ImageHeight = 0.0
    End Sub
End Class
