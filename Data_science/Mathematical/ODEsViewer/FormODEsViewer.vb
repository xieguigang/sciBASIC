Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Linq
Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping

Public Class FormODEsViewer

    Dim model As Type

    Dim defines As New Dictionary(Of String, Double)
    Dim vars As New Dictionary(Of String, PictureBox)
    Dim inputs As New Dictionary(Of String, TextBox)

    Private Sub LoadModelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadModelToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "Application Module(*.dll)|*.dll|.NET Application(*.exe)|*.exe",
            .InitialDirectory = App.HOME
        }
            If file.ShowDialog = DialogResult.OK Then
                Using loader As New FormLoadModel() With {
                    .DllFile = file.FileName
                }
                    If loader.ShowDialog = DialogResult.OK Then
                        model = loader.Model
                        Text = file.FileName.ToFileURL & "!" & model.FullName

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
                                                      PictureBox1.BackgroundImage = DirectCast(picBox, PictureBox).BackgroundImage
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
                    End If
                End Using
            End If
        End Using
    End Sub

    Public Sub Draw(result As ODEsOut)
        ToolStripProgressBar1.Value = 15
        Application.DoEvents()

        Dim delta = 80 / result.y.Count

        For Each y As NamedValue(Of Double()) In result.y.Values
            Dim pts = result.x.SeqIterator.ToArray(Function(i) New PointF(i.obj, y.x(i.i)))
            Try
                vars(y.Name).BackgroundImage = Scatter.Plot(pts, title:=$"Plot({y.Name})", ptSize:=5)
            Catch ex As Exception
            Finally
                ToolStripProgressBar1.Value += delta
                Application.DoEvents()
            End Try
        Next

        ToolStripProgressBar1.Value = 100
    End Sub

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
                    inputs(x.Key).Text = CStr(x.Value)
                Next
            End If
        End Using
    End Sub

    Dim n%, a%, b%

    Private Sub ToolStripTextBox1_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox1.TextChanged
        n = CInt(Val(ToolStripTextBox1.Text))
        ToolStripLabel1.Text = "n:= " & n
    End Sub

    Private Sub ToolStripTextBox2_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox2.TextChanged
        a = CInt(Val(ToolStripTextBox2.Text))
        ToolStripLabel2.Text = "a:= " & a
    End Sub

    Private Sub ToolStripTextBox3_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox3.TextChanged
        b = CInt(Val(ToolStripTextBox3.Text))
        ToolStripLabel3.Text = "b:= " & b
    End Sub

    Private Sub FormODEsViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        ToolStripTextBox1.Text = "10000"
        ToolStripTextBox2.Text = "0"
        ToolStripTextBox3.Text = "10"
    End Sub
End Class