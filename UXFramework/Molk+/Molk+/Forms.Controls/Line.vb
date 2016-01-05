Imports System.ComponentModel

Public Class Line
    Private Sub Line_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Line_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Width <= 0 OrElse Height <= 0 Then
            Return
        End If

        Dim Gr = Me.Size.CreateGDIDevice(BackColor)

        If Me.Size.Width >= Me.Size.Height Then
            '画横线
            Call Gr.Gr_Device.DrawLine(LinePen, New Point(0, Height / 2), New Point(Width, Height / 2))

        Else
            '画数显

            Call Gr.Gr_Device.DrawLine(LinePen, New Point(Width / 2, 0), New Point(Width / 2, Height))

        End If

        Me.BackgroundImage = Gr.ImageResource
    End Sub

    <DefaultValue(1.5)>
    Public Property PenWidth As Single
        Get
            If LinePen Is Nothing Then
                Return 1
            Else
                Return LinePen.Width
            End If
        End Get
        Set(value As Single)
            LinePen = New Pen(New SolidBrush(ForeColor), value)
            Call Line_Resize(Nothing, Nothing)
        End Set
    End Property

    Dim _LinePen As New Pen(New SolidBrush(ForeColor), 2)

    Public Property LinePen As Pen
        Get
            Return _LinePen
        End Get
        Set(value As Pen)
            _LinePen = value
            Call Line_Resize(Nothing, Nothing)
        End Set
    End Property
End Class
