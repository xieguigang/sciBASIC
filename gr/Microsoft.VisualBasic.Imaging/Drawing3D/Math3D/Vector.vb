#Region "Microsoft.VisualBasic::b9e5f459c6c2c27072fceeaceb3a6bce, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Vector.vb"

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

    '   Total Lines: 46
    '    Code Lines: 36
    ' Comment Lines: 1
    '   Blank Lines: 9
    '     File Size: 1.48 KB


    '     Module VectorMath
    ' 
    '         Function: CrossProduct, DotProduct, Magnitude, Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports sys = System.Math
Imports Vector = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing3D.Math3D

    Public Module VectorMath

        <Extension>
        Public Function CrossProduct(v1 As Vector, v2 As Vector) As Vector
            Dim i As Double = v1.Y * v2.Z - v2.Y * v1.Z
            Dim j As Double = -1 * (v1.X * v2.Z - v2.X * v1.Z)
            Dim k As Double = v1.X * v2.Y - v2.X * v1.Y

            Return New Vector(i, j, k)
        End Function

        <Extension>
        Public Function DotProduct(v1 As Vector, v2 As Vector) As Double
            Return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z
        End Function

        <Extension>
        Public Function Magnitude(v As Vector) As Double
            With v
                Return sys.Sqrt(.X ^ 2 + .Y ^ 2 + .Z ^ 2)
            End With
        End Function

        <Extension>
        Public Function Normalize(v As Vector) As Vector
            Dim magnitude As Double = v.Magnitude()

            ' If the magnitude is 0 then return the zero vector instead of dividing by 0
            If magnitude = 0 Then
                Return New Vector(0, 0, 0)
            Else
                Dim i = v.X / magnitude
                Dim j = v.Y / magnitude
                Dim k = v.Z / magnitude

                Return New Point3D(i, j, k)
            End If
        End Function
    End Module
End Namespace
