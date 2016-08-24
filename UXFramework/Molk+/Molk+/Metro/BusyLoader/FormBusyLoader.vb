#Region "Microsoft.VisualBasic::5252e051c78ec485d64a594ff3386333, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\BusyLoader\FormBusyLoader.vb"

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

Friend Class FormBusyLoader : Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Form

    'Public Overrides Property EnableFormMove As Boolean
    '    Get
    '        Return MyBase.EnableFormMove
    '    End Get
    '    Set(value As Boolean)
    '        MoveScreen.Enabled = value
    '        MyBase.EnableFormMove = value
    '    End Set
    'End Property

    Public Overrides Property BackColor As Drawing.Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Drawing.Color)
            MyBase.BackColor = value
            AjaxLoaderSquaresCircles1.BackColor = BackColor
        End Set
    End Property

    Public Property ButtonPromoting As String
        Get
            Return Button1.Text
        End Get
        Set(value As String)
            Button1.Text = value
        End Set
    End Property

    Public Property Message As String
        Get
            Return Label1.Text
        End Get
        Set(value As String)
            Label1.Text = value
        End Set
    End Property

    Public Sub JobOk(Message As String)
        Me.BackColor = Color.FromArgb(223, 242, 191)
        Me.Message = Message
        Me.AjaxLoaderSquaresCircles1.Visible = False

        Dim OkPic As New System.Windows.Forms.PictureBox

        Call Me.Controls.Add(OkPic)

        OkPic.BackColor = BackColor
        OkPic.BackgroundImage = My.Resources.OK
        OkPic.Location = AjaxLoaderSquaresCircles1.Location
        OkPic.Size = AjaxLoaderSquaresCircles1.Size
        OkPic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
    End Sub

    'Dim MoveScreen As API.MoveScreen = New API.MoveScreen(Label1, HookOn:=Me)
    Friend ProcessCompleted As IAsyncResult
    Friend Process As System.Action
    Friend CloseForm As Boolean = True
    Friend JobDoneHandle As Action

    Private Sub FormBusyLoader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim resBitmap As Bitmap = New Bitmap(Width, Height)
        Dim Gr As Graphics = Graphics.FromImage(resBitmap)
        Dim Pen As New Pen(Color.FromArgb(0, 82, 155))

        Call Gr.FillRectangle(New SolidBrush(BackColor), New Rectangle(New Point, Size))
        Call Gr.DrawRectangle(Pen, New Rectangle(New Point, New Size(Width - 1, Height - 1)))

        Me.BackgroundImage = resBitmap

        'MoveScreen.JoinHandle(Me.AjaxLoaderSquaresCircles1.PictureBox1)

        If Not Process Is Nothing Then
            ProcessCompleted = Process.BeginInvoke(Nothing, Nothing)
        End If

        Call New Threading.Thread(AddressOf InternalDetectJobDone).Start()
    End Sub

    Delegate Sub InvokeAction()

    Private Sub InternalDetectJobDone()
        Do While Not Process Is Nothing AndAlso Not ProcessCompleted.IsCompleted
            Call Threading.Thread.Sleep(10)
        Loop

        Try
            If CloseForm Then Call Me.Invoke(New InvokeAction(AddressOf Close)) Else Call Me.Invoke(New InvokeAction(Sub() Call Me.JobDoneHandle()))
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Catch ex As Exception
            Me.DialogResult = System.Windows.Forms.DialogResult.Abort
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Process = Nothing
        ProcessCompleted = Nothing
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
    End Sub
End Class
