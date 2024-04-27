﻿#Region "Microsoft.VisualBasic::3368a71463cbdeae93d9961338ada045, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/data/LP/LP_tool//LPPExamples.vb"

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

    '   Total Lines: 25
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 2.10 KB


    ' Class LPPExamples
    ' 
    '     Function: maximizeExample, minimizeExample, smallMinimizeExample, strictEquality, transshipment
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming

Public Class LPPExamples

    Public Shared Function minimizeExample() As LPP
        Return New LPP("Min", New String() {}, New Double() {1, -1, 2, -5}, New Double()() {New Double() {1, 1, 2, 4}, New Double() {0, 3, 1, 8}}, New String() {"=", "="}, New Double() {6, 3}, 0)
    End Function

    Public Shared Function smallMinimizeExample() As LPP
        Return New LPP("Min", New String() {"a", "b"}, New Double() {0.6, 0.8}, New Double()() {New Double() {0.6, 0.2}, New Double() {0.1, 0.5}}, New String() {"�", "�"}, New Double() {30, 26}, 0)

    End Function

    Public Shared Function maximizeExample() As LPP
        Return New LPP("Max", New String() {}, New Double() {2, 3, 3}, New Double()() {New Double() {3, 2, 0}, New Double() {-1, 1, 4}, New Double() {2, -2, 5}}, New String() {"�", "�", "�"}, New Double() {60, 10, 50}, 0)
    End Function

    Public Shared Function transshipment() As LPP
        Return New LPP("Min", New String() {}, New Double() {16, 21, 18, 16, 22, 25, 23, 15, 29, 20, 17, 24}, New Double()() {New Double() {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, New Double() {0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0}, New Double() {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0}, New Double() {1, 0, 1, 0, 1, 0, -1, -1, -1, 0, 0, 0}, New Double() {0, 1, 0, 1, 0, 1, 0, 0, 0, -1, -1, -1}, New Double() {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0}, New Double() {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0}, New Double() {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1}}, New String() {"=", "=", "=", "=", "=", "�", "�", "�"}, New Double() {72, 105, 83, 0, 0, 90, 80, 120}, 0)
    End Function

    Public Shared Function strictEquality() As LPP
        Return New LPP("Min", New String() {}, New Double() {3, 4, 5, 2, 7, 8}, New Double()() {New Double() {1, 1, 1, 0, 0, 0}, New Double() {0, 0, 0, 0, 1, 1}, New Double() {1, 0, 0, -1, 0, 0}, New Double() {0, 1, 0, 1, -1, 0}, New Double() {0, 0, 1, 0, 0, -1}}, New String() {"=", "=", "=", "=", "="}, New Double() {5, 5, 0, 0, 0}, 0)
    End Function
End Class
