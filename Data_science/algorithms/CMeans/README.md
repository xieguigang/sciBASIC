## Fuzzy c-means

> http://www.cnblogs.com/tianchi/archive/2013/03/06/2946114.html

在k-means中，每个元素只能属于所有类别中的一类。那这样会带来一些问题：

+ 所有的元素对于计算聚类中心的贡献都是相同的。

因为从根本上，对于属于一个类的所有元素来说，在``k-means``中是无法将他们区别开的（如果非要用距离什么的来区分也可以，但是这部分功能不是``k-mean``擅长的）。而在``fuzzy c-means``中，元素可能属于任何一类，不同的是它们之间的可能性是不同的。数学表示如下：

+ J<sub>m</sub> = ΣΣu<sub>ij</sub><sup>m</sup> × |x<sub>i</sub> - c<sub>i</sub>|<sup>2</sup>

其中：

|Element       |Description                                                             |
|--------------|------------------------------------------------------------------------|
|x<sub>i</sub> |元素                                                                    |
|c<sub>j</sub> |聚类中心                                                                 |
|u<sub>ij</sub>|元素 x<sub>i</sub> 对于聚类中心 c<sub>j</sub> 的隶属度（属于这个类的可能性）|
|m             |大于``1``的实数，一般取值``2.0``                                          |

J<sub>m</sub>用来评估聚类效果，J<sub>m</sub> 越大，聚类效果越差。那么聚类的过程其实就是找 J<sub>m</sub> 的极小值的过程。其实从函数的角度看，J<sub>m</sub> 取得极小值时偏导数为``0``，也就是说 u<sub>ij</sub> 和 c<sub>j</sub> 的变换都接近于``0``，而这里其实我们只需要考虑一个（比如在 u<sub>ij</sub> 趋于不变时通常 c<sub>j</sub> 也趋于稳定），而这里选择 u<sub>ij</sub> 的原因是衡量起来简单一点（取值范围为``[0,1]``，设置一个比较小的阀值即可）。

求极值是一个迭代的过程，更新聚类中心c<sub>j</sub>的方法与``k-means``非常相似，如下：

+ c<sub>j</sub> = (Σu<sub>ij</sub><sup>m</sup> × x<sub>i</sub>) / Σu<sub>ij</sub><sup>m</sup>

更新隶属度u<sub>ij</sub>的方法如下：

+ u<sub>ij</sub> = 1 / (∑((|x<sub>i</sub> - c<sub>j</sub>|/|x<sub>i</sub> - c<sub>k</sub>|)<sup>2 / (m - 1)</sup>))

那么迭代结束的条件显然是：

+ max{|u<sub>ij</sub><sup>k+1</sup> - u<sub>ij</sub><sup>k</sup>|} < ε

这样，``fuzzy c-means``的整体的过程如下：

1. 初始化隶属度矩阵；
2. 计算聚类中心``C``；
3. 更新隶属度矩阵``U``；
4. 如果 max{|u<sub>ij</sub><sup>k+1</sup> - u<sub>ij</sub><sup>k</sup>|} < ε 或者迭代次数达到上限，结束迭代，否则跳转到``2``；

注：不管是``k-means``还是``fuzzy c-means``，有没有感觉这个过程和迭代法求线性方程组的解的过程非常相似？其实有时候感觉这两个过程本来就是相同的。

### fuzzy c-means迭代式的推导

利用拉格朗日乘子法构造新的函数：

+ J<sub>m</sub> = ΣΣu<sub>ij</sub><sup>m</sup> × |x<sub>i</sub> - c<sub>i</sub>|<sup>2</sup> + λ × (Σu<sub>ij</sub> - 1)

在 J<sub>m</sub> 取得极值时满足如下条件：

+ ∂J / ∂λ = Σu<sub>ij</sub> - 1 = 0
+ ∂J / ∂u<sub>ij</sub> = m × u<sub>ij</sub><sup>m-1</sup> × |x<sub>i</sub> - c<sub>j</sub>|<sup>2</sup> - λ = 0
+ ∂J / ∂c<sub>j</sub> = Σu<sub>ij</sub><sup>m</sup> × x<sub>i</sub> - c<sub>j</sub> × Σu<sub>ij</sub><sup>m</sup> = 0

根据后面的两条即可得到u<sub>ij</sub>和c<sub>j</sub>的迭代式（想想在第二条中如何消掉``λ``？提示：利用∑u<sub>ij</sub> = 1）。


###### 1. Imports

```vbnet
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
```

###### 2. Fuzzy CMeans

```vbnet
Private Function CMeans() As (raw As Entity(), n%, trace As Dictionary(Of Integer, List(Of Entity)))
    Dim raw As New List(Of Entity)
    Dim rnd As New Random(Now.Millisecond)
    Dim up% = 1500

    ' initizlize of the points
    For i As Integer = 0 To 25
        Call raw.AddPoints(rnd, 50, up)
        up -= 50
    Next

    Dim n% = 10  ' required of 10 clusters
    Dim trace As New Dictionary(Of Integer, List(Of Entity))

    ' invoke cmeans cluster and gets the centra points
    Dim centras = raw.FuzzyCMeans(n, 2, trace:=trace)

#Region "DEBUG INFO OUTPUTS"
    For Each x In centras
        Call $"centra {x.uid} =>  {x.Properties.GetJson}".PrintException
    Next

    For Each x In raw
        Call ($"{x.uid}: {x.Properties.GetJson} => " & x.Memberships.GetJson).__DEBUG_ECHO
    Next
#End Region

    Return (raw, n, trace)
End Function
```

###### 3. Data Visualize

```vbnet
<Extension>
Private Sub Visualize(data As (raw As Entity(), n%, trace As Dictionary(Of Integer, List(Of Entity))))
    Dim trace As Dictionary(Of Integer, List(Of Entity)) = data.trace

    ' data plots visualize
    Dim plotData As New List(Of SerialData)
    ' using ColorBrewer color patterns from the visualbasic internal color designer
    Dim colors As Color() = Designer.GetColors("Paired:c10", data.n)

    ' generates serial data for each point in the raw inputs
    For Each x As Entity In data.raw
        Dim r = colors(x.ProbablyMembership).R
        Dim g = colors(x.ProbablyMembership).G
        Dim b = colors(x.ProbablyMembership).B
        Dim c As Color = Color.FromArgb(CInt(r), CInt(g), CInt(b))

        plotData += Scatter.FromPoints(
            {New PointF(x(0), x(1))},
            c.RGBExpression,
            ptSize:=30,
            title:="Point " & x.uid)
    Next

    Dim traceSerials As New List(Of List(Of Entity))

    For i As Integer = 0 To data.n - 1
        traceSerials += New List(Of Entity)
    Next

    For Each k In trace.Keys.OrderBy(Function(x) x)
        For i As Integer = 0 To data.n - 1
            traceSerials(i) += trace(k)(i)
        Next
    Next

    ' generates the serial data for each centra points
    For i = 0 To data.n - 1
        Dim points As IEnumerable(Of PointF) =
            traceSerials(i) _
            .Select(Function(x) New PointF(x(0), x(1)))
        plotData += Scatter.FromPoints(
            points,
            colors(i).RGBExpression,
            ptSize:=10,
            title:="Cluster " & i)
        plotData.Last.AddMarker(
            plotData.Last.pts.Last.pt.X,
            "Cluster " & i,
            "red",
            style:=LegendStyles.Triangle)
    Next

    Call Scatter.Plot(plotData, New Size(5000, 3000), fillPie:=True, showLegend:=False) _
        .SaveAs("./CMeans.png")
End Sub
```

###### 4. Output Image

![](./CMeans.png)
