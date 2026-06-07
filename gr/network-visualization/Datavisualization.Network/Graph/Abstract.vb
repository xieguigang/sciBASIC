#Region "Microsoft.VisualBasic::bddab4d6d6c8a40594e1d26ef72f929d, gr\network-visualization\Datavisualization.Network\Graph\Abstract.vb"

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

'   Total Lines: 7
'    Code Lines: 4 (57.14%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 3 (42.86%)
'     File Size: 146 B


' 
' /********************************************************************************/

#End Region

Namespace Graph.Abstract

    ''' <summary>
    ''' An class object that contains <see cref="GraphData"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IGraphValueContainer(Of T As GraphData)
        Property data As T
    End Interface

    ''' <summary>
    ''' 这是一个节点对象
    ''' </summary>
    Public Class GraphTreeNode

        ''' <summary>
        ''' 从外部链接到当前节点的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Parents As List(Of GraphTreeNode)
        ''' <summary>
        ''' 从当前的节点出发，链接到的其他的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Childs As List(Of GraphTreeNode)
        Public Property Node As Node

        Sub New()
            Parents = New List(Of GraphTreeNode)
            Childs = New List(Of GraphTreeNode)
        End Sub

        Public Overrides Function ToString() As String
            Return Node.ToString
        End Function
    End Class
End Namespace
