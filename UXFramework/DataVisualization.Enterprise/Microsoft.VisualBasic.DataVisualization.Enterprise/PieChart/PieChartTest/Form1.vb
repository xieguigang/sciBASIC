#Region "Microsoft.VisualBasic::d8d5722f3393e6e6abd2a48d10825b3b, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartTest\Form1.vb"

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

''' Author:  Matthew Johnson
''' Version: 1.0
''' Date:    March 13, 2006
''' Notice:  You are free to use this code as you wish.  There are no guarantees whatsoever about
''' its usability or fitness of purpose.

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports System.Windows.Forms.Nexus
Imports System.Drawing.Imaging
Imports System.Drawing.Printing

Public Partial Class Form1
	Inherits Form
	Public Sub New()
		InitializeComponent()

		PieChart1.Items.Add(New PieChartItem(10, Color.BurlyWood, "Tan", "Tan tool tip", 0))
		PieChart1.Items.Add(New PieChartItem(10, Color.Gold, "Gold", "Gold tool tip", 0))
		PieChart1.Items.Add(New PieChartItem(10, Color.Chocolate, "Brown", "Brown tool tip", 50))
		PieChart1.Items.Add(New PieChartItem(20, Color.DarkRed, "Red", "Red tool tip", 0))


		PieChart1.ItemStyle.SurfaceAlphaTransparency = 0.75F
		PieChart1.FocusedItemStyle.SurfaceAlphaTransparency = 0.75F
		PieChart1.FocusedItemStyle.SurfaceBrightnessFactor = 0.3F

		cmbTextMode.Items.Add(PieChart.TextDisplayTypes.Always)
		cmbTextMode.Items.Add(PieChart.TextDisplayTypes.FitOnly)
		cmbTextMode.Items.Add(PieChart.TextDisplayTypes.Never)
		cmbTextMode.SelectedIndex = 0

		trkRotation.Value = CInt(Math.Truncate(PieChart1.Rotation * 180 / Math.PI))
		trkIncline.Value = CInt(Math.Truncate(PieChart1.Inclination * 180 / Math.PI))
		trkThickness.Value = CInt(Math.Truncate(PieChart1.Thickness))
		trkRadius.Value = CInt(Math.Truncate(PieChart1.Radius))

		trkEdgeBrightness.Value = CInt(Math.Truncate(PieChart1.ItemStyle.EdgeBrightnessFactor * 100))
		trkFocusedEdgeBrightness.Value = CInt(Math.Truncate(PieChart1.FocusedItemStyle.EdgeBrightnessFactor * 100))
		trkSurfaceBrightness.Value = CInt(Math.Truncate(PieChart1.ItemStyle.SurfaceBrightnessFactor * 100))
		trkFocusedSurfaceBrightness.Value = CInt(Math.Truncate(PieChart1.FocusedItemStyle.SurfaceBrightnessFactor * 100))
		trkSurfaceTransparency.Value = CInt(Math.Truncate(PieChart1.ItemStyle.SurfaceAlphaTransparency * 100))
		trkFocusedSurfaceTransparency.Value = CInt(Math.Truncate(PieChart1.FocusedItemStyle.SurfaceAlphaTransparency * 100))

		chkAutoSizeRadius.Checked = PieChart1.AutoSizePie
		chkShowEdges.Checked = PieChart1.ShowEdges
		chkShowToolTips.Checked = PieChart1.ShowToolTips

		cmbTextMode.SelectedItem = PieChart1.TextDisplayMode

		propertyGrid1.SelectedObject = New ItemCollectionProxy(PieChart1)
	End Sub

    Private Sub trkRotation_Scroll(sender As Object, e As EventArgs) Handles trkRotation.Scroll
        PieChart1.Rotation = CSng(trkRotation.Value * Math.PI / 180)
    End Sub

    Private Sub trkIncline_Scroll(sender As Object, e As EventArgs) Handles trkIncline.Scroll
        PieChart1.Inclination = CSng(trkIncline.Value * Math.PI / 180)
    End Sub

    Private Sub trkThickness_Scroll(sender As Object, e As EventArgs) Handles trkThickness.Scroll
        PieChart1.Thickness = trkThickness.Value
    End Sub

    Private Sub trkRadius_Scroll(sender As Object, e As EventArgs) Handles trkRadius.Scroll
        PieChart1.Radius = trkRadius.Value
    End Sub

    Private Sub PieChart1_AutoSizePieChanged(sender As Object, e As EventArgs) Handles PieChart1.AutoSizePieChanged
        trkRadius.Enabled = Not PieChart1.AutoSizePie
    End Sub

    Private Sub chkAutoSizeRadius_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoSizeRadius.CheckedChanged
        PieChart1.AutoSizePie = chkAutoSizeRadius.Checked
    End Sub

    Private Sub chkShowEdges_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowEdges.CheckedChanged
        PieChart1.ShowEdges = chkShowEdges.Checked
    End Sub

    Private Sub chkShowToolTips_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowToolTips.CheckedChanged
        PieChart1.ShowToolTips = chkShowToolTips.Checked
    End Sub

    Private Sub trkSurfaceTransparency_Scroll(sender As Object, e As EventArgs) Handles trkSurfaceTransparency.Scroll
        PieChart1.ItemStyle.SurfaceAlphaTransparency = CSng(trkSurfaceTransparency.Value) / 100
    End Sub

    Private Sub trkFocusedSurfaceTransparency_Scroll(sender As Object, e As EventArgs) Handles trkFocusedSurfaceTransparency.Scroll
        PieChart1.FocusedItemStyle.SurfaceAlphaTransparency = CSng(trkFocusedSurfaceTransparency.Value) / 100
    End Sub

    Private Sub trkEdgeBrightness_Scroll(sender As Object, e As EventArgs) Handles trkEdgeBrightness.Scroll
        PieChart1.ItemStyle.EdgeBrightnessFactor = CSng(trkEdgeBrightness.Value) / 100
    End Sub

    Private Sub trkFocusedEdgeBrightness_Scroll(sender As Object, e As EventArgs) Handles trkFocusedEdgeBrightness.Scroll
        PieChart1.FocusedItemStyle.EdgeBrightnessFactor = CSng(trkFocusedEdgeBrightness.Value) / 100
    End Sub

    Private Sub trkSurfaceBrightness_Scroll(sender As Object, e As EventArgs) Handles trkSurfaceBrightness.Scroll
        PieChart1.ItemStyle.SurfaceBrightnessFactor = CSng(trkSurfaceBrightness.Value) / 100
    End Sub

    Private Sub trkFocusedSurfaceBrightness_Scroll(sender As Object, e As EventArgs) Handles trkFocusedSurfaceBrightness.Scroll
        PieChart1.FocusedItemStyle.SurfaceBrightnessFactor = CSng(trkFocusedSurfaceBrightness.Value) / 100
    End Sub

    Private Sub cmbTextMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTextMode.SelectedIndexChanged
        If cmbTextMode.SelectedItem IsNot Nothing Then
            PieChart1.TextDisplayMode = CType(cmbTextMode.SelectedItem, PieChart.TextDisplayTypes)
        End If
    End Sub

    Private Sub OnItemFocusChanging(sender As Object, e As PieChart.PieChartItemFocusEventArgs) Handles PieChart1.ItemFocusChanging
        If e.NewItem IsNot Nothing AndAlso e.NewItem.Text IsNot Nothing Then
            Me.Text = "PieChartTest - " & e.NewItem.Text
        Else
            Me.Text = "PieChartTest - [no item focused]"
        End If
    End Sub

    Private Sub OnItemClicked(sender As Object, e As PieChart.PieChartItemEventArgs) Handles PieChart1.ItemClicked
        If e.Item.Tag Is Nothing OrElse CBool(e.Item.Tag) = True Then
            e.Item.Offset += 50
            e.Item.Tag = False
        Else
            e.Item.Offset = Math.Max(0, e.Item.Offset - 50)
            e.Item.Tag = True
        End If
    End Sub

    Private Sub mnuFileSaveAs_Click(sender As Object, e As EventArgs) Handles mnuFileSaveAs.Click
        Dim dlg As New SaveFileDialog()
        dlg.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|GIF Image|*.gif"

        Dim imageSize As Size
        If PieChart1.AutoSizePie Then
            imageSize = New Size(PieChart1.Bounds.Width, PieChart1.Bounds.Height)
        Else
            imageSize = PieChart1.GetChartSize(PieChart1.Padding)
        End If

        If dlg.ShowDialog() = DialogResult.OK Then
            Select Case dlg.FilterIndex
                Case 1
                    PieChart1.SaveAs(dlg.FileName, ImageFormat.Png, imageSize, PieChart1.Padding)
                    Exit Select
                Case 2
                    PieChart1.SaveAs(dlg.FileName, ImageFormat.Jpeg, imageSize, PieChart1.Padding)
                    Exit Select
                Case 3
                    PieChart1.SaveAs(dlg.FileName, ImageFormat.Bmp, imageSize, PieChart1.Padding)
                    Exit Select
                Case 4
                    PieChart1.SaveAs(dlg.FileName, ImageFormat.Gif, imageSize, PieChart1.Padding)
                    Exit Select
                Case Else
                    Throw New Exception("Unknown file filter.")
            End Select
        End If
    End Sub

    Private Sub mnuFilePrint_Click(sender As Object, e As EventArgs) Handles mnuFilePrint.Click
        Dim dlg As New PrintDialog()
        dlg.Document = New PrintDocument()
        PieChart1.AttachPrintDocument(dlg.Document)

        If dlg.ShowDialog() = DialogResult.OK Then
            dlg.Document.Print()
        End If
    End Sub

    Private Sub mnuFileExit_Click(sender As Object, e As EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub
End Class
