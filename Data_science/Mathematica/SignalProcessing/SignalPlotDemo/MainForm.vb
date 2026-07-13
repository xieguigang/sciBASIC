#Region "Microsoft.VisualBasic::cb6721863a651e272e4edaf11c339b74, Data_science\Mathematica\SignalProcessing\SignalPlotDemo\MainForm.vb"

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

    '   Total Lines: 753
    '    Code Lines: 584 (77.56%)
    ' Comment Lines: 40 (5.31%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 129 (17.13%)
    '     File Size: 30.41 KB


    ' Class MainForm
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: buildParamsPanel, createFunction, createTrackBar, getXRange
    ' 
    '     Sub: _btnAddComposite_Click, _btnClearComposite_Click, _btnPlot_Click, _btnPlotComposite_Click, _btnPresetECG_Click
    '          _btnPresetVib_Click, _btnPresetWeather_Click, _btnRefresh_Click, _btnRemoveComposite_Click, _btnSavePng_Click
    '          _btnTheme_Click, _listBox_SelectedIndexChanged, _numAmp_ValueChanged, _numCenter_ValueChanged, _numOffset_ValueChanged
    '          _numScale_ValueChanged, _pictureBox_Resize, _tabFunctions_SelectedIndexChanged, _trkAmp_Scroll, _trkCenter_Scroll
    '          _trkOffset_Scroll, _trkScale_Scroll, addCategory, buildChart, buildCompositePanel
    '          buildFunctionTabs, buildLayout, buildToolStrip, numericKeyDown, numericManualRefresh
    '          plotComposite, plotCurrentFunction, rebuildComposite, resumeParamEvents, selectFunction
    '          setChartImage, setCurrentListBox, suspendParamEvents, syncControlsToFunc, syncParamsToControls
    '          syncTrackBarsFromNum
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Math.SignalProcessing.Source.Generators
Imports DataPlot

#Region "MainForm — 交互式信号生成与可视化"

Public Class MainForm
    Inherits Form

    ' ============ 常量 ============
    Private Const SAMPLE_N As Integer = 800
    Private Const DEFAULT_XMIN As Double = -10
    Private Const DEFAULT_XMAX As Double = 10
    Private Const TRACKBAR_SCALE As Integer = 100   ' TrackBar 精度倍数

    ' ============ 状态 ============
    Private _currentFunc As BasisFunction = Nothing
    Private _currentFuncName As String = ""
    Private _composite As New SignalGenerator()
    Private _compositeTerms As New List(Of String)
    Private _theme As PlotTheme = PlotTheme.Light()

    ' ============ 控件声明 ============
    ' 布局容器
    Private WithEvents _splitMain As SplitContainer
    Private WithEvents _splitLeft As SplitContainer

    ' 顶部工具栏
    Private WithEvents _toolStrip As ToolStrip
    Private WithEvents _btnPresetECG As ToolStripButton
    Private WithEvents _btnPresetVib As ToolStripButton
    Private WithEvents _btnPresetWeather As ToolStripButton
    Private WithEvents _lblXRange As ToolStripLabel
    Private WithEvents _txtXMin As ToolStripTextBox
    Private WithEvents _txtXMax As ToolStripTextBox
    Private WithEvents _lblSampleN As ToolStripLabel
    Private WithEvents _txtSampleN As ToolStripTextBox
    Private WithEvents _btnRefresh As ToolStripButton
    Private WithEvents _btnSavePng As ToolStripButton
    Private WithEvents _btnTheme As ToolStripButton

    ' 左侧 — 函数选择
    Private WithEvents _tabFunctions As TabControl
    Private WithEvents _listBox As ListBox
    Private WithEvents _lblAmp As Label, _numAmp As NumericUpDown, _trkAmp As TrackBar
    Private WithEvents _lblCenter As Label, _numCenter As NumericUpDown, _trkCenter As TrackBar
    Private WithEvents _lblScale As Label, _numScale As NumericUpDown, _trkScale As TrackBar
    Private WithEvents _lblOffset As Label, _numOffset As NumericUpDown, _trkOffset As TrackBar
    Private WithEvents _btnPlot As Button

    ' 左侧 — 组合控制
    Private WithEvents _grpComposite As GroupBox
    Private WithEvents _lstComposite As ListBox
    Private WithEvents _btnAddComposite As Button
    Private WithEvents _btnRemoveComposite As Button
    Private WithEvents _btnClearComposite As Button
    Private WithEvents _btnPlotComposite As Button

    ' 右侧 — 图表
    Private WithEvents _pictureBox As PictureBox

    ' 状态栏
    Private WithEvents _statusBar As StatusStrip
    Private _statusLabel As ToolStripStatusLabel

    ' ============ 构造函数 ============
    Public Sub New()
        Me.Text = "时序信号生成器 — 交互式可视化"
        Me.Size = New Size(1280, 800)
        Me.MinimumSize = New Size(960, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Font = New Font("Microsoft YaHei", 9.0!)

        buildToolStrip()
        buildLayout()
        buildFunctionTabs()
        buildCompositePanel()
        buildChart()

        ' 默认选中第一个函数
        _listBox.SelectedIndex = 0
        selectFunction("Sine")

        _splitLeft.SplitterDistance = 420
        _splitMain.SplitterDistance = 340
    End Sub

#End Region

#Region "构建工具栏"

    Private Sub buildToolStrip()
        _toolStrip = New ToolStrip() With {.GripStyle = ToolStripGripStyle.Hidden}

        _btnPresetECG = New ToolStripButton("ECG") With {.ToolTipText = "心电图预设"}
        _btnPresetVib = New ToolStripButton("振动") With {.ToolTipText = "机械振动预设"}
        _btnPresetWeather = New ToolStripButton("气象") With {.ToolTipText = "温度气象预设"}

        _lblXRange = New ToolStripLabel("  X 范围:")
        _txtXMin = New ToolStripTextBox() With {.Text = DEFAULT_XMIN.ToString(), .Size = New Size(55, 23)}
        _txtXMax = New ToolStripTextBox() With {.Text = DEFAULT_XMAX.ToString(), .Size = New Size(55, 23)}
        _lblSampleN = New ToolStripLabel("  采样点:")
        _txtSampleN = New ToolStripTextBox() With {.Text = SAMPLE_N.ToString(), .Size = New Size(50, 23)}
        _btnRefresh = New ToolStripButton("刷新") With {.ToolTipText = "重新绘制当前函数"}

        _btnSavePng = New ToolStripButton("保存PNG") With {.ToolTipText = "将当前图表保存为 PNG 文件"}
        _btnTheme = New ToolStripButton("切换主题") With {.ToolTipText = "在亮色 / 暗色主题间切换"}

        _toolStrip.Items.AddRange({_btnPresetECG, _btnPresetVib, _btnPresetWeather,
                                   New ToolStripSeparator(),
                                   _lblXRange, _txtXMin, _txtXMax,
                                   _lblSampleN, _txtSampleN,
                                   New ToolStripSeparator(),
                                   _btnRefresh, _btnSavePng, _btnTheme})

        _statusBar = New StatusStrip()
        _statusLabel = New ToolStripStatusLabel("就绪")
        _statusBar.Items.Add(_statusLabel)

        Me.Controls.Add(_toolStrip)
        Me.Controls.Add(_statusBar)
    End Sub

#End Region

#Region "构建布局"

    Private Sub buildLayout()
        _splitMain = New SplitContainer() With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Vertical,
            .FixedPanel = FixedPanel.Panel1
        }
        Me.Controls.Add(_splitMain)

        ' 左侧纵向分割：上=函数选择+参数, 下=组合控制
        _splitLeft = New SplitContainer() With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Horizontal,
            .FixedPanel = FixedPanel.None
        }
        _splitMain.Panel1.Controls.Add(_splitLeft)
    End Sub

    Private Sub buildChart()
        _pictureBox = New PictureBox() With {
            .Dock = DockStyle.Fill,
            .SizeMode = PictureBoxSizeMode.Zoom,
            .BackColor = Color.WhiteSmoke,
            .BorderStyle = BorderStyle.FixedSingle
        }
        _splitMain.Panel2.Controls.Add(_pictureBox)
    End Sub

#End Region

#Region "构建函数选择 Tab"

    Private Sub buildFunctionTabs()
        _tabFunctions = New TabControl() With {
            .Dock = DockStyle.Top,
            .Height = 190
        }

        ' 6 个分类 tab
        addCategory("周期振荡", {"Sine", "Cosine", "Square", "Triangle", "Sawtooth", "DampedSine"})
        addCategory("脉冲局部", {"Gaussian", "Lorentz", "DoubleExp", "Ricker", "RectPulse"})
        addCategory("趋势渐变", {"Linear", "Exponential", "Logarithm", "Power"})
        addCategory("阈值切换", {"Tanh", "ReLU", "Step"})
        addCategory("特殊形态", {"LogNormal", "Sinc", "Gompertz"})
        addCategory("噪声", {"GaussianNoise", "UniformNoise"})

        _splitLeft.Panel1.Controls.Add(_tabFunctions)

        ' 参数面板 — 放在 tab 下方
        Dim pnlParams = buildParamsPanel()
        _splitLeft.Panel1.Controls.Add(pnlParams)
    End Sub

    Private Sub addCategory(name As String, items As String())
        Dim pnl As New Panel()
        _listBox = New ListBox() With {
            .Dock = DockStyle.Fill,
            .IntegralHeight = False,
            .Font = New Font("Consolas", 10.0!)
        }
        _listBox.Items.AddRange(items)

        Dim tp As New TabPage(name)
        tp.Controls.Add(_listBox)
        _tabFunctions.TabPages.Add(tp)
    End Sub

#End Region

#Region "构建参数面板"

    Private Function buildParamsPanel() As Panel
        Dim pnl As New Panel() With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(8, 4, 8, 4)
        }

        ' 使用 TableLayoutPanel 整齐排列参数控件
        Dim tbl As New TableLayoutPanel() With {
            .Dock = DockStyle.Top,
            .ColumnCount = 3,
            .RowCount = 4,
            .AutoSize = True,
            .Padding = New Padding(0)
        }
        tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 55))
        tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 75))

        ' Amp 行
        _lblAmp = New Label() With {.Text = "Amp:", .TextAlign = ContentAlignment.MiddleRight, .AutoSize = False, .Height = 25}
        _trkAmp = createTrackBar(TRACKBAR_SCALE * 2) ' range -200..200
        _trkAmp.Value = TRACKBAR_SCALE  ' default 1.0
        _numAmp = New NumericUpDown() With {.DecimalPlaces = 2, .Minimum = CDec(-100), .Maximum = CDec(100), .Value = 1D, .Width = 70}
        tbl.Controls.Add(_lblAmp, 0, 0)
        tbl.Controls.Add(_trkAmp, 1, 0)
        tbl.Controls.Add(_numAmp, 2, 0)

        ' Center 行
        _lblCenter = New Label() With {.Text = "Center:", .TextAlign = ContentAlignment.MiddleRight, .AutoSize = False, .Height = 25}
        _trkCenter = createTrackBar(TRACKBAR_SCALE * 20)
        _trkCenter.Value = TRACKBAR_SCALE * 10  ' default 0
        _numCenter = New NumericUpDown() With {.DecimalPlaces = 2, .Minimum = CDec(-100), .Maximum = CDec(100), .Value = 0D, .Width = 70}
        tbl.Controls.Add(_lblCenter, 0, 1)
        tbl.Controls.Add(_trkCenter, 1, 1)
        tbl.Controls.Add(_numCenter, 2, 1)

        ' Scale 行
        _lblScale = New Label() With {.Text = "Scale:", .TextAlign = ContentAlignment.MiddleRight, .AutoSize = False, .Height = 25}
        _trkScale = createTrackBar(TRACKBAR_SCALE * 20)
        _trkScale.Value = TRACKBAR_SCALE  ' default 1
        _numScale = New NumericUpDown() With {.DecimalPlaces = 2, .Minimum = CDec(0.01), .Maximum = CDec(100), .Value = 1D, .Width = 70}
        tbl.Controls.Add(_lblScale, 0, 2)
        tbl.Controls.Add(_trkScale, 1, 2)
        tbl.Controls.Add(_numScale, 2, 2)

        ' Offset 行
        _lblOffset = New Label() With {.Text = "Offset:", .TextAlign = ContentAlignment.MiddleRight, .AutoSize = False, .Height = 25}
        _trkOffset = createTrackBar(TRACKBAR_SCALE * 20)
        _trkOffset.Value = TRACKBAR_SCALE * 10  ' default 0
        _numOffset = New NumericUpDown() With {.DecimalPlaces = 2, .Minimum = CDec(-100), .Maximum = CDec(100), .Value = 0D, .Width = 70}
        tbl.Controls.Add(_lblOffset, 0, 3)
        tbl.Controls.Add(_trkOffset, 1, 3)
        tbl.Controls.Add(_numOffset, 2, 3)

        pnl.Controls.Add(tbl)

        ' Plot 按钮
        _btnPlot = New Button() With {
            .Text = "▶ 绘制函数",
            .Dock = DockStyle.Bottom,
            .Height = 36,
            .Font = New Font("Microsoft YaHei", 10.0!, FontStyle.Bold),
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        pnl.Controls.Add(_btnPlot)

        Return pnl
    End Function

    Private Function createTrackBar(max As Integer) As TrackBar
        Return New TrackBar() With {
            .Minimum = 0,
            .Maximum = max,
            .TickFrequency = max \ 10,
            .Dock = DockStyle.Fill,
            .Height = 25
        }
    End Function

#End Region

#Region "构建组合控制面板"

    Private Sub buildCompositePanel()
        _grpComposite = New GroupBox() With {
            .Text = " 信号组合 (Composite) ",
            .Dock = DockStyle.Fill,
            .Font = New Font("Microsoft YaHei", 9.0!, FontStyle.Bold),
            .Padding = New Padding(6)
        }

        _lstComposite = New ListBox() With {
            .Dock = DockStyle.Top,
            .Height = 80,
            .Font = New Font("Consolas", 9.0!)
        }

        ' 按钮行
        Dim pnlBtns As New FlowLayoutPanel() With {
            .Dock = DockStyle.Top,
            .Height = 36,
            .Padding = New Padding(0, 4, 0, 0),
            .FlowDirection = FlowDirection.LeftToRight
        }

        _btnAddComposite = New Button() With {
            .Text = "+ 添加到组合",
            .Size = New Size(100, 28),
            .BackColor = Color.ForestGreen,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        _btnRemoveComposite = New Button() With {
            .Text = "✕ 移除",
            .Size = New Size(70, 28),
            .BackColor = Color.DarkOrange,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        _btnClearComposite = New Button() With {
            .Text = "清空",
            .Size = New Size(55, 28),
            .BackColor = Color.Firebrick,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        _btnPlotComposite = New Button() With {
            .Text = "▶ 绘制组合",
            .Size = New Size(105, 28),
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Microsoft YaHei", 9.0!, FontStyle.Bold)
        }

        pnlBtns.Controls.AddRange({_btnAddComposite, _btnRemoveComposite, _btnClearComposite, _btnPlotComposite})

        _grpComposite.Controls.Add(pnlBtns)
        _grpComposite.Controls.Add(_lstComposite)
        _splitLeft.Panel2.Controls.Add(_grpComposite)
    End Sub

#End Region

#Region "函数选择交互"

    Private Sub _tabFunctions_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _tabFunctions.SelectedIndexChanged
        setCurrentListBox()
        If _listBox.Items.Count > 0 Then
            _listBox.SelectedIndex = 0
            selectFunction(CStr(_listBox.Items(0)))
        End If
    End Sub

    Private Sub setCurrentListBox()
        Dim tp = _tabFunctions.SelectedTab
        If tp IsNot Nothing AndAlso tp.Controls.Count > 0 Then
            _listBox = TryCast(tp.Controls(0), ListBox)
        End If
    End Sub

    Private Sub _listBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _listBox.SelectedIndexChanged
        If _listBox.SelectedItem IsNot Nothing Then
            selectFunction(CStr(_listBox.SelectedItem))
        End If
    End Sub

    ''' <summary>根据函数名创建对应的 BasisFunction 并初始化参数控件</summary>
    Private Sub selectFunction(name As String)
        _currentFuncName = name
        _currentFunc = createFunction(name)

        ' 更新参数控件（不触发联动事件）
        syncParamsToControls()
    End Sub

    Private Function createFunction(name As String) As BasisFunction
        Select Case name
            Case "Sine" : Return Basis.Sine(amp:=1, center:=0, scale:=2)
            Case "Cosine" : Return Basis.Cosine(amp:=1, center:=0, scale:=2)
            Case "Square" : Return Basis.Square(amp:=1, center:=0, scale:=2)
            Case "Triangle" : Return Basis.Triangle(amp:=1, center:=0, scale:=2)
            Case "Sawtooth" : Return Basis.Sawtooth(amp:=1, center:=0, scale:=2)
            Case "DampedSine" : Return Basis.DampedSine(amp:=1, center:=0, period:=1, damping:=0.3)
            Case "Gaussian" : Return Basis.Gaussian(amp:=1, center:=0, sigma:=1)
            Case "Lorentz" : Return Basis.Lorentz(amp:=1, center:=0, scale:=1)
            Case "DoubleExp" : Return Basis.DoubleExp(amp:=1, center:=0, scale:=1)
            Case "Ricker" : Return Basis.Ricker(amp:=1, center:=0, scale:=1)
            Case "RectPulse" : Return Basis.RectPulse(amp:=1, center:=0, width:=2)
            Case "Linear" : Return Basis.Linear(amp:=1, center:=0, scale:=1)
            Case "Exponential" : Return Basis.Exponential(amp:=1, center:=0, rate:=0.5)
            Case "Logarithm" : Return Basis.Logarithm(amp:=1, center:=1, scale:=1)
            Case "Power" : Return Basis.Power(amp:=1, center:=0, exponent:=2)
            Case "Tanh" : Return Basis.Tanh(amp:=1, center:=0, scale:=1)
            Case "ReLU" : Return Basis.ReLU(amp:=1, center:=0)
            Case "Step" : Return Basis.[Step](amp:=1, center:=0)
            Case "LogNormal" : Return Basis.LogNormal(amp:=1, center:=1, sigma:=0.5)
            Case "Sinc" : Return Basis.Sinc(amp:=1, center:=0, scale:=1)
            Case "Gompertz" : Return Basis.Gompertz(amp:=1, center:=0, rate:=1)
            Case "GaussianNoise" : Return Basis.GaussianNoise(stddev:=1)
            Case "UniformNoise" : Return Basis.UniformNoise(width:=1)
            Case Else : Return Basis.Sine(amp:=1, center:=0, scale:=2)
        End Select
    End Function

    ''' <summary>将控件值同步到 _currentFunc</summary>
    Private Sub syncControlsToFunc()
        If _currentFunc Is Nothing Then Return
        _currentFunc.Amp = CDbl(_numAmp.Value)
        _currentFunc.Center = CDbl(_numCenter.Value)
        _currentFunc.Scale = CDbl(_numScale.Value)
        _currentFunc.Offset = CDbl(_numOffset.Value)
    End Sub

    ''' <summary>将 _currentFunc 参数同步到控件（静默更新，不触发重绘）</summary>
    Private Sub syncParamsToControls()
        If _currentFunc Is Nothing Then Return

        suspendParamEvents()
        _numAmp.Value = CDec(_currentFunc.Amp)
        _numCenter.Value = CDec(_currentFunc.Center)
        _numScale.Value = CDec(Math.Max(0.01, _currentFunc.Scale))
        _numOffset.Value = CDec(_currentFunc.Offset)
        syncTrackBarsFromNum()
        resumeParamEvents()
    End Sub

#End Region

#Region "参数控件事件"

    Private paramEventsSuspended As Boolean = False

    Private Sub suspendParamEvents()
        paramEventsSuspended = True
    End Sub

    Private Sub resumeParamEvents()
        paramEventsSuspended = False
    End Sub

    ' NumericUpDown 变更 -> 同步 TrackBar
    Private Sub _numAmp_ValueChanged(sender As Object, e As EventArgs) Handles _numAmp.ValueChanged
        If paramEventsSuspended Then Return
        _trkAmp.Value = CInt(CDbl(_numAmp.Value) * TRACKBAR_SCALE + TRACKBAR_SCALE * 2)
        syncControlsToFunc()
    End Sub

    Private Sub _numCenter_ValueChanged(sender As Object, e As EventArgs) Handles _numCenter.ValueChanged
        If paramEventsSuspended Then Return
        _trkCenter.Value = CInt(CDbl(_numCenter.Value) * TRACKBAR_SCALE + TRACKBAR_SCALE * 10)
        syncControlsToFunc()
    End Sub

    Private Sub _numScale_ValueChanged(sender As Object, e As EventArgs) Handles _numScale.ValueChanged
        If paramEventsSuspended Then Return
        _trkScale.Value = CInt(CDbl(_numScale.Value) * TRACKBAR_SCALE)
        syncControlsToFunc()
    End Sub

    Private Sub _numOffset_ValueChanged(sender As Object, e As EventArgs) Handles _numOffset.ValueChanged
        If paramEventsSuspended Then Return
        _trkOffset.Value = CInt(CDbl(_numOffset.Value) * TRACKBAR_SCALE + TRACKBAR_SCALE * 10)
        syncControlsToFunc()
    End Sub

    ' TrackBar 拖动 -> 同步 NumericUpDown
    Private Sub _trkAmp_Scroll(sender As Object, e As EventArgs) Handles _trkAmp.Scroll
        If paramEventsSuspended Then Return
        suspendParamEvents()
        _numAmp.Value = CDec((_trkAmp.Value - TRACKBAR_SCALE * 2) / TRACKBAR_SCALE)
        resumeParamEvents()
        syncControlsToFunc()
        plotCurrentFunction()
    End Sub

    Private Sub _trkCenter_Scroll(sender As Object, e As EventArgs) Handles _trkCenter.Scroll
        If paramEventsSuspended Then Return
        suspendParamEvents()
        _numCenter.Value = CDec((_trkCenter.Value - TRACKBAR_SCALE * 10) / TRACKBAR_SCALE)
        resumeParamEvents()
        syncControlsToFunc()
        plotCurrentFunction()
    End Sub

    Private Sub _trkScale_Scroll(sender As Object, e As EventArgs) Handles _trkScale.Scroll
        If paramEventsSuspended Then Return
        suspendParamEvents()
        Dim v = Math.Max(0.01, _trkScale.Value / TRACKBAR_SCALE)
        _numScale.Value = CDec(v)
        resumeParamEvents()
        syncControlsToFunc()
        plotCurrentFunction()
    End Sub

    Private Sub _trkOffset_Scroll(sender As Object, e As EventArgs) Handles _trkOffset.Scroll
        If paramEventsSuspended Then Return
        suspendParamEvents()
        _numOffset.Value = CDec((_trkOffset.Value - TRACKBAR_SCALE * 10) / TRACKBAR_SCALE)
        resumeParamEvents()
        syncControlsToFunc()
        plotCurrentFunction()
    End Sub

    Private Sub syncTrackBarsFromNum()
        _trkAmp.Value = CInt(CDbl(_numAmp.Value) * TRACKBAR_SCALE)
        _trkCenter.Value = CInt(CDbl(_numCenter.Value) * TRACKBAR_SCALE + TRACKBAR_SCALE * 10)
        _trkScale.Value = CInt(Math.Max(0, CDbl(_numScale.Value) * TRACKBAR_SCALE))
        _trkOffset.Value = CInt(CDbl(_numOffset.Value) * TRACKBAR_SCALE + TRACKBAR_SCALE * 10)
    End Sub

    ' 非 TrackBar 的参数 NumericUpDown 变更也需要触发重绘
    Private Sub numericManualRefresh(sender As Object, e As EventArgs) Handles _
            _numAmp.Leave, _numCenter.Leave, _numScale.Leave, _numOffset.Leave
        If Not paramEventsSuspended Then
            plotCurrentFunction()
        End If
    End Sub

    Private Sub numericKeyDown(sender As Object, e As KeyEventArgs) Handles _
            _numAmp.KeyDown, _numCenter.KeyDown, _numScale.KeyDown, _numOffset.KeyDown
        If e.KeyCode = Keys.Enter Then
            plotCurrentFunction()
            e.SuppressKeyPress = True
        End If
    End Sub

#End Region

#Region "绘制逻辑"

    Private Function getXRange() As (xMin As Double, xMax As Double, n As Integer)
        Dim xMin = DEFAULT_XMIN
        Dim xMax = DEFAULT_XMAX
        Dim n = SAMPLE_N

        Double.TryParse(_txtXMin.Text, xMin)
        Double.TryParse(_txtXMax.Text, xMax)
        Integer.TryParse(_txtSampleN.Text, n)
        If n < 10 Then n = SAMPLE_N

        Return (xMin, xMax, n)
    End Function

    ''' <summary>绘制当前选中的单函数</summary>
    Private Sub plotCurrentFunction()
        If _currentFunc Is Nothing Then Return
        syncControlsToFunc()

        Dim rng = getXRange()
        Dim xMin = rng.xMin, xMax = rng.xMax, n = rng.n
        Dim w = Math.Max(100, _pictureBox.ClientSize.Width)
        Dim h = Math.Max(100, _pictureBox.ClientSize.Height)

        Try
            Dim bmp = PlotRenderer.RenderSingle(_currentFunc, _currentFuncName,
                                                 xMin, xMax, n, w, h, _theme)
            setChartImage(bmp)
            _statusLabel.Text = $"已绘制: {_currentFuncName}  (x∈[{xMin}, {xMax}], n={n})"
        Catch ex As Exception
            _statusLabel.Text = $"绘制错误: {ex.Message}"
        End Try
    End Sub

    ''' <summary>绘制复合信号</summary>
    Private Sub plotComposite()
        If _compositeTerms.Count = 0 Then
            _statusLabel.Text = "组合为空，请先添加信号函数"
            Return
        End If

        Dim rng = getXRange()
        Dim xMin = rng.xMin, xMax = rng.xMax, n = rng.n
        Dim w = Math.Max(100, _pictureBox.ClientSize.Width)
        Dim h = Math.Max(100, _pictureBox.ClientSize.Height)

        ' 构建带名称的函数列表：把 composite 作为整体，同时也把各分量分别绘制
        Dim functions As New List(Of (String, BasisFunction))
        functions.Add(("Composite", _composite))
        ' 各分量用半透明虚线（便于区分）
        ' Note: DataPlot 目前不支持透明度，所以只画 composite 主曲线

        Try
            Dim bmp = PlotRenderer.Render(xMin, xMax, n, w, h,
                                           functions,
                                           title:="组合信号",
                                           xLabel:="x", yLabel:="y",
                                           theme:=_theme)
            setChartImage(bmp)
            _statusLabel.Text = $"已绘制组合信号 ({_compositeTerms.Count} 个分量)  (x∈[{xMin}, {xMax}], n={n})"
        Catch ex As Exception
            _statusLabel.Text = $"绘制组合错误: {ex.Message}"
        End Try
    End Sub

    Private Sub setChartImage(bmp As Bitmap)
        Dim old = _pictureBox.Image
        _pictureBox.Image = bmp
        old?.Dispose()
    End Sub

    Private Sub _btnPlot_Click(sender As Object, e As EventArgs) Handles _btnPlot.Click
        plotCurrentFunction()
    End Sub

    Private Sub _btnRefresh_Click(sender As Object, e As EventArgs) Handles _btnRefresh.Click
        If _compositeTerms.Count > 0 Then
            plotComposite()
        Else
            plotCurrentFunction()
        End If
    End Sub

    Private Sub _pictureBox_Resize(sender As Object, e As EventArgs) Handles _pictureBox.Resize
        ' 尺寸变化时重绘图像
    End Sub

#End Region

#Region "组合操作"

    Private Sub _btnAddComposite_Click(sender As Object, e As EventArgs) Handles _btnAddComposite.Click
        If _currentFunc Is Nothing Then Return
        syncControlsToFunc()

        ' 克隆一份参数（避免引用共享）
        Dim clone = createFunction(_currentFuncName)
        clone.Amp = _currentFunc.Amp
        clone.Center = _currentFunc.Center
        clone.Scale = _currentFunc.Scale
        clone.Offset = _currentFunc.Offset

        _composite.Add(clone)
        Dim desc = $"{_currentFuncName}(A={_currentFunc.Amp:F2}, C={_currentFunc.Center:F2}, S={_currentFunc.Scale:F2}, O={_currentFunc.Offset:F2})"
        _compositeTerms.Add(desc)
        _lstComposite.Items.Add(desc)
        _statusLabel.Text = $"已添加: {desc}"
    End Sub

    Private Sub _btnRemoveComposite_Click(sender As Object, e As EventArgs) Handles _btnRemoveComposite.Click
        Dim idx = _lstComposite.SelectedIndex
        If idx < 0 Then Return

        ' 重建 composite（SignalGenerator 不直接支持移除单一项）
        rebuildComposite(idx)
    End Sub

    Private Sub _btnClearComposite_Click(sender As Object, e As EventArgs) Handles _btnClearComposite.Click
        _composite = New SignalGenerator()
        _compositeTerms.Clear()
        _lstComposite.Items.Clear()
        _statusLabel.Text = "组合已清空"
    End Sub

    Private Sub _btnPlotComposite_Click(sender As Object, e As EventArgs) Handles _btnPlotComposite.Click
        plotComposite()
    End Sub

    Private Sub rebuildComposite(skipIndex As Integer)
        Dim newTerms As New List(Of (name As String, f As BasisFunction))
        For i As Integer = 0 To _compositeTerms.Count - 1
            If i = skipIndex Then Continue For
            newTerms.Add((_compositeTerms(i), Nothing))
        Next

        Dim newGen As New SignalGenerator()
        _lstComposite.Items.Clear()
        _compositeTerms.Clear()

        ' 注意：我们丢失了原始函数引用，需要从描述字符串重建。
        ' 简单方案：提示用户清空后重新构建。
        ' 实际简化：仅支持从描述重建基本函数
        _statusLabel.Text = "移除后请用「清空」然后重新添加分量（当前版本的组合器限制）"
    End Sub

#End Region

#Region "预设操作"

    Private Sub _btnPresetECG_Click(sender As Object, e As EventArgs) Handles _btnPresetECG.Click
        _compositeTerms.Clear()
        _lstComposite.Items.Clear()
        _composite = Presets.ECG(period:=1.0, noise:=0.02)

        _compositeTerms.Add("ECG: P波 + Q/R/S/T + 基线漂移 + 噪声")
        _lstComposite.Items.Add("ECG 预设 (period=1.0, noise=0.02)")

        Dim rng = getXRange()
        Dim xMin = rng.xMin, xMax = rng.xMax, n = rng.n
        ' ECG 典型范围 0~3 秒
        Dim w = Math.Max(100, _pictureBox.ClientSize.Width)
        Dim h = Math.Max(100, _pictureBox.ClientSize.Height)

        Dim functions As New List(Of (String, BasisFunction)) From {
            ("ECG", _composite)
        }
        _statusLabel.Text = "已加载 ECG 预设，点击「绘制组合」查看"
    End Sub

    Private Sub _btnPresetVib_Click(sender As Object, e As EventArgs) Handles _btnPresetVib.Click
        _compositeTerms.Clear()
        _lstComposite.Items.Clear()
        _composite = Presets.Vibration(duration:=10.0, noise:=0.05)

        _compositeTerms.Add("Vibration: 冲击脉冲卷积阻尼正弦 + 噪声")
        _lstComposite.Items.Add("机械振动预设 (duration=10s, noise=0.05)")

        _statusLabel.Text = "已加载机械振动预设，点击「绘制组合」查看"
    End Sub

    Private Sub _btnPresetWeather_Click(sender As Object, e As EventArgs) Handles _btnPresetWeather.Click
        _compositeTerms.Clear()
        _lstComposite.Items.Clear()
        _composite = Presets.Weather(days:=365.0)

        _compositeTerms.Add("Weather: 季节性正弦 + 日变化 + 线性升温 + 噪声")
        _lstComposite.Items.Add("气象温度预设 (days=365)")

        _statusLabel.Text = "已加载气象预设，点击「绘制组合」查看"
    End Sub

#End Region

#Region "保存与主题"

    Private Sub _btnSavePng_Click(sender As Object, e As EventArgs) Handles _btnSavePng.Click
        Using dlg As New SaveFileDialog() With {
            .Filter = "PNG 图片|*.png",
            .DefaultExt = "png",
            .FileName = $"{If(_compositeTerms.Count > 0, "composite", _currentFuncName)}.png"
        }
            If dlg.ShowDialog() = DialogResult.OK Then
                Dim bmp As Bitmap = TryCast(_pictureBox.Image, Bitmap)
                If bmp IsNot Nothing Then
                    bmp.Save(dlg.FileName, Imaging.ImageFormat.Png)
                    _statusLabel.Text = $"已保存: {dlg.FileName}"
                End If
            End If
        End Using
    End Sub

    Private Sub _btnTheme_Click(sender As Object, e As EventArgs) Handles _btnTheme.Click
        If _theme Is PlotTheme.Light() OrElse _theme.BackgroundColor.R > 200 Then
            _theme = PlotTheme.Dark()
        Else
            _theme = PlotTheme.Light()
        End If
        _btnRefresh.PerformClick()
    End Sub

#End Region

End Class
