Option Strict On
Option Explicit On
Option Infer On

Imports System.Linq

Namespace Schema

    ''' <summary>
    ''' Parquet schema 树中的节点。叶子节点对应一个物理列，group 节点对应结构体 / LIST / MAP。
    ''' </summary>
    Public Class ParquetSchemaNode

        Public Property Name As String

        Public Property Repetition As Format.FieldRepetitionType = Format.FieldRepetitionType.REQUIRED

        Public Property PhysicalType As Format.ParquetType?

        Public Property TypeLength As Integer

        Public Property Converted As Format.ConvertedType?

        Public Property Logical As Format.LogicalType

        Public Property Children As New List(Of ParquetSchemaNode)

        Public Property Parent As ParquetSchemaNode

        ''' <summary>
        ''' 原始 footer 中的 schema 元素（写入时用于还原）。
        ''' </summary>
        Public Property Element As Format.SchemaElement

        ''' <summary>
        ''' 当前节点被定义时所达到的定义级别。
        ''' </summary>
        Public Property DefinitionLevel As Integer

        ''' <summary>
        ''' 当前节点所引起的重复级别。
        ''' </summary>
        Public Property RepetitionLevel As Integer

        ''' <summary>
        ''' 从根节点（不包含）到当前节点的完整路径（用于 <c>path_in_schema</c>）。
        ''' </summary>
        Public Property Path As New List(Of String)

        Public Property IsRoot As Boolean

        Public ReadOnly Property IsGroup As Boolean
            Get
                Return Not PhysicalType.HasValue
            End Get
        End Property

        Public ReadOnly Property IsLeaf As Boolean
            Get
                Return PhysicalType.HasValue
            End Get
        End Property

        Public ReadOnly Property IsRepeated As Boolean
            Get
                Return Repetition = Format.FieldRepetitionType.REPEATED
            End Get
        End Property

        Public ReadOnly Property IsOptional As Boolean
            Get
                Return Repetition = Format.FieldRepetitionType.[OPTIONAL]
            End Get
        End Property

        ''' <summary>
        ''' group 是否带有 LIST 逻辑类型注解。
        ''' </summary>
        Public ReadOnly Property IsListGroup As Boolean
            Get
                If Not IsGroup Then
                    Return False
                End If

                Return LogicalTypeName() = Format.LogicalType.KindList OrElse Converted.GetValueOrDefault(Format.ConvertedType.UTF8) = Format.ConvertedType.LIST
            End Get
        End Property

        ''' <summary>
        ''' group 是否带有 MAP 逻辑类型注解。
        ''' </summary>
        Public ReadOnly Property IsMapGroup As Boolean
            Get
                If Not IsGroup Then
                    Return False
                End If

                Return LogicalTypeName() = Format.LogicalType.KindMap OrElse Converted.GetValueOrDefault(Format.ConvertedType.UTF8) = Format.ConvertedType.MAP
            End Get
        End Property

        ''' <summary>
        ''' 该节点是否是 LIST / MAP 注解 group 内部那个 repeated 的包装节点（通常命名为 <c>list</c> 或者 <c>key_value</c>）。
        ''' </summary>
        Public ReadOnly Property IsRepeatedWrapper As Boolean
            Get
                If Not IsRepeated OrElse Parent Is Nothing Then
                    Return False
                End If

                Return Parent.IsListGroup OrElse Parent.IsMapGroup
            End Get
        End Property

        ''' <summary>
        ''' 该节点是否是 LIST 内部包装节点下面的 <c>element</c> 节点。
        ''' </summary>
        Public ReadOnly Property IsListElementWrapper As Boolean
            Get
                If Parent Is Nothing Then
                    Return False
                End If

                Return Parent.IsRepeatedWrapper AndAlso Parent.Parent IsNot Nothing AndAlso Parent.Parent.IsListGroup
            End Get
        End Property

        Private Function LogicalTypeName() As String
            If Logical Is Nothing Then
                Return Nothing
            End If

            Return Logical.Kind
        End Function

        ''' <summary>
        ''' 当前节点路径上的所有叶子节点。
        ''' </summary>
        Public Function Leaves() As IEnumerable(Of ParquetSchemaNode)
            If IsLeaf Then
                Return New ParquetSchemaNode() {Me}
            End If

            Dim result As New List(Of ParquetSchemaNode)

            For Each child As ParquetSchemaNode In Children
                result.AddRange(child.Leaves())
            Next

            Return result
        End Function

        ''' <summary>
        ''' 当前节点路径上是否存在 repeated 的祖先（包含自身）。
        ''' </summary>
        Public Function HasRepeatedAncestor() As Boolean
            Dim node As ParquetSchemaNode = Me

            While node IsNot Nothing
                If node.IsRepeated Then
                    Return True
                End If

                node = node.Parent
            End While

            Return False
        End Function

        ''' <summary>
        ''' 用于 DataFrame 列名的路径：跳过 LIST / MAP 的包装节点以及 <c>element</c> 包装节点。
        ''' </summary>
        Public Function DataPath() As List(Of String)
            Dim names As New List(Of String)
            Dim chain As New List(Of ParquetSchemaNode)
            Dim node As ParquetSchemaNode = Me

            While node IsNot Nothing AndAlso Not node.IsRoot
                Call chain.Insert(0, node)
                node = node.Parent
            End While

            For Each item As ParquetSchemaNode In chain
                If item.IsRepeatedWrapper OrElse item.IsListElementWrapper Then
                    Continue For
                End If

                Call names.Add(item.Name)
            Next

            Return names
        End Function

        ''' <summary>
        ''' 顶层列（根节点下面的一级字段）。
        ''' </summary>
        Public Function TopLevel() As ParquetSchemaNode
            Dim node As ParquetSchemaNode = Me

            While node.Parent IsNot Nothing AndAlso Not node.Parent.IsRoot
                node = node.Parent
            End While

            Return node
        End Function

        ''' <summary>
        ''' 从当前节点向下按名称查找节点。
        ''' </summary>
        Public Function [Select](name As String) As ParquetSchemaNode
            For Each child As ParquetSchemaNode In Children
                If String.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase) Then
                    Return child
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            If IsGroup Then
                Return $"{Name} (group, {Repetition})"
            End If

            Return $"{Name} ({PhysicalType}, {Repetition})"
        End Function

    End Class

    ''' <summary>
    ''' 由 footer 中的平坦 schema 列表还原出来的 schema 树。
    ''' </summary>
    Public Class ParquetSchema

        Public Property Root As ParquetSchemaNode

        Public ReadOnly Property Leaves As List(Of ParquetSchemaNode)
            Get
                Return Root.Leaves().ToList
            End Get
        End Property

        ''' <summary>
        ''' 根据路径查找叶子节点。
        ''' </summary>
        Public Function FindLeaf(path As IEnumerable(Of String)) As ParquetSchemaNode
            Dim names As String() = path.ToArray
            Dim node As ParquetSchemaNode = Root

            For Each name As String In names
                Dim child As ParquetSchemaNode = node.[Select](name)

                If child Is Nothing Then
                    Return Nothing
                End If

                node = child
            Next

            If node Is Nothing OrElse Not node.IsLeaf Then
                Return Nothing
            End If

            Return node
        End Function

        ''' <summary>
        ''' 重新计算整棵树上的定义级别 / 重复级别 / 路径（用于手工构建的 schema）。
        ''' </summary>
        Public Shared Sub ComputeLevels(root As ParquetSchemaNode)
            root.DefinitionLevel = 0
            root.RepetitionLevel = 0
            root.Path = New List(Of String)

            Call ApplyLevels(root)
        End Sub

        Private Shared Sub ApplyLevels(node As ParquetSchemaNode)
            For Each child As ParquetSchemaNode In node.Children
                child.Parent = node
                child.DefinitionLevel = node.DefinitionLevel + If(child.Repetition <> Format.FieldRepetitionType.REQUIRED, 1, 0)
                child.RepetitionLevel = node.RepetitionLevel + If(child.IsRepeated, 1, 0)
                child.Path = New List(Of String)(node.Path)
                Call child.Path.Add(child.Name)

                Call ApplyLevels(child)
            Next
        End Sub

        ''' <summary>
        ''' 由 footer 的 <c>schema</c> 列表还原 schema 树。
        ''' </summary>
        Public Shared Function FromMetadata(elements As List(Of Format.SchemaElement)) As ParquetSchema
            If elements Is Nothing OrElse elements.Count = 0 Then
                Throw New ParquetException("the parquet file contains an empty schema!")
            End If

            Dim root As New ParquetSchemaNode With {
                .Name = elements(0).Name,
                .Element = elements(0),
                .IsRoot = True,
                .Repetition = Format.FieldRepetitionType.REQUIRED,
                .DefinitionLevel = 0,
                .RepetitionLevel = 0
            }

            Dim index As Integer = 1

            Call BuildChildren(root, elements, index, 1)

            Return New ParquetSchema With {.Root = root}
        End Function

        Private Shared Sub BuildChildren(parent As ParquetSchemaNode, elements As List(Of Format.SchemaElement), ByRef index As Integer, depth As Integer)
            If depth > ParquetConstants.MaxNestedDepth Then
                Throw New ParquetException($"the parquet schema is nested too deeply (more than {ParquetConstants.MaxNestedDepth} levels)!")
            End If

            Dim count As Integer = If(parent.Element Is Nothing, 0, parent.Element.NumChildren.GetValueOrDefault(0))

            For i As Integer = 1 To count
                If index >= elements.Count Then
                    Throw New ParquetException("the parquet schema is truncated: the num_children of a group does not match the flat schema list!")
                End If

                Dim element As Format.SchemaElement = elements(index)
                Dim node As New ParquetSchemaNode With {
                    .Name = element.Name,
                    .Element = element,
                    .PhysicalType = element.Type,
                    .TypeLength = element.TypeLength.GetValueOrDefault(0),
                    .Converted = element.Converted,
                    .Logical = element.Logical,
                    .Repetition = element.Repetition.GetValueOrDefault(Format.FieldRepetitionType.REQUIRED),
                    .Parent = parent
                }

                node.DefinitionLevel = parent.DefinitionLevel + If(node.Repetition <> Format.FieldRepetitionType.REQUIRED, 1, 0)
                node.RepetitionLevel = parent.RepetitionLevel + If(node.IsRepeated, 1, 0)
                node.Path = New List(Of String)(parent.Path)
                Call node.Path.Add(node.Name)

                Call parent.Children.Add(node)

                index += 1

                If element.NumChildren.GetValueOrDefault(0) > 0 Then
                    Call BuildChildren(node, elements, index, depth + 1)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 将 schema 树平铺为 footer 中的 <c>schema</c> 列表。
        ''' </summary>
        Public Shared Function ToMetadata(root As ParquetSchemaNode) As List(Of Format.SchemaElement)
            Dim elements As New List(Of Format.SchemaElement)

            Call Flatten(root, elements)

            Return elements
        End Function

        Private Shared Sub Flatten(node As ParquetSchemaNode, elements As List(Of Format.SchemaElement))
            Dim element As Format.SchemaElement = node.Element

            If element Is Nothing Then
                element = New Format.SchemaElement With {
                    .Name = node.Name,
                    .Type = node.PhysicalType,
                    .Repetition = If(node.IsRoot, Format.FieldRepetitionType.REQUIRED, node.Repetition),
                    .Converted = node.Converted,
                    .Logical = node.Logical,
                    .TypeLength = If(node.TypeLength > 0, CType(node.TypeLength, Integer?), Nothing)
                }
            End If

            element.NumChildren = If(node.Children.Count > 0, CType(node.Children.Count, Integer?), Nothing)

            If node.IsRoot Then
                element.Repetition = Nothing
                element.Type = Nothing
            Else
                element.Repetition = node.Repetition
                element.Type = node.PhysicalType
            End If

            Call elements.Add(element)

            For Each child As ParquetSchemaNode In node.Children
                Call Flatten(child, elements)
            Next
        End Sub

    End Class

End Namespace
