
''' <summary>
''' 图数据集
''' 用于存储多个图样本，常用于图分类任务
''' </summary>
Public Class GraphDataset
    ''' <summary>
    ''' 图样本列表
    ''' </summary>

    ''' <summary>
    ''' 图标签（用于图分类任务）
    ''' </summary>
    Private _Graphs As List(Of Graph), _Labels As List(Of Integer)

    Public Property Graphs As List(Of Graph)
        Get
            Return _Graphs
        End Get
        Private Set(value As List(Of Graph))
            _Graphs = value
        End Set
    End Property

    Public Property Labels As List(Of Integer)
        Get
            Return _Labels
        End Get
        Private Set(value As List(Of Integer))
            _Labels = value
        End Set
    End Property

    Public Sub New()
        Graphs = New List(Of Graph)()
        Labels = New List(Of Integer)()
    End Sub

    ''' <summary>
    ''' 添加图样本
    ''' </summary>
    Public Sub Add(graph As Graph, label As Integer)
        Graphs.Add(graph)
        Labels.Add(label)
    End Sub

    ''' <summary>
    ''' 获取数据集大小
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return Graphs.Count
        End Get
    End Property

    ''' <summary>
    ''' 获取类别数量
    ''' </summary>
    Public ReadOnly Property NumClasses As Integer
        Get
            Return Labels.Distinct().Count()
        End Get
    End Property
End Class