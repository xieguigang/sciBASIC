#Region "Microsoft.VisualBasic::8835f87b4953650d6891af727a63ffe9, gr\network-visualization\Visualizer\Styling\CSS\StyleObject.vb"

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

    '   Total Lines: 50
    '    Code Lines: 19
    ' Comment Lines: 20
    '   Blank Lines: 11
    '     File Size: 1.29 KB


    '     Class Styles
    ' 
    '         Properties: fill, stroke
    ' 
    '         Function: ToString
    ' 
    '     Class NodeStyle
    ' 
    '         Properties: shape, size
    ' 
    '     Class EdgeStyle
    ' 
    ' 
    ' 
    '     Class LabelStyle
    ' 
    '         Properties: FontCSS
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling.CSS

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
