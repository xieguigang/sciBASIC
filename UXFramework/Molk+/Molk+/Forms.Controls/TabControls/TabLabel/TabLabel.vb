#Region "Microsoft.VisualBasic::327a59f24ddb29d01a6c6afd9ac3bfa4, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\TabLabel\TabLabel.vb"

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
Imports Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl

Namespace Windows.Forms.Controls.TabControl.TabLabel

    Public Class TabLabel : Inherits Windows.Forms.Controls.TabControl.ITabPage
        Implements ITabPage.ITabPageEx

        Public Event CloseAllTabs() Implements ITabPage.ITabPageEx.CloseAllTabs

        Public Event CloseAllTabsButThis() Implements ITabPage.ITabPageEx.CloseAllTabsButThis

        Public Event CloseTabPage() Implements ITabPage.ITabPageEx.CloseTabPage

        Public Event TabPageActive() Implements ITabPage.ITabPageEx.TabPageActive

        Public Event TabPageInactive() Implements ITabPage.ITabPageEx.TabPageInactive

        Dim _ControlUI As MolkPlusTheme.Visualise.Elements.Checkbox = New Visualise.Elements.Checkbox

        Public Overrides Sub SetActive(f As Boolean)
            Call MyBase.SetActive(f)
            Call ApplyUI()
        End Sub

        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
                Call DrawUI()
                Call ApplyUI()
            End Set
        End Property

        Sub New()

            ' 此调用是设计器所必需的。
            InitializeComponent()

            ' 在 InitializeComponent() 调用之后添加任何初始化。
            Text = "TabLabel"
        End Sub

        Public Overrides Sub ActiveTabPage()
            Call Me.ActiveTab()
            Call Me.SetActive(True)
        End Sub

        Private Sub ActiveTab()
            If Not _Active AndAlso Parent.Controls.Count > 0 Then
                Dim Query As Generic.IEnumerable(Of TabLabel) =
                    From ctl As Control In Parent.Controls
                    Where TypeOf ctl Is TabLabel
                    Select DirectCast(ctl, TabLabel)
                For Each TAB As TabLabel In Query.ToArray
                    Call TAB.SetActive(False)
                Next
            End If
        End Sub

        Private Sub DrawUI()
            Dim Image = New Drawing.Bitmap(Width - LabelHead.Width, Height)
            Dim gr As Graphics = Graphics.FromImage(Image)

            Call gr.FillRectangle(Brushes.CadetBlue, New Drawing.Rectangle(0, 0, Image.Width, Image.Height))
            Call gr.DrawString(MyBase.Text, New Font(YaHei, 10), Brushes.Black, New Point(x:=0, y:=0))

            _ControlUI.Check = Image

            Image = New Bitmap(Width - LabelHead.Width, Height)
            gr = Graphics.FromImage(Image)
            Call gr.FillRectangle(Brushes.White, New Drawing.Rectangle(0, 0, Image.Width, Image.Height))
            Call gr.DrawString(MyBase.Text, New Font(YaHei, 10), Brushes.Black, New Point(x:=0, y:=0))

            _ControlUI.UnCheck = Image
        End Sub

        Private Sub ApplyUI()
            If Active Then
                LabelBase.BackgroundImage = _ControlUI.Check
                LabelHead.BackgroundImage = My.Resources.LabelHead
            Else
                LabelBase.BackgroundImage = _ControlUI.UnCheck
                LabelHead.BackgroundImage = Nothing
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function

        Private Sub TabLabel_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            LabelBase.Width = Width - LabelHead.Width
            Call DrawUI()
            Call ApplyUI()
        End Sub

        Private Sub LabelBase_Click(sender As Object, e As EventArgs) Handles LabelBase.Click
            Call ActiveTabPage()
        End Sub
    End Class
End Namespace
