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

        Private Sub New()
        End Sub
    End Class

    Public Class HatchBrush : Inherits Brush

        Sub New(style As HatchStyle,
                color1 As Color,
                color2 As Color)
        End Sub
    End Class

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