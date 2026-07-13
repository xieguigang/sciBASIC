#Region "Microsoft.VisualBasic::5a75ae4f9fb637ab48e4517c8a450819, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Pen.vb"

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

    '   Total Lines: 496
    '    Code Lines: 329 (66.33%)
    ' Comment Lines: 106 (21.37%)
    '    - Xml Docs: 59.43%
    ' 
    '   Blank Lines: 61 (12.30%)
    '     File Size: 24.96 KB


    '     Class Pen
    ' 
    '         Properties: Alignment, Brush, Color, CustomEndCap, DashCap
    '                     DashOffset, DashStyle, EndCap, LineJoin, MiterLimit
    '                     StartCap, Width
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Enum DashCap
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum PenAlignment
    ' 
    '         Center, Inset, Left, Outset, Right
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum DashStyle
    ' 
    '         Custom, Dash, DashDot, DashDotDot, Dot
    '         Solid
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum LineJoin
    ' 
    '         Bevel, Miter, MiterClipped, Round
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum LineCap
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CustomLineCap
    ' 
    '         Properties: BaseCap, BaseInset, FillPath, HeightScale, StrokeJoin
    '                     StrokePath, WidthScale
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Clone
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class AdjustableArrowCap
    ' 
    '         Properties: Filled, Height, MiddleInset, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Pens
    ' 
    '         Properties: AliceBlue, AntiqueWhite, Aqua, Aquamarine, Azure
    '                     Beige, Bisque, Black, BlanchedAlmond, Blue
    '                     BlueViolet, Brown, BurlyWood, CadetBlue, Chartreuse
    '                     Chocolate, Coral, CornflowerBlue, Cornsilk, Crimson
    '                     Cyan, DarkBlue, DarkCyan, DarkGoldenrod, DarkGray
    '                     DarkGreen, DarkKhaki, DarkMagenta, DarkOliveGreen, DarkOrange
    '                     DarkOrchid, DarkRed, DarkSalmon, DarkSeaGreen, DarkSlateBlue
    '                     DarkSlateGray, DarkTurquoise, DarkViolet, DeepPink, DeepSkyBlue
    '                     DimGray, DodgerBlue, Firebrick, FloralWhite, ForestGreen
    '                     Fuchsia, Gainsboro, GhostWhite, Gold, Goldenrod
    '                     Gray, Green, GreenYellow, Honeydew, HotPink
    '                     IndianRed, Indigo, Ivory, Khaki, Lavender
    '                     LavenderBlush, LawnGreen, LemonChiffon, LightBlue, LightCoral
    '                     LightCyan, LightGoldenrodYellow, LightGray, LightGreen, LightPink
    '                     LightSalmon, LightSeaGreen, LightSkyBlue, LightSlateGray, LightSteelBlue
    '                     LightYellow, Lime, LimeGreen, Linen, Magenta
    '                     Maroon, MediumAquamarine, MediumBlue, MediumOrchid, MediumPurple
    '                     MediumSeaGreen, MediumSlateBlue, MediumSpringGreen, MediumTurquoise, MediumVioletRed
    '                     MidnightBlue, MintCream, MistyRose, Moccasin, NavajoWhite
    '                     Navy, OldLace, Olive, OliveDrab, Orange
    '                     OrangeRed, Orchid, PaleGoldenrod, PaleGreen, PaleTurquoise
    '                     PaleVioletRed, PapayaWhip, PeachPuff, Peru, Pink
    '                     Plum, PowderBlue, Purple, Red, RosyBrown
    '                     RoyalBlue, SaddleBrown, Salmon, SandyBrown, SeaGreen
    '                     SeaShell, Sienna, Silver, SkyBlue, SlateBlue
    '                     SlateGray, Snow, SpringGreen, SteelBlue, Tan
    '                     Teal, Thistle, Tomato, Transparent, Turquoise
    '                     Violet, Wheat, White, WhiteSmoke, Yellow
    '                     YellowGreen
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class SystemPens
    ' 
    '         Properties: ActiveBorder, ActiveCaption, ActiveCaptionText, AppWorkspace, ButtonFace
    '                     ButtonHighlight, ButtonShadow, Control, ControlDark, ControlDarkDark
    '                     ControlLight, ControlLightLight, ControlText, Desktop, GradientActiveCaption
    '                     GradientInactiveCaption, GrayText, Highlight, HighlightText, HotTrack
    '                     InactiveBorder, InactiveCaption, InactiveCaptionText, Info, InfoText
    '                     Menu, MenuBar, MenuHighlight, MenuText, ScrollBar
    '                     Window, WindowFrame, WindowText
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    ''' <summary>
    ''' The stroke pen wrapper for .net 8.0
    ''' </summary>
    Public Class Pen : Implements IDisposable

        Private disposedValue As Boolean

        Public Property Color As Color
        Public Property Width As Single = 1
        Public Property DashStyle As DashStyle
        Public Property Brush As Brush
        Public Property LineJoin As LineJoin
        Public Property CustomEndCap As CustomLineCap
        Public Property EndCap As LineCap
        Public Property Alignment As PenAlignment
        Public Property StartCap As LineCap
        Public Property MiterLimit As Single
        Public Property DashCap As DashCap
        Public Property DashOffset As Single

        Sub New(color As Color, Optional width As Single = 1)
            _Color = color
            _Width = width
            _Brush = New SolidBrush(color)
        End Sub

        Sub New(brush As SolidBrush, Optional width As Single = 1)
            _Brush = brush
            _Color = brush.Color
            _Width = width
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Width}px {DashStyle.ToString.ToLower} ({Color.ToHtmlColor})"
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    '     Specifies the type of graphic shape to use on both ends of each dash in a dashed
    '     line.
    Public Enum DashCap

        '     Specifies a square cap that squares off both ends of each dash.
        Flat = 0
        '     Specifies a circular cap that rounds off both ends of each dash.
        Round = 2
        '     Specifies a triangular cap that points both ends of each dash.
        Triangle = 3
    End Enum

    '     Specifies the alignment of a System.Drawing.Pen object in relation to the theoretical,
    '     zero-width line.
    Public Enum PenAlignment

        '
        '     Specifies that the System.Drawing.Pen object Is centered over the theoretical
        '     line.
        Center

        '     Specifies that the System.Drawing.Pen Is positioned on the inside of the theoretical
        '     line.
        Inset
        '   Specifies the System.Drawing.Pen Is positioned On the outside Of the theoretical
        '     line.
        Outset
        '     Specifies the System.Drawing.Pen Is positioned to the left of the theoretical
        '     line.
        Left

        '     Specifies the System.Drawing.Pen Is positioned to the right of the theoretical
        '     line.
        Right
    End Enum

    Public Enum DashStyle
        Solid
        Dash
        Dot
        DashDot
        DashDotDot
        Custom
    End Enum

    ''' <summary>
    ''' Specifies how to join consecutive line Or curve segments in a figure (subpath)
    ''' contained in a System.Drawing.Drawing2D.GraphicsPath object.
    ''' </summary>
    Public Enum LineJoin
        ''' <summary>
        ''' Specifies a mitered join. This produces a sharp corner Or a clipped corner, depending
        ''' on whether the length of the miter exceeds the miter limit.
        ''' </summary>
        Miter
        ''' <summary>
        ''' Specifies a beveled join. This produces a diagonal corner.    
        ''' </summary>
        Bevel
        ''' <summary>
        ''' Specifies a circular join. This produces a smooth, circular arc between the lines.    
        ''' </summary>
        Round
        ''' <summary>
        ''' Specifies a mitered join. This produces a sharp corner Or a beveled corner, depending
        ''' on whether the length of the miter exceeds the miter limit.
        ''' </summary>
        MiterClipped
    End Enum

    '     Specifies the available cap styles with which a System.Drawing.Pen object can
    '     end a line.
    Public Enum LineCap

        '     Specifies a flat line cap.
        Flat = 0

        '     Specifies a square line cap.
        Square = 1

        ' Specifies a round line cap.
        Round = 2

        '     Specifies a triangular line cap.
        Triangle = 3

        '     Specifies no anchor.
        NoAnchor = 16

        '     Specifies a square anchor line cap.
        SquareAnchor = 17

        '     Specifies a round anchor cap.
        RoundAnchor = 18

        '     Specifies a diamond anchor cap.
        DiamondAnchor = 19

        '     Specifies an arrow-shaped anchor cap.
        ArrowAnchor = 20

        '     Specifies a custom line cap.
        Custom = 255

        '     Specifies a mask used to check whether a line cap Is an anchor cap.
        AnchorMask = 240
    End Enum


    ''' <summary>
    ''' Encapsulates a custom user-defined line cap.
    ''' </summary>
    Public Class CustomLineCap : Implements IDisposable

        Private disposedValue As Boolean

        ''' <summary>
        ''' Gets or sets the scale factor for the width of the line cap.
        ''' </summary>
        Public Property WidthScale As Single = 1.0F

        ''' <summary>
        ''' Gets or sets the scale factor for the height of the line cap.
        ''' </summary>
        Public Property HeightScale As Single = 1.0F

        ''' <summary>
        ''' Gets or sets the distance between the cap and the line.
        ''' </summary>
        Public Property BaseInset As Single

        ''' <summary>
        ''' Gets or sets the line cap used at the base of the cap.
        ''' </summary>
        Public Property BaseCap As LineCap = LineCap.Flat

        ''' <summary>
        ''' Gets or sets the line cap used at the stroke inset from the end of a line.
        ''' </summary>
        Public Property StrokeJoin As LineJoin = LineJoin.Miter

        Sub New()
        End Sub

        Sub New(fillPath As GraphicsPath, strokePath As GraphicsPath, Optional baseCap As LineCap = LineCap.Flat, Optional baseInset As Single = 0)
            _FillPath = fillPath
            _StrokePath = strokePath
            _BaseCap = baseCap
            _BaseInset = baseInset
        End Sub

        Private _FillPath As GraphicsPath
        Private _StrokePath As GraphicsPath

        ''' <summary>
        ''' Gets or sets the path used to fill the interior of the cap.
        ''' </summary>
        Public Property FillPath As GraphicsPath
            Get
                Return _FillPath
            End Get
            Set(value As GraphicsPath)
                _FillPath = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the path used to draw the outline of the cap.
        ''' </summary>
        Public Property StrokePath As GraphicsPath
            Get
                Return _StrokePath
            End Get
            Set(value As GraphicsPath)
                _StrokePath = value
            End Set
        End Property

        Public Function Clone() As CustomLineCap
            Dim cap As New CustomLineCap(_FillPath, _StrokePath, _BaseCap, _BaseInset) With {
                .WidthScale = WidthScale,
                .HeightScale = HeightScale
            }
            Return cap
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If
                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                disposedValue = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    ''' <summary>
    ''' Represents an adjustable arrow-shaped line cap.
    ''' </summary>
    Public Class AdjustableArrowCap : Inherits CustomLineCap

        ''' <summary>
        ''' Gets or sets the width of the arrow cap.
        ''' </summary>
        Public Property Width As Single

        ''' <summary>
        ''' Gets or sets the height of the arrow cap.
        ''' </summary>
        Public Property Height As Single

        ''' <summary>
        ''' Gets or sets the inset of the arrow cap middle point.
        ''' </summary>
        Public Property MiddleInset As Single

        ''' <summary>
        ''' Gets or sets whether the arrow cap is filled.
        ''' </summary>
        Public Property Filled As Boolean

        Sub New(width As Single, height As Single, Optional isFilled As Boolean = True)
            MyBase.New()
            _Width = width
            _Height = height
            _Filled = isFilled
        End Sub
    End Class

    Public NotInheritable Class Pens

        Public Shared ReadOnly Property AliceBlue As New Pen(Color.AliceBlue)
        Public Shared ReadOnly Property AntiqueWhite As New Pen(Color.AntiqueWhite)
        Public Shared ReadOnly Property Aqua As New Pen(Color.Aqua)
        Public Shared ReadOnly Property Aquamarine As New Pen(Color.Aquamarine)
        Public Shared ReadOnly Property Azure As New Pen(Color.Azure)
        Public Shared ReadOnly Property Beige As New Pen(Color.Beige)
        Public Shared ReadOnly Property Bisque As New Pen(Color.Bisque)
        Public Shared ReadOnly Property Black As New Pen(Color.Black)
        Public Shared ReadOnly Property BlanchedAlmond As New Pen(Color.BlanchedAlmond)
        Public Shared ReadOnly Property Blue As New Pen(Color.Blue)
        Public Shared ReadOnly Property BlueViolet As New Pen(Color.BlueViolet)
        Public Shared ReadOnly Property Brown As New Pen(Color.Brown)
        Public Shared ReadOnly Property BurlyWood As New Pen(Color.BurlyWood)
        Public Shared ReadOnly Property CadetBlue As New Pen(Color.CadetBlue)
        Public Shared ReadOnly Property Chartreuse As New Pen(Color.Chartreuse)
        Public Shared ReadOnly Property Chocolate As New Pen(Color.Chocolate)
        Public Shared ReadOnly Property Coral As New Pen(Color.Coral)
        Public Shared ReadOnly Property CornflowerBlue As New Pen(Color.CornflowerBlue)
        Public Shared ReadOnly Property Cornsilk As New Pen(Color.Cornsilk)
        Public Shared ReadOnly Property Crimson As New Pen(Color.Crimson)
        Public Shared ReadOnly Property Cyan As New Pen(Color.Cyan)
        Public Shared ReadOnly Property DarkBlue As New Pen(Color.DarkBlue)
        Public Shared ReadOnly Property DarkCyan As New Pen(Color.DarkCyan)
        Public Shared ReadOnly Property DarkGoldenrod As New Pen(Color.DarkGoldenrod)
        Public Shared ReadOnly Property DarkGray As New Pen(Color.DarkGray)
        Public Shared ReadOnly Property DarkGreen As New Pen(Color.DarkGreen)
        Public Shared ReadOnly Property DarkKhaki As New Pen(Color.DarkKhaki)
        Public Shared ReadOnly Property DarkMagenta As New Pen(Color.DarkMagenta)
        Public Shared ReadOnly Property DarkOliveGreen As New Pen(Color.DarkOliveGreen)
        Public Shared ReadOnly Property DarkOrange As New Pen(Color.DarkOrange)
        Public Shared ReadOnly Property DarkOrchid As New Pen(Color.DarkOrchid)
        Public Shared ReadOnly Property DarkRed As New Pen(Color.DarkRed)
        Public Shared ReadOnly Property DarkSalmon As New Pen(Color.DarkSalmon)
        Public Shared ReadOnly Property DarkSeaGreen As New Pen(Color.DarkSeaGreen)
        Public Shared ReadOnly Property DarkSlateBlue As New Pen(Color.DarkSlateBlue)
        Public Shared ReadOnly Property DarkSlateGray As New Pen(Color.DarkSlateGray)
        Public Shared ReadOnly Property DarkTurquoise As New Pen(Color.DarkTurquoise)
        Public Shared ReadOnly Property DarkViolet As New Pen(Color.DarkViolet)
        Public Shared ReadOnly Property DeepPink As New Pen(Color.DeepPink)
        Public Shared ReadOnly Property DeepSkyBlue As New Pen(Color.DeepSkyBlue)
        Public Shared ReadOnly Property DimGray As New Pen(Color.DimGray)
        Public Shared ReadOnly Property DodgerBlue As New Pen(Color.DodgerBlue)
        Public Shared ReadOnly Property Firebrick As New Pen(Color.Firebrick)
        Public Shared ReadOnly Property FloralWhite As New Pen(Color.FloralWhite)
        Public Shared ReadOnly Property ForestGreen As New Pen(Color.ForestGreen)
        Public Shared ReadOnly Property Fuchsia As New Pen(Color.Fuchsia)
        Public Shared ReadOnly Property Gainsboro As New Pen(Color.Gainsboro)
        Public Shared ReadOnly Property GhostWhite As New Pen(Color.GhostWhite)
        Public Shared ReadOnly Property Gold As New Pen(Color.Gold)
        Public Shared ReadOnly Property Goldenrod As New Pen(Color.Goldenrod)
        Public Shared ReadOnly Property Gray As New Pen(Color.Gray)
        Public Shared ReadOnly Property Green As New Pen(Color.Green)
        Public Shared ReadOnly Property GreenYellow As New Pen(Color.GreenYellow)
        Public Shared ReadOnly Property Honeydew As New Pen(Color.Honeydew)
        Public Shared ReadOnly Property HotPink As New Pen(Color.HotPink)
        Public Shared ReadOnly Property IndianRed As New Pen(Color.IndianRed)
        Public Shared ReadOnly Property Indigo As New Pen(Color.Indigo)
        Public Shared ReadOnly Property Ivory As New Pen(Color.Ivory)
        Public Shared ReadOnly Property Khaki As New Pen(Color.Khaki)
        Public Shared ReadOnly Property Lavender As New Pen(Color.Lavender)
        Public Shared ReadOnly Property LavenderBlush As New Pen(Color.LavenderBlush)
        Public Shared ReadOnly Property LawnGreen As New Pen(Color.LawnGreen)
        Public Shared ReadOnly Property LemonChiffon As New Pen(Color.LemonChiffon)
        Public Shared ReadOnly Property LightBlue As New Pen(Color.LightBlue)
        Public Shared ReadOnly Property LightCoral As New Pen(Color.LightCoral)
        Public Shared ReadOnly Property LightCyan As New Pen(Color.LightCyan)
        Public Shared ReadOnly Property LightGoldenrodYellow As New Pen(Color.LightGoldenrodYellow)
        Public Shared ReadOnly Property LightGray As New Pen(Color.LightGray)
        Public Shared ReadOnly Property LightGreen As New Pen(Color.LightGreen)
        Public Shared ReadOnly Property LightPink As New Pen(Color.LightPink)
        Public Shared ReadOnly Property LightSalmon As New Pen(Color.LightSalmon)
        Public Shared ReadOnly Property LightSeaGreen As New Pen(Color.LightSeaGreen)
        Public Shared ReadOnly Property LightSkyBlue As New Pen(Color.LightSkyBlue)
        Public Shared ReadOnly Property LightSlateGray As New Pen(Color.LightSlateGray)
        Public Shared ReadOnly Property LightSteelBlue As New Pen(Color.LightSteelBlue)
        Public Shared ReadOnly Property LightYellow As New Pen(Color.LightYellow)
        Public Shared ReadOnly Property Lime As New Pen(Color.Lime)
        Public Shared ReadOnly Property LimeGreen As New Pen(Color.LimeGreen)
        Public Shared ReadOnly Property Linen As New Pen(Color.Linen)
        Public Shared ReadOnly Property Magenta As New Pen(Color.Magenta)
        Public Shared ReadOnly Property Maroon As New Pen(Color.Maroon)
        Public Shared ReadOnly Property MediumAquamarine As New Pen(Color.MediumAquamarine)
        Public Shared ReadOnly Property MediumBlue As New Pen(Color.MediumBlue)
        Public Shared ReadOnly Property MediumOrchid As New Pen(Color.MediumOrchid)
        Public Shared ReadOnly Property MediumPurple As New Pen(Color.MediumPurple)
        Public Shared ReadOnly Property MediumSeaGreen As New Pen(Color.MediumSeaGreen)
        Public Shared ReadOnly Property MediumSlateBlue As New Pen(Color.MediumSlateBlue)
        Public Shared ReadOnly Property MediumSpringGreen As New Pen(Color.MediumSpringGreen)
        Public Shared ReadOnly Property MediumTurquoise As New Pen(Color.MediumTurquoise)
        Public Shared ReadOnly Property MediumVioletRed As New Pen(Color.MediumVioletRed)
        Public Shared ReadOnly Property MidnightBlue As New Pen(Color.MidnightBlue)
        Public Shared ReadOnly Property MintCream As New Pen(Color.MintCream)
        Public Shared ReadOnly Property MistyRose As New Pen(Color.MistyRose)
        Public Shared ReadOnly Property Moccasin As New Pen(Color.Moccasin)
        Public Shared ReadOnly Property NavajoWhite As New Pen(Color.NavajoWhite)
        Public Shared ReadOnly Property Navy As New Pen(Color.Navy)
        Public Shared ReadOnly Property OldLace As New Pen(Color.OldLace)
        Public Shared ReadOnly Property Olive As New Pen(Color.Olive)
        Public Shared ReadOnly Property OliveDrab As New Pen(Color.OliveDrab)
        Public Shared ReadOnly Property Orange As New Pen(Color.Orange)
        Public Shared ReadOnly Property OrangeRed As New Pen(Color.OrangeRed)
        Public Shared ReadOnly Property Orchid As New Pen(Color.Orchid)
        Public Shared ReadOnly Property PaleGoldenrod As New Pen(Color.PaleGoldenrod)
        Public Shared ReadOnly Property PaleGreen As New Pen(Color.PaleGreen)
        Public Shared ReadOnly Property PaleTurquoise As New Pen(Color.PaleTurquoise)
        Public Shared ReadOnly Property PaleVioletRed As New Pen(Color.PaleVioletRed)
        Public Shared ReadOnly Property PapayaWhip As New Pen(Color.PapayaWhip)
        Public Shared ReadOnly Property PeachPuff As New Pen(Color.PeachPuff)
        Public Shared ReadOnly Property Peru As New Pen(Color.Peru)
        Public Shared ReadOnly Property Pink As New Pen(Color.Pink)
        Public Shared ReadOnly Property Plum As New Pen(Color.Plum)
        Public Shared ReadOnly Property PowderBlue As New Pen(Color.PowderBlue)
        Public Shared ReadOnly Property Purple As New Pen(Color.Purple)
        Public Shared ReadOnly Property Red As New Pen(Color.Red)
        Public Shared ReadOnly Property RosyBrown As New Pen(Color.RosyBrown)
        Public Shared ReadOnly Property RoyalBlue As New Pen(Color.RoyalBlue)
        Public Shared ReadOnly Property SaddleBrown As New Pen(Color.SaddleBrown)
        Public Shared ReadOnly Property Salmon As New Pen(Color.Salmon)
        Public Shared ReadOnly Property SandyBrown As New Pen(Color.SandyBrown)
        Public Shared ReadOnly Property SeaGreen As New Pen(Color.SeaGreen)
        Public Shared ReadOnly Property SeaShell As New Pen(Color.SeaShell)
        Public Shared ReadOnly Property Sienna As New Pen(Color.Sienna)
        Public Shared ReadOnly Property Silver As New Pen(Color.Silver)
        Public Shared ReadOnly Property SkyBlue As New Pen(Color.SkyBlue)
        Public Shared ReadOnly Property SlateBlue As New Pen(Color.SlateBlue)
        Public Shared ReadOnly Property SlateGray As New Pen(Color.SlateGray)
        Public Shared ReadOnly Property Snow As New Pen(Color.Snow)
        Public Shared ReadOnly Property SpringGreen As New Pen(Color.SpringGreen)
        Public Shared ReadOnly Property SteelBlue As New Pen(Color.SteelBlue)
        Public Shared ReadOnly Property Tan As New Pen(Color.Tan)
        Public Shared ReadOnly Property Teal As New Pen(Color.Teal)
        Public Shared ReadOnly Property Thistle As New Pen(Color.Thistle)
        Public Shared ReadOnly Property Tomato As New Pen(Color.Tomato)
        Public Shared ReadOnly Property Transparent As New Pen(Color.Transparent)
        Public Shared ReadOnly Property Turquoise As New Pen(Color.Turquoise)
        Public Shared ReadOnly Property Violet As New Pen(Color.Violet)
        Public Shared ReadOnly Property Wheat As New Pen(Color.Wheat)
        Public Shared ReadOnly Property White As New Pen(Color.White)
        Public Shared ReadOnly Property WhiteSmoke As New Pen(Color.WhiteSmoke)
        Public Shared ReadOnly Property Yellow As New Pen(Color.Yellow)
        Public Shared ReadOnly Property YellowGreen As New Pen(Color.YellowGreen)

        Private Sub New()
        End Sub

    End Class

#If Not NETSTANDARD Then

    ''' <summary>
    ''' Each property of the SystemPens class is a Pen that is the color of a Windows display element.
    ''' </summary>
    Public NotInheritable Class SystemPens

        Public Shared ReadOnly Property ActiveBorder As New Pen(System.Drawing.SystemColors.ActiveBorder)
        Public Shared ReadOnly Property ActiveCaption As New Pen(System.Drawing.SystemColors.ActiveCaption)
        Public Shared ReadOnly Property ActiveCaptionText As New Pen(System.Drawing.SystemColors.ActiveCaptionText)
        Public Shared ReadOnly Property AppWorkspace As New Pen(System.Drawing.SystemColors.AppWorkspace)
        Public Shared ReadOnly Property ButtonFace As New Pen(System.Drawing.SystemColors.ButtonFace)
        Public Shared ReadOnly Property ButtonHighlight As New Pen(System.Drawing.SystemColors.ButtonHighlight)
        Public Shared ReadOnly Property ButtonShadow As New Pen(System.Drawing.SystemColors.ButtonShadow)
        Public Shared ReadOnly Property Control As New Pen(System.Drawing.SystemColors.Control)
        Public Shared ReadOnly Property ControlDark As New Pen(System.Drawing.SystemColors.ControlDark)
        Public Shared ReadOnly Property ControlDarkDark As New Pen(System.Drawing.SystemColors.ControlDarkDark)
        Public Shared ReadOnly Property ControlLight As New Pen(System.Drawing.SystemColors.ControlLight)
        Public Shared ReadOnly Property ControlLightLight As New Pen(System.Drawing.SystemColors.ControlLightLight)
        Public Shared ReadOnly Property ControlText As New Pen(System.Drawing.SystemColors.ControlText)
        Public Shared ReadOnly Property Desktop As New Pen(System.Drawing.SystemColors.Desktop)
        Public Shared ReadOnly Property GradientActiveCaption As New Pen(System.Drawing.SystemColors.GradientActiveCaption)
        Public Shared ReadOnly Property GradientInactiveCaption As New Pen(System.Drawing.SystemColors.GradientInactiveCaption)
        Public Shared ReadOnly Property GrayText As New Pen(System.Drawing.SystemColors.GrayText)
        Public Shared ReadOnly Property Highlight As New Pen(System.Drawing.SystemColors.Highlight)
        Public Shared ReadOnly Property HighlightText As New Pen(System.Drawing.SystemColors.HighlightText)
        Public Shared ReadOnly Property HotTrack As New Pen(System.Drawing.SystemColors.HotTrack)
        Public Shared ReadOnly Property InactiveBorder As New Pen(System.Drawing.SystemColors.InactiveBorder)
        Public Shared ReadOnly Property InactiveCaption As New Pen(System.Drawing.SystemColors.InactiveCaption)
        Public Shared ReadOnly Property InactiveCaptionText As New Pen(System.Drawing.SystemColors.InactiveCaptionText)
        Public Shared ReadOnly Property Info As New Pen(System.Drawing.SystemColors.Info)
        Public Shared ReadOnly Property InfoText As New Pen(System.Drawing.SystemColors.InfoText)
        Public Shared ReadOnly Property Menu As New Pen(System.Drawing.SystemColors.Menu)
        Public Shared ReadOnly Property MenuBar As New Pen(System.Drawing.SystemColors.MenuBar)
        Public Shared ReadOnly Property MenuHighlight As New Pen(System.Drawing.SystemColors.MenuHighlight)
        Public Shared ReadOnly Property MenuText As New Pen(System.Drawing.SystemColors.MenuText)
        Public Shared ReadOnly Property ScrollBar As New Pen(System.Drawing.SystemColors.ScrollBar)
        Public Shared ReadOnly Property Window As New Pen(System.Drawing.SystemColors.Window)
        Public Shared ReadOnly Property WindowFrame As New Pen(System.Drawing.SystemColors.WindowFrame)
        Public Shared ReadOnly Property WindowText As New Pen(System.Drawing.SystemColors.WindowText)

        Private Sub New()
        End Sub
    End Class
#End If
#End If
End Namespace
