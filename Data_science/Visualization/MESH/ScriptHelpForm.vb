Imports System.Text
Imports System.Windows.Forms

''' <summary>
''' 脚本语法帮助窗口（非模态）。以左侧目录树分类导航、右侧 HTML 渲染的方式，
''' 展示脚本模式的基本语法、绘图指令、内置函数与常量以及可运行示例。
''' </summary>
Public Class ScriptHelpForm
    Inherits Form

    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents WebBrowser1 As WebBrowser
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents SplitContainer1 As SplitContainer

    ''' <summary>目录节点 Tag -> HTML 正文</summary>
    Private ReadOnly helpTopics As New Dictionary(Of String, String)()

    Private ReadOnly css As String =
        "<style>" &
        "body{font-family:'Segoe UI','Microsoft YaHei',sans-serif;margin:14px;color:#222;line-height:1.6;}" &
        "h2{color:#1f6feb;border-bottom:1px solid #e1e4e8;padding-bottom:4px;margin-top:4px;}" &
        "h3{color:#0366d6;margin-bottom:4px;}" &
        "code{font-family:Consolas,'Courier New',monospace;background:#f4f6f8;padding:1px 4px;border-radius:3px;color:#b5188a;}" &
        "pre{font-family:Consolas,'Courier New',monospace;background:#f6f8fa;padding:10px;border:1px solid #e1e4e8;border-radius:4px;overflow:auto;white-space:pre;}" &
        "table{border-collapse:collapse;width:100%;margin:6px 0;}" &
        "th,td{border:1px solid #d0d7de;padding:5px 8px;text-align:left;vertical-align:top;}" &
        "th{background:#eef2f6;}" &
        "tr:nth-child(even) td{background:#fafbfc;}" &
        ".hint{background:#e7f3ff;border-left:4px solid #1f6feb;padding:6px 10px;margin:8px 0;}" &
        ".warn{background:#fff5e6;border-left:4px solid #d9822b;padding:6px 10px;margin:8px 0;}" &
        "</style>"

    Sub New()
        InitializeComponent()
        LoadHelpTopics()
        BuildTree()
        ' 默认显示概览
        If TreeView1.Nodes.Count > 0 Then
            TreeView1.SelectedNode = TreeView1.Nodes(0)
            ShowTopic(TreeView1.Nodes(0))
        End If
    End Sub

    ' ===================== 初始化 =====================

    Private Sub InitializeComponent()
        SplitContainer1 = New SplitContainer()
        TreeView1 = New TreeView()
        txtSearch = New TextBox()
        WebBrowser1 = New WebBrowser()
        StatusStrip1 = New StatusStrip()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.FixedPanel = FixedPanel.Panel1
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(TreeView1)
        SplitContainer1.Panel1.Controls.Add(txtSearch)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(WebBrowser1)
        SplitContainer1.Size = New Size(790, 490)
        SplitContainer1.SplitterDistance = 279
        SplitContainer1.TabIndex = 0
        ' 
        ' TreeView1
        ' 
        TreeView1.Dock = DockStyle.Fill
        TreeView1.Font = New Font("Microsoft YaHei", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        TreeView1.HideSelection = False
        TreeView1.Location = New Point(0, 23)
        TreeView1.Name = "TreeView1"
        TreeView1.Size = New Size(279, 467)
        TreeView1.TabIndex = 1
        ' 
        ' txtSearch
        ' 
        txtSearch.Dock = DockStyle.Top
        txtSearch.Location = New Point(0, 0)
        txtSearch.Name = "txtSearch"
        txtSearch.PlaceholderText = "搜索节点 / 函数名…"
        txtSearch.Size = New Size(279, 23)
        txtSearch.TabIndex = 0
        ' 
        ' WebBrowser1
        ' 
        WebBrowser1.Dock = DockStyle.Fill
        WebBrowser1.Location = New Point(0, 0)
        WebBrowser1.Name = "WebBrowser1"
        WebBrowser1.Size = New Size(507, 490)
        WebBrowser1.TabIndex = 2
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1})
        StatusStrip1.Location = New Point(0, 490)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(790, 22)
        StatusStrip1.TabIndex = 3
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(39, 17)
        ToolStripStatusLabel1.Text = "Ready"
        ' 
        ' ScriptHelpForm
        ' 
        ClientSize = New Size(790, 512)
        Controls.Add(SplitContainer1)
        Controls.Add(StatusStrip1)
        Name = "ScriptHelpForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "脚本语法帮助"
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel1.PerformLayout()
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    ' ===================== 目录树 =====================

    Private Sub BuildTree()
        TreeView1.Nodes.Clear()
        Dim n As TreeNode

        n = TreeView1.Nodes.Add("概览")
        n.Tag = "overview"

        Dim syn = TreeView1.Nodes.Add("语法")
        syn.Tag = "syntax"
        syn.Nodes.Add("变量赋值").Tag = "assign"
        syn.Nodes.Add("函数定义").Tag = "funcdef"
        syn.Nodes.Add("向量生成 axis").Tag = "axis"

        Dim plt = TreeView1.Nodes.Add("绘图指令")
        plt.Tag = "plot"
        plt.Nodes.Add("scatter 散点图").Tag = "scatter"
        plt.Nodes.Add("line 曲线图").Tag = "line"
        plt.Nodes.Add("surface 曲面图").Tag = "surface"

        Dim fns = TreeView1.Nodes.Add("内置函数")
        fns.Tag = "functions"
        fns.Nodes.Add("三角函数").Tag = "fn_trig"
        fns.Nodes.Add("指数与对数").Tag = "fn_log"
        fns.Nodes.Add("幂与开方").Tag = "fn_pow"
        fns.Nodes.Add("舍入与符号").Tag = "fn_round"
        fns.Nodes.Add("极值与杂项").Tag = "fn_misc"

        TreeView1.Nodes.Add("常量 PI / E").Tag = "const"

        Dim ex = TreeView1.Nodes.Add("示例脚本")
        ex.Tag = "examples"
        ex.Nodes.Add("二维曲线 / 散点").Tag = "ex_2d"
        ex.Nodes.Add("三维曲面").Tag = "ex_surface"
        ex.Nodes.Add("三维散点").Tag = "ex_scatter3"

        TreeView1.ExpandAll()
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        ShowTopic(e.Node)
    End Sub

    Private Sub ShowTopic(node As TreeNode)
        If node Is Nothing OrElse node.Tag Is Nothing Then Return
        Dim key = CStr(node.Tag)
        If helpTopics.ContainsKey(key) Then
            WebBrowser1.DocumentText = helpTopics(key)
        Else
            WebBrowser1.DocumentText = Html("<h2>" & Esc(node.Text) & "</h2><p>暂无内容。</p>")
        End If
        ToolStripStatusLabel1.Text = node.Text
    End Sub

    ' ===================== 搜索 =====================

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Dim q = txtSearch.Text.Trim().ToLower()
        If q.Length = 0 Then Return
        For Each node In AllNodes()
            If node.Text.ToLower().Contains(q) Then
                TreeView1.SelectedNode = node
                node.EnsureVisible()
                Return
            End If
        Next
    End Sub

    Private Iterator Function AllNodes() As IEnumerable(Of TreeNode)
        Dim stack As New Stack(Of TreeNode)()
        For Each n In TreeView1.Nodes : stack.Push(n) : Next
        While stack.Count > 0
            Dim cur = stack.Pop()
            Yield cur
            For Each c In cur.Nodes : stack.Push(c) : Next
        End While
    End Function

    ' ===================== HTML 辅助 =====================

    Private Function Html(body As String) As String
        Return "<html><head>" & css & "</head><body>" & body & "</body></html>"
    End Function

    Private Function Esc(s As String) As String
        If s Is Nothing Then Return ""
        Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
    End Function

    Private Function Code(s As String) As String
        Return "<pre><code>" & Esc(s) & "</code></pre>"
    End Function

    Private Function FnTable(rows() As String) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("<table>")
        sb.AppendLine("<tr><th>函数</th><th>说明</th></tr>")
        For Each r In rows
            sb.AppendLine(r)
        Next
        sb.AppendLine("</table>")
        Return sb.ToString()
    End Function

    Private Function Row(name As String, desc As String) As String
        Return "<tr><td><code>" & Esc(name) & "</code></td><td>" & Esc(desc) & "</td></tr>"
    End Function

    ' ===================== 帮助内容 =====================

    Private Sub LoadHelpTopics()
        helpTopics("overview") = Html(
            "<h2>脚本模式帮助</h2>" &
            "<p>本窗口汇总脚本模式的语法规则、绘图指令、内置数学函数与常量，并提供可运行示例。" &
            "编写脚本时保持本窗口打开，即可边写边查。</p>" &
            "<h3>脚本结构</h3>" &
            "<ul>" &
            "<li>每行书写一条语句，从上到下依次解析执行。</li>" &
            "<li>空行与以 <code>#</code> 或 <code>'</code> 开头的行作为注释被忽略。</li>" &
            "<li>解析顺序：函数定义 → 绘图指令 → 变量赋值（见下方说明）。</li>" &
            "</ul>" &
            "<div class='hint'>在脚本编辑器工具栏点击 <b>运行</b> 后，引擎逐行执行；所有绘图指令会汇总后交给主窗口渲染。</div>" &
            "<h3>变量类型</h3>" &
            "<p>变量可以是<strong>标量</strong>（单个数值）或<strong>向量</strong>（等间距数值数组）。" &
            "当赋值表达式引用了向量变量时，会对其<strong>逐元素</strong>求值得到同长度的新向量。</p>" &
            "<h3>出错提示</h3>" &
            "<p>若脚本有误，编辑器状态栏会显示 <code>第 N 行: 错误信息</code>，据此可快速定位问题行。</p>")

        helpTopics("syntax") = Html(
            "<h2>语法</h2>" &
            "<p>脚本由三类语句组成，点击左侧子节点查看每种语句的详细用法：</p>" &
            "<ul>" &
            "<li><b>变量赋值</b>：<code>var = 表达式</code></li>" &
            "<li><b>函数定义</b>：<code>f(参数) = 表达式</code></li>" &
            "<li><b>向量生成</b>：<code>v = axis(min, max, n=100)</code></li>" &
            "</ul>" &
            "<p>此外还有三类<strong>绘图指令</strong>（scatter / line / surface），详见'绘图指令'节点。</p>")

        helpTopics("assign") = Html(
            "<h2>变量赋值</h2>" &
            "<p>将一个表达式的结果保存到变量名，供后续语句引用：</p>" &
            Code("var = 表达式") &
            "<h3>右侧表达式可以是</h3>" &
            "<ul>" &
            "<li>标量常量或四则运算：<code>a = 2</code>、<code>b = 3.14 / 2</code></li>" &
            "<li>引用内置函数：<code>v = abs(x)</code></li>" &
            "<li>引用向量变量并逐元素计算：<code>w = x*x + sin(x)</code></li>" &
            "<li>函数调用：<code>z = myfunc(x, y)</code></li>" &
            "<li>axis 生成的向量：<code>t = axis(0, 10, n=200)</code></li>" &
            "</ul>" &
            "<div class='hint'>若表达式中含有向量变量，结果将是对每个元素独立计算得到的同长度向量。</div>")

        helpTopics("funcdef") = Html(
            "<h2>函数定义</h2>" &
            "<p>定义可复用的单/双参数函数，供绘图指令或表达式调用：</p>" &
            Code("name(参数1, 参数2) = 表达式") &
            "<h3>常见用法</h3>" &
            "<ul>" &
            "<li>单参数（用于 2D 曲线 / 散点）：<code>y(x) = sin(x)</code></li>" &
            "<li>双参数（用于 3D 曲面）：<code>z(x, y) = sin(sqrt(x*x + y*y))</code></li>" &
            "</ul>" &
            "<div class='hint'>函数体内部可调用内置数学函数，也可调用其它已定义的用户函数，支持嵌套组合。</div>" &
            "<div class='warn'>参数名必须是合法标识符（字母 / 下划线开头）。双参数函数用于 <code>surface</code> 时会自动在其 x、y 网格上求值。</div>")

        helpTopics("axis") = Html(
            "<h2>向量生成 axis</h2>" &
            "<p>生成从 <code>min</code> 到 <code>max</code> 的等间距数值向量，是构造坐标轴的核心指令：</p>" &
            Code("v = axis(min, max [, n=数量] [, step=步长])") &
            "<h3>参数</h3>" &
            FnTable({
                Row("min", "区间起点（必填）"),
                Row("max", "区间终点（必填）"),
                Row("n", "点数，默认 1000；生成 n 个等距点，含端点"),
                Row("step", "步长；与 n 二选一，同时给出时 n 优先")
            }) &
            "<h3>示例</h3>" &
            Code("# 80 个点的 -3..3 坐标轴" & vbCrLf &
                 "x = axis(-3, 3, n=80)" & vbCrLf &
                 "# 步长 0.1 的 0..10 坐标轴" & vbCrLf &
                 "t = axis(0, 10, step=0.1)") &
            "<div class='hint'>未指定 n 或 step 时，默认生成 1000 个点。</div>")

        helpTopics("plot") = Html(
            "<h2>绘图指令</h2>" &
            "<p>以下指令向主窗口输出图形。参数可以是向量变量、表达式（如 <code>sin(x)</code>）或函数调用。" &
            "点击左侧子节点查看每种指令的细节。</p>" &
            FnTable({
                Row("scatter(x, y)", "二维散点图"),
                Row("scatter(x, y, z)", "三维散点图"),
                Row("line(x, y)", "二维曲线图"),
                Row("line(x, y, z)", "三维曲线图"),
                Row("surface(x, y, z)", "三维曲面图")
            }))

        helpTopics("scatter") = Html(
            "<h2>scatter 散点图</h2>" &
            Code("scatter(x, y)          # 二维散点" & vbCrLf &
                 "scatter(x, y, z)       # 三维散点") &
            "<p><code>x</code>、<code>y</code>（以及 <code>z</code>）可为向量变量、表达式或函数调用，长度需一致。</p>" &
            "<h3>示例</h3>" &
            Code("x = axis(-2*PI, 2*PI, n=400)" & vbCrLf &
                 "scatter(x, sin(x))") &
            "<div class='hint'>三维散点：<code>scatter(x, y, x*x - y*y)</code>，其中 x、y 为等长向量。</div>")

        helpTopics("line") = Html(
            "<h2>line 曲线图</h2>" &
            Code("line(x, y)            # 二维曲线" & vbCrLf &
                 "line(x, y, z)         # 三维曲线") &
            "<p>参数要求与 scatter 一致，区别在于以连线方式绘制。</p>" &
            "<h3>示例</h3>" &
            Code("x = axis(-2*PI, 2*PI, n=400)" & vbCrLf &
                 "y(x) = sin(x)" & vbCrLf &
                 "line(x, y)"))

        helpTopics("surface") = Html(
            "<h2>surface 曲面图</h2>" &
            Code("surface(x, y, z)") &
            "<p>绘制以 <code>x</code>、<code>y</code> 为底面的三维曲面，<code>z</code> 支持两种形式：</p>" &
            "<ul>" &
            "<li><b>双参数函数</b>：<code>z(x, y) = 表达式</code>，引擎自动在 x×y 网格上求值。</li>" &
            "<li><b>向量</b>：长度必须等于 <code>x.Length * y.Length</code>，按行优先顺序重塑为网格。</li>" &
            "</ul>" &
            "<h3>示例（推荐：双参数函数）</h3>" &
            Code("x = axis(-3, 3, n=80)" & vbCrLf &
                 "y = axis(-3, 3, n=80)" & vbCrLf &
                 "z(x, y) = sin(sqrt(x*x + y*y))" & vbCrLf &
                 "surface(x, y, z)") &
            "<div class='warn'>若 z 是向量，必须保证其长度恰好为 len(x) * len(y)，否则会报错。</div>")

        helpTopics("functions") = Html(
            "<h2>内置函数</h2>" &
            "<p>表达式引擎内置以下数学函数，可在赋值、函数定义与绘图指令中直接调用（名称不区分大小写）：</p>" &
            "<ul>" &
            "<li><b>三角函数</b>：sin / cos / tan / asin / acos / atan / atan2 / sinh / cosh / tanh</li>" &
            "<li><b>指数与对数</b>：exp / ln / log / log10</li>" &
            "<li><b>幂与开方</b>：pow / sqrt</li>" &
            "<li><b>舍入与符号</b>：abs / ceiling / floor / round / truncate / sign / int</li>" &
            "<li><b>极值与杂项</b>：max / min / bigmul / ieeeremainder / rnd</li>" &
            "</ul>" &
            "<p>点击左侧子节点查看各类函数的参数与说明。</p>")

        helpTopics("fn_trig") = Html(
            "<h2>三角函数</h2>" &
            FnTable({
                Row("sin(x)", "正弦，x 以弧度为单位"),
                Row("cos(x)", "余弦"),
                Row("tan(x)", "正切"),
                Row("asin(x)", "反正弦，返回值范围 [-π/2, π/2]"),
                Row("acos(x)", "反余弦，返回值范围 [0, π]"),
                Row("atan(x)", "反正切，返回值范围 (-π/2, π/2)"),
                Row("atan2(y, x)", "四象限反正切，等价于点 (x,y) 的辐角"),
                Row("sinh(x)", "双曲正弦"),
                Row("cosh(x)", "双曲余弦"),
                Row("tanh(x)", "双曲正切")
            }))

        helpTopics("fn_log") = Html(
            "<h2>指数与对数</h2>" &
            FnTable({
                Row("exp(x)", "自然指数 e^x"),
                Row("ln(x)", "自然对数（以 e 为底）"),
                Row("log(x, base)", "以 base 为底的对数，例如 log(100, 10) = 2"),
                Row("log10(x)", "常用对数（以 10 为底）")
            }) &
            "<div class='hint'><code>log</code> 需要两个参数；若只写一个参数会被当作 x，缺少底数将报错，请显式给出底数。</div>")

        helpTopics("fn_pow") = Html(
            "<h2>幂与开方</h2>" &
            FnTable({
                Row("pow(x, y)", "幂运算 x^y"),
                Row("sqrt(x)", "平方根 √x")
            }))

        helpTopics("fn_round") = Html(
            "<h2>舍入与符号</h2>" &
            FnTable({
                Row("abs(x)", "绝对值 |x|"),
                Row("ceiling(x)", "向上取整，返回不小于 x 的最小整数"),
                Row("floor(x)", "向下取整，返回不大于 x 的最大整数"),
                Row("round(x)", "四舍五入到最接近的整数"),
                Row("truncate(x)", "截断小数部分，向零取整"),
                Row("sign(x)", "符号函数：x>0 返回 1，x<0 返回 -1，x=0 返回 0"),
                Row("int(x)", "转换为整数（截断取整）")
            }))

        helpTopics("fn_misc") = Html(
            "<h2>极值与杂项</h2>" &
            FnTable({
                Row("max(a, b)", "返回 a、b 中的较大值"),
                Row("min(a, b)", "返回 a、b 中的较小值"),
                Row("bigmul(a, b)", "返回 a、b 的 64 位整数乘积"),
                Row("ieeeremainder(a, b)", "按 IEEE 754 规范的余数 a mod b"),
                Row("rnd(a, b)", "返回 [a, b] 区间内的随机数")
            }))

        helpTopics("const") = Html(
            "<h2>常量 PI / E</h2>" &
            "<p>表达式引擎内置两个数学常量，可在表达式中直接使用：</p>" &
            FnTable({
                Row("PI", "圆周率 π ≈ 3.14159"),
                Row("E", "自然常数 e ≈ 2.71828")
            }) &
            "<h3>示例</h3>" &
            Code("x = axis(-2*PI, 2*PI, n=400)" & vbCrLf &
                 "y(x) = sin(x) + E"))

        helpTopics("examples") = Html(
            "<h2>示例脚本</h2>" &
            "<p>以下为完整可运行脚本，可复制到脚本编辑器后点击'运行'。点击左侧子节点查看具体分类。</p>")

        helpTopics("ex_2d") = Html(
            "<h2>二维曲线 / 散点示例</h2>" &
            Code("# 二维曲线：正弦与余弦" & vbCrLf &
                 "x = axis(-2*PI, 2*PI, n=400)" & vbCrLf &
                 "y1(x) = sin(x)" & vbCrLf &
                 "y2(x) = cos(x)" & vbCrLf &
                 "line(x, y1)" & vbCrLf &
                 "line(x, y2)" & vbCrLf &
                 vbCrLf &
                 "# 二维散点" & vbCrLf &
                 "scatter(x, sin(x))"))

        helpTopics("ex_surface") = Html(
            "<h2>三维曲面示例</h2>" &
            Code("# 正弦波纹曲面" & vbCrLf &
                 "x = axis(-3, 3, n=80)" & vbCrLf &
                 "y = axis(-3, 3, n=80)" & vbCrLf &
                 "z(x, y) = sin(sqrt(x*x + y*y))" & vbCrLf &
                 "surface(x, y, z)" & vbCrLf &
                 vbCrLf &
                 "# 抛物面（取消注释运行）" & vbCrLf &
                 "# z(x, y) = x*x + y*y" & vbCrLf &
                 "# surface(x, y, z)") &
            "<div class='hint'>更换 z 的表达式即可绘制不同曲面，例如鞍面 <code>x*x - y*y</code>、高斯钟形 <code>exp(-(x*x+y*y)/5)</code>。</div>")

        helpTopics("ex_scatter3") = Html(
            "<h2>三维散点示例</h2>" &
            Code("# 三维螺旋散点" & vbCrLf &
                 "t = axis(0, 10*PI, n=300)" & vbCrLf &
                 "x = cos(t) * t / 10" & vbCrLf &
                 "y = sin(t) * t / 10" & vbCrLf &
                 "z = t / 10" & vbCrLf &
                 "scatter(x, y, z)" & vbCrLf &
                 vbCrLf &
                 "# 球面散点（随机半径）" & vbCrLf &
                 "r = axis(0.2, 1, n=200)" & vbCrLf &
                 "theta = axis(0, 2*PI, n=200)" & vbCrLf &
                 "phi = axis(0, PI, n=200)" & vbCrLf &
                 "xs = r * sin(phi) * cos(theta)" & vbCrLf &
                 "ys = r * sin(phi) * sin(theta)" & vbCrLf &
                 "zs = r * cos(phi)" & vbCrLf &
                 "scatter(xs, ys, zs)"))
    End Sub

End Class
