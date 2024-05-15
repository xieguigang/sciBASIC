#Region "Microsoft.VisualBasic::5886011bc516ba70438fec9e499fdbe5, gr\network-visualization\network_layout\Orthogonal\OEVertex.vb"

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

    '   Total Lines: 34
    '    Code Lines: 20
    ' Comment Lines: 9
    '   Blank Lines: 5
    '     File Size: 974 B


    '     Class OEVertex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OEVertex

        Public embedding As IList(Of OEElement)
        Public v As Integer
        Public x, y As Double

        Public Sub New(a_embedding As IList(Of OEElement), a_v As Integer, a_x As Double, a_y As Double)
            embedding = a_embedding
            v = a_v
            x = a_x
            y = a_y
        End Sub

        Public Overrides Function ToString() As String
            Dim tmp As String = "v(" & x.ToString() & "," & y.ToString() & "):"
            For Each e As OEElement In embedding
                tmp += " " & e.ToString()
            Next
            Return tmp
        End Function
    End Class

End Namespace
