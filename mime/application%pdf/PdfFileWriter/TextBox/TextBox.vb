#Region "Microsoft.VisualBasic::2e9dfa34848c1fc38fbf0eaf00e2f49b, mime\application%pdf\PdfFileWriter\TextBox\TextBox.vb"

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

    '   Total Lines: 604
    '    Code Lines: 302 (50.00%)
    ' Comment Lines: 213 (35.26%)
    '    - Xml Docs: 60.56%
    ' 
    '   Blank Lines: 89 (14.74%)
    '     File Size: 21.61 KB


    ' Class TextBox
    ' 
    '     Properties: BoxHeight, BoxWidth, FirstLineIndent, LineCount, LongestLineWidth
    '                 ParagraphCount
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+3 Overloads) BoxHeightExtra
    ' 
    '     Sub: AddLine, (+8 Overloads) AddText, BreakLine, Clear, Terminate
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	TextBox
'  Support class for PdfContents class. Format text to fit column.
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
''' TextBox class
''' </summary>
''' <remarks>
''' <para>
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawTextBox">For example of drawing TextBox see 3.12. Draw Text Box</a>
''' </para>
''' </remarks>
Public Class TextBox

    ''' <summary>
    ''' Gets first line is indented.
    ''' </summary>
    Private _BoxWidth As Double, _BoxHeight As Double, _ParagraphCount As Integer, _FirstLineIndent As Double

    ''' <summary>
    ''' Gets box width.
    ''' </summary>
    Public Property BoxWidth As Double
        Get
            Return _BoxWidth
        End Get
        Friend Set(value As Double)
            _BoxWidth = value
        End Set
    End Property

    ''' <summary>
    ''' Gets box height.
    ''' </summary>
    Public Property BoxHeight As Double
        Get
            Return _BoxHeight
        End Get
        Friend Set(value As Double)
            _BoxHeight = value
        End Set
    End Property

    ''' <summary>
    ''' Gets line count.
    ''' </summary>
    Public ReadOnly Property LineCount As Integer
        Get
            Return LineArray.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets paragraph count.
    ''' </summary>
    Public Property ParagraphCount As Integer
        Get
            Return _ParagraphCount
        End Get
        Friend Set(value As Integer)
            _ParagraphCount = value
        End Set
    End Property

    Public Property FirstLineIndent As Double
        Get
            Return _FirstLineIndent
        End Get
        Friend Set(value As Double)
            _FirstLineIndent = value
        End Set
    End Property

    Private LineBreakFactor As Double    ' should be >= 0.1 and <= 0.9
    Private PrevChar As Char
    Private LineWidth As Double
    Private LineBreakWidth As Double
    Private BreakSegIndex As Integer
    Private BreakPtr As Integer
    Private BreakWidth As Double
    Private SegArray As List(Of TextBoxSeg)
    Private LineArray As List(Of TextBoxLine)

    ''' <summary>
    ''' TextBox constructor
    ''' </summary>
    ''' <param name="BoxWidth">Box width.</param>
    ''' <param name="FirstLineIndent">First line is indented.</param>
    ''' <param name="LineBreakFactor">Line break factor.</param>
    Public Sub New(BoxWidth As Double, Optional FirstLineIndent As Double = 0.0, Optional LineBreakFactor As Double = 0.5)
        If BoxWidth <= 0.0 Then Throw New ApplicationException("Box width must be greater than zero")
        Me.BoxWidth = BoxWidth
        Me.FirstLineIndent = FirstLineIndent
        If LineBreakFactor < 0.1 OrElse LineBreakFactor > 0.9 Then Throw New ApplicationException("LineBreakFactor must be between 0.1 and 0.9")
        Me.LineBreakFactor = LineBreakFactor
        SegArray = New List(Of TextBoxSeg)()
        LineArray = New List(Of TextBoxLine)()
        Clear()
        Return
    End Sub

    ''' <summary>
    ''' Clear TextBox
    ''' </summary>
    Public Sub Clear()
        BoxHeight = 0.0
        ParagraphCount = 0
        PrevChar = " "c
        LineWidth = 0.0
        LineBreakWidth = 0.0
        BreakSegIndex = 0
        BreakPtr = 0
        BreakWidth = 0
        SegArray.Clear()
        LineArray.Clear()
        Return
    End Sub

    ''' <summary>
    ''' Access TextBoxLine array.
    ''' </summary>
    ''' <param name="Index">Index</param>
    ''' <returns>TextBoxLine</returns>
    Default Public ReadOnly Property Item(Index As Integer) As TextBoxLine
        Get
            Return LineArray(Index)
        End Get
    End Property

    ''' <summary>
    ''' TextBox height including extra line and paragraph space.
    ''' </summary>
    ''' <param name="LineExtraSpace">Extra line space.</param>
    ''' <param name="ParagraphExtraSpace">Extra paragraph space.</param>
    ''' <returns>Height</returns>
    Public Function BoxHeightExtra(LineExtraSpace As Double, ParagraphExtraSpace As Double) As Double
        Dim Height = BoxHeight
        If LineArray.Count > 1 AndAlso LineExtraSpace <> 0.0 Then Height += LineExtraSpace * (LineArray.Count - 1)
        If ParagraphCount > 1 AndAlso ParagraphExtraSpace <> 0.0 Then Height += ParagraphExtraSpace * (ParagraphCount - 1)
        Return Height
    End Function

    ''' <summary>
    ''' Thwe height of the first LineCount lines including extra line and paragraph space.
    ''' </summary>
    ''' <param name="LineCount">The requested number of lines.</param>
    ''' <param name="LineExtraSpace">Extra line space.</param>
    ''' <param name="ParagraphExtraSpace">Extra paragraph space.</param>
    ''' <returns>Height</returns>
    Public Function BoxHeightExtra(LineCount As Integer, LineExtraSpace As Double, ParagraphExtraSpace As Double) As Double
        ' textbox is empty
        If LineArray.Count = 0 Then Return 0.0

        ' line count is greater than available lines
        If LineCount >= LineArray.Count Then
            Return BoxHeightExtra(LineExtraSpace, ParagraphExtraSpace)
        End If

        ' calculate height for requested line count
        Dim Height As Double = 0
        Dim Index = 0

        While True
            Dim Line = LineArray(Index)
            Height += Line.LineHeight
            If Index + 1 = LineCount Then Exit While
            Height += LineExtraSpace
            If Line.EndOfParagraph Then Height += ParagraphExtraSpace
            Index += 1
        End While

        Return Height
    End Function

    ''' <summary>
    ''' The height of a block of lines within TextBox not excedding request height.
    ''' </summary>
    ''' <param name="LineStart">Start line</param>
    ''' <param name="LineEnd">End line</param>
    ''' <param name="RequestHeight">Requested height</param>
    ''' <param name="LineExtraSpace">Extra line space.</param>
    ''' <param name="ParagraphExtraSpace">Extra paragraph space.</param>
    ''' <returns>Height</returns>
    ''' <remarks>
    ''' LineStart will be adjusted forward to skip blank lines. LineEnd 
    ''' will be one after a non blank line. 
    ''' </remarks>
    Public Function BoxHeightExtra(ByRef LineStart As Integer, <Out> ByRef LineEnd As Integer, RequestHeight As Double, LineExtraSpace As Double, ParagraphExtraSpace As Double) As Double
        ' skip blank lines
        While LineStart < LineArray.Count
            Dim Line = LineArray(LineStart)
            If Not Line.EndOfParagraph OrElse Line.SegArray.Length > 1 OrElse Line.SegArray(0).SegWidth <> 0 Then Exit While
            LineStart += 1
        End While

        ' end of textbox
        If LineStart >= LineArray.Count Then
            LineEnd = LineArray.Count
            LineStart = LineEnd
            Return 0.0
        End If

        LineEnd = LineStart

        ' calculate height for requested line count
        Dim Total = 0.0
        Dim Height = 0.0
        Dim [End] As Integer = LineEnd

        While True
            Dim Line = LineArray([End])
            If Total + Line.LineHeight > RequestHeight Then Exit While
            Total += Line.LineHeight
            [End] += 1

            If Not Line.EndOfParagraph OrElse Line.SegArray.Length > 1 OrElse Line.SegArray(0).SegWidth <> 0 Then
                LineEnd = [End]
                Height = Total
            End If

            If [End] = LineCount Then Exit While
            Total += LineExtraSpace
            If Line.EndOfParagraph Then Total += ParagraphExtraSpace
        End While

        Return Height
    End Function

    ''' <summary>
    ''' Longest line width
    ''' </summary>
    Public ReadOnly Property LongestLineWidth As Double
        Get
            Dim MaxWidth As Double = 0

            For Each Line In LineArray
                Dim LineWidth As Double = 0

                For Each Seg In Line.SegArray
                    LineWidth += Seg.SegWidth
                Next

                If LineWidth > MaxWidth Then MaxWidth = LineWidth
            Next

            Return MaxWidth
        End Get
    End Property

    ''' <summary>
    ''' Terminate TextBox
    ''' </summary>
    Public Sub Terminate()
        ' terminate last line
        If SegArray.Count <> 0 Then AddLine(True)

        ' remove trailing empty paragraphs
        For Index = LineArray.Count - 1 To 0 Step -1
            Dim Line = LineArray(Index)
            If Not Line.EndOfParagraph OrElse Line.SegArray.Length > 1 OrElse Line.SegArray(0).SegWidth <> 0 Then Exit For
            BoxHeight -= Line.Ascent + Line.Descent
            ParagraphCount -= 1
            LineArray.RemoveAt(Index)
        Next

        ' exit
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, Text As String)
        AddText(Font, FontSize, DrawStyle.Normal, Color.Black, Text, CType(Nothing, AnnotAction))
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    ''' <param name="AnnotAction">Annotation action</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, Text As String, AnnotAction As AnnotAction)
        AddText(Font, FontSize, DrawStyle.Underline, Color.Blue, Text, AnnotAction)
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    ''' <param name="WebLinkStr">Web link</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, Text As String, WebLinkStr As String)
        AddText(Font, FontSize, DrawStyle.Underline, Color.Blue, Text, New AnnotWebLink(WebLinkStr))
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="DrawStyle">Drawing style</param>
    ''' <param name="Text">Text</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, Text As String)
        AddText(Font, FontSize, DrawStyle, Color.Empty, Text, CType(Nothing, AnnotAction))
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="FontColor">Text color</param>
    ''' <param name="Text">Text</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, FontColor As Color, Text As String)
        AddText(Font, FontSize, DrawStyle.Normal, FontColor, Text, CType(Nothing, AnnotAction))
        Return
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="DrawStyle">Drawing style</param>
    ''' <param name="FontColor">Text color</param>
    ''' <param name="Text">Web link (URL)</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, FontColor As Color, Text As String)
        AddText(Font, FontSize, DrawStyle, FontColor, Text, CType(Nothing, AnnotAction))
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="DrawStyle">Drawing style</param>
    ''' <param name="FontColor">Text color</param>
    ''' <param name="Text">Text</param>
    ''' <param name="WebLinkStr">Web link (URL)</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, FontColor As Color, Text As String, WebLinkStr As String)
        AddText(Font, FontSize, DrawStyle, FontColor, Text, New AnnotWebLink(WebLinkStr))
    End Sub

    ''' <summary>
    ''' Add text to text box.
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="DrawStyle">Drawing style</param>
    ''' <param name="FontColor">Text color</param>
    ''' <param name="Text">Text</param>
    ''' <param name="AnnotAction">Web link</param>
    Public Sub AddText(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, FontColor As Color, Text As String, AnnotAction As AnnotAction)
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then Return

        ' create new text segment
        Dim Seg As TextBoxSeg

        ' segment array is empty or new segment is different than last one
        If SegArray.Count = 0 OrElse Not SegArray(SegArray.Count - 1).IsEqual(Font, FontSize, DrawStyle, FontColor, AnnotAction) Then
            Seg = New TextBoxSeg(Font, FontSize, DrawStyle, FontColor, AnnotAction)

            ' add new text to most recent text segment
            SegArray.Add(Seg)
        Else
            Seg = SegArray(SegArray.Count - 1)
        End If

        ' save text start pointer
        Dim TextStart = 0

        ' loop for characters
        For TextPtr = 0 To Text.Length - 1
            ' shortcut to current character
            Dim CurChar = Text(TextPtr)

            ' end of paragraph
            If CurChar = Microsoft.VisualBasic.Strings.ChrW(10) OrElse CurChar = Microsoft.VisualBasic.Strings.ChrW(13) Then
                ' append text to current segemnt
                Seg.Text += Text.Substring(TextStart, TextPtr - TextStart)

                ' test for new line after carriage return
                If CurChar = Microsoft.VisualBasic.Strings.ChrW(13) AndAlso TextPtr + 1 < Text.Length AndAlso Text(TextPtr + 1) = Microsoft.VisualBasic.Strings.ChrW(10) Then TextPtr += 1

                ' move pointer to one after the eol
                TextStart = TextPtr + 1

                ' add line
                AddLine(True)

                ' update last character
                PrevChar = " "c

                ' end of text
                If TextPtr + 1 = Text.Length Then Return

                ' add new empty segment
                Seg = New TextBoxSeg(Font, FontSize, DrawStyle, FontColor, AnnotAction)
                SegArray.Add(Seg)
                Continue For
            End If

            ' character width
            Dim CharWidth = Font.CharWidth(FontSize, Seg.DrawStyle, CurChar)

            ' space
            If CurChar = " "c Then
                ' test for transition from non space to space
                ' this is a potential line break point
                If PrevChar <> " "c Then
                    ' save potential line break information
                    LineBreakWidth = LineWidth
                    BreakSegIndex = SegArray.Count - 1
                    BreakPtr = Seg.Text.Length + TextPtr - TextStart
                    BreakWidth = Seg.SegWidth
                End If

                ' add to line width
                LineWidth += CharWidth
                Seg.SegWidth += CharWidth

                ' update last character
                PrevChar = CurChar
                Continue For
            End If

            ' add current segment width and to overall line width
            Seg.SegWidth += CharWidth
            LineWidth += CharWidth

            ' for next loop set last character
            PrevChar = CurChar

            ' box width
            Dim Width = BoxWidth
            If FirstLineIndent <> 0 AndAlso (LineArray.Count = 0 OrElse LineArray(LineArray.Count - 1).EndOfParagraph) Then Width -= FirstLineIndent

            ' current line width is less than or equal box width
            If LineWidth <= Width Then Continue For

            ' append text to current segemnt
            Seg.Text += Text.Substring(TextStart, TextPtr - TextStart + 1)
            TextStart = TextPtr + 1

            ' there are no breaks in this line or last segment is too long
            If LineBreakWidth < LineBreakFactor * Width Then
                BreakSegIndex = SegArray.Count - 1
                BreakPtr = Seg.Text.Length - 1
                BreakWidth = Seg.SegWidth - CharWidth
            End If

            ' break line
            BreakLine()

            ' add line up to break point
            AddLine(False)
        Next

        ' save text
        Seg.Text += Text.Substring(TextStart)

        ' exit
        Return
    End Sub

    Private Sub BreakLine()
        ' break segment at line break seg index into two segments
        Dim BreakSeg = SegArray(BreakSegIndex)

        ' add extra segment to segment array
        If BreakPtr <> 0 Then
            Dim ExtraSeg As TextBoxSeg = New TextBoxSeg(BreakSeg)
            ExtraSeg.SegWidth = BreakWidth
            ExtraSeg.Text = BreakSeg.Text.Substring(0, BreakPtr)
            SegArray.Insert(BreakSegIndex, ExtraSeg)
            BreakSegIndex += 1
        End If

        ' remove blanks from the area between the two sides of the segment
        While BreakPtr < BreakSeg.Text.Length AndAlso BreakSeg.Text(BreakPtr) = " "c
            BreakPtr += 1
        End While

        ' save the area after the first line
        If BreakPtr < BreakSeg.Text.Length Then
            BreakSeg.Text = BreakSeg.Text.Substring(BreakPtr)
            BreakSeg.SegWidth = BreakSeg.Font.TextWidth(BreakSeg.FontSize, BreakSeg.Text)
        Else
            BreakSeg.Text = String.Empty
            BreakSeg.SegWidth = 0.0
        End If

        BreakPtr = 0
        BreakWidth = 0.0
        Return
    End Sub

    Private Sub AddLine(EndOfParagraph As Boolean)
        ' end of paragraph
        If EndOfParagraph Then BreakSegIndex = SegArray.Count

        ' test for box too narrow
        If BreakSegIndex < 1 Then Throw New ApplicationException("TextBox is too narrow.")

        ' test for possible trailing blanks
        If SegArray(BreakSegIndex - 1).Text.EndsWith(" ") Then
            ' remove trailing blanks
            While BreakSegIndex > 0
                Dim TempSeg = SegArray(BreakSegIndex - 1)
                TempSeg.Text = TempSeg.Text.TrimEnd(New Char() {" "c})
                TempSeg.SegWidth = TempSeg.Font.TextWidth(TempSeg.FontSize, TempSeg.Text)
                If TempSeg.Text.Length <> 0 OrElse BreakSegIndex = 1 AndAlso EndOfParagraph Then Exit While
                BreakSegIndex -= 1
                SegArray.RemoveAt(BreakSegIndex)
            End While
        End If

        ' test for abnormal case of a blank line and not end of paragraph
        If BreakSegIndex > 0 Then
            ' allocate segment array
            Dim LineSegArray = New TextBoxSeg(BreakSegIndex - 1) {}

            ' copy segments
            SegArray.CopyTo(0, LineSegArray, 0, BreakSegIndex)

            ' line ascent and descent
            Dim LineAscent As Double = 0
            Dim LineDescent As Double = 0

            ' loop for segments until line break segment index
            For Each Seg In LineSegArray
                Dim Ascent = Seg.Font.AscentPlusLeading(Seg.FontSize)
                If Ascent > LineAscent Then LineAscent = Ascent
                Dim Descent = Seg.Font.DescentPlusLeading(Seg.FontSize)
                If Descent > LineDescent Then LineDescent = Descent
                Dim SpaceCount = 0

                For Each Chr As Char In Seg.Text
                    If Chr = " "c Then SpaceCount += 1
                Next

                Seg.SpaceCount = SpaceCount
            Next

            ' add line
            LineArray.Add(New TextBoxLine(LineAscent, LineDescent, EndOfParagraph, LineSegArray))

            ' update column height
            BoxHeight += LineAscent + LineDescent

            ' update paragraph count
            If EndOfParagraph Then ParagraphCount += 1

            ' remove segments
            SegArray.RemoveRange(0, BreakSegIndex)
        End If

        ' switch to next line
        LineBreakWidth = 0.0
        BreakSegIndex = 0

        ' new line width
        LineWidth = 0.0

        For Each Seg In SegArray
            LineWidth += Seg.SegWidth
        Next
    End Sub
End Class
