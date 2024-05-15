#Region "Microsoft.VisualBasic::7e0c689b978f381c65b0f5aad7b4f226, Data_science\Graph\Analysis\Louvain\Edge.vb"

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

    '   Total Lines: 22
    '    Code Lines: 12
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 576 B


    '     Class Edge
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Analysis.Louvain

    Friend Class Edge

        ''' <summary>
        ''' v表示连接点的编号,w表示此边的权值
        ''' </summary>
        Friend v As Integer
        Friend weight As Double
        ''' <summary>
        ''' next负责连接和此点相关的边
        ''' </summary>
        Friend [next] As Integer

        Friend Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{v} -> {[next]}] {weight.ToString("F4")}"
        End Function
    End Class
End Namespace
