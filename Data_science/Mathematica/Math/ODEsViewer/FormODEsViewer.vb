#Region "Microsoft.VisualBasic::a3ea239d238695c4706389e0e5498349, sciBASIC#\Data_science\Mathematica\Math\ODEsViewer\FormODEsViewer.vb"

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


    ' Code Statistics:

    '   Total Lines: 314
    '    Code Lines: 255
    ' Comment Lines: 5
    '   Blank Lines: 54
    '     File Size: 12.35 KB


    ' Class FormODEsViewer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: __plot, LoadModel
    ' 
    '     Sub: AddReference, Draw, FormODEsViewer_Load, LoadParametersToolStripMenuItem_Click, OpenToolStripMenuItem_Click
    '          OpenToolStripMenuItem1_Click, SaveAsGAFInputsToolStripMenuItem_Click, SavePlotToolStripMenuItem_Click, SaveResultToolStripMenuItem_Click, ToolStripButton1_Click
    '          ToolStripTextBox1_TextChanged, ToolStripTextBox2_TextChanged, ToolStripTextBox3_TextChanged
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Windows.Forms

Public Class FormODEsViewer

    Dim model As Type

    Dim defines As New Dictionary(Of String, Double)
    Dim vars As New Dictionary(Of String, PictureBox)
    ''' <summary>
    ''' 需要拟合的参数列表
    ''' </summary>
    Dim inputs As New Dictionary(Of String, TextBox)
    Dim currentSelect As PictureBox
    Dim config As config = config.Load

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "Application Module(*.dll)|*.dll|.NET Application(*.exe)|*.exe"
        }
            If file.ShowDialog = DialogResult.OK Then
                If LoadModel(file.FileName) Then
                    Call config.models.AddFileHistory(file.FileName)
                    Call config.Save()

                    Call TextBox1.AppendText("Load dynamics model: " & file.FileName & vbCrLf)
                End If
            End If
        End Using
    End Sub

    Public Function LoadModel(path$) As Boolean
        Using loader As New FormLoadModel() With {
                  .DllFile = path
              }
            If loader.ShowDialog = DialogResult.OK Then
                model = loader.Model
                Text = path.ToFileURL & "!" & model.FullName

                For Each ctrl As Control In FlowLayoutPanel1.Controls
                    If ctrl.Equals(ToolStrip1) Then
                        Continue For
                    End If
                    Call FlowLayoutPanel1.Controls.Remove(ctrl)
                Next
                For Each ctrl As Control In FlowLayoutPanel2.Controls
                    Call FlowLayoutPanel2.Controls.Remove(ctrl)
                Next

                For Each x In vars.Values
                    Call Controls.Remove(x)
                    Call x.Dispose()
                Next

                For Each var$ In MonteCarlo.Model.GetVariables(model)
                    Dim pic As New PictureBox With {
                        .BackgroundImageLayout = ImageLayout.Zoom,
                        .Size = New Size(200, 100)
                    }
                    vars(var) = pic
                    FlowLayoutPanel2.Controls.Add(vars(var))

                    AddHandler pic.Click, Sub(picBox, arg)
                                              currentSelect = DirectCast(picBox, PictureBox)
                                              PictureBox1.BackgroundImage = currentSelect.BackgroundImage
                                          End Sub
                Next

                For Each var$ In MonteCarlo.Model.GetParameters(model) _
                    .Join(MonteCarlo.Model.GetVariables(model))

                    Dim lb As New Label With {
                        .Text = var & ": "
                    }
                    Dim text As New TextBox With {
                        .Name = var,
                        .Tag = lb
                    }

                    Call FlowLayoutPanel1.Controls.Add(lb)
                    Call FlowLayoutPanel1.Controls.Add(text)

                    defines(var) = 0
                    inputs(var) = text

                    AddHandler text.TextChanged, Sub(txt, args)
                                                     Dim txtBox = DirectCast(txt, TextBox)
                                                     defines(txtBox.Name) = Val(txtBox.Text)
                                                     DirectCast(txtBox.Tag, Label).Text = $"{txtBox.Name}:= {defines(txtBox.Name)}"
                                                 End Sub
                Next

                Return True
            End If
        End Using

        Return False
    End Function

    Public Sub Draw(result As ODEsOut)
        ToolStripProgressBar1.Value = 15
        Application.DoEvents()

        Dim delta = 80 / result.y.Count
        Dim plots As New List(Of (String, Image))
        Dim x = result.x.SeqIterator.ToArray

        For Each y As NamedValue(Of Double()) In result.y.Values
            Try
                Call plots.Add(__plot(x, y))
            Catch ex As Exception
                ex = New Exception(y.Name, ex)
                Call BeginInvoke(Sub() Call TextBox1.WriteLine(vbCrLf & ex.ToString & vbCrLf))
            End Try
        Next

        For Each y As (String, Image) In plots
            vars(y.Item1).BackgroundImage = y.Item2
            ToolStripProgressBar1.Value += delta
            Application.DoEvents()
        Next

        If Not currentSelect Is Nothing Then
            PictureBox1.BackgroundImage = currentSelect.BackgroundImage
        End If

        ToolStripProgressBar1.Value = 100
    End Sub

    Private Function __plot(x As SeqValue(Of Double)(), y As NamedValue(Of Double())) As (String, Image)
        Dim pts = x.ToArray(Function(i) New PointF(i.value, y.Value(i.i)))
        Dim hasRef = Not ref.IsNullOrEmpty AndAlso ref.ContainsKey(y.Name)
        Dim img As Image

        If hasRef Then
            Dim refS = Scatter.FromPoints(ref(y.Name).Value, "red", $"ReferenceOf({y.Name})", lineType:=DashStyle.Dash)
            Dim cal = Scatter.FromPoints(pts,, $"Plot({y.Name})")

            img = Scatter.Plot({refS, cal})
            Application.DoEvents()

            Call BeginInvoke(Sub() Call TextBox1.WriteLine("---> " & y.Name))
        Else
            img = Scatter.Plot(pts, title:=$"Plot({y.Name})", ptSize:=5)
            Application.DoEvents()

            Call BeginInvoke(Sub() Call TextBox1.WriteLine("---> " & y.Name))
        End If

        Return (y.Name, img)
    End Function

    Private Sub SaveResultToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveResultToolStripMenuItem.Click
        Using saveFile As New SaveFileDialog With {
            .Filter = "Excel(*.csv)|*.csv"
        }
            If saveFile.ShowDialog = DialogResult.OK Then
                Call MonteCarlo.Model.RunTest(model, defines, defines, n, a, b).DataFrame("#TIME").Save(saveFile.FileName, Encodings.ASCII)
            End If
        End Using
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        ToolStripProgressBar1.Value = 0
        TextBox1.AppendText($"a:={a}, b:={b}, n:={n}" & vbCrLf)
        TextBox1.AppendText(defines.GetJson & vbCrLf)
        Application.DoEvents()
        Call Draw(MonteCarlo.Model.RunTest(model, defines, defines, n, a, b))
    End Sub

    Private Sub LoadParametersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadParametersToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "Excel(*.csv)|*.csv"
        }
            If file.ShowDialog = DialogResult.OK Then
                defines = ODEsOut.LoadFromDataFrame(file.FileName).params

                For Each x In defines.ToArray
                    If inputs.ContainsKey(x.Key) Then
                        inputs(x.Key).Text = CStr(x.Value)
                    End If
                    Call App.JoinVariable(x.Key, x.Value)
                Next

                Call TextBox1.AppendText("Load parameter data from file: " & file.FileName & vbCrLf)
            End If
        End Using
    End Sub

    Dim n%, a%, b%

    Private Sub ToolStripTextBox1_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox1.TextChanged
        n = CInt(Val(ToolStripTextBox1.Text))
    End Sub

    Private Sub ToolStripTextBox2_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox2.TextChanged
        a = CInt(Val(ToolStripTextBox2.Text))
    End Sub

    Dim ref As Dictionary(Of NamedValue(Of PointF()))

    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。

    End Sub

    Private Sub OpenToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem1.Click
        Using file As New OpenFileDialog With {
            .Filter = "Excel(*.csv)|*.csv"
        }
            If file.ShowDialog = DialogResult.OK Then
                Call AddReference(file.FileName)
                Call config.references.AddFileHistory(file.FileName)
                Call config.Save()
            End If
        End Using
    End Sub

    Public Sub AddReference(path$)
        Call TextBox1.AppendText($"Using {path} as reference" & vbCrLf)

        Try
            With ODEsOut.LoadFromDataFrame(path, noVars:=True)
                Dim x#() = .x

                ref = .y _
                    .Select(
                    Function(y) New NamedValue(Of PointF()) With {
                        .Name = y.Key,
                        .Value = x.SeqIterator.ToArray(
                            Function(xi) New PointF(xi.value, y.Value.Value(xi)))
                    }).ToDictionary
            End With
        Catch ex As Exception
            With ODEsOut.LoadFromDataFrame(path, noVars:=False)
                Dim x#() = .x

                ref = .y _
                    .Select(
                    Function(y) New NamedValue(Of PointF()) With {
                        .Name = y.Key,
                        .Value = x.SeqIterator.ToArray(
                            Function(xi) New PointF(xi.value, y.Value.Value(xi)))
                    }).ToDictionary
            End With

            Call TextBox1.AppendText(ex.ToString)
        End Try
    End Sub

    Private Sub SaveAsGAFInputsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAsGAFInputsToolStripMenuItem.Click
        Using saveFile As New SaveFileDialog With {
            .Filter = "Excel tsv file(*.tsv)|*.tsv",
            .FileName = "estimates-parms.tsv"
        }
            If saveFile.ShowDialog = DialogResult.OK Then
                Dim params As String() = inputs _
                    .Select(Function(x) x.Key & "=" & x.Value.Text) _
                    .ToArray
                Dim out As String = {"0", "0", params.JoinBy(",")}.JoinBy(vbTab)

                Call out.SaveTo(saveFile.FileName, Encodings.ASCII.GetEncodings)
            End If
        End Using
    End Sub

    Private Sub SavePlotToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SavePlotToolStripMenuItem.Click
        If Not PictureBox1.BackgroundImage Is Nothing Then
            Using file As New SaveFileDialog With {.Filter = "Png image(*.png)|*.png"}
                If file.ShowDialog = DialogResult.OK Then
                    Call PictureBox1.BackgroundImage.SaveAs(file.FileName)
                End If
            End Using
        End If
    End Sub

    Private Sub ToolStripTextBox3_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox3.TextChanged
        b = CInt(Val(ToolStripTextBox3.Text))
    End Sub

    Private Sub FormODEsViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        ToolStripTextBox1.Text = "15000"
        ToolStripTextBox2.Text = "0"
        ToolStripTextBox3.Text = "15"

        Call LoadModelToolStripMenuItem.AddFilesHistory(
            config.models,
            Sub(path)
                LoadModel(path)
                config.models.AddFileHistory(path)
                config.Save()
            End Sub)
        Call AddReferenceToolStripMenuItem.AddFilesHistory(
            config.references,
            Sub(path)
                AddReference(path)
                config.references.AddFileHistory(path)
                config.Save()
            End Sub)
    End Sub
End Class
