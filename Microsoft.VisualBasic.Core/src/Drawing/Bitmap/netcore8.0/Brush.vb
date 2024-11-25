#Region "Microsoft.VisualBasic::5f842bfb70abb8273e2deef7b38246d7, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\netcore8.0\Brush.vb"

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

    '   Total Lines: 174
    '    Code Lines: 134 (77.01%)
    ' Comment Lines: 10 (5.75%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 30 (17.24%)
    '     File Size: 5.37 KB


    '     Class Brush
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class SolidBrush
    ' 
    '         Properties: Color
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class TextureBrush
    ' 
    '         Properties: Image
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Brushes
    ' 
    '         Properties: Black, Blue, BlueViolet, Brown, DarkGreen
    '                     DarkOliveGreen, Gray, Green, LightGray, Orange
    '                     Red, SkyBlue, Transparent, Violet, White
    '                     Yellow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class HatchBrush
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class PathGradientBrush
    ' 
    '         Properties: InterpolationColors, WrapMode
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class ColorBlend
    ' 
    '         Properties: Colors, Positions
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

#If NET8_0_OR_GREATER Then

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
    End Class

    Public Class SolidBrush : Inherits Brush

        Public Property Color As Color

        Sub New(color As Color)
            Me.Color = color
        End Sub
    End Class

    Public Class TextureBrush : Inherits Brush

        Public Property Image As Image

        Sub New(image As Image)
            Me.Image = image
        End Sub

    End Class

    Public NotInheritable Class Brushes

        Public Shared ReadOnly Property Red As New SolidBrush(Color.Red)
        Public Shared ReadOnly Property Black As New SolidBrush(Color.Black)
        Public Shared ReadOnly Property White As New SolidBrush(Color.White)
        Public Shared ReadOnly Property Gray As New SolidBrush(Color.Gray)
        Public Shared ReadOnly Property LightGray As New SolidBrush(Color.LightGray)
        Public Shared ReadOnly Property SkyBlue As New SolidBrush(Color.SkyBlue)
        Public Shared ReadOnly Property Violet As New SolidBrush(Color.Violet)
        Public Shared ReadOnly Property Blue As New SolidBrush(Color.Blue)
        Public Shared ReadOnly Property Green As New SolidBrush(Color.Green)
        Public Shared ReadOnly Property Yellow As New SolidBrush(Color.Yellow)
        Public Shared ReadOnly Property DarkGreen As New SolidBrush(Color.DarkGreen)
        Public Shared ReadOnly Property BlueViolet As New SolidBrush(Color.BlueViolet)
        Public Shared ReadOnly Property Brown As New SolidBrush(Color.Brown)
        Public Shared ReadOnly Property DarkOliveGreen As New SolidBrush(Color.DarkOliveGreen)
        Public Shared ReadOnly Property Orange As New SolidBrush(Color.Orange)
        Public Shared ReadOnly Property Transparent As New SolidBrush(Color.Transparent)

        Private Sub New()
        End Sub
    End Class

    Public Class HatchBrush : Inherits Brush

        Sub New(style As HatchStyle,
                color1 As Color,
                color2 As Color)
        End Sub
    End Class

    Public Class PathGradientBrush : Inherits Brush

        Public Property WrapMode As WrapMode
        Public Property InterpolationColors As ColorBlend

        Sub New(polygon As GraphicsPath)

        End Sub
    End Class

    Public Class ColorBlend

        Public Property Colors As Color()
        Public Property Positions As Single()

        Sub New(size As Integer)

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
