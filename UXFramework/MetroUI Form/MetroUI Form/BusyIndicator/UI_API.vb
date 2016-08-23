#Region "Microsoft.VisualBasic::e2827c89b5e740e5d5dfad3f2e1270f2, ..\visualbasic_App\UXFramework\MetroUI Form\MetroUI Form\BusyIndicator\UI_API.vb"

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

    ''' <summary>
    ''' 工作完成之后对话框将会消失，中途取消的时候，任务还是会继续下去的
    ''' </summary>
    ''' <param name="Message"></param>
    ''' <param name="ProcessHandle"></param>
    ''' <param name="ButtonPromoting"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MessageBox(Message As String, ProcessHandle As Action, Optional ButtonPromoting As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader With {.Message = Message, .Process = ProcessHandle, .EnableFormMove = False}
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If

        Dim Result = bLoader.ShowDialog
        Return Result
    End Function

    Public Function MessageBox(Message As String, JobOkMessage As String, ProcessHandle As Action,
                               Optional ButtonPromoting As String = "",
                               Optional ButtonPromoting2 As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader With {.Message = Message, .Process = ProcessHandle, .EnableFormMove = False, .CloseForm = False}
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If
        Dim JobDoneHandle = Sub()
                                If Not String.IsNullOrEmpty(ButtonPromoting2) Then
                                    bLoader.ButtonPromoting = ButtonPromoting2
                                End If
                                Call bLoader.JobOk(JobOkMessage)
                            End Sub
        bLoader.JobDoneHandle = JobDoneHandle

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
    Public Function MessageBox(Message As String, ProcessHandle As Action, Mask As Form, Optional ButtonPromoting As String = "") As System.Windows.Forms.DialogResult
        Dim bLoader As New FormBusyLoader With {.Message = Message, .Process = ProcessHandle, .EnableFormMove = False}
        If Not String.IsNullOrEmpty(ButtonPromoting) Then
            bLoader.ButtonPromoting = ButtonPromoting
        End If

        Dim Result = New InternalMaskForm(Mask).ShowDialog(MaskFor:=bLoader)
        Return Result
    End Function

    Private Class InternalMaskForm : Inherits System.Windows.Forms.Form

        Sub New(MaskOn As Form)
            Call MyBase.New()
            BackColor = Drawing.Color.Black
            Opacity = 0.5
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
            Me.Location = MaskOn.Location
            Me.Size = MaskOn.Size
        End Sub

        Public Overloads Function ShowDialog(MaskFor As Form) As System.Windows.Forms.DialogResult
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
