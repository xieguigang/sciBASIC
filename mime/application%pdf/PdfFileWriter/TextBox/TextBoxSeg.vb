
Imports System.Drawing

''' <summary>
''' TextBox line segment class
''' </summary>
Public Class TextBoxSeg

    ''' <summary>
    ''' Gets segment font.
    ''' </summary>
    Public Property Font As PdfFont

    ''' <summary>
    ''' Gets segment font size.
    ''' </summary>
    Public Property FontSize As Double

    ''' <summary>
    ''' Gets segment drawing style.
    ''' </summary>
    Public Property DrawStyle As DrawStyle

    ''' <summary>
    ''' Gets segment color.
    ''' </summary>
    Public Property FontColor As Color

    ''' <summary>
    ''' Gets segment width.
    ''' </summary>
    Public Property SegWidth As Double

    ''' <summary>
    ''' Gets segment space character count.
    ''' </summary>
    Public Property SpaceCount As Integer

    ''' <summary>
    ''' Gets segment text.
    ''' </summary>
    Public Property Text As String

    ''' <summary>
    ''' Gets annotation action
    ''' </summary>
    Public Property AnnotAction As AnnotAction

    ''' <summary>
    ''' TextBox segment constructor.
    ''' </summary>
    ''' <param name="Font">Segment font.</param>
    ''' <param name="FontSize">Segment font size.</param>
    ''' <param name="DrawStyle">Segment drawing style.</param>
    ''' <param name="FontColor">Segment color.</param>
    ''' <param name="AnnotAction">Segment annotation action.</param>
    Public Sub New(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, FontColor As Color, AnnotAction As AnnotAction)
        Me.Font = Font
        Me.FontSize = FontSize
        Me.DrawStyle = DrawStyle
        Me.FontColor = FontColor
        Text = String.Empty
        Me.AnnotAction = AnnotAction
        Return
    End Sub

    ''' <summary>
    ''' TextBox segment copy constructor.
    ''' </summary>
    ''' <param name="CopySeg">Source TextBox segment.</param>
    Friend Sub New(CopySeg As TextBoxSeg)
        Font = CopySeg.Font
        FontSize = CopySeg.FontSize
        DrawStyle = CopySeg.DrawStyle
        FontColor = CopySeg.FontColor
        Text = String.Empty
        AnnotAction = CopySeg.AnnotAction
        Return
    End Sub

    ''' <summary>
    ''' Compare two TextBox segments.
    ''' </summary>
    ''' <param name="Font">Segment font.</param>
    ''' <param name="FontSize">Segment font size.</param>
    ''' <param name="DrawStyle">Segment drawing style.</param>
    ''' <param name="FontColor">Segment color.</param>
    ''' <param name="AnnotAction">Segment annotation action.</param>
    ''' <returns>Result</returns>
    Friend Function IsEqual(Font As PdfFont, FontSize As Double, DrawStyle As DrawStyle, FontColor As Color, AnnotAction As AnnotAction) As Boolean
        ' test all but annotation action
        Return Me.Font Is Font AndAlso Me.FontSize = FontSize AndAlso Me.DrawStyle = DrawStyle AndAlso Me.FontColor = FontColor AndAlso AnnotAction.IsEqual(Me.AnnotAction, AnnotAction)
    End Function
End Class
