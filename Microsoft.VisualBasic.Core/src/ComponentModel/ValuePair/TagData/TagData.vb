#Region "Microsoft.VisualBasic::827f8bb732aeff55eaa035038c6674e9, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\TagData\TagData.vb"

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

    '   Total Lines: 121
    '    Code Lines: 55 (45.45%)
    ' Comment Lines: 41 (33.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 25 (20.66%)
    '     File Size: 3.79 KB


    '     Class TagData
    ' 
    '         Properties: Key, Tag, TagStr
    ' 
    '     Class IntegerTagged
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class DoubleTagged
    ' 
    '         Properties: Tag
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Interface INumericKey
    ' 
    '         Properties: key
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
    ''' <typeparam name="T">The type of the <see cref="Tag"/> key</typeparam>
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
        Public Overridable Property Tag As T

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

        Sub New()
        End Sub

        Sub New(i As Integer, val As T)
            Tag = i
            Value = val
        End Sub

    End Class

    ''' <summary>
    ''' 使用一个实数作为目标对象值的标签信息
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DoubleTagged(Of T) : Inherits TagData(Of Double, T)
        Implements INumericKey

        Public Overrides Property Tag As Double Implements INumericKey.key

        Sub New()
        End Sub

        Sub New(tag As Double, data As T)
            Me.Tag = tag
            Me.Value = data
        End Sub

        Public Overrides Function ToString() As String
            Return $"({TagStr}) {Tag.ToString("G3")} - {Value.ToString}"
        End Function
    End Class

    ''' <summary>
    ''' an object that tagged a number value as its index key
    ''' </summary>
    Public Interface INumericKey

        ''' <summary>
        ''' a number index key
        ''' </summary>
        ''' <returns></returns>
        Property key As Double

    End Interface

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
