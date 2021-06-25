#Region "Microsoft.VisualBasic::82eeef22797b4aa17b8248cba0d99c9d, mime\text%html\Render\CSS\CssLineBox.vb"

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

    '     Class CssLineBox
    ' 
    '         Properties: OwnerBox, Rectangles, RelatedBoxes, Words
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBaseLineHeight, GetMaxWordBottom, ToString, WordsOf
    ' 
    '         Sub: AssignRectanglesToBoxes, DrawRectangles, ReportExistanceOf, SetBaseLine, UpdateRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports rect = System.Drawing.Rectangle

Namespace Render.CSS

    ''' <summary>
    ''' Represents a line of text.
    ''' </summary>
    ''' <remarks>
    ''' To learn more about line-boxes see CSS spec:
    ''' http://www.w3.org/TR/CSS21/visuren.html
    ''' </remarks>
    Friend Class CssLineBox

#Region "Fields"

        Private _words As List(Of CssBoxWord)
        Private _ownerBox As CssBox
        Private _rects As Dictionary(Of CssBox, RectangleF)
        Private _relatedBoxes As List(Of CssBox)

#End Region

#Region "Ctors"

        ''' <summary>
        ''' Creates a new LineBox
        ''' </summary>
        Public Sub New(ownerBox As CssBox)
            _rects = New Dictionary(Of CssBox, RectangleF)()
            _relatedBoxes = New List(Of CssBox)()
            _words = New List(Of CssBoxWord)()
            _ownerBox = ownerBox
            _ownerBox.LineBoxes.Add(Me)
        End Sub

#End Region

#Region "Props"



        ''' <summary>
        ''' Gets a list of boxes related with the linebox. 
        ''' To know the words of the box inside this linebox, use the <see cref="WordsOf"/> method.
        ''' </summary>
        Public ReadOnly Property RelatedBoxes() As List(Of CssBox)
            Get
                Return _relatedBoxes
            End Get
        End Property


        ''' <summary>
        ''' Gets the words inside the linebox
        ''' </summary>
        Public ReadOnly Property Words() As List(Of CssBoxWord)
            Get
                Return _words
            End Get
        End Property

        ''' <summary>
        ''' Gets the owner box
        ''' </summary>
        Public ReadOnly Property OwnerBox() As CssBox
            Get
                Return _ownerBox
            End Get
        End Property

        ''' <summary>
        ''' Gets a List of rectangles that are to be painted on this linebox
        ''' </summary>
        Public ReadOnly Property Rectangles() As Dictionary(Of CssBox, RectangleF)
            Get
                Return _rects
            End Get
        End Property


#End Region

#Region "Methods"



        ''' <summary>
        ''' Gets the maximum bottom of the words
        ''' </summary>
        ''' <returns></returns>
        Public Function GetMaxWordBottom() As Single
            Dim res As Single = Single.MinValue

            For Each word As CssBoxWord In Words
                res = Max(res, word.Bottom)
            Next

            Return res
        End Function

#End Region

        ''' <summary>
        ''' Lets the linebox add the word an its box to their lists if necessary.
        ''' </summary>
        ''' <param name="word"></param>
        Friend Sub ReportExistanceOf(word As CssBoxWord)
            If Not Words.Contains(word) Then
                Words.Add(word)
            End If

            If Not RelatedBoxes.Contains(word.OwnerBox) Then
                RelatedBoxes.Add(word.OwnerBox)
            End If
        End Sub

        ''' <summary>
        ''' Return the words of the specified box that live in this linebox
        ''' </summary>
        ''' <param name="box"></param>
        ''' <returns></returns>
        Friend Function WordsOf(box As CssBox) As List(Of CssBoxWord)
            Dim r As New List(Of CssBoxWord)()

            For Each word As CssBoxWord In Words
                If word.OwnerBox.Equals(box) Then
                    r.Add(word)
                End If
            Next

            Return r
        End Function

        ''' <summary>
        ''' Updates the specified rectangle of the specified box.
        ''' </summary>
        ''' <param name="box"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="r"></param>
        ''' <param name="b"></param>
        Friend Sub UpdateRectangle(box As CssBox, x As Single, y As Single, r As Single, b As Single)
            Dim leftspacing As Single = box.ActualBorderLeftWidth + box.ActualPaddingLeft
            Dim rightspacing As Single = box.ActualBorderRightWidth + box.ActualPaddingRight
            Dim topspacing As Single = box.ActualBorderTopWidth + box.ActualPaddingTop
            Dim bottomspacing As Single = box.ActualBorderBottomWidth + box.ActualPaddingTop

            If (box.FirstHostingLineBox IsNot Nothing AndAlso box.FirstHostingLineBox.Equals(Me)) OrElse box.IsImage Then
                x -= leftspacing
            End If
            If (box.LastHostingLineBox IsNot Nothing AndAlso box.LastHostingLineBox.Equals(Me)) OrElse box.IsImage Then
                r += rightspacing
            End If

            If Not box.IsImage Then
                y -= topspacing
                b += bottomspacing
            End If


            If Not Rectangles.ContainsKey(box) Then
                Rectangles.Add(box, RectangleF.FromLTRB(x, y, r, b))
            Else
                Dim f As RectangleF = Rectangles(box)
                Rectangles(box) = RectangleF.FromLTRB(Min(f.X, x), Min(f.Y, y), Max(f.Right, r), Max(f.Bottom, b))
            End If

            If box.ParentBox IsNot Nothing AndAlso box.ParentBox.Display = CssConstants.Inline Then
                UpdateRectangle(box.ParentBox, x, y, r, b)
            End If
        End Sub

        ''' <summary>
        ''' Copies the rectangles to their specified box
        ''' </summary>
        Friend Sub AssignRectanglesToBoxes()
            For Each b As CssBox In Rectangles.Keys
                b.Rectangles.Add(Me, Rectangles(b))
            Next
        End Sub

        ''' <summary>
        ''' Draws the rectangles for debug purposes
        ''' </summary>
        ''' <param name="g"></param>
        Friend Sub DrawRectangles(g As Graphics)
            For Each b As CssBox In Rectangles.Keys
                If Single.IsInfinity(Rectangles(b).Width) Then
                    Continue For
                End If
                g.FillRectangle(New SolidBrush(Color.FromArgb(50, Color.Black)), rect.Round(Rectangles(b)))
                g.DrawRectangle(Pens.Red, rect.Round(Rectangles(b)))
            Next
        End Sub

        ''' <summary>
        ''' Gets the baseline Height of the rectangle
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        Public Function GetBaseLineHeight(b As CssBox, g As Graphics) As Single
            Dim f As Font = b.ActualFont
            Dim ff As FontFamily = f.FontFamily
            Dim s As FontStyle = f.Style
            Return f.GetHeight(g) * ff.GetCellAscent(s) / ff.GetLineSpacing(s)
        End Function

        ''' <summary>
        ''' Sets the baseline of the words of the specified box to certain height
        ''' </summary>
        ''' <param name="g">Device info</param>
        ''' <param name="b">box to check words</param>
        ''' <param name="baseline">baseline</param>
        Friend Sub SetBaseLine(g As Graphics, b As CssBox, baseline As Single)
            'TODO: Aqui me quede, checar poniendo "by the" con un font-size de 3em
            Dim ws As List(Of CssBoxWord) = WordsOf(b)

            If Not Rectangles.ContainsKey(b) Then
                Return
            End If

            Dim r As RectangleF = Rectangles(b)

            'Save top of words related to the top of rectangle
            Dim gap As Single = 0F

            If ws.Count > 0 Then
                gap = ws(0).Top - r.Top
            Else
                Dim firstw As CssBoxWord = b.FirstWordOccourence(b, Me)

                If firstw IsNot Nothing Then
                    gap = firstw.Top - r.Top
                End If
            End If

            'New top that words will have
            'float newtop = baseline - (Height - OwnerBox.FontDescent - 3); //OLD
            Dim newtop As Single = baseline - GetBaseLineHeight(b, g)
            'OLD
            If b.ParentBox IsNot Nothing AndAlso b.ParentBox.Rectangles.ContainsKey(Me) AndAlso r.Height < b.ParentBox.Rectangles(Me).Height Then
                'Do this only if rectangle is shorter than parent's
                Dim recttop As Single = newtop - gap
                Dim newr As New RectangleF(r.X, recttop, r.Width, r.Height)
                Rectangles(b) = newr
                b.OffsetRectangle(Me, gap)
            End If
            For Each w As CssBoxWord In ws
                If Not w.IsImage Then
                    w.Top = newtop
                End If
            Next
        End Sub

        ''' <summary>
        ''' Returns the words of the linebox
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim ws As String() = New String(Words.Count - 1) {}
            For i As Integer = 0 To ws.Length - 1
                ws(i) = Words(i).Text
            Next
            Return String.Join(" ", ws)
        End Function
    End Class
End Namespace
