#Region "Microsoft.VisualBasic::7a7884058b36800e271831ad5665e65b, ..\sciBASIC#\Data_science\Mathematical\data\LP\LP_tool\Program.vb"

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

Imports Microsoft.VisualBasic.Mathematical.LP
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Call Test()
        Call Test2()
    End Sub

    Sub Test()
        Dim min As New ObjectiveFunction With {
            .type = OptimizationType.MIN,
            .xyz = {-2, -3, -4},
            .Z = 1
        }
        Dim matrix = {
            New Equation With {
                .xyz = {3, 2, 1},
                .c = 10
            },
            New Equation With {
                .xyz = {2, 5, 3},
                .c = 15
            }
        }
        Dim result#() = min.Solve(matrix)

        ' x=y=z=0,s=10,t=15
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Public Sub Test2()
        Dim min As New ObjectiveFunction With {
            .type = OptimizationType.MAX,
            .xyz = {6, 5, 4},
            .Z = 1
        }
        Dim matrix = {
            New Equation With {.xyz = {2, 1, 1}, .c = 180},
            New Equation With {.xyz = {1, 3, 2}, .c = 300},
            New Equation With {.xyz = {2, 1, 2}, .c = 240}
        }
        Dim result#() = min.Solve(matrix)

        ' {708.0, 48.0, 84.0, 0.0, 0.0, 0.0, 60.0, 0.0}
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub
End Module
