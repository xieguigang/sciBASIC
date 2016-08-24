#Region "Microsoft.VisualBasic::6f1460c84e098cb7efd295561d9b1211, ..\visualbasic_App\UXFramework\Molk+\Molk+\Data\PagerNavigator.vb"

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

Imports Microsoft.VisualBasic.Imaging

Public Class PagerNavigator

    Dim _DataPager As IDataPager

    Public Property DataPager As IDataPager
        Get
            Return _DataPager
        End Get
        Set(value As IDataPager)
            _DataPager = value

            If Not value Is Nothing Then
                tbSummary.Text = value.GetDataSummary
            End If
        End Set
    End Property

    Private Sub btnFirstPage_ButtonClick() Handles btnFirstPage.ButtonClick
        Call DataPager.InvokeFirstPage()
        Call __updatePageNumbers(DataPager.GetCurrentPages(3))
    End Sub

    Private Sub __updatePageNumbers(value As String())
        cbOne.Text = value(Scan0)
        cbTwo.Text = value(1)
        cbThree.Text = value(2)
    End Sub

    Private Sub btnLastPage_ButtonClick() Handles btnLastPage.ButtonClick
        Call DataPager.InvokeLastPage()
        Call __updatePageNumbers(DataPager.GetCurrentPages(3))
    End Sub

    Private Sub btnNext_ButtonClick() Handles btnNext.ButtonClick
        Call DataPager.InvokeNextPage()
        Call __updatePageNumbers(DataPager.GetCurrentPages(3))
    End Sub

    Private Sub btnPrevious_ButtonClick() Handles btnPrevious.ButtonClick
        Call DataPager.InvokePreviousPage()
        Call __updatePageNumbers(DataPager.GetCurrentPages(3))
    End Sub

    Private Sub Checkbox1_CheckStateChanged(Checked As Boolean) Handles cbOne.CheckStateChanged
        cbTwo.Checked = False
        cbThree.Checked = False

        Call DataPager.InvokePage(cbOne.Text)
    End Sub

    Private Sub Checkbox2_CheckStateChanged(Checked As Boolean) Handles cbTwo.CheckStateChanged
        cbOne.Checked = False
        cbThree.Checked = False

        Call DataPager.InvokePage(cbTwo.Text)
    End Sub

    Private Sub Checkbox3_CheckStateChanged(Checked As Boolean) Handles cbThree.CheckStateChanged
        cbTwo.Checked = False
        cbOne.Checked = False

        Call DataPager.InvokePage(cbThree.Text)
    End Sub

    Private Sub PagerNavigator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim render As New PNButton
        Dim Size As New Size(24, 32)

        Call render.RenderButton(Me.btnFirstPage)
        Call render.RenderButton(Me.btnPrevious)
        Call render.RenderButton(Me.cbOne, Size)
        Call render.RenderButton(Me.cbTwo, Size)
        Call render.RenderButton(Me.cbThree, Size)
        Call render.RenderButton(Me.btnNext)
        Call render.RenderButton(Me.btnLastPage)

        Me.cbOne.RatioMode = True
        Me.cbTwo.RatioMode = True
        Me.cbThree.RatioMode = True

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles tbSummary.TextChanged

    End Sub
End Class

Public Class PNButton : Inherits Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonRender

    Sub New()
        Call MyBase.New(HighlightColor:=Color.FromArgb(213, 233, 254),
                         PressColor:=Color.FromArgb(213, 233, 254),
                         NormalColor:=Color.White,
                        TextColor:=Color.Black,
                         Font:=New Font(FontFace.MicrosoftYaHei, 9))
    End Sub

End Class
