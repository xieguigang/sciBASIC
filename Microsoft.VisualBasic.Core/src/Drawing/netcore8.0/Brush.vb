#Region "Microsoft.VisualBasic::74109b8f333894d8e829023f4d41f94d, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Brush.vb"

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

    '   Total Lines: 585
    '    Code Lines: 409 (69.91%)
    ' Comment Lines: 110 (18.80%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 66 (11.28%)
    '     File Size: 29.22 KB


    '     Class Brush
    ' 
    '         Function: SolidColor
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class SolidBrush
    ' 
    '         Properties: Color
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class TextureBrush
    ' 
    '         Properties: Image
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Brushes
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
    '     Class HatchBrush
    ' 
    '         Properties: BackgroundColor, ForegroundColor, HatchStyle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class PathGradientBrush
    ' 
    '         Properties: Blend, CenterColor, CenterPoint, FocusScales, InterpolationColors
    '                     Rectangle, SurroundColors, Transform, WrapMode
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ColorBlend
    ' 
    '         Properties: Colors, Positions
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Blend
    ' 
    '         Properties: Factors, Positions
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Enum LinearGradientMode
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class LinearGradientBrush
    ' 
    '         Properties: Angle, Blend, GammaCorrection, InterpolationColors, LinearColors
    '                     Rectangle, Transform, WrapMode
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: SetBlendTriangularShape, SetSigmaBellShape
    ' 
    '     Class SystemBrushes
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
    '     Enum WrapMode
    ' 
    '         Clamp, Tile, TileFlipX, TileFlipXY, TileFlipY
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum HatchStyle
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    Public MustInherit Class Brush : Implements IDisposable

        Private disposedValue As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Public Shared Function SolidColor(b As Brush) As Color
            If TypeOf b Is SolidBrush Then
                Return DirectCast(b, SolidBrush).Color
            Else
                Return Nothing
            End If
        End Function
    End Class

    Public Class SolidBrush : Inherits Brush

        Public Property Color As Color

        Sub New(color As Color)
            Me.Color = color
        End Sub

        Public Overrides Function ToString() As String
            Return Color.ToHtmlColor
        End Function
    End Class

    Public Class TextureBrush : Inherits Brush

        Public Property Image As Image

        Sub New(image As Image)
            Me.Image = image
        End Sub

        Public Overrides Function ToString() As String
            Return Image.ToString
        End Function
    End Class

    Public NotInheritable Class Brushes

        Public Shared ReadOnly Property AliceBlue As New SolidBrush(Color.AliceBlue)
        Public Shared ReadOnly Property AntiqueWhite As New SolidBrush(Color.AntiqueWhite)
        Public Shared ReadOnly Property Aqua As New SolidBrush(Color.Aqua)
        Public Shared ReadOnly Property Aquamarine As New SolidBrush(Color.Aquamarine)
        Public Shared ReadOnly Property Azure As New SolidBrush(Color.Azure)
        Public Shared ReadOnly Property Beige As New SolidBrush(Color.Beige)
        Public Shared ReadOnly Property Bisque As New SolidBrush(Color.Bisque)
        Public Shared ReadOnly Property Black As New SolidBrush(Color.Black)
        Public Shared ReadOnly Property BlanchedAlmond As New SolidBrush(Color.BlanchedAlmond)
        Public Shared ReadOnly Property Blue As New SolidBrush(Color.Blue)
        Public Shared ReadOnly Property BlueViolet As New SolidBrush(Color.BlueViolet)
        Public Shared ReadOnly Property Brown As New SolidBrush(Color.Brown)
        Public Shared ReadOnly Property BurlyWood As New SolidBrush(Color.BurlyWood)
        Public Shared ReadOnly Property CadetBlue As New SolidBrush(Color.CadetBlue)
        Public Shared ReadOnly Property Chartreuse As New SolidBrush(Color.Chartreuse)
        Public Shared ReadOnly Property Chocolate As New SolidBrush(Color.Chocolate)
        Public Shared ReadOnly Property Coral As New SolidBrush(Color.Coral)
        Public Shared ReadOnly Property CornflowerBlue As New SolidBrush(Color.CornflowerBlue)
        Public Shared ReadOnly Property Cornsilk As New SolidBrush(Color.Cornsilk)
        Public Shared ReadOnly Property Crimson As New SolidBrush(Color.Crimson)
        Public Shared ReadOnly Property Cyan As New SolidBrush(Color.Cyan)
        Public Shared ReadOnly Property DarkBlue As New SolidBrush(Color.DarkBlue)
        Public Shared ReadOnly Property DarkCyan As New SolidBrush(Color.DarkCyan)
        Public Shared ReadOnly Property DarkGoldenrod As New SolidBrush(Color.DarkGoldenrod)
        Public Shared ReadOnly Property DarkGray As New SolidBrush(Color.DarkGray)
        Public Shared ReadOnly Property DarkGreen As New SolidBrush(Color.DarkGreen)
        Public Shared ReadOnly Property DarkKhaki As New SolidBrush(Color.DarkKhaki)
        Public Shared ReadOnly Property DarkMagenta As New SolidBrush(Color.DarkMagenta)
        Public Shared ReadOnly Property DarkOliveGreen As New SolidBrush(Color.DarkOliveGreen)
        Public Shared ReadOnly Property DarkOrange As New SolidBrush(Color.DarkOrange)
        Public Shared ReadOnly Property DarkOrchid As New SolidBrush(Color.DarkOrchid)
        Public Shared ReadOnly Property DarkRed As New SolidBrush(Color.DarkRed)
        Public Shared ReadOnly Property DarkSalmon As New SolidBrush(Color.DarkSalmon)
        Public Shared ReadOnly Property DarkSeaGreen As New SolidBrush(Color.DarkSeaGreen)
        Public Shared ReadOnly Property DarkSlateBlue As New SolidBrush(Color.DarkSlateBlue)
        Public Shared ReadOnly Property DarkSlateGray As New SolidBrush(Color.DarkSlateGray)
        Public Shared ReadOnly Property DarkTurquoise As New SolidBrush(Color.DarkTurquoise)
        Public Shared ReadOnly Property DarkViolet As New SolidBrush(Color.DarkViolet)
        Public Shared ReadOnly Property DeepPink As New SolidBrush(Color.DeepPink)
        Public Shared ReadOnly Property DeepSkyBlue As New SolidBrush(Color.DeepSkyBlue)
        Public Shared ReadOnly Property DimGray As New SolidBrush(Color.DimGray)
        Public Shared ReadOnly Property DodgerBlue As New SolidBrush(Color.DodgerBlue)
        Public Shared ReadOnly Property Firebrick As New SolidBrush(Color.Firebrick)
        Public Shared ReadOnly Property FloralWhite As New SolidBrush(Color.FloralWhite)
        Public Shared ReadOnly Property ForestGreen As New SolidBrush(Color.ForestGreen)
        Public Shared ReadOnly Property Fuchsia As New SolidBrush(Color.Fuchsia)
        Public Shared ReadOnly Property Gainsboro As New SolidBrush(Color.Gainsboro)
        Public Shared ReadOnly Property GhostWhite As New SolidBrush(Color.GhostWhite)
        Public Shared ReadOnly Property Gold As New SolidBrush(Color.Gold)
        Public Shared ReadOnly Property Goldenrod As New SolidBrush(Color.Goldenrod)
        Public Shared ReadOnly Property Gray As New SolidBrush(Color.Gray)
        Public Shared ReadOnly Property Green As New SolidBrush(Color.Green)
        Public Shared ReadOnly Property GreenYellow As New SolidBrush(Color.GreenYellow)
        Public Shared ReadOnly Property Honeydew As New SolidBrush(Color.Honeydew)
        Public Shared ReadOnly Property HotPink As New SolidBrush(Color.HotPink)
        Public Shared ReadOnly Property IndianRed As New SolidBrush(Color.IndianRed)
        Public Shared ReadOnly Property Indigo As New SolidBrush(Color.Indigo)
        Public Shared ReadOnly Property Ivory As New SolidBrush(Color.Ivory)
        Public Shared ReadOnly Property Khaki As New SolidBrush(Color.Khaki)
        Public Shared ReadOnly Property Lavender As New SolidBrush(Color.Lavender)
        Public Shared ReadOnly Property LavenderBlush As New SolidBrush(Color.LavenderBlush)
        Public Shared ReadOnly Property LawnGreen As New SolidBrush(Color.LawnGreen)
        Public Shared ReadOnly Property LemonChiffon As New SolidBrush(Color.LemonChiffon)
        Public Shared ReadOnly Property LightBlue As New SolidBrush(Color.LightBlue)
        Public Shared ReadOnly Property LightCoral As New SolidBrush(Color.LightCoral)
        Public Shared ReadOnly Property LightCyan As New SolidBrush(Color.LightCyan)
        Public Shared ReadOnly Property LightGoldenrodYellow As New SolidBrush(Color.LightGoldenrodYellow)
        Public Shared ReadOnly Property LightGray As New SolidBrush(Color.LightGray)
        Public Shared ReadOnly Property LightGreen As New SolidBrush(Color.LightGreen)
        Public Shared ReadOnly Property LightPink As New SolidBrush(Color.LightPink)
        Public Shared ReadOnly Property LightSalmon As New SolidBrush(Color.LightSalmon)
        Public Shared ReadOnly Property LightSeaGreen As New SolidBrush(Color.LightSeaGreen)
        Public Shared ReadOnly Property LightSkyBlue As New SolidBrush(Color.LightSkyBlue)
        Public Shared ReadOnly Property LightSlateGray As New SolidBrush(Color.LightSlateGray)
        Public Shared ReadOnly Property LightSteelBlue As New SolidBrush(Color.LightSteelBlue)
        Public Shared ReadOnly Property LightYellow As New SolidBrush(Color.LightYellow)
        Public Shared ReadOnly Property Lime As New SolidBrush(Color.Lime)
        Public Shared ReadOnly Property LimeGreen As New SolidBrush(Color.LimeGreen)
        Public Shared ReadOnly Property Linen As New SolidBrush(Color.Linen)
        Public Shared ReadOnly Property Magenta As New SolidBrush(Color.Magenta)
        Public Shared ReadOnly Property Maroon As New SolidBrush(Color.Maroon)
        Public Shared ReadOnly Property MediumAquamarine As New SolidBrush(Color.MediumAquamarine)
        Public Shared ReadOnly Property MediumBlue As New SolidBrush(Color.MediumBlue)
        Public Shared ReadOnly Property MediumOrchid As New SolidBrush(Color.MediumOrchid)
        Public Shared ReadOnly Property MediumPurple As New SolidBrush(Color.MediumPurple)
        Public Shared ReadOnly Property MediumSeaGreen As New SolidBrush(Color.MediumSeaGreen)
        Public Shared ReadOnly Property MediumSlateBlue As New SolidBrush(Color.MediumSlateBlue)
        Public Shared ReadOnly Property MediumSpringGreen As New SolidBrush(Color.MediumSpringGreen)
        Public Shared ReadOnly Property MediumTurquoise As New SolidBrush(Color.MediumTurquoise)
        Public Shared ReadOnly Property MediumVioletRed As New SolidBrush(Color.MediumVioletRed)
        Public Shared ReadOnly Property MidnightBlue As New SolidBrush(Color.MidnightBlue)
        Public Shared ReadOnly Property MintCream As New SolidBrush(Color.MintCream)
        Public Shared ReadOnly Property MistyRose As New SolidBrush(Color.MistyRose)
        Public Shared ReadOnly Property Moccasin As New SolidBrush(Color.Moccasin)
        Public Shared ReadOnly Property NavajoWhite As New SolidBrush(Color.NavajoWhite)
        Public Shared ReadOnly Property Navy As New SolidBrush(Color.Navy)
        Public Shared ReadOnly Property OldLace As New SolidBrush(Color.OldLace)
        Public Shared ReadOnly Property Olive As New SolidBrush(Color.Olive)
        Public Shared ReadOnly Property OliveDrab As New SolidBrush(Color.OliveDrab)
        Public Shared ReadOnly Property Orange As New SolidBrush(Color.Orange)
        Public Shared ReadOnly Property OrangeRed As New SolidBrush(Color.OrangeRed)
        Public Shared ReadOnly Property Orchid As New SolidBrush(Color.Orchid)
        Public Shared ReadOnly Property PaleGoldenrod As New SolidBrush(Color.PaleGoldenrod)
        Public Shared ReadOnly Property PaleGreen As New SolidBrush(Color.PaleGreen)
        Public Shared ReadOnly Property PaleTurquoise As New SolidBrush(Color.PaleTurquoise)
        Public Shared ReadOnly Property PaleVioletRed As New SolidBrush(Color.PaleVioletRed)
        Public Shared ReadOnly Property PapayaWhip As New SolidBrush(Color.PapayaWhip)
        Public Shared ReadOnly Property PeachPuff As New SolidBrush(Color.PeachPuff)
        Public Shared ReadOnly Property Peru As New SolidBrush(Color.Peru)
        Public Shared ReadOnly Property Pink As New SolidBrush(Color.Pink)
        Public Shared ReadOnly Property Plum As New SolidBrush(Color.Plum)
        Public Shared ReadOnly Property PowderBlue As New SolidBrush(Color.PowderBlue)
        Public Shared ReadOnly Property Purple As New SolidBrush(Color.Purple)
        Public Shared ReadOnly Property Red As New SolidBrush(Color.Red)
        Public Shared ReadOnly Property RosyBrown As New SolidBrush(Color.RosyBrown)
        Public Shared ReadOnly Property RoyalBlue As New SolidBrush(Color.RoyalBlue)
        Public Shared ReadOnly Property SaddleBrown As New SolidBrush(Color.SaddleBrown)
        Public Shared ReadOnly Property Salmon As New SolidBrush(Color.Salmon)
        Public Shared ReadOnly Property SandyBrown As New SolidBrush(Color.SandyBrown)
        Public Shared ReadOnly Property SeaGreen As New SolidBrush(Color.SeaGreen)
        Public Shared ReadOnly Property SeaShell As New SolidBrush(Color.SeaShell)
        Public Shared ReadOnly Property Sienna As New SolidBrush(Color.Sienna)
        Public Shared ReadOnly Property Silver As New SolidBrush(Color.Silver)
        Public Shared ReadOnly Property SkyBlue As New SolidBrush(Color.SkyBlue)
        Public Shared ReadOnly Property SlateBlue As New SolidBrush(Color.SlateBlue)
        Public Shared ReadOnly Property SlateGray As New SolidBrush(Color.SlateGray)
        Public Shared ReadOnly Property Snow As New SolidBrush(Color.Snow)
        Public Shared ReadOnly Property SpringGreen As New SolidBrush(Color.SpringGreen)
        Public Shared ReadOnly Property SteelBlue As New SolidBrush(Color.SteelBlue)
        Public Shared ReadOnly Property Tan As New SolidBrush(Color.Tan)
        Public Shared ReadOnly Property Teal As New SolidBrush(Color.Teal)
        Public Shared ReadOnly Property Thistle As New SolidBrush(Color.Thistle)
        Public Shared ReadOnly Property Tomato As New SolidBrush(Color.Tomato)
        Public Shared ReadOnly Property Transparent As New SolidBrush(Color.Transparent)
        Public Shared ReadOnly Property Turquoise As New SolidBrush(Color.Turquoise)
        Public Shared ReadOnly Property Violet As New SolidBrush(Color.Violet)
        Public Shared ReadOnly Property Wheat As New SolidBrush(Color.Wheat)
        Public Shared ReadOnly Property White As New SolidBrush(Color.White)
        Public Shared ReadOnly Property WhiteSmoke As New SolidBrush(Color.WhiteSmoke)
        Public Shared ReadOnly Property Yellow As New SolidBrush(Color.Yellow)
        Public Shared ReadOnly Property YellowGreen As New SolidBrush(Color.YellowGreen)

        Private Sub New()
        End Sub
    End Class

    Public Class HatchBrush : Inherits Brush

        Public ReadOnly Property HatchStyle As HatchStyle
        Public ReadOnly Property ForegroundColor As Color
        Public ReadOnly Property BackgroundColor As Color

        Sub New(style As HatchStyle,
                foreColor As Color,
                backColor As Color)
            _HatchStyle = style
            _ForegroundColor = foreColor
            _BackgroundColor = backColor
        End Sub

        Public Overrides Function ToString() As String
            Return $"{HatchStyle} {ForegroundColor.ToHtmlColor}/{BackgroundColor.ToHtmlColor}"
        End Function
    End Class

    Public Class PathGradientBrush : Inherits Brush

        ''' <summary>
        ''' Gets or sets the color at the center of the path gradient.
        ''' </summary>
        Public Property CenterColor As Color

        ''' <summary>
        ''' Gets or sets an array of colors that correspond to the points in the path.
        ''' </summary>
        Public Property SurroundColors As Color()

        ''' <summary>
        ''' Gets or sets the center point of the path gradient.
        ''' </summary>
        Public Property CenterPoint As PointF

        ''' <summary>
        ''' Gets or sets the focus point for the gradient falloff.
        ''' </summary>
        Public Property FocusScales As PointF

        ''' <summary>
        ''' Gets or sets a Blend that specifies positions and factors for a custom falloff.
        ''' </summary>
        Public Property Blend As Blend

        ''' <summary>
        ''' Gets or sets the encompass rectangle for this gradient.
        ''' </summary>
        Public Property Rectangle As RectangleF

        Public Property WrapMode As WrapMode
        Public Property InterpolationColors As ColorBlend
        Public Property Transform As Matrix

        Sub New(polygon As GraphicsPath)
            _CenterColor = Color.White
            _CenterPoint = New PointF(0, 0)
            _FocusScales = New PointF(0, 0)
        End Sub

        ''' <summary>
        ''' Creates a copy of the PathGradientBrush from a given point array.
        ''' </summary>
        Sub New(points As PointF(), Optional wrapMode As WrapMode = WrapMode.Clamp)
            _CenterColor = Color.White
            _CenterPoint = New PointF(0, 0)
            _FocusScales = New PointF(0, 0)
            _WrapMode = wrapMode
        End Sub

        Public Overrides Function ToString() As String
            Return $"PathGradientBrush: {CenterColor.ToHtmlColor}"
        End Function
    End Class

    Public Class ColorBlend

        ''' <summary>
        ''' Gets or sets an array of colors to use at corresponding positions along a gradient.
        ''' </summary>
        Public Property Colors As Color()

        ''' <summary>
        ''' Gets or sets the positions along a gradient line.
        ''' </summary>
        Public Property Positions As Single()

        Sub New()
            Colors = New Color() {}
            Positions = New Single() {}
        End Sub

        Sub New(size As Integer)
            Colors = New Color(size - 1) {}
            Positions = New Single(size - 1) {}
        End Sub
    End Class

    ''' <summary>
    ''' Defines a blend pattern for a gradient. Positions are from 0 to 1 and correspond 
    ''' to percentage distance along the gradient line.
    ''' </summary>
    Public Class Blend

        ''' <summary>
        ''' Gets or sets an array of blend factors for the gradient. Values range from 0 to 1.
        ''' </summary>
        Public Property Factors As Single()

        ''' <summary>
        ''' Gets or sets an array of positions for the blend factors. Values range from 0 to 1.
        ''' </summary>
        Public Property Positions As Single()

        Sub New()
            Factors = New Single() {1.0F}
            Positions = New Single() {1.0F}
        End Sub

        Sub New(count As Integer)
            Factors = New Single(count - 1) {}
            Positions = New Single(count - 1) {}
        End Sub
    End Class

    ''' <summary>
    ''' Specifies the direction of a linear gradient.
    ''' </summary>
    Public Enum LinearGradientMode
        ''' <summary>
        ''' Specifies a gradient from left to right.
        ''' </summary>
        Horizontal = 0
        ''' <summary>
        ''' Specifies a gradient from top to bottom.
        ''' </summary>
        Vertical = 1
        ''' <summary>
        ''' Specifies a gradient from upper-left to lower-right.
        ''' </summary>
        ForwardDiagonal = 2
        ''' <summary>
        ''' Specifies a gradient from upper-right to lower-left.
        ''' </summary>
        BackwardDiagonal = 3
    End Enum

    ''' <summary>
    ''' Encapsulates a brush with a linear gradient. This data object stores 
    ''' linear gradient parameters for SkiaSharp rendering.
    ''' </summary>
    Public Class LinearGradientBrush : Inherits Brush

        ''' <summary>
        ''' Gets or sets the starting and ending colors of the gradient.
        ''' </summary>
        Public Property LinearColors As Color()

        ''' <summary>
        ''' Gets or sets the bounding rectangle of the gradient.
        ''' </summary>
        Public Property Rectangle As RectangleF

        ''' <summary>
        ''' Gets or sets the angle orientation of the gradient, in degrees.
        ''' </summary>
        Public Property Angle As Single

        ''' <summary>
        ''' Gets or sets a value indicating whether gamma correction is enabled for this brush.
        ''' </summary>
        Public Property GammaCorrection As Boolean

        ''' <summary>
        ''' Gets or sets the wrap mode for this LinearGradientBrush.
        ''' </summary>
        Public Property WrapMode As WrapMode

        ''' <summary>
        ''' Gets or sets a Blend that specifies positions and factors for custom falloff.
        ''' </summary>
        Public Property Blend As Blend

        ''' <summary>
        ''' Gets or sets a ColorBlend that defines a multicolor linear gradient.
        ''' </summary>
        Public Property InterpolationColors As ColorBlend

        ''' <summary>
        ''' Gets or sets a copy of the Matrix that defines a local geometric transform.
        ''' </summary>
        Public Property Transform As Matrix

        ''' <summary>
        ''' Creates a linear gradient brush from a rectangle and two colors.
        ''' </summary>
        ''' <param name="rect">The bounding rectangle for the gradient.</param>
        ''' <param name="color1">The starting color of the gradient.</param>
        ''' <param name="color2">The ending color of the gradient.</param>
        ''' <param name="angle">The angle of the gradient line, in degrees, from horizontal.</param>
        Sub New(rect As RectangleF, color1 As Color, color2 As Color, Optional angle As Single = 0)
            _Rectangle = rect
            _LinearColors = {color1, color2}
            _Angle = angle
        End Sub

        ''' <summary>
        ''' Creates a linear gradient brush from a rectangle, two colors, and a gradient mode.
        ''' </summary>
        Sub New(rect As RectangleF, color1 As Color, color2 As Color, mode As LinearGradientMode)
            _Rectangle = rect
            _LinearColors = {color1, color2}
            Select Case mode
                Case LinearGradientMode.Horizontal
                    _Angle = 0
                Case LinearGradientMode.Vertical
                    _Angle = 90
                Case LinearGradientMode.ForwardDiagonal
                    _Angle = 45
                Case LinearGradientMode.BackwardDiagonal
                    _Angle = 135
            End Select
        End Sub

        ''' <summary>
        ''' Sets the blend factors and positions to create a triangular falloff from center to edges.
        ''' </summary>
        Public Sub SetBlendTriangularShape(focus As Single, Optional scale As Single = 1.0F)
            _triangularFocus = focus
            _triangularScale = scale
        End Sub

        ''' <summary>
        ''' Creates a linear gradient brush from rectangle and the sigma bell gradient profile.
        ''' </summary>
        Public Sub SetSigmaBellShape(focus As Single, Optional scale As Single = 1.0F)
            _sigmaFocus = focus
            _sigmaScale = scale
        End Sub

        Private _triangularFocus As Single
        Private _triangularScale As Single
        Private _sigmaFocus As Single
        Private _sigmaScale As Single

        Public Overrides Function ToString() As String
            If LinearColors IsNot Nothing AndAlso LinearColors.Length >= 2 Then
                Return $"LinearGradientBrush: {LinearColors(0).ToHtmlColor} -> {LinearColors(1).ToHtmlColor}"
            Else
                Return "LinearGradientBrush"
            End If
        End Function
    End Class

    ''' <summary>
    ''' Each property of the SystemBrushes class is a SolidBrush that is the color 
    ''' of a Windows display element.
    ''' </summary>
    Public NotInheritable Class SystemBrushes

        Public Shared ReadOnly Property ActiveBorder As New SolidBrush(System.Drawing.SystemColors.ActiveBorder)
        Public Shared ReadOnly Property ActiveCaption As New SolidBrush(System.Drawing.SystemColors.ActiveCaption)
        Public Shared ReadOnly Property ActiveCaptionText As New SolidBrush(System.Drawing.SystemColors.ActiveCaptionText)
        Public Shared ReadOnly Property AppWorkspace As New SolidBrush(System.Drawing.SystemColors.AppWorkspace)
        Public Shared ReadOnly Property ButtonFace As New SolidBrush(System.Drawing.SystemColors.ButtonFace)
        Public Shared ReadOnly Property ButtonHighlight As New SolidBrush(System.Drawing.SystemColors.ButtonHighlight)
        Public Shared ReadOnly Property ButtonShadow As New SolidBrush(System.Drawing.SystemColors.ButtonShadow)
        Public Shared ReadOnly Property Control As New SolidBrush(System.Drawing.SystemColors.Control)
        Public Shared ReadOnly Property ControlDark As New SolidBrush(System.Drawing.SystemColors.ControlDark)
        Public Shared ReadOnly Property ControlDarkDark As New SolidBrush(System.Drawing.SystemColors.ControlDarkDark)
        Public Shared ReadOnly Property ControlLight As New SolidBrush(System.Drawing.SystemColors.ControlLight)
        Public Shared ReadOnly Property ControlLightLight As New SolidBrush(System.Drawing.SystemColors.ControlLightLight)
        Public Shared ReadOnly Property ControlText As New SolidBrush(System.Drawing.SystemColors.ControlText)
        Public Shared ReadOnly Property Desktop As New SolidBrush(System.Drawing.SystemColors.Desktop)
        Public Shared ReadOnly Property GradientActiveCaption As New SolidBrush(System.Drawing.SystemColors.GradientActiveCaption)
        Public Shared ReadOnly Property GradientInactiveCaption As New SolidBrush(System.Drawing.SystemColors.GradientInactiveCaption)
        Public Shared ReadOnly Property GrayText As New SolidBrush(System.Drawing.SystemColors.GrayText)
        Public Shared ReadOnly Property Highlight As New SolidBrush(System.Drawing.SystemColors.Highlight)
        Public Shared ReadOnly Property HighlightText As New SolidBrush(System.Drawing.SystemColors.HighlightText)
        Public Shared ReadOnly Property HotTrack As New SolidBrush(System.Drawing.SystemColors.HotTrack)
        Public Shared ReadOnly Property InactiveBorder As New SolidBrush(System.Drawing.SystemColors.InactiveBorder)
        Public Shared ReadOnly Property InactiveCaption As New SolidBrush(System.Drawing.SystemColors.InactiveCaption)
        Public Shared ReadOnly Property InactiveCaptionText As New SolidBrush(System.Drawing.SystemColors.InactiveCaptionText)
        Public Shared ReadOnly Property Info As New SolidBrush(System.Drawing.SystemColors.Info)
        Public Shared ReadOnly Property InfoText As New SolidBrush(System.Drawing.SystemColors.InfoText)
        Public Shared ReadOnly Property Menu As New SolidBrush(System.Drawing.SystemColors.Menu)
        Public Shared ReadOnly Property MenuBar As New SolidBrush(System.Drawing.SystemColors.MenuBar)
        Public Shared ReadOnly Property MenuHighlight As New SolidBrush(System.Drawing.SystemColors.MenuHighlight)
        Public Shared ReadOnly Property MenuText As New SolidBrush(System.Drawing.SystemColors.MenuText)
        Public Shared ReadOnly Property ScrollBar As New SolidBrush(System.Drawing.SystemColors.ScrollBar)
        Public Shared ReadOnly Property Window As New SolidBrush(System.Drawing.SystemColors.Window)
        Public Shared ReadOnly Property WindowFrame As New SolidBrush(System.Drawing.SystemColors.WindowFrame)
        Public Shared ReadOnly Property WindowText As New SolidBrush(System.Drawing.SystemColors.WindowText)

        Private Sub New()
        End Sub
    End Class

    Public Enum WrapMode
        Tile
        TileFlipX
        TileFlipY
        TileFlipXY
        Clamp
    End Enum

    Public Enum HatchStyle
        Horizontal = 0
        Vertical = 1
        ForwardDiagonal = 2
        BackwardDiagonal = 3
        Cross = 4
        DiagonalCross = 5
        Percent05 = 6
        Percent10 = 7
        Percent20 = 8
        Percent25 = 9
        Percent30 = 10
        Percent40 = 11
        Percent50 = 12
        Percent60 = 13
        Percent70 = 14
        Percent75 = 15
        Percent80 = 16
        Percent90 = 17
        LightDownwardDiagonal = 18
        LightUpwardDiagonal = 19
        DarkDownwardDiagonal = 20
        DarkUpwardDiagonal = 21
        WideDownwardDiagonal = 22
        WideUpwardDiagonal = 23
        LightVertical = 24
        LightHorizontal = 25
        NarrowVertical = 26
        NarrowHorizontal = 27
        DarkVertical = 28
        DarkHorizontal = 29
        DashedDownwardDiagonal = 30
        DashedUpwardDiagonal = 31
        DashedHorizontal = 32
        DashedVertical = 33
        SmallConfetti = 34
        LargeConfetti = 35
        ZigZag = 36
        Wave = 37
        DiagonalBrick = 38
        HorizontalBrick = 39
        Weave = 40
        Plaid = 41
        Divot = 42
        DottedGrid = 43
        DottedDiamond = 44
        Shingle = 45
        Trellis = 46
        Sphere = 47
        SmallGrid = 48
        SmallCheckerBoard = 49
        LargeCheckerBoard = 50
        OutlinedDiamond = 51
        SolidDiamond = 52
        LargeGrid = 4
        Min = 0
        Max = 4
    End Enum
#End If
End Namespace
