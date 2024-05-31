#Region "Microsoft.VisualBasic::4943cd413342032c18c9e4f2746f746c, mime\text%html\Render\CSS\CssBoxWord.vb"

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

    '   Total Lines: 195
    '    Code Lines: 106 (54.36%)
    ' Comment Lines: 56 (28.72%)
    '    - Xml Docs: 96.43%
    ' 
    '   Blank Lines: 33 (16.92%)
    '     File Size: 5.88 KB


    '     Class CssBoxWord
    ' 
    '         Properties: FullWidth, Image, IsImage, IsLineBreak, IsSpaces
    '                     IsTab, LastMeasureOffset, OwnerBox, Text
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: AppendChar, ReplaceLineBreaksAndTabs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Render.CSS

    ''' <summary>
    ''' Represents a word inside an inline box
    ''' </summary>
    ''' <remarks>
    ''' Because of performance, words of text are the most atomic 
    ''' element in the project. It should be characters, but come on,
    ''' imagine the performance when drawing char by char on the device.
    ''' 
    ''' It may change for future versions of the library
    ''' </remarks>
    Friend Class CssBoxWord : Inherits CssRectangle

#Region "Fields"


        Private _word As String
        Private _lastMeasureOffset As PointF
        Private _ownerBox As CssBox
        Private _spacesAfter As Integer
        Private _breakAfter As Boolean
        Private _spacesBefore As Integer
        Private _breakBefore As Boolean
        Private _image As Drawing.Image


#End Region

#Region "Ctor"

        Friend Sub New(owner As CssBox)
            _ownerBox = owner
            _word = String.Empty
        End Sub

        ''' <summary>
        ''' Creates a new BoxWord which represents an image
        ''' </summary>
        ''' <param name="owner"></param>
        ''' <param name="image"></param>
        Public Sub New(owner As CssBox, image As Drawing.Image)
            Me.New(owner)
            Me.Image = image
        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets the width of the word including white-spaces
        ''' </summary>
        Public ReadOnly Property FullWidth() As Single
            'get { return OwnerBox.ActualWordSpacing * (SpacesBefore + SpacesAfter) + Width; }
            Get
                Return Width
            End Get
        End Property

        ''' <summary>
        ''' Gets the image this words represents (if one)
        ''' </summary>
        Public Property Image() As Drawing.Image
            Get
                Return _image
            End Get
            Set
                _image = Value

                If Value IsNot Nothing Then
                    Dim w As New CssLength(OwnerBox.Width)
                    Dim h As New CssLength(OwnerBox.Height)
                    If w.Number > 0 AndAlso w.Unit = CssUnit.Pixels Then
                        Width = w.Number
                    Else
                        Width = Value.Width
                    End If

                    If h.Number > 0 AndAlso h.Unit = CssUnit.Pixels Then

                        Height = h.Number
                    Else
                        Height = Value.Height
                    End If


                    Height += OwnerBox.ActualBorderBottomWidth + OwnerBox.ActualBorderTopWidth + OwnerBox.ActualPaddingTop + OwnerBox.ActualPaddingBottom
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets if the word represents an image.
        ''' </summary>
        Public ReadOnly Property IsImage() As Boolean
            Get
                Return Image IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets a bool indicating if this word is composed only by spaces.
        ''' Spaces include tabs and line breaks
        ''' </summary>
        Public ReadOnly Property IsSpaces() As Boolean
            Get
                Return String.IsNullOrEmpty(Text.Trim())
            End Get
        End Property

        ''' <summary>
        ''' Gets if the word is composed by only a line break
        ''' </summary>
        Public ReadOnly Property IsLineBreak() As Boolean
            Get
                Return Text = vbLf
            End Get
        End Property

        ''' <summary>
        ''' Gets if the word is composed by only a tab
        ''' </summary>
        Public ReadOnly Property IsTab() As Boolean
            Get
                Return Text = vbTab
            End Get
        End Property

        ''' <summary>
        ''' Gets the Box where this word belongs.
        ''' </summary>
        Public ReadOnly Property OwnerBox() As CssBox
            Get
                Return _ownerBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the text of the word
        ''' </summary>
        Public ReadOnly Property Text() As String
            Get
                Return _word
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets an offset to be considered in measurements
        ''' </summary>
        Friend Property LastMeasureOffset() As PointF
            Get
                Return _lastMeasureOffset
            End Get
            Set
                _lastMeasureOffset = Value
            End Set
        End Property

#End Region

#Region "Methods"

        ''' <summary>
        ''' Removes line breaks and tabs on the text of the word,
        ''' replacing them with white spaces
        ''' </summary>
        Friend Sub ReplaceLineBreaksAndTabs()
            _word = _word.Replace(ControlChars.Lf, " "c)
            _word = _word.Replace(ControlChars.Tab, " "c)
        End Sub

        ''' <summary>
        ''' Appends the specified char to the word's text
        ''' </summary>
        ''' <param name="c"></param>
        Friend Sub AppendChar(c As Char)
            _word += c
        End Sub

        ''' <summary>
        ''' Represents this word for debugging purposes
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String

            Return String.Format("{0} ({1} char{2})", Text.Replace(" "c, "-"c).Replace(vbLf, "\n"), Text.Length, If(Text.Length <> 1, "s", String.Empty))
        End Function

#End Region
    End Class
End Namespace
