Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Renderer

    ''' <summary>
    ''' Class for rendering list item.
    ''' </summary>
    Public Class ListItem
#Region "Color Blend"
        ''' <summary>
        ''' Represent a color blend for selected item in a list that lost it focus input.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedBlurBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(248, 248, 248)
                        colors(1) = Color.FromArgb(229, 229, 229)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for selected item in focused list.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(239, 246, 252)
                        colors(1) = Color.FromArgb(212, 239, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for selected and highlighted item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedHLiteBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(236, 245, 255)
                        colors(1) = Color.FromArgb(208, 229, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for highlighted item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property HLitedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(252, 253, 255)
                        colors(1) = Color.FromArgb(237, 245, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for pressed item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property PressedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(160, 189, 227)
                        colors(1) = Color.FromArgb(255, 255, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
#End Region
#Region "Border Pen"
        ''' <summary>
        ''' Represent a border pen for selected item in a list that lost it focus input.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedBlurBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(217, 217, 217))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for selected item in a focused list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(177, 217, 229))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for highlighted item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property HLitedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(185, 215, 252))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for selected and highlighted item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedHLiteBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(132, 172, 221))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for pressed item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property PressedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(104, 140, 175))
                End Select
                Return Pens.Black
            End Get
        End Property
#End Region
    End Class
End Namespace