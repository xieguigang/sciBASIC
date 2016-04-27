Imports Microsoft.VisualBasic.Imaging

Public Class PreviewHandle

    Public Event ItemClick()
    Public Event ItemDoubleClick()

    Public Overrides Property Text As String
        Get
            Return Label1.Text
        End Get
        Set(value As String)
            Label1.Text = value
        End Set
    End Property

    Public Property UserData As Object

    Public Property NoAnimation As Boolean = True

#Region "Internal Animation Control"

    Dim _CurrentAngle As Double = -15
    Dim _Direction As Double
    Dim _EndsAngle As Double
    Dim _InternalBackgroundColor As Color
    Dim WithEvents _InternalBubbleRotationTimer As Timers.Timer = New Timers.Timer

    Public Property HighlightColor As Color = Color.AliceBlue


    Private Sub PreviewHandle_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox1.MouseEnter, Label1.MouseEnter
        _InternalBackgroundColor = HighlightColor
        Label1.BackColor = _InternalBackgroundColor

        If _CurrentAngle < 0 Then
            _Direction = 2.5
            _EndsAngle = 0
            EndsCondition = Function() _CurrentAngle > 0

            If NoAnimation Then

                _CurrentAngle = _EndsAngle
                Call InternalUpdateUI()

            Else
                _InternalBubbleRotationTimer.Enabled = True
                Call _InternalBubbleRotationTimer.Start()
            End If
        End If
    End Sub

    Dim EndsCondition As Func(Of Boolean)

    Private Sub UserControl1_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox1.MouseLeave
        _InternalBackgroundColor = BackColor
        Label1.BackColor = _InternalBackgroundColor

        If _CurrentAngle >= 0 Then
            _Direction = -2.5
            _EndsAngle = -15
            EndsCondition = Function() _CurrentAngle < _EndsAngle

            If NoAnimation Then

                _CurrentAngle = _EndsAngle
                Call InternalUpdateUI()

            Else

                _InternalBubbleRotationTimer.Enabled = True
                Call _InternalBubbleRotationTimer.Start()

            End If

        End If
    End Sub

    Private Sub PreviewHandle_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _InternalBubbleRotationTimer.Interval = 1
        _InternalBubbleRotationTimer.Enabled = False

        BackgroundImage = Nothing
    End Sub

    Private Sub _InternalBubbleRotationTimer_Tick() Handles _InternalBubbleRotationTimer.Elapsed

        If Not EndsCondition() Then
            _CurrentAngle += _Direction
            Call InternalUpdateUI()
        Else
            _InternalBubbleRotationTimer.Enabled = False
            Call _InternalBubbleRotationTimer.[Stop]()
        End If
    End Sub
#End Region

#Region "GDI++ Image Drawing"
    Public Property Value As String
        Get
            Return _InternalBubbleValue
        End Get
        Set(value As String)
            _InternalBubbleValue = value
            _InternalBubbleImage = InternalCreateMessageBubbleUI()
            Call InternalUpdateUI()
        End Set
    End Property
    Public Overrides Property BackgroundImage As Image
        Get
            Return _InternalBackgroundImage
        End Get
        Set(value As Image)
            If value Is Nothing Then
                value = New Bitmap(CInt(Width * 0.8), CInt(Height * 0.4))

                Using Gr As Graphics = Graphics.FromImage(value)
                    Call Gr.FillRectangle(Brushes.White, New Rectangle(New Point, value.Size))
                End Using
            End If

            _InternalBackgroundImage = value
            If _InternalBubbleImage Is Nothing Then
                _InternalBubbleImage = My.Resources._515151_speech_bubble_512
            End If
            Call InternalUpdateUI()
        End Set
    End Property

    Public Overloads Property Margin As Integer = 20

    Dim _InternalBackgroundImage As Image = DirectCast(My.Resources._515151_speech_bubble_512.Clone, Image)
    Dim _InternalBubbleImage As Image
    Dim _InternalBubbleValue As String

    Private Sub InternalUpdateUI()
        Dim res As Image = InternalCompositionImage()
        PictureBox1.BackgroundImage = res
    End Sub

    Private Function InternalCompositionImage() As Image
        Dim res As Image = New Bitmap(Width, Height)
        Dim Bubble As Image = ImageRotationUtilities.RotateImage(_InternalBubbleImage, _CurrentAngle)
        Dim _InternalBackgroundImage As Image = DirectCast(Me._InternalBackgroundImage.Clone, Image)

        Using Gr As Graphics = Graphics.FromImage(res)
            Call Gr.FillRectangle(New SolidBrush(_InternalBackgroundColor), New Rectangle(New Point(Margin, Margin), New Size(res.Width - 2 * Margin, res.Height - 2 * Margin)))
            Call Gr.DrawImage(_InternalBackgroundImage, CInt(Margin * 1.5), CInt(1.5 * Margin), CInt(res.Width - 3 * Margin), CInt(res.Height * 0.45))
            Call Gr.DrawImage(Bubble, CInt(Margin / 2), CInt(Margin / 2), CInt(res.Width * 0.15), CInt(res.Width * 0.15))
        End Using

        Return res
    End Function

    Private Function InternalCreateMessageBubbleUI() As Image
        Dim res As Image = DirectCast(My.Resources._515151_speech_bubble_512.Clone, Bitmap)
        Dim DrawingFont As New Font(FontFace.MicrosoftYaHei, 108, FontStyle.Bold)

        Using Gr As Graphics = Graphics.FromImage(res)
            Dim Size = Gr.MeasureString(Value, DrawingFont)

            Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.DrawString(Value, DrawingFont, Brushes.White, New Point((res.Width - Size.Width) / 2, (res.Height - Size.Height * 1.2) / 2))
        End Using

        Return res
    End Function
#End Region

    Private Sub PreviewHandle_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Try
            Call InternalUpdateUI()
        Catch ex As Exception

        End Try

        Label1.BringToFront()
        Label1.Location = New Point(Margin, Height * 0.6)
        Label1.Size = New Size(Width - 2 * Margin, Height * 0.4)
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click, PictureBox1.Click
        RaiseEvent ItemClick()
    End Sub
    Private Sub Label1_dblClick(sender As Object, e As EventArgs) Handles Label1.DoubleClick, PictureBox1.DoubleClick
        RaiseEvent ItemDoubleClick()
    End Sub

End Class
