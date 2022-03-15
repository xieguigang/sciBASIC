#Region "Microsoft.VisualBasic::50f2e605b892092a30e287fa91ffc6b3, sciBASIC#\Data_science\Visualization\data\PCA\PCA\Matrix.vb"

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

    '   Total Lines: 68
    '    Code Lines: 38
    ' Comment Lines: 13
    '   Blank Lines: 17
    '     File Size: 1.57 KB


    ' Module Matrix
    ' 
    '     Sub: Ei, fffff, Test
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Module Matrix
    Sub Test()
        Dim m As GeneralMatrix = {
            {1, 8, 0.0},
            {2, 8, 0},
            {3, 8, 0},
            {4, 8, 1},
            {5, 9, 5}
        }

        '   Call test3(m)

        Dim result = m.SVD()

        Call result.S.Print
        Call Console.WriteLine()
        Call result.U.Print
        Call Console.WriteLine()
        Call result.V.Print

        Pause()
    End Sub

    Sub fffff()

        Dim C As GeneralMatrix = {{0.716, 0.615}, {0.615, 0.616}}
        Dim SVD = C.SVD
        Dim V = SVD.V
        Dim L = V(Which.IsTrue(SVD.SingularValues.Top(2)))
        Dim PCA = L * C

        Pause()
    End Sub

    Sub Ei()
        Dim m As GeneralMatrix = {{0.716, 0.615}, {0.615, 0.616}}
        Dim e = m.Eigen
        Dim d = e.D

        Dim svd = m.SVD

        Dim svd2 = m.Covariance.SVD
        Dim eeee = m.Covariance.Eigen

        Pause()
    End Sub

    'Sub test3(m As GeneralMatrix)
    '    Dim SVD As Double()()() = Microsoft.VisualBasic.DataMining.PCA.Matrix.singularValueDecomposition(m.Array)
    '    Dim U As GeneralMatrix = SVD(0)
    '    Dim S As GeneralMatrix = SVD(1)
    '    Dim V As GeneralMatrix = SVD(2)


    '    Call U.Print
    '    Call Console.WriteLine()
    '    Call V.Print
    '    Call Console.WriteLine()
    '    Call V.Print


    '    Pause()
    'End Sub
End Module
