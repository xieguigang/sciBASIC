Imports System.Drawing
Imports System.Windows.Forms

Namespace API

    Public Class MoveScreen

        Dim UserCursor, FormLocation As Point

        Friend WithEvents TargetForm As Form
        Friend WithEvents Ctrl As System.Windows.Forms.Control

        Public Property MoveScreen As Boolean

        ''' <summary>
        ''' Enabled this feature or not?
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Enabled As Boolean = True

        Private Sub Caption_MouseDown(sender As Object, e As MouseEventArgs) Handles Ctrl.MouseDown
            If Enabled Then
                UserCursor = e.Location
                MoveScreen = True
            End If
        End Sub

        Private Sub Caption_MouseMove(sender As Object, e As MouseEventArgs) Handles Ctrl.MouseMove
            If MoveScreen Then
                FormLocation = New Point With {
                    .X = TargetForm.Location.X - UserCursor.X + e.X,
                    .Y = TargetForm.Location.Y - UserCursor.Y + e.Y}

                TargetForm.Location = FormLocation
            End If
        End Sub

        Private Sub Caption_MouseUp(sender As Object, e As MouseEventArgs) Handles Ctrl.MouseUp
            MoveScreen = False
        End Sub

        Public Sub JoinHandle(ctrl As Control)
            AddHandler ctrl.MouseUp, AddressOf Caption_MouseUp
            AddHandler ctrl.MouseMove, AddressOf Caption_MouseMove
            AddHandler ctrl.MouseDown, AddressOf Caption_MouseDown
        End Sub

        Sub New(Target As System.Windows.Forms.Control, HookOn As Form)
            Ctrl = Target
            TargetForm = HookOn
        End Sub
    End Class
End Namespace