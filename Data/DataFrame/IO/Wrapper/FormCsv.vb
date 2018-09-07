#Region "Microsoft.VisualBasic::fa19970d8dd8077d5f33557cc7cd2829, Data\DataFrame\IO\Wrapper\FormCsv.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    ' Class CsvChartDevice
    ' 
    '     Sub: CloseToolStripMenuItem_Click, CreateSerial, Draw, EditToolStripMenuItem_Click, FormCsv_FormClosing
    '          FormCsv_Load, SaveToolStripMenuItem_Click
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows.Forms.DataVisualization.Charting

Public Class CsvChartDevice

    Public Data As Csv.DocumentStream.DataFrame
    Public Const Scan0 = 0

    Public Sub Draw(Csv As Csv.DocumentStream.DataFrame)
        Dim x, y As Double

        If Csv.Width = 2 Then
            _chart.ChartAreas("ChartArea1").AxisX.Title = Csv.SchemaOridinal.First.Key
            _chart.ChartAreas("ChartArea1").AxisY.Title = Csv.SchemaOridinal.Last.Key
        Else
            _chart.ChartAreas("ChartArea1").AxisX.Title = Csv.SchemaOridinal.First.Key
        End If

        For Each Column In Csv.SchemaOridinal  '第一个是X轴，则可以跳过去
            If Column.Value = 0 Then
                Continue For
            End If

            Call CreateSerial(Column.Key)
            Call Csv.Reset()

            Dim p As Integer = Column.Value

            Do While Csv.Read
                x = Val(Csv.GetValue(Scan0))
                y = Val(Csv.GetValue(p))

                _chart.Series(Column.Key).Points.AddXY(x, y)
            Loop

            _chart.Invalidate()
        Next

        Data = Csv
    End Sub

    ''' <summary>
    ''' 创建一条曲线
    ''' </summary>
    ''' <param name="name">曲线的名称</param>
    ''' <remarks></remarks>
    Private Sub CreateSerial(Name As String)
        'Create a Series

        Dim series1 As Series = New Series()
        series1.ChartArea = "ChartArea1"
        series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine

        ' Randomize()
        series1.Color = Color.Black   'colors(Rnd() * (colors.Count - 1))
        series1.BorderWidth = 3
        series1.Legend = "Legend1"
        series1.Name = Name
        _chart.Series.Add(series1)
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        Close()
    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click
        Dim File As String = SaveFileDialog("Bitmap(*.bmp)|*.bmp", Environment.GetFolderPath(Environment.SpecialFolder.Desktop))

        If Not String.IsNullOrEmpty(File) Then
            Dim BMP = New System.Drawing.Bitmap(_chart.Width, _chart.Height)

            Call _chart.DrawToBitmap(BMP, New Rectangle(New Point(x:=0, y:=0), _chart.Size))
            Call BMP.Save(File)
        End If
    End Sub

    Friend EditDialog As FormCustom, ShownDialog As Boolean = False

    Private Sub EditToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditToolStripMenuItem.Click
        If Not ShownDialog Then
            EditDialog = New FormCustom With {.Form = Me}
            Call New Threading.Thread(AddressOf EditDialog.ShowDialog).Start()
            ShownDialog = True
        Else
            EditDialog.Activate()
        End If
    End Sub

    Private Sub FormCsv_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not EditDialog Is Nothing Then
            EditDialog.Close()
        End If
    End Sub

    Private Sub FormCsv_Load(sender As Object, e As EventArgs) Handles Me.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub
End Class
