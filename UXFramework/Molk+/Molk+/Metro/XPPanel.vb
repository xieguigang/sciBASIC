#Region "Microsoft.VisualBasic::91be7ebc10509cfb6b9c536af1e7aab1, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\XPPanel.vb"

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

Imports System.ComponentModel

Public Class XPPanel

    ''' <summary>
    ''' 在<see cref="panel2"/>之中容纳的控件，设置这个控件的大小会改变本控件的宽度
    ''' </summary>
    ''' <returns></returns>
    Public Property Control As Control
        Get
            Return _Control
        End Get
        Set(value As Control)
            If Not Control Is Nothing Then
                Call Me.Panel2.Controls.Remove(Control)
            End If

            _Control = value

            If Control Is Nothing Then
                _OldHeight = d
            Else
                Me.Panel2.Controls.Add(value)

                _OldHeight = _Control.Height

                '  Width = Control.Width + 4

                Control.Location = New Point(2, 2)

                Me.Height = Panel1.Height + _OldHeight + 1.5 * d
                Me.Panel2.Height = _OldHeight + 0.5 * d
            End If
        End Set
    End Property

    Dim d As Integer = 15

    Public Property Icon As Image
        Get
            Return PictureBox1.BackgroundImage
        End Get
        Set(value As Image)
            PictureBox1.BackgroundImage = value
        End Set
    End Property

    <Localizable(True)>
    Public Property LabelText As String
        Get
            Return Label1.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Label1.Text = value
        End Set
    End Property

    Public Property LabelForeColor As Color
        Get
            Return Label1.ForeColor
        End Get
        Set(value As Color)
            Label1.ForeColor = value
        End Set
    End Property

    <Localizable(True)>
    Public Property LabelFont As Font
        Get
            Return Label1.Font
        End Get
        Set(value As Font)
            Label1.Font = value
        End Set
    End Property

    Public Property Expanded As Boolean
        Get
            Return Checkbox1.Checked
        End Get
        Set(value As Boolean)
            Checkbox1.Checked = value
            Call ExpandInvoke(value)
        End Set
    End Property

    Dim _OldHeight As Decimal
    Dim _Control As Control

    Dim _DoInvokeAnimation As Boolean

    Public Sub StartCollapse()
        Call DoAnimation(Function(currentHeight As Integer) currentHeight > 0, -1, 0)
    End Sub

    Private Sub DoAnimation(condition As Func(Of Integer, Boolean), direction As Integer, Finall As Integer)
        If _DoInvokeAnimation Then
            Return
        Else
            _DoInvokeAnimation = True
        End If

        Dim currentHeight As Integer = Panel2.Height
        Dim speed As Integer = direction * Me.Speed

        Do While condition(currentHeight) AndAlso Not Disposing
            currentHeight += speed

            Panel2.Size = New Size(Panel2.Width, currentHeight)

            Me.Height = Panel1.Height + currentHeight + 2 + d

            Call Threading.Thread.Sleep(2)
            Call Application.DoEvents()
        Loop

        _DoInvokeAnimation = False
        Panel2.Height = Finall
    End Sub

    Public Sub StartExpand()
        Call DoAnimation(Function(currentHeight As Integer) currentHeight < _OldHeight, 1, _OldHeight + d)
    End Sub

    ''' <summary>
    ''' 值越大动画越快
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <DefaultValue(18)>
    Public Property Speed As Integer = 18

    Private Sub ExpandInvoke(Checked As Boolean) Handles Checkbox1.CheckStateChanged
        If Expanded = True Then
            '  Expanded = False
            Call New Threading.Thread(AddressOf StartCollapse).Start()
        Else
            '    Expanded = True
            Call New Threading.Thread(AddressOf StartExpand).Start()
        End If
    End Sub

    Public Property ArrowIcon As Visualise.Elements.Checkbox
        Get
            Return Checkbox1.UI
        End Get
        Set(value As Visualise.Elements.Checkbox)
            Checkbox1.UI = value
        End Set
    End Property

    Private Sub XPPanel_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim Normal = My.Resources.arrow_Expanded
        Dim Checked = My.Resources.arrow_Collapsed

        If Checkbox1.UI Is Nothing Then
            Checkbox1.UI = New Visualise.Elements.Checkbox With {
                .Check = Checked,
                .CheckPreLight = Checked,
                .Disable = Normal,
                .UnCheck = Normal,
                .UncheckPreLight = Normal,
                .CheckboxMargin = New Drawing.Rectangle(New Point, Normal.Size),
                .LabelMargin = 0,
                .BackgroundColor = Panel1.BackColor
            }
        End If

        Panel2.Location = New Point(2, Panel1.Height + 1)
    End Sub

    Private Sub Panel1_Resize(sender As Object, e As EventArgs) Handles Panel1.Resize
        Me.Checkbox1.Location = New Point(Width - Checkbox1.Width - 20, (Panel1.Height - Checkbox1.Height) / 2 - 2)
    End Sub
End Class

Public Class XPPanelVisual
    Public Property TitleBox As Color
    Public Property Panel As Color

    Public Shared Function XPDefault() As XPPanelVisual
        Return New XPPanelVisual With {.TitleBox = Color.FromArgb(32, 88, 200), .Panel = Color.FromArgb(239, 243, 255)}
    End Function

    Public Overrides Function ToString() As String
        Return $"{TitleBox.ToString} //  {Panel.ToString}"
    End Function
End Class
