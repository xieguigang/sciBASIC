#Region "Microsoft.VisualBasic::7d7cf03c832af2f22bbf06260527867a, Data_science\Graph\EMD\Edge.vb"

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

    '   Total Lines: 86
    '    Code Lines: 65 (75.58%)
    ' Comment Lines: 4 (4.65%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 17 (19.77%)
    '     File Size: 2.33 KB


    '     Class Edge
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Edge0
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Edge1
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Edge2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Edge3
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace EMD

    ''' <summary>
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' 
    ''' </summary>
    Friend Class Edge
        Friend Sub New([to] As Integer, cost As Long)
            _to = [to]
            _cost = cost
        End Sub

        Friend _to As Integer
        Friend _cost As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (cost:{_cost})"
        End Function
    End Class

    Friend Class Edge0
        Friend Sub New([to] As Integer, cost As Long, flow As Long)
            _to = [to]
            _cost = cost
            _flow = flow
        End Sub

        Friend _to As Integer
        Friend _cost As Long
        Friend _flow As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (cost:{_cost}; flow:{_flow})"
        End Function
    End Class

    Friend Class Edge1
        Friend Sub New([to] As Integer, reduced_cost As Long)
            _to = [to]
            _reduced_cost = reduced_cost
        End Sub

        Friend _to As Integer
        Friend _reduced_cost As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (reduced_cost:{_reduced_cost})"
        End Function
    End Class

    Friend Class Edge2
        Friend Sub New([to] As Integer, reduced_cost As Long, residual_capacity As Long)
            _to = [to]
            _reduced_cost = reduced_cost
            _residual_capacity = residual_capacity
        End Sub

        Friend _to As Integer
        Friend _reduced_cost As Long
        Friend _residual_capacity As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (reduced_cost:{_reduced_cost}; residual_capacity:{_residual_capacity})"
        End Function
    End Class

    Friend Class Edge3
        Friend Sub New()
            _to = 0
            _dist = 0
        End Sub

        Friend Sub New([to] As Integer, dist As Long)
            _to = [to]
            _dist = dist
        End Sub

        Friend _to As Integer
        Friend _dist As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (dist:{_dist})"
        End Function
    End Class

End Namespace
