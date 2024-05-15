#Region "Microsoft.VisualBasic::7d0b58a6f6b3b0ca1023df018be39184, gr\network-visualization\Datavisualization.Network\Analysis\Model\EdgeSet.vb"

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

    '   Total Lines: 27
    '    Code Lines: 16
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 865 B


    '     Class EdgeSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace Analysis.Model

    ''' <summary>
    ''' 两个节点对象之间的重复的边链接的集合
    ''' </summary>
    ''' <remarks>
    ''' 所有的<see cref="IInteraction.source"/>和<see cref="IInteraction.target"/>都是一样的
    ''' </remarks>
    Public Class EdgeSet(Of Edge As IInteraction) : Inherits List(Of Edge)

        Sub New()
            Call MyBase.New
        End Sub

        Public Overrides Function ToString() As String
            Dim first As Edge = Me.First

            If Count = 1 Then
                Return $"[{first.source}, {first.target}]"
            Else
                Return $"[{first.source}, {first.target}] have {Count} duplicated connections."
            End If
        End Function
    End Class
End Namespace
