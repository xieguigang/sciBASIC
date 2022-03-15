#Region "Microsoft.VisualBasic::56d056d2ab346e48034388638d38b322, sciBASIC#\Data_science\Visualization\data\PCA\PCA\Module1.vb"

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

    '   Total Lines: 42
    '    Code Lines: 26
    ' Comment Lines: 4
    '   Blank Lines: 12
    '     File Size: 1.20 KB


    ' Module Module1
    ' 
    '     Sub: Main, methodTest, test2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module Module1

    Sub Main()
        Call test2()
        Call methodTest()

        Pause()
    End Sub

    Sub test2()
        Dim data = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\data.csv")
        Dim pca As New PCA(data.Matrix, scale:=True)

        ' Call pca.ExplainedVariance.ToString.__DEBUG_ECHO
        Call Console.WriteLine(pca.Summary)
        Call pca.Project(data.Matrix.Select(Function(v) v.AsVector).ToArray, nPC:=2)

        Pause()
    End Sub

    ''' <summary>
    ''' https://github.com/mljs/pca
    ''' </summary>
    Sub methodTest()
        Dim data = DataSet.LoadDataSet("C:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\iris.csv", uidMap:="class")

        Dim pca As New PCA(data.Matrix)

        Call pca.ExplainedVariance.ToString.__DEBUG_ECHO

        Dim newPoints = {{4.9, 3.2, 1.2, 0.4}.AsVector, {5.4, 3.3, 1.4, 0.9}.AsVector}

        For Each x In pca.Project(newPoints, 2)
            Call x.ToString.__DEBUG_ECHO
        Next

        Pause()
    End Sub
End Module
