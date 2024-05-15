#Region "Microsoft.VisualBasic::4a75ab3da834bbea031a70d56464666b, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Vector.vb"

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

    '   Total Lines: 56
    '    Code Lines: 42
    ' Comment Lines: 4
    '   Blank Lines: 10
    '     File Size: 1.77 KB


    '     Module VectorMath
    ' 
    '         Function: CrossProduct, DotProduct, Magnitude, Normalize, SqrMagnitude
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math
Imports Vector3 = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing3D.Math3D

    ''' <summary>
    ''' helper for unity function
    ''' </summary>
    Public Module VectorMath

        <Extension>
        Public Function CrossProduct(v1 As Vector3, v2 As Vector3) As Vector3
            Dim i As Double = v1.Y * v2.Z - v2.Y * v1.Z
            Dim j As Double = -1 * (v1.X * v2.Z - v2.X * v1.Z)
            Dim k As Double = v1.X * v2.Y - v2.X * v1.Y

            Return New Vector3(i, j, k)
        End Function

        <Extension>
        Public Function DotProduct(v1 As Vector3, v2 As Vector3) As Double
            Return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z
        End Function

        <Extension>
        Public Function Magnitude(v As Vector3) As Double
            With v
                Return std.Sqrt(.X ^ 2 + .Y ^ 2 + .Z ^ 2)
            End With
        End Function

        <Extension>
        Public Function SqrMagnitude(v As Vector3) As Double
            With v
                Return .X ^ 2 + .Y ^ 2 + .Z ^ 2
            End With
        End Function

        <Extension>
        Public Function Normalize(v As Vector3) As Vector3
            Dim magnitude As Double = v.Magnitude()

            ' If the magnitude is 0 then return the zero vector instead of dividing by 0
            If magnitude = 0 Then
                Return New Vector3(0, 0, 0)
            Else
                Dim i = v.X / magnitude
                Dim j = v.Y / magnitude
                Dim k = v.Z / magnitude

                Return New Point3D(i, j, k)
            End If
        End Function
    End Module
End Namespace
