#Region "Microsoft.VisualBasic::ShapDataset, MachineLearning\ML_SHAP\Data\ShapDataset.vb"

Imports System.Globalization
Imports System.IO
Imports System.Linq

''' <summary>
''' 用于 SHAP 分析的数值数据集。
''' 
''' 从空格（或制表符）分隔的文本文件之中加载，文件格式为：
''' 
''' ```
''' label id feature1 feature2 ... featureN
''' ```
''' 
''' 其中第一列为标签（分类任务为 0/1，回归任务为连续值），第二列为样本 ID，
''' 其余各列为数值特征。
''' </summary>
Public Class ShapDataset

    ''' <summary>
    ''' 数据集的名字（例如 ``training_categorical``）。
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String

    ''' <summary>
    ''' 是否为分类数据集（标签为类别编号）。
    ''' </summary>
    ''' <returns></returns>
    Public Property IsCategorical As Boolean

    ''' <summary>
    ''' 样本 ID。
    ''' </summary>
    ''' <returns></returns>
    Public Property IDs As String()

    ''' <summary>
    ''' 样本标签。
    ''' </summary>
    ''' <returns></returns>
    Public Property Labels As Double()

    ''' <summary>
    ''' 行主序的特征矩阵 ``features(sample)(feature)``。
    ''' </summary>
    ''' <returns></returns>
    Public Property Features As Double()()

    ''' <summary>
    ''' 特征名字。
    ''' </summary>
    ''' <returns></returns>
    Public Property FeatureNames As String()

    ''' <summary>
    ''' 样本的数量。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Size As Integer
        Get
            Return If(Labels Is Nothing, 0, Labels.Length)
        End Get
    End Property

    ''' <summary>
    ''' 特征的数量。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Width As Integer
        Get
            Return If(FeatureNames Is Nothing, 0, FeatureNames.Length)
        End Get
    End Property

    ''' <summary>
    ''' 标签的整数化视图（用于分类任务的混淆矩阵计算）。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ClassLabels As Integer()
        Get
            If Labels Is Nothing Then
                Return New Integer() {}
            End If

            Return Labels.Select(Function(d) CInt(System.Math.Round(d))).ToArray
        End Get
    End Property

    ''' <summary>
    ''' 从文本文件加载数据集。
    ''' </summary>
    ''' <param name="path">数据文件路径</param>
    ''' <param name="isCategorical">标签是否为类别编号</param>
    ''' <param name="name">数据集名字，缺省为文件名</param>
    ''' <returns></returns>
    Public Shared Function LoadFile(path As String, isCategorical As Boolean, Optional name As String = Nothing) As ShapDataset
        Dim datasetName As String = If(String.IsNullOrEmpty(name), IO.Path.GetFileNameWithoutExtension(path), name)
        Return Parse(File.ReadAllLines(path), isCategorical, datasetName)
    End Function

    ''' <summary>
    ''' 从文本行集合解析数据集。
    ''' </summary>
    ''' <param name="lines">文本行</param>
    ''' <param name="isCategorical">标签是否为类别编号</param>
    ''' <param name="name">数据集名字</param>
    ''' <returns></returns>
    Public Shared Function Parse(lines As IEnumerable(Of String), isCategorical As Boolean, Optional name As String = Nothing) As ShapDataset
        Dim ids As New List(Of String)()
        Dim labels As New List(Of Double)()
        Dim rows As New List(Of Double())()
        Dim no As Integer = 0

        For Each line As String In lines
            no += 1

            Dim text As String = If(line, "").Trim()

            If text.Length = 0 OrElse text.StartsWith("#") Then
                Continue For
            End If

            Dim parts As String() = text.Split(New Char() {" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)

            If parts.Length < 3 Then
                Throw New InvalidDataException($"invalid data line #{no}: a label, an id and at least one feature are required!")
            End If

            Dim vector As Double() = New Double(parts.Length - 3) {}

            For i As Integer = 2 To parts.Length - 1
                vector(i - 2) = ParseValue(parts(i), no)
            Next

            labels.Add(ParseValue(parts(0), no))
            ids.Add(parts(1))
            rows.Add(vector)
        Next

        If rows.Count = 0 Then
            Throw New InvalidDataException("the given data file contains no valid sample!")
        End If

        Dim width As Integer = rows(0).Length

        For i As Integer = 0 To rows.Count - 1
            If rows(i).Length <> width Then
                Throw New InvalidDataException($"inconsistent feature dimension at sample #{i}: expected {width} but got {rows(i).Length}!")
            End If
        Next

        Return New ShapDataset With {
            .Name = name,
            .IsCategorical = isCategorical,
            .IDs = ids.ToArray,
            .Labels = labels.ToArray,
            .Features = rows.ToArray,
            .FeatureNames = Enumerable.Range(1, width).Select(Function(i) $"F{i}").ToArray
        }
    End Function

    Private Shared Function ParseValue(s As String, line As Integer) As Double
        Dim value As Double

        If Double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, value) Then
            Return value
        Else
            Throw New InvalidDataException($"invalid numeric token '{s}' at data line #{line}!")
        End If
    End Function

End Class

#End Region
