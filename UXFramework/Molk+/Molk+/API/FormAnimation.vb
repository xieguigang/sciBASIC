#Region "Microsoft.VisualBasic::0dd8372fd841b77c8a46ee7a2f8ec96f, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\FormAnimation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace API
    Public Class FormAnimation

        Dim WithEvents DoThread As New Timers.Timer(interval:=1)
        Dim WithEvents WindForm As System.Windows.Forms.Form

        Dim FormAnimated As System.Action

        Dim NewOpacity As Double
        Dim OldOpacity As Double

        Public Property FadeWithMovement As Boolean = True

        Public Property Opacity As Double
            Get
                Return WindForm.Opacity
            End Get
            Set(value As Double)
                WindForm.Opacity = value
            End Set
        End Property

        Public Overloads Sub FadeIn(Optional InitialOpacity As Double = 0.2, Optional StopOpacity As Double = 0.93)
            WindForm.Opacity = InitialOpacity
            NewOpacity = StopOpacity
            FormAnimated = AddressOf FadeIn

            Call DoThread.Start()
        End Sub

        Public Overloads Sub FadeOut(Optional StopOpacity As Double = 0)
            NewOpacity = StopOpacity
            FormAnimated = AddressOf FadeOut

            Call DoThread.Start()
        End Sub

        Friend Overloads Sub FadeIn()
            If WindForm.Opacity >= NewOpacity Then
                Call DoThread.Stop()
                Return
            End If

            WindForm.Opacity += 0.08
            If FadeWithMovement Then
                WindForm.Location = New Point With {
                    .X = WindForm.Location.X, .Y = WindForm.Location.Y - 1}
            End If
        End Sub

        Friend Overloads Sub FadeOut()
            If WindForm.Opacity <= NewOpacity Then
                Call DoThread.Stop()
                Call WindForm.Close()
                Return
            End If

            WindForm.Opacity -= 0.08
            If FadeWithMovement Then
                WindForm.Location = New Point With {
                    .X = WindForm.Location.X, .Y = WindForm.Location.Y - 1}
            End If
        End Sub

        Public Sub Minimize()
            NewOpacity = 0
            OldOpacity = WindForm.Opacity
            FormAnimated = AddressOf FadeOut2

            Call DoThread.Start()
        End Sub

        Friend Sub FadeOut2()
            If WindForm.Opacity <= NewOpacity Then
                Call DoThread.Stop()
                WindForm.WindowState = FormWindowState.Minimized
                WindForm.Opacity = OldOpacity
                Return
            End If

            WindForm.Opacity -= 0.08
            If FadeWithMovement Then
                WindForm.Location = New Point With {
                   .X = WindForm.Location.X, .Y = WindForm.Location.Y + 1}
            End If
        End Sub

        Friend Sub New()
            Call DoThread.Stop()
        End Sub

        Shared Widening Operator CType(e As System.Windows.Forms.Form) As FormAnimation
            Return New FormAnimation With {.WindForm = e}
        End Operator

        Private Sub DoThread_Elapsed(sender As Object, e As Timers.ElapsedEventArgs) Handles DoThread.Elapsed
            Call FormAnimated()
        End Sub
    End Class
End Namespace
