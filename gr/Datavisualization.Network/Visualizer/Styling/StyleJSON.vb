#Region "Microsoft.VisualBasic::f145c2473f3da02478bed8a21815c23f, ..\sciBASIC#\gr\Datavisualization.Network\Visualizer\Styling\StyleJSON.vb"

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

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' 字典之中的键名都是一个条件表达式，selector的数据源为``<see cref="NodeData"/>``和``<see cref="EdgeData"/>``
    ''' </summary>
    Public Class StyleJSON

        Public Property nodes As Dictionary(Of String, NodeStyle)
        Public Property edge As Dictionary(Of String, EdgeStyle)
        ''' <summary>
        ''' 这个指的是node label
        ''' </summary>
        ''' <returns></returns>
        Public Property labels As Dictionary(Of String, LabelStyle)

    End Class

    Public MustInherit Class Styles

        ''' <summary>
        ''' 线条的样式
        ''' </summary>
        ''' <returns></returns>
        Public Property stroke As String
        ''' <summary>
        ''' 节点的填充颜色，边的填充颜色，以及文本的填充颜色
        ''' </summary>
        ''' <returns></returns>
        Public Property fill As String

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class

    Public Class NodeStyle : Inherits Styles

        ''' <summary>
        ''' 节点大小的表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property size As String
        ''' <summary>
        ''' 节点的形状的表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property shape As String

    End Class

    Public Class EdgeStyle : Inherits Styles
    End Class

    Public Class LabelStyle : Inherits Styles

        ''' <summary>
        ''' CSS字体表达式
        ''' </summary>
        ''' <returns></returns>
        Public Property FontCSS As String

    End Class
End Namespace
