' ============================================================================
' KnowledgeGraph.vb - 知识图谱核心数据结构
'
' 采用二部图（Bipartite Graph）模型：
'   - 实体节点（EntityNode）：表示知识词条，如 water、H2O、老虎
'   - 属性节点（AttributeNode）：表示知识属性，如 化学式、沸点、分类等级
'   - 边（Edge）：仅存在于实体节点与属性节点之间
'
' 实体与实体之间无直接连边，通过共享的属性节点间接连接。
' 这种设计使得同义实体（如 water / 水 / H2O）会因为共享大量属性
' 而在 Jaccard 相似度和 Adamic-Adar 指数上表现出显著高分。
' ============================================================================

Imports System.Collections.Generic

''' <summary>
''' 知识词条节点：表示一个知识实体。
''' </summary>
Public Class EntityNode

    ''' <summary>唯一标识符。</summary>
    Public Property Id As Integer

    ''' <summary>词条名称。</summary>
    Public Property Name As String

    ''' <summary>语言标识：en / zh / formula。</summary>
    Public Property Language As String

    ''' <summary>实体类型：substance / organism / company / concept 等。</summary>
    Public Property EntityType As String

    Public Overrides Function ToString() As String
        Return $"{Name} ({Language})"
    End Function

End Class

''' <summary>
''' 属性节点：表示知识的一条属性。
''' </summary>
Public Class AttributeNode

    ''' <summary>唯一标识符。</summary>
    Public Property Id As Integer

    ''' <summary>属性名称（规范化的小写下划线格式）。</summary>
    Public Property Name As String

    ''' <summary>属性类别：chemical / physical / biological / commercial / functional 等。</summary>
    Public Property Category As String

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class

''' <summary>
''' 知识图谱：管理实体节点、属性节点及其连接关系。
''' 内部使用邻接表存储二部图结构。
''' </summary>
Public Class KnowledgeGraph

    ' --- 节点存储 ---
    Private _entities As New List(Of EntityNode)
    Private _attributes As New List(Of AttributeNode)
    Private _entityNameMap As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private _attrNameMap As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

    ' --- 邻接表（双向）---
    Private _entityToAttrs As New Dictionary(Of Integer, HashSet(Of Integer))
    Private _attrToEntities As New Dictionary(Of Integer, HashSet(Of Integer))

    ''' <summary>所有实体节点（只读视图）。</summary>
    Public ReadOnly Property Entities As IReadOnlyList(Of EntityNode)
        Get
            Return _entities
        End Get
    End Property

    ''' <summary>所有属性节点（只读视图）。</summary>
    Public ReadOnly Property Attributes As IReadOnlyList(Of AttributeNode)
        Get
            Return _attributes
        End Get
    End Property

    ''' <summary>图中的边数。</summary>
    Public ReadOnly Property TotalEdges As Integer
        Get
            Dim count As Integer = 0
            For Each kvp In _entityToAttrs
                count += kvp.Value.Count
            Next
            Return count
        End Get
    End Property

    ''' <summary>
    ''' 添加一个实体节点，返回其 ID。
    ''' 如果同名实体已存在则返回已有 ID。
    ''' </summary>
    Public Function AddEntity(name As String, language As String, entityType As String) As Integer
        Dim key As String = name & "|" & language
        If _entityNameMap.ContainsKey(key) Then
            Return _entityNameMap(key)
        End If

        Dim id As Integer = _entities.Count
        _entities.Add(New EntityNode With {
            .Id = id,
            .Name = name,
            .Language = language,
            .EntityType = entityType
        })
        _entityNameMap(key) = id
        _entityToAttrs(id) = New HashSet(Of Integer)
        Return id
    End Function

    ''' <summary>
    ''' 添加一个属性节点，返回其 ID。
    ''' 如果同名属性已存在则返回已有 ID。
    ''' </summary>
    Public Function AddAttribute(name As String, category As String) As Integer
        If _attrNameMap.ContainsKey(name) Then
            Return _attrNameMap(name)
        End If

        Dim id As Integer = _attributes.Count
        _attributes.Add(New AttributeNode With {
            .Id = id,
            .Name = name,
            .Category = category
        })
        _attrNameMap(name) = id
        _attrToEntities(id) = New HashSet(Of Integer)
        Return id
    End Function

    ''' <summary>
    ''' 在实体与属性之间添加一条边。
    ''' </summary>
    Public Sub AddEdge(entityId As Integer, attributeId As Integer)
        If Not _entityToAttrs.ContainsKey(entityId) Then
            Throw New ArgumentOutOfRangeException(NameOf(entityId))
        End If
        If Not _attrToEntities.ContainsKey(attributeId) Then
            Throw New ArgumentOutOfRangeException(NameOf(attributeId))
        End If
        _entityToAttrs(entityId).Add(attributeId)
        _attrToEntities(attributeId).Add(entityId)
    End Sub

    ''' <summary>
    ''' 便捷方法：按名称为实体添加属性（自动创建属性节点）。
    ''' </summary>
    Public Sub AddEntityAttribute(entityId As Integer, attrName As String, attrCategory As String)
        Dim attrId As Integer = AddAttribute(attrName, attrCategory)
        AddEdge(entityId, attrId)
    End Sub

    ''' <summary>
    ''' 获取实体连接的所有属性 ID 集合。
    ''' </summary>
    Public Function GetEntityAttributes(entityId As Integer) As HashSet(Of Integer)
        Dim result As HashSet(Of Integer) = Nothing
        If _entityToAttrs.TryGetValue(entityId, result) Then
            Return New HashSet(Of Integer)(result)
        End If
        Return New HashSet(Of Integer)
    End Function

    ''' <summary>
    ''' 获取属性连接的所有实体 ID 集合。
    ''' </summary>
    Public Function GetAttributeEntities(attributeId As Integer) As HashSet(Of Integer)
        Dim result As HashSet(Of Integer) = Nothing
        If _attrToEntities.TryGetValue(attributeId, result) Then
            Return New HashSet(Of Integer)(result)
        End If
        Return New HashSet(Of Integer)
    End Function

    ''' <summary>
    ''' 获取实体的度（连接的属性数）。
    ''' </summary>
    Public Function GetEntityDegree(entityId As Integer) As Integer
        Dim s As HashSet(Of Integer) = Nothing
        If _entityToAttrs.TryGetValue(entityId, s) Then
            Return s.Count
        End If
        Return 0
    End Function

    ''' <summary>
    ''' 获取属性的度（连接的实体数）。
    ''' </summary>
    Public Function GetAttributeDegree(attributeId As Integer) As Integer
        Dim s As HashSet(Of Integer) = Nothing
        If _attrToEntities.TryGetValue(attributeId, s) Then
            Return s.Count
        End If
        Return 0
    End Function

    ''' <summary>
    ''' 按名称查找实体。
    ''' </summary>
    Public Function FindEntity(name As String, Optional language As String = "") As EntityNode
        For Each e In _entities
            If String.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase) Then
                If language = "" OrElse String.Equals(e.Language, language, StringComparison.OrdinalIgnoreCase) Then
                    Return e
                End If
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' 获取实体名称。
    ''' </summary>
    Public Function GetEntityName(entityId As Integer) As String
        If entityId >= 0 AndAlso entityId < _entities.Count Then
            Return _entities(entityId).Name
        End If
        Return "?"
    End Function

    ''' <summary>
    ''' 获取属性名称。
    ''' </summary>
    Public Function GetAttributeName(attrId As Integer) As String
        If attrId >= 0 AndAlso attrId < _attributes.Count Then
            Return _attributes(attrId).Name
        End If
        Return "?"
    End Function

    ''' <summary>
    ''' 获取实体节点。
    ''' </summary>
    Public Function GetEntity(entityId As Integer) As EntityNode
        If entityId >= 0 AndAlso entityId < _entities.Count Then
            Return _entities(entityId)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' 获取属性节点。
    ''' </summary>
    Public Function GetAttribute(attrId As Integer) As AttributeNode
        If attrId >= 0 AndAlso attrId < _attributes.Count Then
            Return _attributes(attrId)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' 获取两个实体共享的属性 ID 集合。
    ''' </summary>
    Public Function GetSharedAttributes(entityA As Integer, entityB As Integer) As HashSet(Of Integer)
        Dim attrsA As HashSet(Of Integer) = GetEntityAttributes(entityA)
        Dim attrsB As HashSet(Of Integer) = GetEntityAttributes(entityB)
        attrsA.IntersectWith(attrsB)
        Return attrsA
    End Function

    ''' <summary>
    ''' 获取属性节点名称列表（用于展示共享属性）。
    ''' </summary>
    Public Function GetAttributeNames(attrIds As IEnumerable(Of Integer)) As List(Of String)
        Dim result As New List(Of String)
        For Each id In attrIds
            If id >= 0 AndAlso id < _attributes.Count Then
                result.Add(_attributes(id).Name)
            End If
        Next
        Return result
    End Function

    ''' <summary>
    ''' 获取按类别统计的属性数量。
    ''' </summary>
    Public Function GetAttributeCategoryStats() As Dictionary(Of String, Integer)
        Dim stats As New Dictionary(Of String, Integer)
        For Each attr In _attributes
            If stats.ContainsKey(attr.Category) Then
                stats(attr.Category) += 1
            Else
                stats(attr.Category) = 1
            End If
        Next
        Return stats
    End Function

    ''' <summary>
    ''' 获取每个实体的度序列（用于置换检验）。
    ''' </summary>
    Public Function GetDegreeSequence() As Integer()
        Dim result(_entities.Count - 1) As Integer
        For i As Integer = 0 To _entities.Count - 1
            result(i) = GetEntityDegree(i)
        Next
        Return result
    End Function

End Class
