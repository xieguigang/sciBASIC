Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports RfLeafCriterion = Microsoft.VisualBasic.MachineLearning.RandomForests.LF_c

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 随机森林回归的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 森林的预测只依赖每条分支的 6 个字段：
    ''' <c>status</c>（是否为叶）、<c>Feature</c>、<c>mean_snp</c>（分裂阈值）、
    ''' <c>Child1</c> / <c>Child2</c>（同一数组内的下标）与 <c>mean</c>（叶值）。
    ''' 因此 CSV 采用「一行一个节点」的扁平长表：
    ''' </para>
    ''' <code>
    ''' tree,node,status,feature,threshold,child1,child2,mean,class_val
    ''' </code>
    ''' <para>
    ''' 行序不做保证，加载时按 <c>(tree, node)</c> 回填，因此文件可以随意排序、
    ''' 也可以被外部工具筛掉不关心的树。训练期的行下标 <c>Branch.list</c>
    ''' 不参与预测，不予持久化。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RandomForestCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "random-forest"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is RandomForestRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As RandomForestRegressor = DirectCast(model, RandomForestRegressor)
            Dim fitted As RanFog = adapter.FittedModel

            If fitted Is Nothing OrElse fitted.Trees Is Nothing Then
                Throw New InvalidOperationException("the random forest adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("regressor", "base", AlgorithmName)

            manifest.featureWidth = adapter.FeatureWidth
            manifest.SetInteger("max_tree", fitted.max_tree)
            manifest.SetInteger("max_branch", fitted.max_branch)
            manifest.SetInteger("mtry", fitted.mtry)
            manifest.SetParameter("splitCriterion", fitted.LF_c.ToString())
            manifest.SetScalar("falsePositiveCost", fitted.false_positive_cost)
            manifest.SetScalar("falseNegativeCost", fitted.false_negative_cost)
            manifest.SetInteger("treeCount", fitted.Trees.Count)

            If fitted.VI IsNot Nothing AndAlso fitted.VI.Length > 0 Then
                manifest.SetVector("VI", fitted.VI)
            End If
            If fitted.Selected IsNot Nothing AndAlso fitted.Selected.Length > 0 Then
                manifest.SetIntegerVector("Selected", fitted.Selected)
            End If

            Call bundle.WriteCsv(manifest, "trees", folder,
                                 New String() {"tree", "node", "status", "feature", "threshold", "child1", "child2", "mean", "class_val"},
                                 EnumerateTrees(fitted))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim table As CsvTable = bundle.ReadCsv(manifest, "trees")
            Dim treeIds As Integer() = table.Integers("tree", "random-forest/trees")
            Dim nodeIds As Integer() = table.Integers("node", "random-forest/trees")
            Dim statuses As String() = table.Texts("status")
            Dim features As Integer() = table.Integers("feature", "random-forest/trees")
            Dim thresholds As Double() = table.Doubles("threshold", "random-forest/trees")
            Dim child1 As Integer() = table.Integers("child1", "random-forest/trees")
            Dim child2 As Integer() = table.Integers("child2", "random-forest/trees")
            Dim means As Double() = table.Doubles("mean", "random-forest/trees")
            Dim classValues As Integer() = table.Integers("class_val", "random-forest/trees")
            Dim treeCount As Integer = 0
            Dim sizes As Integer() = New Integer(-1) {}

            ' 第一遍：确定每棵树的节点数（节点下标可能不连续，按最大值 +1 分配）
            For i As Integer = 0 To table.RowCount - 1
                Dim t As Integer = treeIds(i)

                If t + 1 > treeCount Then
                    treeCount = t + 1
                    If sizes.Length < treeCount Then
                        Array.Resize(sizes, treeCount)
                    End If
                End If
                If nodeIds(i) + 1 > sizes(t) Then
                    sizes(t) = nodeIds(i) + 1
                End If
            Next

            If treeCount = 0 Then
                Throw New System.IO.InvalidDataException("the random forest model contains no tree node!")
            End If

            Dim declared As Integer = manifest.GetInteger("treeCount")

            If declared > treeCount Then
                Throw New System.IO.InvalidDataException(
                    $"the random forest manifest declares {declared} tree(s), " &
                    $"but the CSV table only covers {treeCount}!")
            End If

            Dim trees As Branch()() = New Branch(treeCount - 1)() {}

            For t As Integer = 0 To treeCount - 1
                trees(t) = New Branch(sizes(t) - 1) {}
            Next

            ' 第二遍：按 (tree, node) 回填全部字段
            For i As Integer = 0 To table.RowCount - 1
                Dim t As Integer = treeIds(i)
                Dim n As Integer = nodeIds(i)

                trees(t)(n) = New Branch With {
                    .status = statuses(i),
                    .Feature = features(i),
                    .mean_snp = thresholds(i),
                    .Child1 = child1(i),
                    .Child2 = child2(i),
                    .mean = means(i),
                    .class_val = classValues(i)
                }
            Next

            ' RanFog.Trees 的类型是 sciBASIC 的 List(Of T)（继承自 System.List），
            ' 只能 AddRange 逐项填充，不能直接赋值一个 System.List。
            Dim forest As New RanFog With {
                .max_tree = manifest.GetInteger("max_tree"),
                .max_branch = manifest.GetInteger("max_branch"),
                .mtry = manifest.GetInteger("mtry"),
                .LF_c = ParseCriterion(manifest.GetParameter("splitCriterion")),
                .false_positive_cost = manifest.GetScalar("falsePositiveCost"),
                .false_negative_cost = manifest.GetScalar("falseNegativeCost")
            }

            Call forest.Trees.AddRange(trees)

            Dim restored As RanFog = forest

            If manifest.vectors IsNot Nothing AndAlso manifest.vectors.ContainsKey("VI") Then
                restored.VI = manifest.GetVector("VI")
            End If
            If manifest.integerVectors IsNot Nothing AndAlso manifest.integerVectors.ContainsKey("Selected") Then
                restored.Selected = manifest.GetIntegerVector("Selected")
            End If

            Return New RandomForestRegressor(restored, manifest.featureWidth)
        End Function

        Private Shared Function ParseCriterion(text As String) As RfLeafCriterion
            Dim criterion As RfLeafCriterion

            If Not [Enum].TryParse(text, criterion) Then
                Throw New System.IO.InvalidDataException(
                    $"the random forest manifest has an invalid split criterion ('{text}')!")
            End If

            Return criterion
        End Function

        Private Shared Iterator Function EnumerateTrees(fitted As RanFog) As IEnumerable(Of String())
            For t As Integer = 0 To fitted.Trees.Count - 1
                Dim nodes As Branch() = fitted.Trees(t)

                If nodes Is Nothing Then
                    Continue For
                End If

                For n As Integer = 0 To nodes.Length - 1
                    Dim branch As Branch = nodes(n)

                    If branch Is Nothing Then
                        Continue For
                    End If

                    Yield New String() {
                        CStr(t),
                        CStr(n),
                        If(branch.status, " "),
                        CStr(branch.Feature),
                        ModelFormat.Number(branch.mean_snp),
                        CStr(branch.Child1),
                        CStr(branch.Child2),
                        ModelFormat.Number(branch.mean),
                        CStr(branch.class_val)
                    }
                Next
            Next
        End Function

    End Class

End Namespace
