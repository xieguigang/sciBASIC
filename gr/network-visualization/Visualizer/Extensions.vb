#Region "Microsoft.VisualBasic::8d20631774732ad993085b4270173581, gr\network-visualization\Visualizer\Extensions.vb"

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

    '   Total Lines: 30
    '    Code Lines: 22
    ' Comment Lines: 5
    '   Blank Lines: 3
    '     File Size: 1.06 KB


    ' Module Extensions
    ' 
    '     Function: GetDisplayText, NodeBrushAssert
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

<HideModuleName> Module Extensions

    <Extension>
    Public Function NodeBrushAssert(node As Node) As Predicate(Of Object)
        Return Function(null)
                   Return node Is Nothing OrElse
                          node.data Is Nothing OrElse
                          node.data.color Is Nothing
               End Function
    End Function

    ''' <summary>
    ''' 优先显示： <see cref="NodeData.label"/> -> <see cref="NodeData.origID"/> -> <see cref="Node.ID"/>
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetDisplayText(n As Node) As String
        If n.data Is Nothing OrElse (n.data.origID.StringEmpty AndAlso n.data.label.StringEmpty) Then
            Return n.label
        ElseIf n.data.label.StringEmpty Then
            Return n.data.origID
        Else
            Return n.data.label
        End If
    End Function
End Module
