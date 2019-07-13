#Region "Microsoft.VisualBasic::68b027c2426ee954592b66c111ef89b7, Microsoft.VisualBasic.Core\ComponentModel\ValuePair\TagData\TagData.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class TagData
    ' 
    '         Properties: Key, Tag, TagStr
    ' 
    '     Class IntegerTagged
    ' 
    ' 
    ' 
    '     Class DoubleTagged
    ' 
    ' 
    ' 
    '     Class LongTagged
    ' 
    ' 
    ' 
    '     Class DateTagged
    ' 
    ' 
    ' 
    '     Class VectorTagged
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.TagData

    ''' <summary>
    ''' Target value have a specific tag key.
    ''' </summary>
    ''' <typeparam name="T">The type of the tag key</typeparam>
    ''' <typeparam name="V">The type of the value data</typeparam>
    Public Class TagData(Of T, V) : Inherits Value(Of V)
        Implements INamedValue

        ''' <summary>
        ''' ``<see cref="Tag"/> -> <see cref="GetJson"/>``
        ''' </summary>
        ''' <returns></returns>
        Private Property Key As String Implements IKeyedEntity(Of String).Key
            Get
                Return Tag.GetJson
            End Get
            Set(value As String)
                Tag = value.LoadJSON(Of T)
            End Set
        End Property

        ''' <summary>
        ''' Target value have this specific tag data.
        ''' </summary>
        ''' <returns></returns>
        Public Property Tag As T

        ''' <summary>
        ''' 默认都有一个字符串类型的标签用于保存其他的数据，但是主要还是使用``<see cref="Tag"/>``属性来进行标记
        ''' </summary>
        ''' <returns></returns>
        Public Property TagStr As String

        Public Overloads Shared Narrowing Operator CType(t As TagData(Of T, V)) As KeyValuePair(Of T, V)
            Return New KeyValuePair(Of T, V)(t.Tag, t.Value)
        End Operator

        ''' <summary>
        ''' 将这个标签数据转换为``元组``类型
        ''' </summary>
        ''' <param name="r"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(r As TagData(Of T, V)) As Tuple(Of T, V)
            Return New Tuple(Of T, V)(r.Tag, r.Value)
        End Operator
    End Class

    ''' <summary>
    ''' 使用一个整形数作为目标对象值的标签信息
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class IntegerTagged(Of T) : Inherits TagData(Of Integer, T)

    End Class

    ''' <summary>
    ''' 使用一个实数作为目标对象值的标签信息
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DoubleTagged(Of T) : Inherits TagData(Of Double, T)

    End Class

    ''' <summary>
    ''' 使用一个长整形数作为目标对象值的标签信息
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class LongTagged(Of T) : Inherits TagData(Of Long, T)

    End Class

    Public Class DateTagged(Of T) : Inherits TagData(Of Date, T)

    End Class

    Public Class VectorTagged(Of T) : Inherits TagData(Of Double(), T)

    End Class
End Namespace
