﻿#Region "Microsoft.VisualBasic::f4be25b50901a1aed729f2ace89b47a2, G:/GCModeller/src/runtime/sciBASIC#/gr/Microsoft.VisualBasic.Imaging//Drawing3D/Math3D/MarchingCubes/Utils.vb"

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

    '   Total Lines: 10
    '    Code Lines: 7
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 288 B


    '     Module Utils
    ' 
    '         Function: VertexIndexToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Drawing3D.Math3D.MarchingCubes

    Module Utils

        Public Function VertexIndexToString(index As Integer) As String
            Return String.Format("{0}:{1}:{2}", index And 1, index >> 1 And 1, index >> 2 And 1)
        End Function
    End Module
End Namespace
