Imports System.Text
Imports System.Xml.Serialization
Imports std = System.Math
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯信念网络中的一个节点。节点持有一个离散随机变量，其取值域为 <c>0 .. Range - 1</c>。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 条件概率表 <see cref="CP_Table"/> 是一个二维数组，布局为 <c>(parentConfigRow, value)</c>：
    ''' 行索引是把所有父节点的取值按照 <see cref="Parents"/> 的声明顺序做混合进制展开的结果
    ''' (第一个父节点是最低位，见 <see cref="RowIndex"/>)；列索引就是本节点自己的取值。
    ''' </para>
    ''' <para>
    ''' 与旧实现相比这里有三处关键修正：父节点列表由未泛型的 <c>ArrayList</c> 改为 <c>List(Of BeliefNode)</c>；
    ''' <see cref="Range"/> 不再被硬编码为 2；证据值统一使用 <see cref="NotObserved"/>(-1)表示未观测，
    ''' 并且由 <see cref="BeliefNetwork.Observe(Integer())"/> 以合并的语义写入，不会互相覆盖。
    ''' </para>
    ''' </remarks>
    Public Class BeliefNode

        ''' <summary>
        ''' 表示「该节点没有观测到证据」的哨兵值
        ''' </summary>
        Public Const NotObserved As Integer = -1

        ''' <summary>
        ''' 本节点的父节点，顺序决定了条件概率表的行索引展开方式
        ''' </summary>
        Public Property Parents As List(Of BeliefNode)

        ''' <summary>
        ''' 条件概率表，布局为 <c>(parentConfigRow, value)</c>
        ''' </summary>
        Public Property CP_Table As Double(,)

        ''' <summary>
        ''' 本节点当前的证据值，<see cref="NotObserved"/> 表示未观测
        ''' </summary>
        Public Property Evidence As Integer

        Private ReadOnly _name As String
        Private ReadOnly _id As Integer
        Private ReadOnly _range As Integer
        Private _logCPT As Double() = Nothing

        Public Sub New(name As String, id As Integer, range As Integer)
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentException("The node name can not be empty.", NameOf(name))
            End If
            If range < 2 Then
                Throw New ArgumentOutOfRangeException(NameOf(range), $"The range of node '{name}' must be at least 2 but got {range}")
            End If

            _name = name
            _id = id
            _range = range
            Parents = New List(Of BeliefNode)
            Call Clear()
        End Sub

        ''' <summary>
        ''' 节点的名称，在同一个网络之中必须唯一
        ''' </summary>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>
        ''' 节点在所属网络之中的下标
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return _id
            End Get
        End Property

        ''' <summary>
        ''' 本节点随机变量的取值个数
        ''' </summary>
        Public ReadOnly Property Range As Integer
            Get
                Return _range
            End Get
        End Property

        ''' <summary>
        ''' 条件概率表的行数，即所有父节点取值组合的个数
        ''' </summary>
        Public ReadOnly Property RowCount As Integer
            Get
                If CP_Table Is Nothing Then
                    Return 0
                Else
                    Return CP_Table.GetLength(0)
                End If
            End Get
        End Property

        ''' <summary>
        ''' 是否观测到了证据
        ''' </summary>
        Public ReadOnly Property IsObserved As Boolean
            Get
                Return Evidence <> NotObserved
            End Get
        End Property

        ''' <summary>
        ''' 清除本节点的证据
        ''' </summary>
        Public Sub Clear()
            Evidence = NotObserved
        End Sub

        ''' <summary>
        ''' 写入本节点的证据值，同时检查取值范围
        ''' </summary>
        Public Sub Observe(value As Integer)
            If value = NotObserved Then
                Evidence = NotObserved
                Return
            End If

            DebugAudit.Require(value >= 0 AndAlso value < _range,
                               $"evidence value {value} is out of the range [0, {_range}) of node '{_name}'")

            Evidence = value
        End Sub

        ''' <summary>
        ''' 计算一组父节点取值所对应的条件概率表行下标。
        ''' </summary>
        ''' <param name="parentValues">按照 <see cref="Parents"/> 顺序排列的父节点取值</param>
        ''' <returns>混合进制展开之后的行下标</returns>
        ''' <remarks>
        ''' 展开方式与条件概率表的构造方式保持一致：<c>row = Σ value(k) · Π_(m&lt;k) Range(m)</c>，
        ''' 也就是说第一个父节点是最低位。
        ''' </remarks>
        Public Function RowIndex(parentValues As Integer()) As Integer
            If Parents.Count = 0 Then
                Return 0
            End If

            Dim index As Integer = 0
            Dim stride As Integer = 1

            For k As Integer = 0 To Parents.Count - 1
                Dim value As Integer = parentValues(k)

                DebugAudit.Check(value >= 0 AndAlso value < Parents(k).Range,
                                 $"the value {value} of parent '{Parents(k).Name}' is out of range when computing the CPT row of '{_name}'")

                index += value * stride
                stride *= Parents(k).Range
            Next

            Return index
        End Function

        ''' <summary>
        ''' 读取条件概率 <c>P(node = value | parents = parentValues)</c>
        ''' </summary>
        Public Function Probability(value As Integer, parentValues As Integer()) As Double
            Return CP_Table(RowIndex(parentValues), value)
        End Function

        ''' <summary>
        ''' 获取条件概率表的对数值(扁平化的行优先布局，下标为 <c>row * Range + value</c>)。
        ''' 结果会被缓存，直到 <see cref="InvalidateCache"/> 被调用。
        ''' </summary>
        Public Function LogCPT() As Double()
            If _logCPT IsNot Nothing Then
                Return _logCPT
            End If

            Dim rows As Integer = RowCount
            Dim values As Double() = New Double(rows * _range - 1) {}

            For r As Integer = 0 To rows - 1
                For v As Integer = 0 To _range - 1
                    Dim p As Double = CP_Table(r, v)
                    values(r * _range + v) = If(p <= 0, Double.NegativeInfinity, std.Log(p))
                Next
            Next

            _logCPT = values
            Return _logCPT
        End Function

        ''' <summary>
        ''' 使对数条件概率表的缓存失效，在修改了 <see cref="CP_Table"/> 之后必须调用
        ''' </summary>
        Public Sub InvalidateCache()
            _logCPT = Nothing
        End Sub

        ''' <summary>
        ''' 逐行把条件概率归一化，用于在参数学习或者手工写入之后修复数值误差
        ''' </summary>
        Public Sub NormalizeRows()
            Dim rows As Integer = RowCount

            For r As Integer = 0 To rows - 1
                Dim sum As Double = 0.0

                For v As Integer = 0 To _range - 1
                    Dim p As Double = CP_Table(r, v)

                    If p < 0 Then
                        CP_Table(r, v) = 0.0
                        p = 0.0
                    End If

                    sum += p
                Next

                If sum <= 0 Then
                    ' 退化情形：均匀分布
                    For v As Integer = 0 To _range - 1
                        CP_Table(r, v) = 1.0 / _range
                    Next
                Else
                    For v As Integer = 0 To _range - 1
                        CP_Table(r, v) /= sum
                    Next
                End If
            Next

            Call InvalidateCache()
        End Sub

        Public Overrides Function ToString() As String
            Return $"Name:={Name}; Id:={Id}; Range:={Range}; Parents:={If(Parents Is Nothing, 0, Parents.Count)}"
        End Function
    End Class

    ''' <summary>
    ''' 离散贝叶斯信念网络。
    ''' </summary>
    ''' <remarks>
    ''' 节点之间的依赖关系由 <see cref="BeliefNode.Parents"/> 给出，整个网络总是被当作有向无环图使用；
    ''' 由于节点在内部列表之中的顺序就是「父节点一定先于子节点」的拓扑序，所以构造阶段可以直接按顺序
    ''' 逐个解析父节点的名字。
    ''' </remarks>
    Public Class BeliefNetwork

        Protected Friend _bnNodes As List(Of BeliefNode) = New List(Of BeliefNode)

        ''' <summary>
        ''' 网络之中的全部节点，按照拓扑序排列
        ''' </summary>
        Public ReadOnly Property Nodes() As BeliefNode()
            Get
                Return _bnNodes.ToArray
            End Get
        End Property

        ''' <summary>
        ''' 节点的个数
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _bnNodes.Count
            End Get
        End Property

        ''' <summary>
        ''' 按照名字查找节点，找不到的时候返回 <c>Nothing</c>
        ''' </summary>
        Default Public ReadOnly Property Item(name As String) As BeliefNode
            Get
                Dim index As Integer = NodeIndex(name)
                Return If(index < 0, Nothing, _bnNodes(index))
            End Get
        End Property

        ''' <summary>
        ''' 按照名字查找节点的下标，找不到的时候返回 -1
        ''' </summary>
        Public Function NodeIndex(name As String) As Integer
            For i As Integer = 0 To _bnNodes.Count - 1
                If String.Equals(_bnNodes(i).Name, name, StringComparison.Ordinal) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 清除全部节点的证据
        ''' </summary>
        Public Sub ResetNodes()
            For Each node As BeliefNode In _bnNodes
                Call node.Clear()
            Next
        End Sub

        ''' <summary>
        ''' 清除全部节点的证据。语义与 <see cref="ResetNodes"/> 相同，命名更贴近推断代码的意图。
        ''' </summary>
        Public Sub ClearEvidence()
            Call ResetNodes()
        End Sub

        ''' <summary>
        ''' 整体设置全部节点的证据值。
        ''' </summary>
        ''' <param name="values">元素个数必须与节点的数目相等，取值 -1 表示该节点未观测</param>
        Public Sub SetNodes(values As Integer())
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If values.Length <> _bnNodes.Count Then
                Throw New ArgumentException($"expected {_bnNodes.Count} evidence values but got {values.Length}", NameOf(values))
            End If

            For i As Integer = 0 To _bnNodes.Count - 1
                Call _bnNodes(i).Observe(values(i))
            Next
        End Sub

        ''' <summary>
        ''' 以「合并」的语义写入证据：数组中取值为 -1 的位置不会清除该节点上已经存在的证据。
        ''' </summary>
        ''' <param name="values">元素个数必须与节点的数目相等</param>
        ''' <remarks>
        ''' 旧实现在 <c>BElim.GetBelief</c> 之中先写入条件、再写入查询变量，而查询变量数组里
        ''' 「错开」的位置是 -1，于是第二次写入把第一次写入的条件证据全部清掉了，这是旧版本
        ''' 结果不可用的根本原因。所有新的推断代码都必须使用本方法而不是 <see cref="SetNodes"/>。
        ''' </remarks>
        Public Sub Observe(values As Integer())
            If values Is Nothing Then
                Return
            End If

            DebugAudit.Require(values.Length = _bnNodes.Count,
                               $"expected {_bnNodes.Count} evidence values but got {values.Length}")

            For i As Integer = 0 To _bnNodes.Count - 1
                If values(i) <> BeliefNode.NotObserved Then
                    Call _bnNodes(i).Observe(values(i))
                End If
            Next
        End Sub

        ''' <summary>
        ''' 写入单个节点的证据
        ''' </summary>
        Public Sub Observe(nodeId As Integer, value As Integer)
            Call _bnNodes(nodeId).Observe(value)
        End Sub

        ''' <summary>
        ''' 导出当前全部的证据值
        ''' </summary>
        Public Function GetEvidence() As Integer()
            Dim values As Integer() = New Integer(_bnNodes.Count - 1) {}

            For i As Integer = 0 To _bnNodes.Count - 1
                values(i) = _bnNodes(i).Evidence
            Next

            Return values
        End Function

        ''' <summary>
        ''' 复制一份结构相同的网络(包含条件概率表的深拷贝)，证据状态不复制。
        ''' </summary>
        Public Function Clone() As BeliefNetwork
            Return NetworkInitializer.Build(Me.ToLayout(), New BeliefNetwork)
        End Function

        ''' <summary>
        ''' 把当前的网络导出为可序列化的布局对象
        ''' </summary>
        Public Function ToLayout() As NetworkLayout
            Dim nodes As NetworkLayout.BeliefNode() = New NetworkLayout.BeliefNode(_bnNodes.Count - 1) {}

            For i As Integer = 0 To _bnNodes.Count - 1
                Dim node As BeliefNode = _bnNodes(i)
                Dim columns As NetworkLayout.BeliefNode.CPTableF.CPColumn() = New NetworkLayout.BeliefNode.CPTableF.CPColumn(node.Range - 2) {}

                For v As Integer = 0 To node.Range - 2
                    Dim data As Double() = New Double(node.RowCount - 1) {}

                    For r As Integer = 0 To node.RowCount - 1
                        data(r) = node.CP_Table(r, v)
                    Next

                    columns(v) = New NetworkLayout.BeliefNode.CPTableF.CPColumn With {.Data = data}
                Next

                nodes(i) = New NetworkLayout.BeliefNode With {
                    .Name = node.Name,
                    .Range = node.Range,
                    .Parents = New NetworkLayout.BeliefNode.ParentList With {
                        .ParentNodes = node.Parents.Select(Function(p) p.Name).ToArray()
                    },
                    .CPTable = New NetworkLayout.BeliefNode.CPTableF With {.CPColumns = columns}
                }
            Next

            Return New NetworkLayout With {.Nodes = nodes}
        End Function

        ''' <summary>
        ''' 把网络布局保存为 XML 文件
        ''' </summary>
        Public Sub SaveXml(path As String)
            Call System.IO.File.WriteAllText(path, ToLayout().GetXml, Encoding.UTF8)
        End Sub

        ''' <summary>
        ''' 检查网络结构的一致性：节点名唯一、父节点已知、条件概率表归一化。
        ''' </summary>
        ''' <returns>存在问题时返回 <c>False</c>，并且通过 <see cref="DebugAudit"/> 记录失败原因</returns>
        Public Function Validate() As Boolean
            Dim ok As Boolean = True
            Dim names As New HashSet(Of String)(StringComparer.Ordinal)

            For i As Integer = 0 To _bnNodes.Count - 1
                Dim node As BeliefNode = _bnNodes(i)

                If Not names.Add(node.Name) Then
                    ok = DebugAudit.Test(False, $"duplicated node name '{node.Name}'") AndAlso ok
                End If

                If node.CP_Table Is Nothing Then
                    ok = DebugAudit.Test(False, $"node '{node.Name}' has no conditional probability table") AndAlso ok
                    Continue For
                End If

                Dim expectedRows As Integer = 1

                For Each parent As BeliefNode In node.Parents
                    expectedRows *= parent.Range

                    If parent.Id >= node.Id Then
                        ok = DebugAudit.Test(False, $"parent '{parent.Name}' of '{node.Name}' is not in topological order") AndAlso ok
                    End If
                Next

                If node.RowCount <> expectedRows Then
                    ok = DebugAudit.Test(False, $"node '{node.Name}' has {node.RowCount} CPT rows but {expectedRows} were expected") AndAlso ok
                    Continue For
                End If

                For r As Integer = 0 To node.RowCount - 1
                    Dim sum As Double = 0.0

                    For v As Integer = 0 To node.Range - 1
                        sum += node.CP_Table(r, v)
                    Next

                    If std.Abs(sum - 1.0) > 1.0E-06 Then
                        ok = DebugAudit.Test(False, $"the CPT row {r} of node '{node.Name}' sums to {sum} instead of 1") AndAlso ok
                    End If
                Next
            Next

            Return ok
        End Function

        ''' <summary>
        ''' 从 XML 文件之中加载网络
        ''' </summary>
        Public Shared Function CreateFrom(file As String) As BeliefNetwork
            Return NetworkInitializer.Build(file, New BeliefNetwork)
        End Function

        ''' <summary>
        ''' 从网络布局对象构建网络
        ''' </summary>
        Public Shared Function CreateFrom(layout As NetworkLayout) As BeliefNetwork
            Return NetworkInitializer.Build(layout, New BeliefNetwork)
        End Function

        ''' <summary>
        ''' 直接由节点表构建网络，主要用于参数学习与结构学习的结果装配
        ''' </summary>
        Public Shared Function CreateFrom(nodes As IEnumerable(Of BeliefNode)) As BeliefNetwork
            Dim network As New BeliefNetwork

            For Each node As BeliefNode In nodes
                node.Parents = node.Parents.ToList()
                Call network._bnNodes.Add(node)
            Next

            Return network
        End Function

        Public Overrides Function ToString() As String
            Return $"{_bnNodes.Count} nodes in the belief network"
        End Function

        ''' <summary>
        ''' 由序列化的网络布局构造 <see cref="BeliefNetwork"/> 实例
        ''' </summary>
        Protected Friend Class NetworkInitializer

            ''' <summary>
            ''' 条件概率表最后一个取值的归一化容差
            ''' </summary>
            ''' <remarks>
            ''' 旧代码写作 <c>Const EValue As Double = -10 ^ -10</c>，由于 VB 之中 <c>^</c> 是幂运算
            ''' 并且优先级高于一元负号，这个表达式的实际值是 -1E-10 而不是 -(10^-10) = -1E-10...
            ''' 虽然两者恰好相等，但写法极易误读，这里显式写成负的容差常量。
            ''' </remarks>
            Private Const SumTolerance As Double = -1.0E-10

            Public Shared Function Build(xmlfile As String, network As BeliefNetwork) As BeliefNetwork
                Dim layout As BeliefNetwork.NetworkLayout = xmlfile.LoadXml(Of BeliefNetwork.NetworkLayout)()
                Return Build(layout, network)
            End Function

            Public Shared Function Build(networkLayout As BeliefNetwork.NetworkLayout, network As BeliefNetwork) As BeliefNetwork
                If networkLayout Is Nothing OrElse networkLayout.Nodes.IsNullOrEmpty Then
                    Throw New ArgumentException("The network layout contains no node.", NameOf(networkLayout))
                End If

                For Each node As NetworkLayout.BeliefNode In networkLayout.Nodes
                    Call CreateNode(node, network)
                Next

                Call network.Validate()

                Return network
            End Function

            Private Shared Sub CreateNode(theNode As NetworkLayout.BeliefNode, network As BeliefNetwork)
                If network.NodeIndex(theNode.Name) >= 0 Then
                    Throw New ArgumentException($"duplicated node name '{theNode.Name}' in the network layout")
                End If

                Dim range As Integer = theNode.Range

                If range < 2 Then
                    Throw New ArgumentException($"the range of node '{theNode.Name}' must be at least 2 but got {range}")
                End If

                Dim nId As Integer = network._bnNodes.Count
                Dim newNode As New BeliefNode(theNode.Name, nId, range)

                ' 连接父节点：布局之中父节点必须已经创建(即布局本身已经是拓扑序)
                If theNode.Parents IsNot Nothing AndAlso theNode.Parents.ParentNodes IsNot Nothing Then
                    For Each parentName As String In theNode.Parents.ParentNodes
                        Dim parentId As Integer = network.NodeIndex(parentName)

                        If parentId < 0 Then
                            Throw New ArgumentException(
                                $"the parent node '{parentName}' of '{theNode.Name}' is not defined before it; " &
                                "the network layout must be sorted in topological order")
                        End If

                        Call newNode.Parents.Add(network._bnNodes(parentId))
                    Next
                End If

                ' 准备条件概率表
                Dim cpColumns As NetworkLayout.BeliefNode.CPTableF.CPColumn() =
                    If(theNode.CPTable Is Nothing, Nothing, theNode.CPTable.CPColumns)

                If cpColumns.IsNullOrEmpty Then
                    Throw New ArgumentException($"node '{theNode.Name}' has no conditional probability column")
                End If
                If cpColumns.Length <> range - 1 Then
                    Throw New ArgumentException(
                        $"CPT cols mismatch on node '{theNode.Name}': expected {range - 1} columns for range {range} but got {cpColumns.Length}")
                End If

                Dim tableRows As Integer = 1

                For Each parent As BeliefNode In newNode.Parents
                    tableRows *= parent.Range
                Next

                newNode.CP_Table = New Double(tableRows - 1, range - 1) {}

                For i As Integer = 0 To cpColumns.Length - 1
                    Dim column As NetworkLayout.BeliefNode.CPTableF.CPColumn = cpColumns(i)

                    If column Is Nothing OrElse column.Count <> tableRows Then
                        Throw New ArgumentException(
                            $"CPT Rows mismatch on node '{theNode.Name}': expected {tableRows} rows but got {If(column Is Nothing, 0, column.Count)}")
                    End If

                    For j As Integer = 0 To tableRows - 1
                        Dim p As Double = column(j)

                        If p < 0 OrElse p > 1 Then
                            Throw New ArgumentException($"the probability {p} in node '{theNode.Name}' is out of [0, 1]")
                        End If

                        newNode.CP_Table(j, i) = p
                    Next
                Next

                ' 最后一列由 Σ P = 1 的约束补全
                For i As Integer = 0 To tableRows - 1
                    Dim pr As Double = 1.0

                    For j As Integer = 0 To range - 2
                        pr -= newNode.CP_Table(i, j)
                    Next

                    DebugAudit.Check(pr >= SumTolerance,
                                     $"the condition probability of node '{theNode.Name}' does not normalize at row {i}, remainder={pr}")

                    newNode.CP_Table(i, range - 1) = std.Max(pr, 0.0)
                Next

                Call newNode.NormalizeRows()
                Call newNode.InvalidateCache()

                Call network._bnNodes.Add(newNode)
            End Sub
        End Class

        ''' <summary>
        ''' 贝叶斯信念网络的 XML 序列化布局。
        ''' </summary>
        ''' <remarks>
        ''' XML 根节点名、元素名以及 <see cref="NetworkLayout.BeliefNode.CPTableF.CPColumn.Data"/> 的扁平数组语义
        ''' 全部保持与旧版本完全一致，因此历史网络文件可以继续被读取。
        ''' </remarks>
        <XmlRoot("Microsoft.VisualBasic.DataVisualization.DataMining.Framework.BeliefNetwork.NetworkLayout")>
        Public Class NetworkLayout

            <XmlElement> Public Property Nodes As BeliefNode()

            Public Class BeliefNode

                <XmlAttribute> Public Property Name As String
                <XmlAttribute> Public Property Range As Integer
                Public Property Parents As ParentList
                Public Property CPTable As CPTableF

                Public Class ParentList

                    <XmlIgnore> Public ReadOnly Property Num As Integer
                        Get
                            If ParentNodes.IsNullOrEmpty Then
                                Return 0
                            Else
                                Return ParentNodes.Length
                            End If
                        End Get
                    End Property

                    <XmlElement> Public Property ParentNodes As String()

                    Public Overrides Function ToString() As String
                        Return String.Format("{0} parent nodes", Num)
                    End Function
                End Class

                Public Class CPTableF

                    <XmlElement> Public Property CPColumns As CPColumn()

                    Public Class CPColumn

                        <XmlAttribute> Public Property Data As Double()

                        Default Public ReadOnly Property Item(Idx As Integer) As Double
                            Get
                                Return Data(Idx)
                            End Get
                        End Property

                        Public ReadOnly Property Count As Integer
                            Get
                                If Data Is Nothing Then
                                    Return 0
                                Else
                                    Return Data.Length
                                End If
                            End Get
                        End Property

                        Public Overrides Function ToString() As String
                            Dim sBuilder As StringBuilder = New StringBuilder(1024)

                            If Data IsNot Nothing Then
                                For Each e As Double In Data
                                    Call sBuilder.Append(e & ", ")
                                Next

                                If sBuilder.Length >= 2 Then
                                    Call sBuilder.Remove(sBuilder.Length - 2, 2)
                                End If
                            End If

                            Return sBuilder.ToString
                        End Function
                    End Class
                End Class

                Public Overrides Function ToString() As String
                    Return String.Format("Named:={0}; Range:={1}", Name, Range)
                End Function
            End Class

            Public Overrides Function ToString() As String
                Return String.Format("{0} nodes in the belief network", If(Nodes Is Nothing, 0, Nodes.Length))
            End Function

            Public Shared Function Load(FilePath As String) As NetworkLayout
                Return FilePath.LoadXml(Of NetworkLayout)()
            End Function

            Public Shared Widening Operator CType(FilePath As String) As NetworkLayout
                Return FilePath.LoadXml(Of NetworkLayout)()
            End Operator

            ''' <summary>
            ''' 经典的 Cloudy / Sprinkler / Rain / WetGrass 网络，用于推断算法的数值对照测试
            ''' </summary>
            Public Shared Function TestData() As NetworkLayout
                Dim NodeList As List(Of BeliefNode) = New List(Of BeliefNode)

                Call NodeList.Add(New BeliefNode With {
                                  .Name = "Cloudy",
                                  .Range = 2,
                                  .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {}},
                                  .CPTable = New BeliefNode.CPTableF With {.CPColumns =
                                      New BeliefNode.CPTableF.CPColumn() {New BeliefNode.CPTableF.CPColumn() With {
                                              .Data = {0.3}}}}})
                Call NodeList.Add(New BeliefNode With {
                                  .Name = "Sprinkler",
                                  .Range = 2,
                                  .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Cloudy"}},
                                  .CPTable = New BeliefNode.CPTableF With {.CPColumns =
                                      New BeliefNode.CPTableF.CPColumn() {New BeliefNode.CPTableF.CPColumn() With {
                                              .Data = {0.5, 0.9}}}}})
                Call NodeList.Add(New BeliefNode With {
                                  .Name = "Rain",
                                  .Range = 2,
                                  .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Cloudy"}},
                                  .CPTable = New BeliefNode.CPTableF With {.CPColumns =
                                      New BeliefNode.CPTableF.CPColumn() {New BeliefNode.CPTableF.CPColumn() With {
                                              .Data = {0.8, 0.2}}}}})
                Call NodeList.Add(New BeliefNode With {
                                  .Name = "WetGrass",
                                  .Range = 2,
                                  .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Sprinkler", "Rain"}},
                                  .CPTable = New BeliefNode.CPTableF With {.CPColumns =
                                      New BeliefNode.CPTableF.CPColumn() {New BeliefNode.CPTableF.CPColumn() With {
                                              .Data = {1.0, 0.1, 0.1, 0.01}}}}})

                Return New NetworkLayout With {.Nodes = NodeList.ToArray}
            End Function
        End Class
    End Class
End Namespace
