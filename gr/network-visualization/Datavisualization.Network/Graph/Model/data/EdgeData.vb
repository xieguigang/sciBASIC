#Region "Microsoft.VisualBasic::e806ebc1fcd2e284ed465b34ec0027c1, gr\network-visualization\Datavisualization.Network\Graph\Model\data\EdgeData.vb"

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

'     Class EdgeData
' 
'         Properties: bends, color, length
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Clone, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graph

    Public Class EdgeData : Inherits GraphData

        ''' <summary>
        ''' 这个属性值一般是由两个节点之间的坐标位置所计算出来的欧几里得距离
        ''' </summary>
        ''' <returns></returns>
        Public Property length As Single
        Public Property bends As XYMetaHandle()
        Public Property color As SolidBrush

        Public Sub New()
            MyBase.New()

            length = 1.0F
        End Sub

        ''' <summary>
        ''' Value copy
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As EdgeData)
            Me.label = copy.label
            Me.length = copy.length
            Me.Properties = New Dictionary(Of String, String)(copy.Properties)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function Clone() As EdgeData
            Return New EdgeData With {
                .label = label,
                .bends = bends.SafeQuery.Select(Function(a) New XYMetaHandle(a)).ToArray,
                .color = color,
                .length = length,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function
    End Class
End Namespace
