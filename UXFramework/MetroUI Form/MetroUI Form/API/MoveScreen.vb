#Region "Microsoft.VisualBasic::8511283df0d8d874ddeaa337cb37fa38, ..\visualbasic_App\UXFramework\MetroUI Form\MetroUI Form\API\MoveScreen.vb"

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
