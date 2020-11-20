#Region "Microsoft.VisualBasic::0413b9bf6f675dbfe644352b9033524a, mime\text%html\HTML\CSS\Render\CssBox.vb"

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

    '     Class CssBox
    ' 
    '         Properties: ActualBackgroundColor, ActualBackgroundGradient, ActualBackgroundGradientAngle, ActualBorderBottomColor, ActualBorderBottomWidth
    '                     ActualBorderLeftColor, ActualBorderLeftWidth, ActualBorderRightColor, ActualBorderRightWidth, ActualBorderSpacingHorizontal
    '                     ActualBorderSpacingVertical, ActualBorderTopColor, ActualBorderTopWidth, ActualBottom, ActualColor
    '                     ActualCornerNE, ActualCornerNW, ActualCornerSE, ActualCornerSW, ActualFont
    '                     ActualMarginBottom, ActualMarginLeft, ActualMarginRight, ActualMarginTop, ActualPaddingBottom
    '                     ActualPaddingLeft, ActualPaddingRight, ActualPaddingTop, ActualParentFont, ActualRight
    '                     ActualTextIndent, ActualWordSpacing, AvailableWidth, BackgroundColor, BackgroundGradient
    '                     BackgroundGradientAngle, BackgroundImage, BackgroundRepeat, Border, BorderBottom
    '                     BorderBottomColor, BorderBottomStyle, BorderBottomWidth, BorderCollapse, BorderColor
    '                     BorderLeft, BorderLeftColor, BorderLeftStyle, BorderLeftWidth, BorderRight
    '                     BorderRightColor, BorderRightStyle, BorderRightWidth, BorderSpacing, BorderStyle
    '                     BorderTop, BorderTopColor, BorderTopStyle, BorderTopWidth, BorderWidth
    '                     Bounds, Boxes, ClientBottom, ClientLeft, ClientRectangle
    '                     ClientRight, ClientTop, Color, ContainingBlock, CornerNERadius
    '                     CornerNWRadius, CornerRadius, CornerSERadius, CornerSWRadius, Direction
    '                     Display, EmptyCells, FirstHostingLineBox, FirstWord, Float
    '                     Font, FontAscent, FontDescent, FontFamily, FontLineSpacing
    '                     FontSize, FontStyle, FontVariant, FontWeight, Height
    '                     HtmlTag, InitialContainer, IsImage, IsRounded, IsSpaceOrEmpty
    '                     LastHostingLineBox, LastWord, Left, LineBoxes, LineHeight
    '                     ListItemBox, ListStyle, ListStyleImage, ListStylePosition, ListStyleType
    '                     Location, Margin, MarginBottom, MarginLeft, MarginRight
    '                     MarginTop, Padding, PaddingBottom, PaddingLeft, PaddingRight
    '                     PaddingTop, ParentBox, ParentLineBoxes, Position, Rectangles
    '                     Size, Text, TextAlign, TextDecoration, TextIndent
    '                     Top, VerticalAlign, WhiteSpace, Width, Words
    '                     WordSpacing
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: ContainsInlinesOnly, FirstWordOccourence, (+2 Overloads) GetAttribute, GetDefaultValue, GetEmHeight
    '                   GetFullWidth, GetIndexForList, GetMaximumBottom, GetMinimumWidth, GetNextSibling
    '                   GetPreviousSibling, HasJustInlineSiblings, InlineAssignHelper, MarginCollapse, NoEms
    '                   ToString
    ' 
    '         Sub: CreateListItemBox, GetFullWidth_WordsWith, GetMinimumWidth_BubblePadding, GetMinimumWidth_LongestWord, (+2 Overloads) InheritStyle
    '              MeasureBounds, MeasureWordSpacing, MeasureWordsSize, OffsetRectangle, OffsetTop
    '              Paint, PaintBackground, PaintBorder, PaintDecoration, RectanglesReset
    '              RemoveAnonymousSpaces, (+2 Overloads) SetBounds, SetInitialContainer, UpdateWords
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.Render
Imports sys = System.Math
Imports HTMLParser = Microsoft.VisualBasic.MIME.Markup.HTML.Render.Parser
Imports rect = System.Drawing.Rectangle

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents a CSS Box of text or replaced elements.
    ''' </summary>
    ''' <remarks>
    ''' The Box can contains other boxes, that's the way that the CSS Tree
    ''' is composed.
    ''' 
    ''' To know more about boxes visit CSS spec:
    ''' http://www.w3.org/TR/CSS21/box.html
    ''' </remarks>
    Public Class CssBox

#Region "Static"

        Shared ReadOnly EmptyColor As Color = Global.System.Drawing.Color.Empty

        ''' <summary>
        ''' An empty box with empty values.
        ''' </summary>
        Friend Shared ReadOnly Empty As CssBox

        ''' <summary>
        ''' Table of 'css-property' => .NET property
        ''' </summary>
        Friend Shared _properties As Dictionary(Of String, PropertyInfo)

        ''' <summary>
        ''' Dictionary of default values
        ''' </summary>
        Private Shared _defaults As Dictionary(Of String, String)

        ''' <summary>
        ''' Hosts all inhertiable properties
        ''' </summary>
        Private Shared _inheritables As List(Of PropertyInfo)

        ''' <summary>
        ''' Hosts css properties
        ''' </summary>
        Private Shared _cssproperties As List(Of PropertyInfo)

        ''' <summary>
        ''' Static constructor and initialization
        ''' </summary>
        Shared Sub New()
            '#Region "Initialize _properties, _inheritables and _defaults Dictionaries"

            _properties = New Dictionary(Of String, PropertyInfo)()
            _defaults = New Dictionary(Of String, String)()
            _inheritables = New List(Of PropertyInfo)()
            _cssproperties = New List(Of PropertyInfo)()

            Dim props As PropertyInfo() = GetType(CssBox).GetProperties()

            For i As Integer = 0 To props.Length - 1
                Dim att As CssPropertyAttribute = TryCast(Attribute.GetCustomAttribute(props(i), GetType(CssPropertyAttribute)), CssPropertyAttribute)

                If att IsNot Nothing Then
                    _properties.Add(att.Name, props(i))
                    _defaults.Add(att.Name, GetDefaultValue(props(i)))
                    _cssproperties.Add(props(i))

                    Dim inh As CssPropertyInheritedAttribute = TryCast(Attribute.GetCustomAttribute(props(i), GetType(CssPropertyInheritedAttribute)), CssPropertyInheritedAttribute)

                    If inh IsNot Nothing Then
                        _inheritables.Add(props(i))
                    End If
                End If
            Next
            '#End Region

            Empty = New CssBox()
        End Sub

        ''' <summary>
        ''' Gets the default value of the specified css property
        ''' </summary>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Shared Function GetDefaultValue(prop As PropertyInfo) As String
            Dim att As DefaultValueAttribute = TryCast(Attribute.GetCustomAttribute(prop, GetType(DefaultValueAttribute)), DefaultValueAttribute)

            If att Is Nothing Then
                Return String.Empty
            Else
                Dim s As String = Convert.ToString(att.Value)
                Return If(String.IsNullOrEmpty(s), String.Empty, s)
            End If
        End Function

#End Region

#Region "CSS Fields"

        Private _backgroundColor As String
        Private _backgroundGradient As String
        Private _backgroundGradientAngle As String
        Private _BackgroundImage As String
        Private _backgroundRepeat As String
        Private _borderWidth As String
        Private _borderTopColor As String
        Private _borderRightColor As String
        Private _borderBottomColor As String
        Private _borderLeftColor As String
        Private _borderColor As String
        Private _borderTopStyle As String
        Private _borderRightStyle As String
        Private _borderBottomStyle As String
        Private _borderLeftStyle As String
        Private _borderStyle As String
        Private _borderBottom As String
        Private _borderLeft As String
        Private _borderRight As String
        Private _borderTop As String
        Private _borderSpacing As String
        Private _borderCollapse As String
        Private _border As String
        Private _bottom As String
        Private _color As String
        Private _cornerNWRadius As String
        Private _cornerNERadius As String
        Private _cornerSERadius As String
        Private _cornerSWRadius As String
        Private _cornerRadius As String
        Private _emptyCells As String
        Private _direction As String
        Private _display As String
        Private _font As String
        Private _fontFamily As String
        Private _fontSize As String
        Private _fontStyle As String
        Private _fontVariant As String
        Private _fontWeight As String
        Private _float As String
        Private _margin As String
        Private _lineHeight As String
        Private _listStyleType As String
        Private _listStyleImage As String
        Private _listStylePosition As String
        Private _listStyle As String
        Private _paddingLeft As String
        Private _paddingBottom As String
        Private _paddingRight As String
        Private _paddingTop As String
        Private _padding As String
        Private _right As String
        Private _text As String
        Private _textAlign As String
        Private _textDecoration As String
        Private _textIndent As String
        Private _position As String
        Private _verticalAlign As String
        Private _wordSpacing As String
        Private _whiteSpace As String

#End Region

#Region "Fields"

        ''' <summary>
        ''' Do not use or alter this flag
        ''' </summary>
        ''' <remarks>
        ''' Flag that indicates that CssTable algorithm already made fixes on it.
        ''' </remarks>
        Friend TableFixed As Boolean

        Private _boxWords As List(Of CssBoxWord)
        Private _boxes As List(Of CssBox)
        Private _parentBox As CssBox
        Private _wordsSizeMeasured As Boolean
        Private _size As SizeF
        Private _location As PointF
        Private _lineBoxes As List(Of CssLineBox)
        Private _parentLineBoxes As List(Of CssLineBox)
        Private _fontAscent As Single = Single.NaN
        Private _fontDescent As Single = Single.NaN
        Private _fontLineSpacing As Single = Single.NaN
        Private _htmltag As HtmlTag
        Private _rectangles As Dictionary(Of CssLineBox, RectangleF) = Nothing
        Protected _initialContainer As InitialContainer = Nothing
        Private _listItemBox As CssBox
        Private _firstHostingLineBox As CssLineBox
        Private _lastHostingLineBox As CssLineBox

#End Region

#Region "Ctor"

        Protected Sub New()
            _boxWords = New List(Of CssBoxWord)()
            _boxes = New List(Of CssBox)()
            _lineBoxes = New List(Of CssLineBox)()
            _parentLineBoxes = New List(Of CssLineBox)()
            _rectangles = New Dictionary(Of CssLineBox, RectangleF)()

            '#Region "Initialize properties with default values"

            For Each prop As String In _properties.Keys
                _properties(prop).SetValue(Me, _defaults(prop), Nothing)

                '#End Region
            Next
        End Sub

        Public Sub New(parentBox As CssBox)
            Me.New()
            Me.ParentBox = parentBox
        End Sub

        Friend Sub New(parentBox As CssBox, tag As HtmlTag)
            Me.New(parentBox)
            _htmltag = tag
        End Sub

#End Region

#Region "CSS Properties"

#Region "Visual Formatting"

#Region "Border"

#Region "Border Width"

        <CssProperty("border-bottom-width")>
        <DefaultValue("medium")>
        Public Property BorderBottomWidth() As String

        <CssProperty("border-left-width")>
        <DefaultValue("medium")>
        Public Property BorderLeftWidth() As String

        <CssProperty("border-right-width")>
        <DefaultValue("medium")>
        Public Property BorderRightWidth() As String

        <CssProperty("border-top-width")>
        <DefaultValue("medium")>
        Public Property BorderTopWidth() As String

        <CssProperty("border-width")>
        <DefaultValue("")>
        Public Property BorderWidth() As String
            Get
                Return _borderWidth
            End Get
            Set
                _borderWidth = Value

                Dim values As String() = CssValue.SplitValues(Value)

                Select Case values.Length
                    Case 1
                        BorderTopWidth = InlineAssignHelper(BorderLeftWidth, InlineAssignHelper(BorderRightWidth, InlineAssignHelper(BorderBottomWidth, values(0))))

                    Case 2
                        BorderTopWidth = InlineAssignHelper(BorderBottomWidth, values(0))
                        BorderLeftWidth = InlineAssignHelper(BorderRightWidth, values(1))

                    Case 3
                        BorderTopWidth = values(0)
                        BorderLeftWidth = InlineAssignHelper(BorderRightWidth, values(1))
                        BorderBottomWidth = values(2)

                    Case 4
                        BorderTopWidth = values(0)
                        BorderRightWidth = values(1)
                        BorderBottomWidth = values(2)
                        BorderLeftWidth = values(3)

                    Case Else


                End Select
            End Set
        End Property

#End Region

#Region "Border Style"

        <CssProperty("border-bottom-style")>
        <DefaultValue("none")>
        Public Property BorderBottomStyle() As String
            Get
                Return _borderBottomStyle
            End Get
            Set
                _borderBottomStyle = Value
            End Set
        End Property


        <CssProperty("border-left-style")>
        <DefaultValue("none")>
        Public Property BorderLeftStyle() As String
            Get
                Return _borderLeftStyle
            End Get
            Set
                _borderLeftStyle = Value
            End Set
        End Property

        <CssProperty("border-right-style")>
        <DefaultValue("none")>
        Public Property BorderRightStyle() As String
            Get
                Return _borderRightStyle
            End Get
            Set
                _borderRightStyle = Value
            End Set
        End Property

        <CssProperty("border-style")>
        <DefaultValue("")>
        Public Property BorderStyle() As String
            Get
                Return _borderStyle
            End Get
            Set
                _borderStyle = Value

                Dim values As String() = CssValue.SplitValues(Value)

                Select Case values.Length
                    Case 1
                        BorderTopStyle = InlineAssignHelper(BorderLeftStyle, InlineAssignHelper(BorderRightStyle, InlineAssignHelper(BorderBottomStyle, values(0))))

                    Case 2
                        BorderTopStyle = InlineAssignHelper(BorderBottomStyle, values(0))
                        BorderLeftStyle = InlineAssignHelper(BorderRightStyle, values(1))

                    Case 3
                        BorderTopStyle = values(0)
                        BorderLeftStyle = InlineAssignHelper(BorderRightStyle, values(1))
                        BorderBottomStyle = values(2)

                    Case 4
                        BorderTopStyle = values(0)
                        BorderRightStyle = values(1)
                        BorderBottomStyle = values(2)
                        BorderLeftStyle = values(3)

                    Case Else


                End Select
            End Set
        End Property

        <CssProperty("border-top-style")>
        <DefaultValue("none")>
        Public Property BorderTopStyle() As String
            Get
                Return _borderTopStyle
            End Get
            Set
                _borderTopStyle = Value
            End Set
        End Property
#End Region

#Region "Border Color"

        <CssProperty("border-color")>
        <DefaultValue("black")>
        Public Property BorderColor() As String
            Get
                Return _borderColor
            End Get
            Set
                _borderColor = Value

                Dim colors As MatchCollection = HTMLParser.Match(HTMLParser.CssColors, Value)

                Dim values As String() = New String(colors.Count - 1) {}

                For i As Integer = 0 To values.Length - 1
                    values(i) = colors(i).Value
                Next

                Select Case values.Length
                    Case 1
                        BorderTopColor = InlineAssignHelper(BorderLeftColor, InlineAssignHelper(BorderRightColor, InlineAssignHelper(BorderBottomColor, values(0))))

                    Case 2
                        BorderTopColor = InlineAssignHelper(BorderBottomColor, values(0))
                        BorderLeftColor = InlineAssignHelper(BorderRightColor, values(1))

                    Case 3
                        BorderTopColor = values(0)
                        BorderLeftColor = InlineAssignHelper(BorderRightColor, values(1))
                        BorderBottomColor = values(2)

                    Case 4
                        BorderTopColor = values(0)
                        BorderRightColor = values(1)
                        BorderBottomColor = values(2)
                        BorderLeftColor = values(3)

                    Case Else

                End Select
            End Set
        End Property

        <CssProperty("border-bottom-color")>
        <DefaultValue("black")>
        Public Property BorderBottomColor() As String
            Get
                Return _borderBottomColor
            End Get
            Set
                _borderBottomColor = Value
            End Set
        End Property

        <CssProperty("border-left-color")>
        <DefaultValue("black")>
        Public Property BorderLeftColor() As String
            Get
                Return _borderLeftColor
            End Get
            Set
                _borderLeftColor = Value
            End Set
        End Property

        <CssProperty("border-right-color")>
        <DefaultValue("black")>
        Public Property BorderRightColor() As String
            Get
                Return _borderRightColor
            End Get
            Set
                _borderRightColor = Value
            End Set
        End Property

        <CssProperty("border-top-color")>
        <DefaultValue("black")>
        Public Property BorderTopColor() As String
            Get
                Return _borderTopColor
            End Get
            Set
                _borderTopColor = Value
            End Set
        End Property

#End Region

#Region "Border ShortHands"
        <CssProperty("border")>
        <DefaultValue("")>
        Public Property Border() As String
            Get
                Return _border
            End Get
            Set
                _border = Value

                Dim borderWidth__1 As String = HTMLParser.Search(HTMLParser.CssBorderWidth, Value)
                Dim borderStyle__2 As String = HTMLParser.Search(HTMLParser.CssBorderStyle, Value)
                Dim borderColor__3 As String = HTMLParser.Search(HTMLParser.CssColors, Value)

                If borderWidth__1 IsNot Nothing Then
                    BorderWidth = borderWidth__1
                End If
                If borderStyle__2 IsNot Nothing Then
                    BorderStyle = borderStyle__2
                End If
                If borderColor__3 IsNot Nothing Then
                    BorderColor = borderColor__3
                End If
            End Set
        End Property

        <CssProperty("border-bottom")>
        <DefaultValue("")>
        Public Property BorderBottom() As String
            Get
                Return _borderBottom
            End Get
            Set
                _borderBottom = Value

                Dim borderWidth As String = HTMLParser.Search(HTMLParser.CssBorderWidth, Value)
                Dim borderStyle As String = HTMLParser.Search(HTMLParser.CssBorderStyle, Value)
                Dim borderColor As String = HTMLParser.Search(HTMLParser.CssColors, Value)

                If borderWidth IsNot Nothing Then
                    BorderBottomWidth = borderWidth
                End If
                If borderStyle IsNot Nothing Then
                    BorderBottomStyle = borderStyle
                End If
                If borderColor IsNot Nothing Then
                    BorderBottomColor = borderColor
                End If
            End Set
        End Property

        <CssProperty("border-left")>
        <DefaultValue("")>
        Public Property BorderLeft() As String
            Get
                Return _borderLeft
            End Get
            Set
                _borderLeft = Value

                Dim borderWidth As String = HTMLParser.Search(HTMLParser.CssBorderWidth, Value)
                Dim borderStyle As String = HTMLParser.Search(HTMLParser.CssBorderStyle, Value)
                Dim borderColor As String = HTMLParser.Search(HTMLParser.CssColors, Value)

                If borderWidth IsNot Nothing Then
                    BorderLeftWidth = borderWidth
                End If
                If borderStyle IsNot Nothing Then
                    BorderLeftStyle = borderStyle
                End If
                If borderColor IsNot Nothing Then
                    BorderLeftColor = borderColor
                End If
            End Set
        End Property

        <CssProperty("border-right")>
        <DefaultValue("")>
        Public Property BorderRight() As String
            Get
                Return _borderRight
            End Get
            Set
                _borderRight = Value

                Dim borderWidth As String = HTMLParser.Search(HTMLParser.CssBorderWidth, Value)
                Dim borderStyle As String = HTMLParser.Search(HTMLParser.CssBorderStyle, Value)
                Dim borderColor As String = HTMLParser.Search(HTMLParser.CssColors, Value)

                If borderWidth IsNot Nothing Then
                    BorderRightWidth = borderWidth
                End If
                If borderStyle IsNot Nothing Then
                    BorderRightStyle = borderStyle
                End If
                If borderColor IsNot Nothing Then
                    BorderRightColor = borderColor
                End If
            End Set
        End Property

        <CssProperty("border-top")>
        <DefaultValue("")>
        Public Property BorderTop() As String
            Get
                Return _borderTop
            End Get
            Set
                _borderTop = Value

                Dim borderWidth As String = HTMLParser.Search(HTMLParser.CssBorderWidth, Value)
                Dim borderStyle As String = HTMLParser.Search(HTMLParser.CssBorderStyle, Value)
                Dim borderColor As String = HTMLParser.Search(HTMLParser.CssColors, Value)

                If borderWidth IsNot Nothing Then
                    BorderTopWidth = borderWidth
                End If
                If borderStyle IsNot Nothing Then
                    BorderTopStyle = borderStyle
                End If
                If borderColor IsNot Nothing Then
                    BorderTopColor = borderColor
                End If
            End Set
        End Property

#End Region

#Region "Table borders"

        <CssProperty("border-spacing")>
        <DefaultValue("0")>
        <CssPropertyInherited>
        Public Property BorderSpacing() As String
            Get
                Return _borderSpacing
            End Get
            Set
                _borderSpacing = Value
            End Set
        End Property

        <CssProperty("border-collapse")>
        <DefaultValue("separate")>
        <CssPropertyInherited>
        Public Property BorderCollapse() As String
            Get
                Return _borderCollapse
            End Get
            Set
                _borderCollapse = Value
            End Set
        End Property


#End Region

#Region "Rounded corners"

        <CssProperty("corner-radius")>
        <DefaultValue("0")>
        Public Property CornerRadius() As String
            Get
                Return _cornerRadius
            End Get
            Set
                Dim r As MatchCollection = HTMLParser.Match(HTMLParser.CssLength, Value)

                Select Case r.Count
                    Case 1
                        CornerNERadius = r(0).Value
                        CornerNWRadius = r(0).Value
                        CornerSERadius = r(0).Value
                        CornerSWRadius = r(0).Value

                    Case 2
                        CornerNERadius = r(0).Value
                        CornerNWRadius = r(0).Value
                        CornerSERadius = r(1).Value
                        CornerSWRadius = r(1).Value

                    Case 3
                        CornerNERadius = r(0).Value
                        CornerNWRadius = r(1).Value
                        CornerSERadius = r(2).Value

                    Case 4
                        CornerNERadius = r(0).Value
                        CornerNWRadius = r(1).Value
                        CornerSERadius = r(2).Value
                        CornerSWRadius = r(3).Value

                End Select

                _cornerRadius = Value
            End Set
        End Property


        <CssProperty("corner-nw-radius")>
        <DefaultValue("0")>
        Public Property CornerNWRadius() As String
            Get
                Return _cornerNWRadius
            End Get
            Set
                _cornerNWRadius = Value
            End Set
        End Property

        <CssProperty("corner-ne-radius")>
        <DefaultValue("0")>
        Public Property CornerNERadius() As String
            Get
                Return _cornerNERadius
            End Get
            Set
                _cornerNERadius = Value
            End Set
        End Property

        <CssProperty("corner-se-radius")>
        <DefaultValue("0")>
        Public Property CornerSERadius() As String
            Get
                Return _cornerSERadius
            End Get
            Set
                _cornerSERadius = Value
            End Set
        End Property

        <CssProperty("corner-sw-radius")>
        <DefaultValue("0")>
        Public Property CornerSWRadius() As String
            Get
                Return _cornerSWRadius
            End Get
            Set
                _cornerSWRadius = Value
            End Set
        End Property


#End Region

#End Region

#Region "Margin"
        <CssProperty("margin")>
        <DefaultValue("")>
        Public Property Margin() As String
            Get
                Return _margin
            End Get
            Set
                _margin = Value
                Dim values As String() = CssValue.SplitValues(Value)

                Select Case values.Length
                    Case 1
                        MarginTop = InlineAssignHelper(MarginLeft, InlineAssignHelper(MarginRight, InlineAssignHelper(MarginBottom, values(0))))

                    Case 2
                        MarginTop = InlineAssignHelper(MarginBottom, values(0))
                        MarginLeft = InlineAssignHelper(MarginRight, values(1))

                    Case 3
                        MarginTop = values(0)
                        MarginLeft = InlineAssignHelper(MarginRight, values(1))
                        MarginBottom = values(2)

                    Case 4
                        MarginTop = values(0)
                        MarginRight = values(1)
                        MarginBottom = values(2)
                        MarginLeft = values(3)

                    Case Else


                End Select
            End Set
        End Property

        <CssProperty("margin-bottom")>
        <DefaultValue("0")>
        Public Property MarginBottom() As String

        <CssProperty("margin-left")>
        <DefaultValue("0")>
        Public Property MarginLeft() As String

        <CssProperty("margin-right")>
        <DefaultValue("0")>
        Public Property MarginRight() As String

        <CssProperty("margin-top")>
        <DefaultValue("0")>
        Public Property MarginTop() As String
#End Region

#Region "Padding"
        <CssProperty("padding")>
        <DefaultValue("")>
        Public Property Padding() As String
            Get
                Return _padding
            End Get
            Set
                _padding = Value

                Dim values As String() = CssValue.SplitValues(Value)

                Select Case values.Length
                    Case 1
                        PaddingTop = InlineAssignHelper(PaddingLeft, InlineAssignHelper(PaddingRight, InlineAssignHelper(PaddingBottom, values(0))))

                    Case 2
                        PaddingTop = InlineAssignHelper(PaddingBottom, values(0))
                        PaddingLeft = InlineAssignHelper(PaddingRight, values(1))

                    Case 3
                        PaddingTop = values(0)
                        PaddingLeft = InlineAssignHelper(PaddingRight, values(1))
                        PaddingBottom = values(2)

                    Case 4
                        PaddingTop = values(0)
                        PaddingRight = values(1)
                        PaddingBottom = values(2)
                        PaddingLeft = values(3)

                    Case Else

                End Select
            End Set
        End Property

        <CssProperty("padding-bottom")>
        <DefaultValue("0")>
        Public Property PaddingBottom() As String
            Get
                Return _paddingBottom
            End Get
            Set
                _paddingBottom = Value
                _actualPaddingBottom = Single.NaN
            End Set
        End Property

        <CssProperty("padding-left")>
        <DefaultValue("0")>
        Public Property PaddingLeft() As String
            Get
                Return _paddingLeft
            End Get
            Set
                _paddingLeft = Value
                _actualPaddingLeft = Single.NaN
            End Set
        End Property

        <CssProperty("padding-right")>
        <DefaultValue("0")>
        Public Property PaddingRight() As String
            Get
                Return _paddingRight
            End Get
            Set
                _paddingRight = Value
                _actualPaddingRight = Single.NaN
            End Set
        End Property

        <CssProperty("padding-top")>
        <DefaultValue("0")>
        Public Property PaddingTop() As String
            Get
                Return _paddingTop
            End Get
            Set
                _paddingTop = Value
                _actualPaddingTop = Single.NaN
            End Set
        End Property
#End Region

#Region "Bounds"

        <CssProperty("left")>
        <DefaultValue("auto")>
        Public Property Left() As String

        <CssProperty("top")>
        <DefaultValue("auto")>
        Public Property Top() As String

        '[CssProperty("right")]
        '[DefaultValue("auto")]
        'public string Right
        '{
        '    get { return _right; }
        '    set { _right = value; }
        '}

        '[CssProperty("bottom")]
        '[DefaultValue("auto")]
        'public string Bottom
        '{
        '    get { return _bottom; }
        '    set { _bottom = value; }
        '}

        <CssProperty("width")>
        <DefaultValue("auto")>
        Public Property Width() As String

        <CssProperty("height")>
        <DefaultValue("auto")>
        Public Property Height() As String
#End Region

#End Region

#Region "Colors and Backgrounds"

        <CssProperty("background-color")>
        <DefaultValue("transparent")>
        Public Property BackgroundColor() As String
            Get
                Return _backgroundColor
            End Get
            Set
                _backgroundColor = Value
            End Set
        End Property

        <CssProperty("background-image")>
        <DefaultValue("none")>
        Public Property BackgroundImage() As String
            Get
                Return _BackgroundImage
            End Get
            Set
                _BackgroundImage = Value
            End Set
        End Property

        <CssProperty("background-repeat")>
        <DefaultValue("repeat")>
        Public Property BackgroundRepeat() As String
            Get
                Return _backgroundRepeat
            End Get
            Set
                _backgroundRepeat = Value
            End Set
        End Property

        <CssProperty("background-gradient")>
        <DefaultValue("none")>
        Public Property BackgroundGradient() As String
            Get
                Return _backgroundGradient
            End Get
            Set
                _backgroundGradient = Value
            End Set
        End Property

        <CssProperty("background-gradient-angle")>
        <DefaultValue("90")>
        Public Property BackgroundGradientAngle() As String
            Get
                Return _backgroundGradientAngle
            End Get
            Set
                _backgroundGradientAngle = Value
            End Set
        End Property

        <CssProperty("color")>
        <DefaultValue("black")>
        <CssPropertyInherited>
        Public Property Color() As String
            Get
                Return _color
            End Get
            Set
                _color = Value
                _actualColor = EmptyColor
            End Set
        End Property

        <CssProperty("display")>
        <DefaultValue("inline")>
        Public Property Display() As String
            Get
                Return _display
            End Get
            Set
                _display = Value
            End Set
        End Property

        <CssProperty("direction")>
        <DefaultValue("ltr")>
        Public Property Direction() As String
            Get
                Return _direction
            End Get
            Set
                _direction = Value
            End Set
        End Property


        <CssProperty("empty-cells")>
        <DefaultValue("show")>
        <CssPropertyInherited>
        Public Property EmptyCells() As String
            Get
                Return _emptyCells
            End Get
            Set
                _emptyCells = Value
            End Set
        End Property


        <CssProperty("float")>
        <DefaultValue("none")>
        Public Property Float() As String
            Get
                Return _float
            End Get
            Set
                _float = Value
            End Set
        End Property

        <CssProperty("position")>
        <DefaultValue("static")>
        Public Property Position() As String
            Get
                Return _position
            End Get
            Set
                _position = Value
            End Set
        End Property
#End Region

#Region "Text"


        <CssProperty("line-height")>
        <DefaultValue("normal")>
        Public Property LineHeight() As String
            Get
                Return _lineHeight
            End Get
            Set
                _lineHeight = NoEms(Value)
            End Set
        End Property

        <CssProperty("vertical-align")>
        <DefaultValue("baseline")>
        <CssPropertyInherited>
        Public Property VerticalAlign() As String
            Get
                Return _verticalAlign
            End Get
            Set
                _verticalAlign = Value
            End Set
        End Property

        <CssProperty("text-indent")>
        <DefaultValue("0")>
        <CssPropertyInherited>
        Public Property TextIndent() As String
            Get
                Return _textIndent
            End Get
            Set
                _textIndent = NoEms(Value)
            End Set
        End Property

        <CssProperty("text-align")>
        <DefaultValue("")>
        <CssPropertyInherited>
        Public Property TextAlign() As String
            Get
                Return _textAlign
            End Get
            Set
                _textAlign = Value
            End Set
        End Property

        <CssProperty("text-decoration")>
        <DefaultValue("")>
        Public Property TextDecoration() As String
            Get
                Return _textDecoration
            End Get
            Set
                _textDecoration = Value
            End Set
        End Property

        <CssProperty("white-space")>
        <DefaultValue("normal")>
        <CssPropertyInherited>
        Public Property WhiteSpace() As String
            Get
                Return _whiteSpace
            End Get
            Set
                _whiteSpace = Value
            End Set
        End Property


        <CssProperty("word-spacing")>
        <DefaultValue("normal")>
        Public Property WordSpacing() As String
            Get
                Return _wordSpacing
            End Get
            Set
                _wordSpacing = NoEms(Value)
            End Set
        End Property

#End Region

#Region "Font"

        <CssProperty("font")>
        <DefaultValue("")>
        <CssPropertyInherited>
        Public Property Font() As String
            Get
                Return _font
            End Get
            Set
                _font = Value

                Dim mustBePos As Integer
                Dim mustBe As String = HTMLParser.Search(HTMLParser.CssFontSizeAndLineHeight, Value, mustBePos)

                If Not String.IsNullOrEmpty(mustBe) Then
                    mustBe = mustBe.Trim()
                    'Check for style||variant||weight on the left
                    Dim leftSide As String = Value.Substring(0, mustBePos)
                    Dim fontStyle__1 As String = HTMLParser.Search(HTMLParser.CssFontStyle, leftSide)
                    Dim fontVariant__2 As String = HTMLParser.Search(HTMLParser.CssFontVariant, leftSide)
                    Dim fontWeight__3 As String = HTMLParser.Search(HTMLParser.CssFontWeight, leftSide)

                    'Check for family on the right
                    Dim rightSide As String = Value.Substring(mustBePos + mustBe.Length)
                    Dim fontFamily__4 As String = rightSide.Trim()
                    'Parser.Search(Parser.CssFontFamily, rightSide); //TODO: Would this be right?
                    'Check for font-size and line-height
                    Dim fontSize__5 As String = mustBe
                    Dim lineHeight__6 As String = String.Empty

                    If mustBe.Contains("/") AndAlso mustBe.Length > mustBe.IndexOf("/") + 1 Then
                        Dim slashPos As Integer = mustBe.IndexOf("/")
                        fontSize__5 = mustBe.Substring(0, slashPos)
                        lineHeight__6 = mustBe.Substring(slashPos + 1)
                    End If

                    'Assign values p { font: 12px/14px sans-serif }
                    If Not String.IsNullOrEmpty(fontStyle__1) Then
                        FontStyle = fontStyle__1
                    End If
                    If Not String.IsNullOrEmpty(fontVariant__2) Then
                        FontVariant = fontVariant__2
                    End If
                    If Not String.IsNullOrEmpty(fontWeight__3) Then
                        FontWeight = fontWeight__3
                    End If
                    If Not String.IsNullOrEmpty(fontFamily__4) Then
                        FontFamily = fontFamily__4
                    End If
                    If Not String.IsNullOrEmpty(fontSize__5) Then
                        FontSize = fontSize__5
                    End If
                    If Not String.IsNullOrEmpty(lineHeight__6) Then
                        LineHeight = lineHeight__6
                    End If
                    ' Check for: caption | icon | menu | message-box | small-caption | status-bar
                    'TODO: Interpret font values of: caption | icon | menu | message-box | small-caption | status-bar
                Else
                End If
            End Set
        End Property

        <CssProperty("font-family")>
        <DefaultValue("serif")>
        <CssPropertyInherited>
        Public Property FontFamily() As String
            Get
                Return _fontFamily
            End Get
            Set

                ' HACK: Because of performance, generic font families
                '       will be checked when only the generic font 
                '       family is given.

                Select Case Value
                    Case CssConstants.Serif
                        _fontFamily = CssDefaults.FontSerif

                    Case CssConstants.SansSerif
                        _fontFamily = CssDefaults.FontSansSerif

                    Case CssConstants.Cursive
                        _fontFamily = CssDefaults.FontCursive

                    Case CssConstants.Fantasy
                        _fontFamily = CssDefaults.FontFantasy

                    Case CssConstants.Monospace
                        _fontFamily = CssDefaults.FontMonospace

                    Case Else
                        _fontFamily = Value
#If DEBUG Then
                        If _fontFamily.TextEquals("Ubuntu") Then
                            Call Me.ToString.__DEBUG_ECHO
                        End If
#End If
                        _fontFamily = FontFace.GetFontName(_fontFamily)
                End Select
            End Set
        End Property

        <CssProperty("font-size")>
        <DefaultValue("medium")>
        <CssPropertyInherited>
        Public Property FontSize() As String
            Get
                Return _fontSize
            End Get
            Set
                Dim length As String = HTMLParser.Search(HTMLParser.CssLength, Value)

                If length IsNot Nothing Then
                    Dim computedValue As String = String.Empty
                    Dim len As New CssLength(length)

                    If len.HasError Then
                        computedValue = _defaults("font-size")
                    ElseIf len.Unit = CssLength.CssUnit.Ems AndAlso ParentBox IsNot Nothing Then
                        computedValue = len.ConvertEmToPoints(ParentBox.ActualFont.SizeInPoints).ToString()
                    Else
                        computedValue = len.ToString()
                    End If

                    _fontSize = computedValue
                Else
                    _fontSize = Value

                End If
            End Set
        End Property

        <CssProperty("font-style")>
        <DefaultValue("normal")>
        <CssPropertyInherited>
        Public Property FontStyle() As String
            Get
                Return _fontStyle
            End Get
            Set
                _fontStyle = Value
            End Set
        End Property

        <CssProperty("font-variant")>
        <DefaultValue("normal")>
        <CssPropertyInherited>
        Public Property FontVariant() As String
            Get
                Return _fontVariant
            End Get
            Set
                _fontVariant = Value
            End Set
        End Property


        <CssProperty("font-weight")>
        <DefaultValue("normal")>
        <CssPropertyInherited>
        Public Property FontWeight() As String
            Get
                Return _fontWeight
            End Get
            Set
                _fontWeight = Value
            End Set
        End Property


#End Region

#Region "Lists"

        <CssProperty("list-style")>
        <DefaultValue("")>
        <CssPropertyInherited>
        Public Property ListStyle() As String
            Get
                Return _listStyle
            End Get
            Set
                _listStyle = Value
            End Set
        End Property

        <CssProperty("list-style-position")>
        <DefaultValue("outside")>
        <CssPropertyInherited>
        Public Property ListStylePosition() As String
            Get
                Return _listStylePosition
            End Get
            Set
                _listStylePosition = Value
            End Set
        End Property

        <CssProperty("list-style-image")>
        <DefaultValue("")>
        <CssPropertyInherited>
        Public Property ListStyleImage() As String
            Get
                Return _listStyleImage
            End Get
            Set
                _listStyleImage = Value
            End Set
        End Property

        <CssProperty("list-style-type")>
        <DefaultValue("disc")>
        <CssPropertyInherited>
        Public Property ListStyleType() As String
            Get
                Return _listStyleType
            End Get
            Set
                _listStyleType = Value
            End Set
        End Property


#End Region

#End Region

#Region "Actual Values Properties"

#Region "Fields"
        Private _actualCornerNW As Single = Single.NaN
        Private _actualCornerNE As Single = Single.NaN
        Private _actualCornerSW As Single = Single.NaN
        Private _actualCornerSE As Single = Single.NaN
        Private _actualColor As Color = EmptyColor
        Private _actualBackgroundGradientAngle As Single = Single.NaN
        Private _actualPaddingTop As Single = Single.NaN
        Private _actualPaddingBottom As Single = Single.NaN
        Private _actualPaddingRight As Single = Single.NaN
        Private _actualPaddingLeft As Single = Single.NaN
        Private _actualMarginTop As Single = Single.NaN
        Private _actualMarginBottom As Single = Single.NaN
        Private _actualMarginRight As Single = Single.NaN
        Private _actualMarginLeft As Single = Single.NaN
        Private _actualBorderTopWidth As Single = Single.NaN
        Private _actualBorderLeftWidth As Single = Single.NaN
        Private _actualBorderBottomWidth As Single = Single.NaN
        Private _actualBorderRightWidth As Single = Single.NaN
        Private _actualBackgroundGradient As Color = EmptyColor
        Private _actualBorderTopColor As Color = EmptyColor
        Private _actualBorderLeftColor As Color = EmptyColor
        Private _actualBorderBottomColor As Color = EmptyColor
        Private _actualBorderRightColor As Color = EmptyColor
        Private _actualWordSpacing As Single = Single.NaN
        Private _actualBackgroundColor As Color = EmptyColor
        Private _actualFont As Font = Nothing
        Private _actualTextIndent As Single = Single.NaN
        Private _actualBorderSpacingHorizontal As Single = Single.NaN
        Private _actualBorderSpacingVertical As Single = Single.NaN

#End Region

#Region "Boxing"
#Region "Padding"
        ''' <summary>
        ''' Gets the actual top's padding
        ''' </summary>
        Public ReadOnly Property ActualPaddingTop() As Single
            Get

                If Single.IsNaN(_actualPaddingTop) Then
                    _actualPaddingTop = CssValue.ParseLength(PaddingTop, Size.Width, Me)
                End If


                Return _actualPaddingTop
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual padding on the left
        ''' </summary>
        Public ReadOnly Property ActualPaddingLeft() As Single
            Get
                If Single.IsNaN(_actualPaddingLeft) Then
                    _actualPaddingLeft = CssValue.ParseLength(PaddingLeft, Size.Width, Me)
                End If
                Return _actualPaddingLeft
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual Padding of the bottom
        ''' </summary>
        Public ReadOnly Property ActualPaddingBottom() As Single
            Get
                If Single.IsNaN(_actualPaddingBottom) Then
                    _actualPaddingBottom = CssValue.ParseLength(PaddingBottom, Size.Width, Me)
                End If
                Return _actualPaddingBottom
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual padding on the right
        ''' </summary>
        Public ReadOnly Property ActualPaddingRight() As Single
            Get
                If Single.IsNaN(_actualPaddingRight) Then
                    _actualPaddingRight = CssValue.ParseLength(PaddingRight, Size.Width, Me)
                End If
                Return _actualPaddingRight
            End Get
        End Property

#End Region

#Region "Margin"
        ''' <summary>
        ''' Gets the actual top's Margin
        ''' </summary>
        Public ReadOnly Property ActualMarginTop() As Single
            Get

                If Single.IsNaN(_actualMarginTop) Then
                    If MarginTop = CssConstants.Auto Then
                        MarginTop = "0"
                    End If
                    _actualMarginTop = CssValue.ParseLength(MarginTop, Size.Width, Me)
                End If


                Return _actualMarginTop
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual Margin on the left
        ''' </summary>
        Public ReadOnly Property ActualMarginLeft() As Single
            Get
                If Single.IsNaN(_actualMarginLeft) Then
                    If MarginLeft = CssConstants.Auto Then
                        MarginLeft = "0"
                    End If
                    _actualMarginLeft = CssValue.ParseLength(MarginLeft, Size.Width, Me)
                End If
                Return _actualMarginLeft
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual Margin of the bottom
        ''' </summary>
        Public ReadOnly Property ActualMarginBottom() As Single
            Get
                If Single.IsNaN(_actualMarginBottom) Then
                    If MarginBottom = CssConstants.Auto Then
                        MarginBottom = "0"
                    End If
                    _actualMarginBottom = CssValue.ParseLength(MarginBottom, Size.Width, Me)
                End If
                Return _actualMarginBottom
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual Margin on the right
        ''' </summary>
        Public ReadOnly Property ActualMarginRight() As Single
            Get
                If Single.IsNaN(_actualMarginRight) Then
                    If MarginRight = CssConstants.Auto Then
                        MarginRight = "0"
                    End If
                    _actualMarginRight = CssValue.ParseLength(MarginRight, Size.Width, Me)
                End If
                Return _actualMarginRight
            End Get
        End Property
#End Region

#Region "Border"

#Region "Border Width"

        ''' <summary>
        ''' Gets the actual top border width
        ''' </summary>
        Public ReadOnly Property ActualBorderTopWidth() As Single
            Get
                If Single.IsNaN(_actualBorderTopWidth) Then
                    _actualBorderTopWidth = CssValue.GetActualBorderWidth(BorderTopWidth, Me)

                    If String.IsNullOrEmpty(BorderTopStyle) OrElse BorderTopStyle = CssConstants.None Then
                        _actualBorderTopWidth = 0F
                    End If
                End If

                Return _actualBorderTopWidth
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Left border width
        ''' </summary>
        Public ReadOnly Property ActualBorderLeftWidth() As Single
            Get
                If Single.IsNaN(_actualBorderLeftWidth) Then
                    _actualBorderLeftWidth = CssValue.GetActualBorderWidth(BorderLeftWidth, Me)

                    If String.IsNullOrEmpty(BorderLeftStyle) OrElse BorderLeftStyle = CssConstants.None Then
                        _actualBorderLeftWidth = 0F
                    End If
                End If

                Return _actualBorderLeftWidth
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Bottom border width
        ''' </summary>
        Public ReadOnly Property ActualBorderBottomWidth() As Single
            Get
                If Single.IsNaN(_actualBorderBottomWidth) Then
                    _actualBorderBottomWidth = CssValue.GetActualBorderWidth(BorderBottomWidth, Me)

                    If String.IsNullOrEmpty(BorderBottomStyle) OrElse BorderBottomStyle = CssConstants.None Then
                        _actualBorderBottomWidth = 0F
                    End If
                End If

                Return _actualBorderBottomWidth
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Right border width
        ''' </summary>
        Public ReadOnly Property ActualBorderRightWidth() As Single
            Get
                If Single.IsNaN(_actualBorderRightWidth) Then
                    _actualBorderRightWidth = CssValue.GetActualBorderWidth(BorderRightWidth, Me)

                    If String.IsNullOrEmpty(BorderRightStyle) OrElse BorderRightStyle = CssConstants.None Then
                        _actualBorderRightWidth = 0F
                    End If
                End If

                Return _actualBorderRightWidth
            End Get
        End Property

#End Region

#Region "Border Color"

        ''' <summary>
        ''' Gets the actual top border Color
        ''' </summary>
        Public ReadOnly Property ActualBorderTopColor() As Color
            Get
                If (_actualBorderTopColor.IsEmpty) Then
                    _actualBorderTopColor = CssValue.GetActualColor(BorderTopColor)
                End If

                Return _actualBorderTopColor
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Left border Color
        ''' </summary>
        Public ReadOnly Property ActualBorderLeftColor() As Color
            Get
                If (_actualBorderLeftColor.IsEmpty) Then
                    _actualBorderLeftColor = CssValue.GetActualColor(BorderLeftColor)
                End If

                Return _actualBorderLeftColor
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Bottom border Color
        ''' </summary>
        Public ReadOnly Property ActualBorderBottomColor() As Color
            Get
                If (_actualBorderBottomColor.IsEmpty) Then
                    _actualBorderBottomColor = CssValue.GetActualColor(BorderBottomColor)
                End If

                Return _actualBorderBottomColor
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual Right border Color
        ''' </summary>
        Public ReadOnly Property ActualBorderRightColor() As Color
            Get
                If (_actualBorderRightColor.IsEmpty) Then
                    _actualBorderRightColor = CssValue.GetActualColor(BorderRightColor)
                End If

                Return _actualBorderRightColor
            End Get
        End Property

#End Region

#End Region

#Region "Corners"

        ''' <summary>
        ''' Gets the actual lenght of the north west corner
        ''' </summary>
        Public ReadOnly Property ActualCornerNW() As Single
            Get
                If Single.IsNaN(_actualCornerNW) Then
                    _actualCornerNW = CssValue.ParseLength(CornerNWRadius, 0, Me)
                End If

                Return _actualCornerNW
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual lenght of the north east corner
        ''' </summary>
        Public ReadOnly Property ActualCornerNE() As Single
            Get
                If Single.IsNaN(_actualCornerNE) Then
                    _actualCornerNE = CssValue.ParseLength(CornerNERadius, 0, Me)
                End If
                Return _actualCornerNE
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual lenght of the south east corner
        ''' </summary>
        Public ReadOnly Property ActualCornerSE() As Single
            Get
                If Single.IsNaN(_actualCornerSE) Then
                    _actualCornerSE = CssValue.ParseLength(CornerSERadius, 0, Me)
                End If

                Return _actualCornerSE
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual lenght of the south west corner
        ''' </summary>
        Public ReadOnly Property ActualCornerSW() As Single
            Get
                If Single.IsNaN(_actualCornerSW) Then
                    _actualCornerSW = CssValue.ParseLength(CornerSWRadius, 0, Me)
                End If

                Return _actualCornerSW
            End Get
        End Property


#End Region
#End Region

#Region "Layout Formatting"

        ''' <summary>
        ''' Gets the actual word spacing of the word.
        ''' </summary>
        Public ReadOnly Property ActualWordSpacing() As Single
            Get
                If Single.IsNaN(_actualWordSpacing) Then
                    Throw New Exception("Space must be calculated before using this property")
                End If
                Return _actualWordSpacing
            End Get
        End Property


#End Region

#Region "Colors and Backgrounds"

        ''' <summary>
        ''' 
        ''' Gets the actual color for the text.
        ''' </summary>
        Public ReadOnly Property ActualColor() As Color
            Get

                If _actualColor.IsEmpty Then
                    _actualColor = CssValue.GetActualColor(Color)
                End If


                Return _actualColor
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual background color of the box
        ''' </summary>
        Public ReadOnly Property ActualBackgroundColor() As Color
            Get
                If _actualBackgroundColor.IsEmpty Then
                    _actualBackgroundColor = CssValue.GetActualColor(BackgroundColor)
                End If

                Return _actualBackgroundColor
            End Get
        End Property

        ''' <summary>
        ''' Gets the second color that creates a gradient for the background
        ''' </summary>
        Public ReadOnly Property ActualBackgroundGradient() As Color
            Get
                If _actualBackgroundGradient.IsEmpty Then
                    _actualBackgroundGradient = CssValue.GetActualColor(BackgroundGradient)
                End If
                Return _actualBackgroundGradient
            End Get
        End Property


        ''' <summary>
        ''' Gets the actual angle specified for the background gradient
        ''' </summary>
        Public ReadOnly Property ActualBackgroundGradientAngle() As Single
            Get
                If Single.IsNaN(_actualBackgroundGradientAngle) Then
                    _actualBackgroundGradientAngle = CssValue.ParseNumber(BackgroundGradientAngle, 360.0F)
                End If

                Return _actualBackgroundGradientAngle
            End Get
        End Property

#End Region

#Region "Fonts"

        ''' <summary>
        ''' Gets the actual font of the parent
        ''' </summary>
        Public ReadOnly Property ActualParentFont() As Font
            Get
                If ParentBox Is Nothing Then
                    Return ActualFont
                End If

                Return ParentBox.ActualFont
            End Get
        End Property

        ''' <summary>
        ''' Gets the font that should be actually used to paint the text of the box
        ''' </summary>
        Public ReadOnly Property ActualFont() As Font
            Get
                If _actualFont Is Nothing Then
                    If String.IsNullOrEmpty(FontFamily) Then
                        FontFamily = CssDefaults.FontSerif
                    End If
                    If String.IsNullOrEmpty(FontSize) Then
                        FontSize = CssDefaults.FontSize & "pt"
                    End If

                    Dim st As FontStyle = Global.System.Drawing.FontStyle.Regular

                    If FontStyle = CssConstants.Italic OrElse FontStyle = CssConstants.Oblique Then
                        st = st Or Global.System.Drawing.FontStyle.Italic
                    End If

                    If FontWeight <> CssConstants.Normal AndAlso FontWeight <> CssConstants.Lighter AndAlso Not String.IsNullOrEmpty(FontWeight) Then
                        st = st Or Global.System.Drawing.FontStyle.Bold
                    End If

                    Dim fsize As Single = 0F
                    Dim parentSize As Single = CssDefaults.FontSize

                    If ParentBox IsNot Nothing Then
                        parentSize = ParentBox.ActualFont.Size
                    End If

                    Select Case FontSize
                        Case CssConstants.Medium
                            fsize = CssDefaults.FontSize

                        Case CssConstants.XXSmall
                            fsize = CssDefaults.FontSize - 4

                        Case CssConstants.XSmall
                            fsize = CssDefaults.FontSize - 3

                        Case CssConstants.Small
                            fsize = CssDefaults.FontSize - 2

                        Case CssConstants.Large
                            fsize = CssDefaults.FontSize + 2

                        Case CssConstants.XLarge
                            fsize = CssDefaults.FontSize + 3

                        Case CssConstants.XXLarge
                            fsize = CssDefaults.FontSize + 4

                        Case CssConstants.Smaller
                            fsize = parentSize - 2

                        Case CssConstants.Larger
                            fsize = parentSize + 2

                        Case Else
                            fsize = CssValue.ParseLength(FontSize, parentSize, Me, parentSize, True)

                    End Select

                    If fsize <= 1.0F Then
                        fsize = CssDefaults.FontSize
                    End If

                    _actualFont = New Font(FontFamily, fsize, st)
                End If
                Return _actualFont
            End Get
        End Property

#End Region

#Region "Text"


        ''' <summary>
        ''' Gets the text indentation (on first line only)
        ''' </summary>
        Public ReadOnly Property ActualTextIndent() As Single
            Get
                If Single.IsNaN(_actualTextIndent) Then
                    _actualTextIndent = CssValue.ParseLength(TextIndent, Size.Width, Me)
                End If

                Return _actualTextIndent
            End Get
        End Property


#End Region

#Region "Tables"

        ''' <summary>
        ''' Gets the actual horizontal border spacing for tables
        ''' </summary>
        Public ReadOnly Property ActualBorderSpacingHorizontal() As Single
            Get
                If Single.IsNaN(_actualBorderSpacingHorizontal) Then
                    Dim matches As MatchCollection = HTMLParser.Match(HTMLParser.CssLength, BorderSpacing)

                    If matches.Count = 0 Then
                        _actualBorderSpacingHorizontal = 0
                    ElseIf matches.Count > 0 Then
                        _actualBorderSpacingHorizontal = CssValue.ParseLength(matches(0).Value, 1, Me)
                    End If
                End If


                Return _actualBorderSpacingHorizontal
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual vertical border spacing for tables
        ''' </summary>
        Public ReadOnly Property ActualBorderSpacingVertical() As Single
            Get
                If Single.IsNaN(_actualBorderSpacingVertical) Then
                    Dim matches As MatchCollection = HTMLParser.Match(HTMLParser.CssLength, BorderSpacing)

                    If matches.Count = 0 Then
                        _actualBorderSpacingVertical = 0
                    ElseIf matches.Count = 1 Then
                        _actualBorderSpacingVertical = CssValue.ParseLength(matches(0).Value, 1, Me)
                    Else
                        _actualBorderSpacingVertical = CssValue.ParseLength(matches(1).Value, 1, Me)
                    End If
                End If
                Return _actualBorderSpacingVertical
            End Get
        End Property


#End Region

#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets the • box
        ''' </summary>
        Public ReadOnly Property ListItemBox() As CssBox
            Get
                Return _listItemBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the width available on the box, counting padding and margin.
        ''' </summary>
        Public ReadOnly Property AvailableWidth() As Single
            Get
                Return Size.Width - ActualBorderLeftWidth - ActualPaddingLeft - ActualPaddingRight - ActualBorderRightWidth
            End Get
        End Property

        ''' <summary>
        ''' Gets the bounds of the box
        ''' </summary>
        Public ReadOnly Property Bounds() As RectangleF
            Get
                Return New RectangleF(Location, Size)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the bottom of the box. 
        ''' (When setting, alters only the Size.Height of the box)
        ''' </summary>
        Public Property ActualBottom() As Single
            Get
                Return Location.Y + Size.Height
            End Get
            Set
                Size = New SizeF(Size.Width, Value - Location.Y)
            End Set
        End Property

        ''' <summary>
        ''' Gets the childrenn boxes of this box
        ''' </summary>
        Public ReadOnly Property Boxes() As List(Of CssBox)
            Get
                Return _boxes
            End Get
        End Property

        ''' <summary>
        ''' Gets the left of the client rectangle (Where content starts rendering)
        ''' </summary>
        Public ReadOnly Property ClientLeft() As Single
            Get
                Return Location.X + ActualBorderLeftWidth + ActualPaddingLeft
            End Get
        End Property

        ''' <summary>
        ''' Gets the top of the client rectangle (Where content starts rendering)
        ''' </summary>
        Public ReadOnly Property ClientTop() As Single
            Get
                Return Location.Y + ActualBorderTopWidth + ActualPaddingTop
            End Get
        End Property

        ''' <summary>
        ''' Gets the right of the client rectangle
        ''' </summary>
        Public ReadOnly Property ClientRight() As Single
            Get
                Return ActualRight - ActualPaddingRight - ActualBorderRightWidth
            End Get
        End Property

        ''' <summary>
        ''' Gets the bottom of the client rectangle
        ''' </summary>
        Public ReadOnly Property ClientBottom() As Single
            Get
                Return ActualBottom - ActualPaddingBottom - ActualBorderBottomWidth
            End Get
        End Property

        ''' <summary>
        ''' Gets the client rectangle
        ''' </summary>
        Public ReadOnly Property ClientRectangle() As RectangleF
            Get
                Return RectangleF.FromLTRB(ClientLeft, ClientTop, ClientRight, ClientBottom)
            End Get
        End Property

        ''' <summary>
        ''' Gets the containing block-box of this box. (The nearest parent box with display=block)
        ''' </summary>
        Public ReadOnly Property ContainingBlock() As CssBox
            Get
                If ParentBox Is Nothing Then
                    'This is the initial containing block.
                    Return Me
                End If

                Dim b As CssBox = ParentBox

                While b.Display <> CssConstants.Block AndAlso b.Display <> CssConstants.Table AndAlso b.Display <> CssConstants.TableCell AndAlso b.ParentBox IsNot Nothing
                    b = b.ParentBox
                End While

                'Comment this following line to treat always superior box as block
                If b Is Nothing Then
                    Throw New Exception("There's no containing block on the chain")
                End If

                Return b
            End Get
        End Property

        ''' <summary>
        ''' Gets the font's ascent
        ''' </summary>
        Public ReadOnly Property FontAscent() As Single
            Get
                If Single.IsNaN(_fontAscent) Then
                    _fontAscent = CssLayoutEngine.GetAscent(ActualFont)
                End If
                Return _fontAscent
            End Get
        End Property

        ''' <summary>
        ''' Gets the font's line spacing
        ''' </summary>
        Public ReadOnly Property FontLineSpacing() As Single
            Get
                If Single.IsNaN(_fontLineSpacing) Then
                    _fontLineSpacing = CssLayoutEngine.GetLineSpacing(ActualFont)
                End If

                Return _fontLineSpacing
            End Get
        End Property

        ''' <summary>
        ''' Gets the font's descent
        ''' </summary>
        Public ReadOnly Property FontDescent() As Single
            Get
                If Single.IsNaN(_fontDescent) Then
                    _fontDescent = CssLayoutEngine.GetDescent(ActualFont)
                End If
                Return _fontDescent
            End Get
        End Property

        ''' <summary>
        ''' Gets the first word of the box
        ''' </summary>
        Friend ReadOnly Property FirstWord() As CssBoxWord
            Get
                Return Words(0)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the first linebox where content of this box appear
        ''' </summary>
        Friend Property FirstHostingLineBox() As CssLineBox
            Get
                Return _firstHostingLineBox
            End Get
            Set
                _firstHostingLineBox = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the last linebox where content of this box appear
        ''' </summary>
        Friend Property LastHostingLineBox() As CssLineBox
            Get
                Return _lastHostingLineBox
            End Get
            Set
                _lastHostingLineBox = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets the HTMLTag that hosts this box
        ''' </summary>
        Public ReadOnly Property HtmlTag() As HtmlTag
            Get
                Return _htmltag
            End Get
        End Property

        ''' <summary>
        ''' Gets the InitialContainer of the Box.
        ''' WARNING: May be null.
        ''' </summary>
        Public ReadOnly Property InitialContainer() As InitialContainer
            Get
                Return _initialContainer
            End Get
        End Property

        ''' <summary>
        ''' Gets if this box represents an image
        ''' </summary>
        Public ReadOnly Property IsImage() As Boolean
            Get
                Return Words.Count = 1 AndAlso Words(0).IsImage
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating if at least one of the corners of the box is rounded
        ''' </summary>
        Public ReadOnly Property IsRounded() As Boolean
            Get
                Return ActualCornerNE > 0F OrElse ActualCornerNW > 0F OrElse ActualCornerSE > 0F OrElse ActualCornerSW > 0F
            End Get
        End Property

        ''' <summary>
        ''' Tells if the box is empty or contains just blank spaces
        ''' </summary>
        Public ReadOnly Property IsSpaceOrEmpty() As Boolean
            Get


                If (Words.Count = 0 AndAlso Boxes.Count = 0) OrElse (Words.Count = 1 AndAlso Words(0).IsSpaces) OrElse Boxes.Count = 1 AndAlso TypeOf Boxes(0) Is CssAnonymousSpaceBlockBox Then
                    Return True
                End If

                For Each word As CssBoxWord In Words
                    If Not word.IsSpaces Then
                        Return False
                    End If
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' Gets the last word of the box
        ''' </summary>
        Friend ReadOnly Property LastWord() As CssBoxWord
            Get
                Return Words(Words.Count - 1)
            End Get
        End Property

        ''' <summary>
        ''' Gets the line-boxes of this box (if block box)
        ''' </summary>
        Friend ReadOnly Property LineBoxes() As List(Of CssLineBox)
            Get
                Return _lineBoxes
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the location of the box
        ''' </summary>
        Public Property Location() As PointF
            Get
                Return _location
            End Get
            Set
                _location = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the parent box of this box
        ''' </summary>
        Public Property ParentBox() As CssBox
            Get
                Return _parentBox
            End Get
            Set
                'Remove from last parent
                If _parentBox IsNot Nothing AndAlso _parentBox.Boxes.Contains(Me) Then
                    _parentBox.Boxes.Remove(Me)
                End If

                _parentBox = Value

                'Add to new parent
                If Value IsNot Nothing AndAlso Not Value.Boxes.Contains(Me) Then
                    _parentBox.Boxes.Add(Me)
                    _initialContainer = Value.InitialContainer
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the linebox(es) that contains words of this box (if inline)
        ''' </summary>
        Friend ReadOnly Property ParentLineBoxes() As List(Of CssLineBox)
            Get
                Return _parentLineBoxes
            End Get
        End Property

        ''' <summary>
        ''' Gets the rectangles where this box should be painted
        ''' </summary>
        Friend ReadOnly Property Rectangles() As Dictionary(Of CssLineBox, RectangleF)
            Get


                Return _rectangles
            End Get
        End Property

        ''' <summary>
        ''' Gets the right of the box. When setting, it will affect only the width of the box.
        ''' </summary>
        Public Property ActualRight() As Single
            Get
                Return Location.X + Size.Width
            End Get
            Set
                Size = New SizeF(Value - Location.X, Size.Height)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the size of the box
        ''' </summary>
        Public Property Size() As SizeF
            Get
                Return _size
            End Get
            Set
                _size = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the inner text of the box
        ''' </summary>
        Public Property Text() As String
            Get
                Return _text
            End Get
            Set
                _text = Value
                UpdateWords()
            End Set
        End Property

        ''' <summary>
        ''' Gets the BoxWords of text in the box
        ''' </summary>
        Friend ReadOnly Property Words() As List(Of CssBoxWord)
            Get
                Return _boxWords
            End Get
        End Property

#End Region

#Region "Methods"

        ''' <summary>
        ''' Sets the initial container of the box
        ''' </summary>
        ''' <param name="b"></param>
        Private Sub SetInitialContainer(b As InitialContainer)
            _initialContainer = b
        End Sub

        ''' <summary>
        ''' Returns false if some of the boxes
        ''' </summary>
        ''' <returns></returns>
        Friend Function ContainsInlinesOnly() As Boolean
            For Each b As CssBox In Boxes
                If b.Display <> CssConstants.Inline Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' Gets the index of the box to be used on a (ordered) list
        ''' </summary>
        ''' <returns></returns>
        Private Function GetIndexForList() As Integer
            Dim index As Integer = 0

            For Each b As CssBox In ParentBox.Boxes
                If b.Display = CssConstants.ListItem Then
                    index += 1
                End If

                If b.Equals(Me) Then
                    Return index
                End If
            Next

            Return index
        End Function

        ''' <summary>
        ''' Creates the <see cref="ListItemBox"/>
        ''' </summary>
        ''' <param name="g"></param>
        Private Sub CreateListItemBox(g As Graphics)
            If Display = CssConstants.ListItem Then
                If _listItemBox Is Nothing Then
                    _listItemBox = New CssBox()
                    _listItemBox.InheritStyle(Me, False)
                    _listItemBox.Display = CssConstants.Inline
                    _listItemBox.SetInitialContainer(InitialContainer)

                    If ParentBox IsNot Nothing AndAlso ListStyleType = CssConstants.[Decimal] Then
                        _listItemBox.Text = GetIndexForList().ToString() & "."
                    Else
                        _listItemBox.Text = "•"
                    End If

                    _listItemBox.MeasureBounds(g)
                    _listItemBox.Size = New SizeF(_listItemBox.Words(0).Width, _listItemBox.Words(0).Height)
                End If
                _listItemBox.Words(0).Left = Location.X - _listItemBox.Size.Width - 5
                ' +FontAscent;
                _listItemBox.Words(0).Top = Location.Y + ActualPaddingTop
            End If
        End Sub

        ''' <summary>
        ''' Searches for the first word occourence inside the box, on the specified linebox
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Friend Function FirstWordOccourence(b As CssBox, line As CssLineBox) As CssBoxWord
            If b.Words.Count = 0 AndAlso b.Boxes.Count = 0 Then
                Return Nothing
            End If

            If b.Words.Count > 0 Then
                For Each word As CssBoxWord In b.Words
                    If line.Words.Contains(word) Then
                        Return word
                    End If
                Next
                Return Nothing
            Else
                For Each bb As CssBox In b.Boxes
                    Dim w As CssBoxWord = FirstWordOccourence(bb, line)

                    If w IsNot Nothing Then
                        Return w
                    End If
                Next

                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Gets the specified Attribute, returns string.Empty if no attribute specified
        ''' </summary>
        ''' <param name="attribute">Attribute to retrieve</param>
        ''' <returns>Attribute value or string.Empty if no attribute specified</returns>
        Friend Function GetAttribute(attribute As String) As String
            Return GetAttribute(attribute, String.Empty)
        End Function

        ''' <summary>
        ''' Gets the value of the specified attribute of the source HTML tag.
        ''' </summary>
        ''' <param name="attribute">Attribute to retrieve</param>
        ''' <param name="defaultValue">Value to return if attribute is not specified</param>
        ''' <returns>Attribute value or defaultValue if no attribute specified</returns>
        Friend Function GetAttribute(attribute As String, defaultValue As String) As String
            If HtmlTag Is Nothing Then
                Return defaultValue
            End If

            If Not HtmlTag.HasAttribute(attribute) Then
                Return defaultValue
            End If

            Return HtmlTag.Attributes(attribute)
        End Function

        ''' <summary>
        ''' Gets the height of the font in the specified units
        ''' </summary>
        ''' <returns></returns>
        Public Function GetEmHeight() As Single
            'float res = Convert.ToSingle(ActualFont.Height);
            'float res = ActualFont.Size * ActualFont.FontFamily.GetCellAscent(f.Style) / ActualFont.FontFamily.GetEmHeight(f.Style);
            Dim res As Single = ActualFont.GetHeight()
            Return res
        End Function

        ''' <summary>
        ''' Gets the previous sibling of this box.
        ''' </summary>
        ''' <returns>Box before this one on the tree. Null if its the first</returns>
        Private Function GetPreviousSibling(b As CssBox) As CssBox
            If b.ParentBox Is Nothing Then
                'This is initial containing block
                Return Nothing
            End If

            Dim index As Integer = b.ParentBox.Boxes.IndexOf(Me)

            If index < 0 Then
                Throw New Exception("Box doesn't exist on parent's Box list")
            End If

            If index = 0 Then
                Return Nothing
            End If
            'This is the first sibling.

            Dim diff As Integer = 1
            Dim sib As CssBox = b.ParentBox.Boxes(index - diff)

            While (sib.Display = CssConstants.None OrElse sib.Position = CssConstants.Absolute) AndAlso index - diff - 1 >= 0
                sib = b.ParentBox.Boxes(index - Interlocked.Increment(diff))
            End While

            Return If(sib.Display = CssConstants.None, Nothing, sib)
        End Function

        ''' <summary>
        ''' Gets the minimum width that the box can be.
        ''' The box can be as thin as the longest word plus padding.
        ''' The check is deep thru box tree.
        ''' </summary>
        ''' <returns></returns>
        Friend Function GetMinimumWidth() As Single
            Dim maxw As Single = 0F
            Dim padding As Single = 0F
            Dim word As CssBoxWord = Nothing

            GetMinimumWidth_LongestWord(Me, maxw, word)

            If word IsNot Nothing Then
                GetMinimumWidth_BubblePadding(word.OwnerBox, Me, padding)
            End If

            Return maxw + padding
        End Function

        ''' <summary>
        ''' Bubbles up the padding from the starting box
        ''' </summary>
        ''' <param name="box"></param>
        Private Sub GetMinimumWidth_BubblePadding(box As CssBox, endbox As CssBox, ByRef sum As Single)
            'float padding = box.ActualMarginLeft + box.ActualBorderLeftWidth + box.ActualPaddingLeft +
            '    box.ActualMarginRight + box.ActualBorderRightWidth + box.ActualPaddingRight;

            Dim padding As Single = box.ActualBorderLeftWidth + box.ActualPaddingLeft + box.ActualBorderRightWidth + box.ActualPaddingRight

            sum += padding

            If Not box.Equals(endbox) Then
                GetMinimumWidth_BubblePadding(box.ParentBox, endbox, sum)
            End If
        End Sub

        ''' <summary>
        ''' Gets the longest word (in width) inside the box, deeply.
        ''' </summary>
        ''' <param name="b"></param>
        Private Sub GetMinimumWidth_LongestWord(b As CssBox, ByRef maxw As Single, ByRef word As CssBoxWord)

            If b.Words.Count > 0 Then
                For Each w As CssBoxWord In b.Words
                    If w.FullWidth > maxw Then
                        maxw = w.FullWidth
                        word = w
                    End If
                Next
            Else
                For Each bb As CssBox In b.Boxes
                    GetMinimumWidth_LongestWord(bb, maxw, word)
                Next
            End If

        End Sub

        ''' <summary>
        ''' Gets the maximum bottom of the boxes inside the startBox
        ''' </summary>
        ''' <param name="startBox"></param>
        ''' <param name="currentMaxBottom"></param>
        ''' <returns></returns>
        Friend Function GetMaximumBottom(startBox As CssBox, currentMaxBottom As Single) As Single
            For Each line As CssLineBox In startBox.Rectangles.Keys
                currentMaxBottom = sys.Max(currentMaxBottom, startBox.Rectangles(line).Bottom)
            Next

            For Each b As CssBox In startBox.Boxes
                currentMaxBottom = sys.Max(currentMaxBottom, b.ActualBottom)
                currentMaxBottom = sys.Max(currentMaxBottom, GetMaximumBottom(b, currentMaxBottom))
            Next

            Return currentMaxBottom
        End Function

        ''' <summary>
        ''' Get the width of the box at full width (No line breaks)
        ''' </summary>
        ''' <returns></returns>
        Friend Function GetFullWidth(g As Graphics) As Single
            Dim sum As Single = 0F
            Dim paddingsum As Single = 0F
            GetFullWidth_WordsWith(Me, g, sum, paddingsum)

            Return paddingsum + sum
        End Function

        ''' <summary>
        ''' Gets the longest word (in width) inside the box, deeply.
        ''' </summary>
        ''' <param name="b"></param>
        Private Sub GetFullWidth_WordsWith(b As CssBox, g As Graphics, ByRef sum As Single, ByRef paddingsum As Single)
            If b.Display <> CssConstants.Inline Then
                sum = 0
            End If

            paddingsum += b.ActualBorderLeftWidth + b.ActualBorderRightWidth + b.ActualPaddingRight + b.ActualPaddingLeft

            If b.Words.Count > 0 Then
                For Each word As CssBoxWord In b.Words
                    sum += word.FullWidth
                Next
            Else
                For Each bb As CssBox In b.Boxes
                    GetFullWidth_WordsWith(bb, g, sum, paddingsum)
                Next
            End If

        End Sub

        ''' <summary>
        ''' Gets the next sibling of this box.
        ''' </summary>
        ''' <returns>Box after this one on the tree. Null if its the last one.</returns>
        Private Function GetNextSibling() As CssBox
            If ParentBox Is Nothing Then
                'This is initial containing block
                Return Nothing
            End If

            Dim index As Integer = ParentBox.Boxes.IndexOf(Me)

            If index < 0 Then
                Throw New Exception("Box doesn't exist on parent's Box list")
            End If

            If index = ParentBox.Boxes.Count - 1 Then
                Return Nothing
            End If
            'This is the last sibling
            Return ParentBox.Boxes(index + 1)
        End Function

        ''' <summary>
        ''' Gets if this box has only inline siblings (including itself)
        ''' </summary>
        ''' <returns></returns>
        Friend Function HasJustInlineSiblings() As Boolean
            If ParentBox Is Nothing Then
                Return False
            End If

            Return ParentBox.ContainsInlinesOnly()
        End Function

        '''' <summary>
        '''' Gets the rectangles where inline box will be drawn. See Remarks for more info.
        '''' </summary>
        '''' <returns>Rectangles where content should be placed</returns>
        '''' <remarks>
        '''' Inline boxes can be splitted across different LineBoxes, that's why this method
        '''' Delivers a rectangle for each LineBox related to this box, if inline.
        '''' </remarks>

        ''' <summary>
        ''' Inherits inheritable values from parent.
        ''' </summary>
        Friend Sub InheritStyle()
            InheritStyle(ParentBox, False)
        End Sub

        ''' <summary>
        ''' Inherits inheritable values from specified box.
        ''' </summary>
        ''' <param name="everything">Set to true to inherit all CSS properties instead of only the ineritables</param>
        ''' <param name="godfather">Box to inherit the properties</param>
        Friend Sub InheritStyle(godfather As CssBox, everything As Boolean)
            If godfather IsNot Nothing Then
                Dim pps As IEnumerable(Of PropertyInfo) = If(everything, _cssproperties, _inheritables)
                For Each prop As PropertyInfo In pps
                    prop.SetValue(Me, prop.GetValue(godfather, Nothing), Nothing)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Gets the result of collapsing the vertical margins of the two boxes
        ''' </summary>
        ''' <param name="a">Superior box (checks for margin-bottom)</param>
        ''' <param name="b">Inferior box (checks for margin-top)</param>
        ''' <returns>Maximum of margins</returns>
        Private Function MarginCollapse(a As CssBox, b As CssBox) As Single

            Return sys.Max(If(a Is Nothing, 0, a.ActualMarginBottom), If(b Is Nothing, 0, b.ActualMarginTop))
        End Function

        ''' <summary>
        ''' Measures the bounds of box and children, recursively.
        ''' </summary>
        ''' <param name="g">Device context to draw</param>
        Public Overridable Sub MeasureBounds(g As Graphics)
            If Display = CssConstants.None Then
                Return
            End If

            RectanglesReset()

            MeasureWordsSize(g)

            If Display = CssConstants.Block OrElse Display = CssConstants.ListItem OrElse Display = CssConstants.Table OrElse Display = CssConstants.InlineTable OrElse Display = CssConstants.TableCell OrElse Display = CssConstants.None Then
                '#Region "Measure Bounds"
                If Display <> CssConstants.TableCell Then
                    Dim prevSibling As CssBox = GetPreviousSibling(Me)
                    Dim left As Single = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth
                    Dim top As Single = (If(prevSibling Is Nothing AndAlso ParentBox IsNot Nothing, ParentBox.ClientTop, 0)) + MarginCollapse(prevSibling, Me) + (If(prevSibling IsNot Nothing, prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth, 0))
                    Location = New PointF(left, top)
                    ActualBottom = top
                End If

                If Display <> CssConstants.TableCell AndAlso Display <> CssConstants.Table Then
                    'Because their width and height are set by CssTable
                    '#Region "Set Width"
                    'width at 100% (or auto)
                    Dim minwidth As Single = GetMinimumWidth()
                    Dim width__1 As Single = ContainingBlock.Size.Width - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth - ActualMarginLeft - ActualMarginRight - ActualBorderLeftWidth - ActualBorderRightWidth

                    'Check width if not auto
                    If Width <> CssConstants.Auto AndAlso Not String.IsNullOrEmpty(Width) Then
                        width__1 = CssValue.ParseLength(Width, width__1, Me)
                    End If

                    If width__1 < minwidth Then
                        width__1 = minwidth
                    End If


                    '#End Region
                    Size = New SizeF(width__1, Size.Height)
                End If

                'If we're talking about a table here..
                If Display = CssConstants.Table OrElse Display = CssConstants.InlineTable Then
                    Dim tbl As New CssTable(Me, g)
                ElseIf Display <> CssConstants.None Then
                    'If there's just inlines, create LineBoxes
                    If ContainsInlinesOnly() Then
                        ActualBottom = Location.Y
                        'This will automatically set the bottom of this block
                        CssLayoutEngine.CreateLineBoxes(g, Me)
                    Else
                        Dim lastOne As CssBox = Nothing
                        ' Boxes[Boxes.Count - 1];
                        'Treat as BlockBox
                        For Each box As CssBox In Boxes
                            If box.Display = CssConstants.None Then
                                Continue For
                            End If
                            'box.Display = CssConstants.Block; //Force to be block, according to CSS spec
                            box.MeasureBounds(g)
                            lastOne = box
                        Next

                        If lastOne IsNot Nothing Then
                            ActualBottom = sys.Max(ActualBottom, lastOne.ActualBottom + lastOne.ActualMarginBottom + ActualPaddingBottom)
                        End If
                    End If
                    '#End Region
                End If
            End If

            If InitialContainer IsNot Nothing Then
                InitialContainer.MaximumSize = New SizeF(
                    sys.Max(InitialContainer.MaximumSize.Width, ActualRight),
                    sys.Max(InitialContainer.MaximumSize.Height, ActualBottom))
            End If
        End Sub

        ''' <summary>
        ''' Measures the word spacing
        ''' </summary>
        ''' <param name="g"></param>
        Private Sub MeasureWordSpacing(g As Graphics)
            _actualWordSpacing = CssLayoutEngine.WhiteSpace(g, Me)

            If WordSpacing <> CssConstants.Normal Then
                Dim len As String = HTMLParser.Search(HTMLParser.CssLength, WordSpacing)

                _actualWordSpacing += CssValue.ParseLength(len, 1, Me)
            End If
        End Sub

        ''' <summary>
        ''' Assigns words its width and height
        ''' </summary>
        ''' <param name="g"></param>
        Friend Sub MeasureWordsSize(g As Graphics)
            If _wordsSizeMeasured Then
                Return
            End If

            'Measure white space if not yet done
            If Single.IsNaN(_actualWordSpacing) Then
                MeasureWordSpacing(g)
            End If

            If HtmlTag IsNot Nothing AndAlso HtmlTag.TagName.Equals("img", StringComparison.CurrentCultureIgnoreCase) Then
                '#Region "Measure image"

                Dim word As New CssBoxWord(Me, CssValue.GetImage(GetAttribute("src")))
                Words.Clear()

                '#End Region
                Words.Add(word)
            Else
                '#Region "Measure text words"

                Dim lastWasSpace As Boolean = False

                For Each b As CssBoxWord In Words
                    Dim collapse As Boolean = CssBoxWordSplitter.CollapsesWhiteSpaces(Me)
                    If CssBoxWordSplitter.EliminatesLineBreaks(Me) Then
                        b.ReplaceLineBreaksAndTabs()
                    End If

                    If b.IsSpaces Then
                        b.Height = FontLineSpacing

                        If b.IsTab Then
                            'TODO: Configure tab size
                            b.Width = ActualWordSpacing * 4
                        ElseIf b.IsLineBreak Then
                            b.Width = 0
                        Else
                            If Not (lastWasSpace AndAlso collapse) Then
                                b.Width = ActualWordSpacing * (If(collapse, 1, b.Text.Length))
                            End If
                        End If

                        lastWasSpace = True
                    Else
                        Dim word As String = b.Text

                        Dim measurable As CharacterRange() = {New CharacterRange(0, word.Length)}
                        Dim sf As New StringFormat()

                        sf.SetMeasurableCharacterRanges(measurable)

                        Dim regions As Region() = g.MeasureCharacterRanges(word, ActualFont, New RectangleF(0, 0, Single.MaxValue, Single.MaxValue), sf)

                        Dim s As SizeF = regions(0).GetBounds(g).Size
                        Dim p As PointF = regions(0).GetBounds(g).Location

                        b.LastMeasureOffset = New PointF(p.X, p.Y)
                        b.Width = s.Width
                        ' +p.X;
                        b.Height = s.Height
                        ' +p.Y;
                        lastWasSpace = False
                    End If
                    '#End Region
                Next
            End If

            _wordsSizeMeasured = True
        End Sub

        ''' <summary>
        ''' Ensures that the specified length is converted to pixels if necessary
        ''' </summary>
        ''' <param name="length"></param>
        Private Function NoEms(length As String) As String
            Dim len As New CssLength(length)

            If len.Unit = CssLength.CssUnit.Ems Then
                length = len.ConvertEmToPixels(GetEmHeight()).ToString()
            End If

            Return length
        End Function

        ''' <summary>
        ''' Deeply offsets the top of the box and its contents
        ''' </summary>
        ''' <param name="amount"></param>
        Friend Sub OffsetTop(amount As Single)
            Dim lines As New List(Of CssLineBox)()
            For Each line As CssLineBox In Rectangles.Keys
                lines.Add(line)
            Next

            For Each line As CssLineBox In lines
                Dim r As RectangleF = Rectangles(line)
                Rectangles(line) = New RectangleF(r.X, r.Y + amount, r.Width, r.Height)
            Next

            For Each word As CssBoxWord In Words
                word.Top += amount
            Next

            For Each b As CssBox In Boxes
                b.OffsetTop(amount)
            Next
            'TODO: Aquí me quede: no se mueve bien todo (probar con las tablas rojas)
            Location = New PointF(Location.X, Location.Y + amount)
        End Sub

        ''' <summary>
        ''' Paints the fragment
        ''' </summary>
        ''' <param name="g"></param>
        Public Sub Paint(g As Graphics)
            If Display = CssConstants.None Then
                Return
            End If

            If Display = CssConstants.TableCell AndAlso EmptyCells = CssConstants.Hide AndAlso IsSpaceOrEmpty Then
                Return
            End If

            Dim areas As List(Of RectangleF) = If(
                Rectangles.Count = 0,
                New List(Of RectangleF)(New RectangleF() {Bounds}),
                New List(Of RectangleF)(Rectangles.Values))

            Dim rects As RectangleF() = areas.ToArray()
            Dim offset As PointF = If(InitialContainer IsNot Nothing, InitialContainer.ScrollOffset, PointF.Empty)

            For i As Integer = 0 To rects.Length - 1
                Dim actualRect As RectangleF = rects(i)
                actualRect.Offset(offset)

                If InitialContainer IsNot Nothing AndAlso HtmlTag IsNot Nothing AndAlso HtmlTag.TagName.Equals("a", StringComparison.CurrentCultureIgnoreCase) Then
                    If InitialContainer.LinkRegions.ContainsKey(Me) Then
                        InitialContainer.LinkRegions.Remove(Me)
                    End If

                    InitialContainer.LinkRegions.Add(Me, actualRect)
                End If

                PaintBackground(g, actualRect)
                PaintBorder(g, actualRect, i = 0, i = rects.Length - 1)
            Next

            If IsImage Then
                Dim r As RectangleF = Words(0).Bounds
                r.Offset(offset)
                r.Height -= ActualBorderTopWidth + ActualBorderBottomWidth + ActualPaddingTop + ActualPaddingBottom
                r.Y += ActualBorderTopWidth + ActualPaddingTop
                'HACK: round rectangle only when necessary
                g.DrawImage(Words(0).Image, rect.Round(r))
            Else
                Dim f As Font = ActualFont
                Using b As New SolidBrush(CssValue.GetActualColor(Color))
                    For Each word As CssBoxWord In Words
                        g.DrawString(word.Text, f, b, word.Left - word.LastMeasureOffset.X + offset.X, word.Top + offset.Y)
                    Next

                End Using
            End If
            For i As Integer = 0 To rects.Length - 1
                Dim actualRect As RectangleF = rects(i)
                actualRect.Offset(offset)

                PaintDecoration(g, actualRect, i = 0, i = rects.Length - 1)
            Next

            For Each b As CssBox In Boxes
                b.Paint(g)
            Next

            CreateListItemBox(g)

            If ListItemBox IsNot Nothing Then
                ListItemBox.Paint(g)
            End If
        End Sub

        ''' <summary>
        ''' Paints the border of the box
        ''' </summary>
        ''' <param name="g"></param>
        Private Sub PaintBorder(g As Graphics, rectangle As RectangleF, isFirst As Boolean, isLast As Boolean)

            Dim smooth As SmoothingMode = g.SmoothingMode

            If InitialContainer IsNot Nothing AndAlso Not InitialContainer.AvoidGeometryAntialias AndAlso IsRounded Then
                g.SmoothingMode = SmoothingMode.AntiAlias
            End If

            'Top border
            If Not (String.IsNullOrEmpty(BorderTopStyle) OrElse BorderTopStyle = CssConstants.None) Then
                Using b As New SolidBrush(ActualBorderTopColor)
                    If BorderTopStyle = CssConstants.Inset Then
                        b.Color = ActualBorderTopColor.Darken
                    End If
                    g.FillPath(b, CssDrawingHelper.GetBorderPath(CssDrawingHelper.Border.Top, Me, rectangle, isFirst, isLast))
                End Using
            End If


            If isLast Then
                'Right Border
                If Not (String.IsNullOrEmpty(BorderRightStyle) OrElse BorderRightStyle = CssConstants.None) Then
                    Using b As New SolidBrush(ActualBorderRightColor)
                        If BorderRightStyle = CssConstants.Outset Then
                            b.Color = ActualBorderRightColor.Darken
                        End If
                        g.FillPath(b, CssDrawingHelper.GetBorderPath(CssDrawingHelper.Border.Right, Me, rectangle, isFirst, isLast))
                    End Using
                End If
            End If

            'Bottom border
            If Not (String.IsNullOrEmpty(BorderBottomStyle) OrElse BorderBottomStyle = CssConstants.None) Then
                Using b As New SolidBrush(ActualBorderBottomColor)
                    If BorderBottomStyle = CssConstants.Outset Then
                        b.Color = ActualBorderBottomColor.Darken
                    End If
                    g.FillPath(b, CssDrawingHelper.GetBorderPath(CssDrawingHelper.Border.Bottom, Me, rectangle, isFirst, isLast))
                End Using
            End If

            If isFirst Then
                'Left Border
                If Not (String.IsNullOrEmpty(BorderLeftStyle) OrElse BorderLeftStyle = CssConstants.None) Then
                    Using b As New SolidBrush(ActualBorderLeftColor)
                        If BorderLeftStyle = CssConstants.Inset Then
                            b.Color = ActualBorderLeftColor.Darken
                        End If
                        g.FillPath(b, CssDrawingHelper.GetBorderPath(CssDrawingHelper.Border.Left, Me, rectangle, isFirst, isLast))
                    End Using
                End If
            End If

            g.SmoothingMode = smooth

        End Sub

        ''' <summary>
        ''' Paints the background of the box
        ''' </summary>
        ''' <param name="g"></param>
        Private Sub PaintBackground(g As Graphics, rectangle As RectangleF)
            'HACK: Background rectangles are being deactivated when justifying text.
            If ContainingBlock.TextAlign = CssConstants.Justify Then
                Return
            End If

            Dim roundrect As GraphicsPath = Nothing
            Dim b As Brush = Nothing
            Dim smooth As SmoothingMode = g.SmoothingMode

            If IsRounded Then
                roundrect = CssDrawingHelper.GetRoundRect(rectangle, ActualCornerNW, ActualCornerNE, ActualCornerSE, ActualCornerSW)
            End If

            If BackgroundGradient <> CssConstants.None AndAlso rectangle.Width > 0 AndAlso rectangle.Height > 0 Then
                b = New LinearGradientBrush(rectangle, ActualBackgroundColor, ActualBackgroundGradient, ActualBackgroundGradientAngle)
            Else
                b = New SolidBrush(ActualBackgroundColor)
            End If

            If InitialContainer IsNot Nothing AndAlso Not InitialContainer.AvoidGeometryAntialias AndAlso IsRounded Then
                g.SmoothingMode = SmoothingMode.AntiAlias
            End If

            If roundrect IsNot Nothing Then
                g.FillPath(b, roundrect)
            Else
                g.FillRectangle(b, rectangle)
            End If

            g.SmoothingMode = smooth

            If roundrect IsNot Nothing Then
                roundrect.Dispose()
            End If
            If b IsNot Nothing Then
                b.Dispose()
            End If
        End Sub

        ''' <summary>
        ''' Paints the text decoration
        ''' </summary>
        ''' <param name="g"></param>
        Private Sub PaintDecoration(g As Graphics, rectangle As RectangleF, isFirst As Boolean, isLast As Boolean)
            If String.IsNullOrEmpty(TextDecoration) OrElse TextDecoration = CssConstants.None OrElse IsImage Then
                Return
            End If

            Dim desc As Single = CssLayoutEngine.GetDescent(ActualFont)
            Dim asc As Single = CssLayoutEngine.GetAscent(ActualFont)
            Dim y As Single = 0F

            If TextDecoration = CssConstants.Underline Then
                y = rectangle.Bottom - desc
            ElseIf TextDecoration = CssConstants.LineThrough Then
                y = rectangle.Bottom - desc - asc / 2
            ElseIf TextDecoration = CssConstants.Overline Then
                y = rectangle.Bottom - desc - asc - 2
            End If

            y -= ActualPaddingBottom - ActualBorderBottomWidth

            Dim x1 As Single = rectangle.X
            Dim x2 As Single = rectangle.Right

            If isFirst Then
                x1 += ActualPaddingLeft + ActualBorderLeftWidth
            End If
            If isLast Then
                x2 -= ActualPaddingRight + ActualBorderRightWidth
            End If

            g.DrawLine(New Pen(ActualColor), x1, y, x2, y)
        End Sub

        ''' <summary>
        ''' Offsets the rectangle of the specified linebox by the specified gap,
        ''' and goes deep for rectangles of children in that linebox.
        ''' </summary>
        ''' <param name="lineBox"></param>
        ''' <param name="gap"></param>
        Friend Sub OffsetRectangle(lineBox As CssLineBox, gap As Single)
            If Rectangles.ContainsKey(lineBox) Then
                Dim r As RectangleF = Rectangles(lineBox)
                Rectangles(lineBox) = New RectangleF(r.X, r.Y + gap, r.Width, r.Height)
            End If

            'foreach (Box b in Boxes)
            '{
            '    b.OffsetRectangle(lineBox, gap);
            '}
        End Sub

        ''' <summary>
        ''' Resets the <see cref="Rectangles"/> array
        ''' </summary>
        Friend Sub RectanglesReset()
            _rectangles.Clear()
        End Sub

        ''' <summary>
        ''' Removes boxes that are just blank spaces
        ''' </summary>
        Friend Sub RemoveAnonymousSpaces()
            For i As Integer = 0 To Boxes.Count - 1
                If TypeOf Boxes(i) Is CssAnonymousSpaceBlockBox OrElse TypeOf Boxes(i) Is CssAnonymousSpaceBox Then
                    Boxes.RemoveAt(i)
                    i -= 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' Sets the bounds of the box
        ''' </summary>
        ''' <param name="r"></param>
        Public Sub SetBounds(r As Rectangle)
            SetBounds(New RectangleF(r.X, r.Y, r.Width, r.Height))
        End Sub

        ''' <summary>
        ''' Sets the bounds of the box
        ''' </summary>
        ''' <param name="rectangle"></param>
        Public Sub SetBounds(rectangle As RectangleF)
            Size = rectangle.Size
            Location = rectangle.Location
        End Sub

        ''' <summary>
        ''' ToString override.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim t As String = [GetType]().Name
            If HtmlTag IsNot Nothing Then
                t = String.Format("<{0}>", HtmlTag.TagName)
            End If

            If ParentBox Is Nothing Then
                Return "Initial Container"
            ElseIf Display = CssConstants.Block Then
                Return String.Format("{0} BlockBox {2}, Children:{1}", t, Boxes.Count, FontSize)
            ElseIf Display = CssConstants.None Then
                Return String.Format("{0} None", t)
            Else
                Return String.Format("{0} {2}: {1}", t, Text, Display)
            End If


            'return base.ToString();
        End Function

        ''' <summary>
        ''' Splits the text into words and saves the result
        ''' </summary>
        Private Sub UpdateWords()

            Words.Clear()

            Dim splitter As New CssBoxWordSplitter(Me, Text)
            splitter.SplitWords()

            Words.AddRange(splitter.Words)
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
#End Region
    End Class
End Namespace
