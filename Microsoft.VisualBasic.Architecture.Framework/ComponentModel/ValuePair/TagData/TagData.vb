#Region "Microsoft.VisualBasic::f5b1e6f603e42074c2829d7ad34bce42, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\ValuePair\TagData\TagData.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.TagData

    Public Class TagData(Of T, V) : Inherits Value(Of V)

        Public Property Tag As T
        ''' <summary>
        ''' 默认都有一个字符串类型的标签用于保存其他的数据，但是主要还是使用<see cref="Tag"/>属性来进行标记
        ''' </summary>
        ''' <returns></returns>
        Public Property TagStr As String

        Public Overloads Shared Narrowing Operator CType(t As TagData(Of T, V)) As KeyValuePair(Of T, V)
            Return New KeyValuePair(Of T, V)(t.Tag, t.value)
        End Operator
    End Class

    ''' <summary>
    ''' 作用几乎等同于<see cref="DoubleTagged(Of T)"/>，只不过这个是Structure类型，开销会小一些
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NumericTagged(Of T)

        Dim Tag#
        Dim value As T

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Class IntegerTagged(Of T) : Inherits TagData(Of Integer, T)

    End Class

    Public Class DoubleTagged(Of T) : Inherits TagData(Of Double, T)

    End Class

    Public Class LongTagged(Of T) : Inherits TagData(Of Long, T)

    End Class

    Public Class DateTagged(Of T) : Inherits TagData(Of Date, T)

    End Class

    Public Class VectorTagged(Of T) : Inherits TagData(Of Double(), T)

    End Class
End Namespace
