Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging.Driver
Imports stdf = System.Math

' ============================================================================
'  DriverOutputs.vb - 多驱动输出冒烟测试
'
'  同一套图表代码分别用 GDI / SVG / PostScript / PDF 四种驱动出图，
'  用于人工目检 png / svg / ps / pdf 文件是否正常。
' ============================================================================

Public Module DriverOutputs

    ''' <summary>需要冒烟的四种输出驱动</summary>
    Private ReadOnly outputDrivers As Drivers() = {
        Drivers.GDI, Drivers.SVG, Drivers.PostScript, Drivers.PDF
    }

    Public Sub RunAll(outputDir As String)
        Call PlotDrivers.Register()
        Call Directory.CreateDirectory(outputDir)

        For Each driver As Drivers In outputDrivers
            Dim tag As String = driver.ToString.ToLower()

            Call Save(MakeScatter(driver), Path.Combine(outputDir, $"scatter-{tag}"), driver)
            Call Save(MakeLine(driver), Path.Combine(outputDir, $"line-{tag}"), driver)
            Call Save(MakeBar(driver), Path.Combine(outputDir, $"bar-{tag}"), driver)
            Call Save(MakeHistogram(driver), Path.Combine(outputDir, $"hist-{tag}"), driver)
            Call Save(MakeBubble(driver), Path.Combine(outputDir, $"bubble-{tag}"), driver)
            Call Save(MakeViolin(driver), Path.Combine(outputDir, $"violin-{tag}"), driver)
        Next

        Console.WriteLine("driver outputs saved to: " & outputDir)
    End Sub

    Private Sub Save(data As GraphicsData, path As String, driver As Drivers)
        Dim file As String = path & "." & ExtensionOf(driver)

        Call data.Save(file)
        Console.WriteLine($"  [{driver}] {file} ({New FileInfo(file).Length} bytes)")
    End Sub

    Private Function ExtensionOf(driver As Drivers) As String
        Select Case driver
            Case Drivers.SVG : Return "svg"
            Case Drivers.PostScript : Return "ps"
            Case Drivers.PDF : Return "pdf"
            Case Else : Return "png"
        End Select
    End Function

    Private Function SampleSeries() As Series()
        Dim rnd As New Random(42)

        Return {
            New Series With {
                .Name = "Group A",
                .X = Enumerable.Range(0, 40).Select(Function(i) CDbl(i)).ToArray(),
                .Y = Enumerable.Range(0, 40).Select(Function(i) i * 0.5 + rnd.NextDouble() * 5).ToArray(),
                .MarkerShape = MarkerShape.Circle
            },
            New Series With {
                .Name = "Group B",
                .X = Enumerable.Range(0, 40).Select(Function(i) CDbl(i)).ToArray(),
                .Y = Enumerable.Range(0, 40).Select(Function(i) i * 0.3 + 10 + rnd.NextDouble() * 5).ToArray(),
                .MarkerShape = MarkerShape.Triangle,
                .Color = Color.FromArgb(214, 39, 40)
            }
        }
    End Function

    ' ---------------- 散点图 ----------------
    Private Function MakeScatter(driver As Drivers) As GraphicsData
        Using plt As New ScatterPlot(1200, 800, PlotTheme.Nature())
            plt.Title = "Scatter Plot Demo"
            plt.SubTitle = "GDI / SVG / PostScript / PDF"
            plt.XLabel = "X value"
            plt.YLabel = "Y value"

            Return plt.Plot(SampleSeries().ToList(), New Size(1200, 800), 100, driver)
        End Using
    End Function

    ' ---------------- 折线图 ----------------
    Private Function MakeLine(driver As Drivers) As GraphicsData
        Dim rnd As New Random(7)
        Dim serials As Series() = {
            New Series With {
                .Name = "Method A",
                .X = Enumerable.Range(0, 30).Select(Function(i) CDbl(i)).ToArray(),
                .Y = Enumerable.Range(0, 30).Select(Function(i) stdf.Sin(i * 0.3) * 10 + 20 + rnd.NextDouble()).ToArray()
            },
            New Series With {
                .Name = "Method B",
                .X = Enumerable.Range(0, 30).Select(Function(i) CDbl(i)).ToArray(),
                .Y = Enumerable.Range(0, 30).Select(Function(i) stdf.Cos(i * 0.3) * 8 + 18 + rnd.NextDouble()).ToArray(),
                .Color = Color.FromArgb(214, 39, 40)
            }
        }

        Using plt As New LinePlot(1200, 800, PlotTheme.Science())
            plt.Title = "Line Plot Demo"
            plt.XLabel = "Time (s)"
            plt.YLabel = "Amplitude"

            Return plt.Plot(serials.ToList(), New Size(1200, 800), 100, driver)
        End Using
    End Function

    ' ---------------- 柱状图 ----------------
    Private Function MakeBar(driver As Drivers) As GraphicsData
        Using plt As New BarPlot(1200, 800, PlotTheme.Light())
            plt.Title = "Bar Plot Demo"
            plt.XLabel = "Category"
            plt.YLabel = "Count"
            plt.Categories = {"A", "B", "C", "D", "E"}
            plt.MultiValues = New Double(,) {
                {12, 25, 18, 22, 30},
                {8, 20, 15, 18, 25},
                {5, 12, 9, 14, 17}
            }
            plt.SeriesNames = {"2023", "2024", "2025"}

            Return plt.Plot(New Size(1200, 800), 100, driver)
        End Using
    End Function

    ' ---------------- 直方图 ----------------
    Private Function MakeHistogram(driver As Drivers) As GraphicsData
        Dim rnd As New Random(11)
        Dim data = Enumerable.Range(0, 800).Select(Function(i) rnd.NextDouble() * 100).ToArray()

        Using plt As New HistogramPlot(1200, 800, PlotTheme.Nature())
            plt.Title = "Histogram Demo"
            plt.XLabel = "Value"
            plt.YLabel = "Frequency"
            plt.Data = data
            plt.Bins = 30

            Return plt.Plot(New Size(1200, 800), 100, driver)
        End Using
    End Function

    ' ---------------- 气泡图 ----------------
    Private Function MakeBubble(driver As Drivers) As GraphicsData
        Dim rnd As New Random(3)

        Dim s As New BubbleSeries With {
            .Name = "Population",
            .X = Enumerable.Range(0, 30).Select(Function(i) rnd.NextDouble() * 100).ToArray(),
            .Y = Enumerable.Range(0, 30).Select(Function(i) rnd.NextDouble() * 100).ToArray(),
            .Sizes = Enumerable.Range(0, 30).Select(Function(i) rnd.NextDouble() * 80 + 5).ToArray(),
            .Color = Color.FromArgb(31, 119, 180)
        }

        Using plt As New BubblePlot(1200, 800, PlotTheme.Light())
            plt.Title = "Bubble Plot Demo"
            plt.XLabel = "GDP"
            plt.YLabel = "Life expectancy"

            Return plt.Plot({s}.ToList(), New Size(1200, 800), 100, driver)
        End Using
    End Function

    ' ---------------- 小提琴图 ----------------
    Private Function MakeViolin(driver As Drivers) As GraphicsData
        Dim rnd As New Random(5)
        Dim groups As New List(Of BoxGroup)()

        For i As Integer = 1 To 4
            Dim offset As Double = i * 2

            groups.Add(New BoxGroup With {
                .Name = $"Group {i}",
                .Data = Enumerable.Range(0, 200).Select(Function(k) rnd.NextDouble() * 10 + offset).ToArray()
            })
        Next

        Using plt As New ViolinPlot(1200, 800, PlotTheme.Nature())
            plt.Title = "Violin Plot Demo"
            plt.XLabel = "Group"
            plt.YLabel = "Value"
            plt.Groups = groups

            Return plt.Plot(New Size(1200, 800), 100, driver)
        End Using
    End Function
End Module
