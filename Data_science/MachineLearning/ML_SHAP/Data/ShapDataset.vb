#Region "Microsoft.VisualBasic::3f59bda3900691c2488b4094fbcc459d, Data_science\MachineLearning\ML_SHAP\Data\ShapDataset.vb"

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

    '   Total Lines: 205
    '    Code Lines: 105 (51.22%)
    ' Comment Lines: 70 (34.15%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 30 (14.63%)
    '     File Size: 8.36 KB


    ' Class ShapDataset
    ' 
    '     Constructor: (+1 Overload) Sub New
    ' 
    '     Properties: ClassLabels, FeatureNames, Features, IDs, IsCategorical
    '                 Labels, Name, Size, Table, Width
    ' 
    '     Function: ArrayPack, GetLabels, LoadFile, Parse, ParseValue
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Math

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
''' 
''' 本对象采用「组合」的方式实现：所有数值数据都存放在只读成员 <see cref="Table"/> 之中，
''' 自身只保留「标签是否为类别编号」这一附加信息以及文本装载逻辑。
''' 下面的 <see cref="Size"/>、<see cref="Width"/>、<see cref="IDs"/>、<see cref="Features"/>、
''' <see cref="FeatureNames"/>、<see cref="Name"/>、<see cref="Labels"/>、<see cref="ClassLabels"/>
''' 都是指向 <see cref="Table"/> 的只读转发视图，不会复制任何数值数据。
''' </summary>
Public Class ShapDataset
    Implements INumericMatrix
    Implements ILabeledMatrix

    ''' <summary>
    ''' 底层数据表，作为本数据集唯一的数值数据来源：
    ''' 
    ''' 1. <see cref="NumericTable.rowNames"/>：样本 ID
    ''' 2. <see cref="NumericTable.features"/> + <see cref="NumericTable.featureNames"/>：特征矩阵
    ''' 3. <see cref="NumericTable.labels"/> + <see cref="NumericTable.labelNames"/>：响应列（单列，列名 ``Y``）
    '''
    ''' 该属性只在构造函数之中赋值，因此下面的转发视图永远不需要做空值判断。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Table As NumericTable

    ''' <summary>
    ''' 是否为分类数据集（标签为类别编号）。
    ''' </summary>
    ''' <returns></returns>
    Public Property IsCategorical As Boolean

    ''' <summary>
    ''' 包装一张已经准备好的数值表。
    ''' </summary>
    ''' <param name="table">底层数值表，响应数据应当位于第一列标签列</param>
    ''' <param name="isCategorical">标签是否为类别编号</param>
    Sub New(table As NumericTable, Optional isCategorical As Boolean = False)
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Me.Table = table
        Me.IsCategorical = isCategorical
    End Sub

    ''' <summary>
    ''' 样本的数量。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Size As Integer
        Get
            Return Table.nsamples
        End Get
    End Property

    ''' <summary>
    ''' 特征的数量。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Width As Integer
        Get
            Return Table.nfeatures
        End Get
    End Property

    ''' <summary>
    ''' 样本 ID。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IDs As String()
        Get
            Return Table.rowNames
        End Get
    End Property

    ''' <summary>
    ''' 行主序的特征矩阵 ``features(sample)(feature)``。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Features As Double()()
        Get
            Return Table.features
        End Get
    End Property

    ''' <summary>
    ''' 特征名字。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property FeatureNames As String()
        Get
            Return Table.featureNames
        End Get
    End Property

    ''' <summary>
    ''' 数据集的名字（例如 ``training_categorical``）。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
        Get
            Return Table.name
        End Get
    End Property

    ''' <summary>
    ''' 单列响应向量，即 <see cref="NumericTable.labels"/> 的第 0 列。
    ''' 
    ''' 该属性为派生视图，每次访问都会新建一个数组：在循环或者是 LINQ 查询
    ''' 之中请不要重复访问，应该先取出到局部变量。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Labels As Double()
        Get
            If Table.labels Is Nothing OrElse Table.nlabels = 0 Then
                Return New Double() {}
            End If

            Return Table.GetLabel(0)
        End Get
    End Property

    ''' <summary>
    ''' 标签的整数化视图（用于分类任务的混淆矩阵计算）。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ClassLabels As Integer()
        Get
            If Table.labels Is Nothing OrElse Table.nlabels = 0 Then
                Return New Integer() {}
            End If

            Return Table.GetClassLabel(0)
        End Get
    End Property

    ''' <summary>
    ''' 转发到 <see cref="NumericTable"/> 的矩阵打包实现
    ''' （该成员在 <see cref="NumericTable"/> 之中是私有接口实现，只能经接口引用调用）
    ''' </summary>
    Private Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements INumericMatrix.ArrayPack
        Return DirectCast(Table, INumericMatrix).ArrayPack(deepcopy)
    End Function

    ''' <summary>
    ''' 转发到 <see cref="NumericTable"/> 的行名视图，实现 <see cref="ILabeledMatrix.GetLabels"/> 接口
    ''' </summary>
    Private Function GetLabels() As IEnumerable(Of String) Implements ILabeledMatrix.GetLabels
        Return DirectCast(Table, ILabeledMatrix).GetLabels()
    End Function

    ''' <summary>
    ''' 从文本文件加载数据集。
    ''' </summary>
    ''' <param name="path">数据文件路径</param>
    ''' <param name="isCategorical">标签是否为类别编号</param>
    ''' <param name="name">数据集名字，缺省为文件名</param>
    ''' <returns></returns>
    Public Shared Function LoadFile(path As String, isCategorical As Boolean, Optional name As String = Nothing) As ShapDataset
        ' 注意：本文件导入了 Microsoft.VisualBasic.Data，简写的 IO.Path 会被解析到
        ' Microsoft.VisualBasic.Data.IO 命名空间，因此这里显式使用 System.IO.Path
        Dim datasetName As String = If(String.IsNullOrEmpty(name), System.IO.Path.GetFileNameWithoutExtension(path), name)
        Return Parse(System.IO.File.ReadAllLines(path), isCategorical, datasetName)
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
        Dim responses As New List(Of Double)()
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

            responses.Add(ParseValue(parts(0), no))
            ids.Add(parts(1))
            rows.Add(vector)
        Next

        If rows.Count = 0 Then
            Throw New InvalidDataException("the given data file contains no valid sample!")
        End If

        Dim columnNames As String() = Enumerable.Range(1, rows(0).Length).Select(Function(i) $"F{i}").ToArray
        Dim table As New NumericTable(rows.ToArray, ids.ToArray, columnNames) With {
            .name = name,
            .labels = responses.Select(Function(y) New Double() {y}).ToArray,
            .labelNames = New String() {"Y"}
        }

        ' 维度一致性与列名长度校验统一交给 NumericTable
        Call table.Validate()

        Return New ShapDataset(table, isCategorical)
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
