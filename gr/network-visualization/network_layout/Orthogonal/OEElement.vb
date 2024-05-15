#Region "Microsoft.VisualBasic::4919e066066a2a81ab8a38a0a8843795, gr\network-visualization\network_layout\Orthogonal\OEElement.vb"

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

    '   Total Lines: 62
    '    Code Lines: 35
    ' Comment Lines: 17
    '   Blank Lines: 10
    '     File Size: 2.21 KB


    '     Class OEElement
    ' 
    '         Constructor: (+2 Overloads) Sub New
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
    Public Class OEElement

        ' 
        ' 		    // counter clock wise:
        ' 		    public static final int RIGHT = 0;
        ' 		    public static final int UP = 1;
        ' 		    public static final int LEFT = 2;
        ' 		    public static final int DOWN = 3;
        ' 		 
        ' clock wise (in mathematical coordinates, in which y is inverted wrt to screen coordinates):
        Public Shared ReadOnly directionNames As String() = New String() {"right", "down", "left", "up"}
        Public Const RIGHT As Integer = 0
        Public Const DOWN As Integer = 1
        Public Const LEFT As Integer = 2
        Public Const UP As Integer = 3

        Public v As Integer ' the origin node
        Public dest As Integer ' the target node
        Public angle As Integer ' 0 : right, 1: up, 2: left, 3: down
        Public bends As Integer ' number of left bends
        Public sym As OEElement ' the symmetric element in the target node

        Public bendsToAddToSymmetric As Integer

        Public Sub New(a_v As Integer, a_dest As Integer, a_angle As Integer, a_bends As Integer)
            v = a_v
            dest = a_dest
            angle = a_angle
            bends = a_bends
            sym = Nothing

            bendsToAddToSymmetric = 0
        End Sub

        Public Sub New(a_v As Integer, a_dest As Integer, a_angle As Integer, a_bends As Integer, a_sym As OEElement)
            v = a_v
            dest = a_dest
            angle = a_angle
            bends = a_bends
            sym = a_sym

            bendsToAddToSymmetric = 0
        End Sub

        Public Overrides Function ToString() As String
            Dim angles = New String() {"right", "up", "left", "down"}
            Return "(" & v.ToString() & " -> " & dest.ToString() & "," & angles(angle) & "," & bends.ToString() & ")"
        End Function
    End Class

End Namespace
