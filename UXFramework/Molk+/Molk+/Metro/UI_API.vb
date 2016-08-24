#Region "Microsoft.VisualBasic::6c10930df9f8431cec7993ed774bd2d9, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\UI_API.vb"

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

Public Module UI_API

    Public Function MessageBox(Message As String, ProcessHandle As Action, Optional ButtonPromoting As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader With {.Message = Message, .Process = ProcessHandle, .EnableFormMove = False}
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If

        Dim Result = bLoader.ShowDialog
        Return Result
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Message"></param>
    ''' <param name="ProcessHandle"></param>
    ''' <param name="Mask">会形成黑影</param>
    ''' <param name="ButtonPromoting"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MessageBox(Message As String, ProcessHandle As Action, Mask As System.Windows.Forms.Form, Optional ButtonPromoting As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader With {.Message = Message, .Process = ProcessHandle, .EnableFormMove = False}
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If

        Dim Result = New InternalMaskForm(Mask).ShowDialog(MaskFor:=bLoader)
        Return Result
    End Function

    Private Class InternalMaskForm : Inherits System.Windows.Forms.Form

        Sub New(MaskOn As System.Windows.Forms.Form)
            Call MyBase.New()
            BackColor = Drawing.Color.Black
            Opacity = 0.5
            FormBorderStyle = Global.System.Windows.Forms.FormBorderStyle.None
            Me.Location = MaskOn.Location
            Me.Size = MaskOn.Size
        End Sub

        Public Overloads Function ShowDialog(MaskFor As System.Windows.Forms.Form) As System.Windows.Forms.DialogResult
            Call New Threading.Thread(AddressOf ShowDialog).Start()
            Call Threading.Thread.Sleep(100)
            Dim Result = MaskFor.ShowDialog()
            Close()
            Return Result
        End Function
    End Class

    Public Function MessageBoxJobOk(Message As String, Optional ButtonPromoting As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If
        Call bLoader.JobOk(Message)
        Return bLoader.ShowDialog
    End Function
End Module
