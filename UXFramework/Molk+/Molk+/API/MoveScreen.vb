#Region "Microsoft.VisualBasic::f74cc0d492df0880d4f0b417d0e692f0, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\MoveScreen.vb"

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

    Public Class MoveScreen : Implements System.IDisposable

        Dim UserCursor, FormLocation As Point

        Friend WithEvents TargetForm As System.Windows.Forms.Form
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
            If Not disposedValue AndAlso Enabled Then
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

        Sub New(Target As System.Windows.Forms.Control, HookOn As System.Windows.Forms.Form)
            Ctrl = Target
            TargetForm = HookOn
        End Sub

        Sub New(cpCtrl As Windows.Forms.Controls.Caption)
            Ctrl = cpCtrl
            TargetForm = cpCtrl.ParentForm
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
