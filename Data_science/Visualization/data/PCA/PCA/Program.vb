#Region "Microsoft.VisualBasic::a3d4657f2edc3f42e13c14501ea7ddfa, sciBASIC#\Data_science\Visualization\data\PCA\PCA\Program.vb"

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

    '   Total Lines: 87
    '    Code Lines: 0
    ' Comment Lines: 67
    '   Blank Lines: 20
    '     File Size: 3.04 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::b085b4c4061a7062ff73b40c9197934d, Data_science\algorithms\PCA\PCA\Program.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    ' Module Program
'    ' 
'    '     Sub: Main, TestPCA_scores
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Data.csv
'Imports Microsoft.VisualBasic.Data.csv.IO
'Imports Microsoft.VisualBasic.Math.LinearAlgebra
'Imports Microsoft.VisualBasic.Math.Matrix

'Module Program

'    Sub Main()
'        Matrix.fffff()
'        Matrix.Ei()
'        Call TestPCA_scores()

'        ' Call Matrix.Test()
'        Dim input As GeneralMatrix = File.Load("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\flower.csv").AsMatrix
'        Dim PCA = DataMining.PCA.PrincipalComponentAnalysis(input, nPC:=2)

'        Call PCA.Print(out:="./PCA_result.txt".OpenWriter)

'        Call Test()
'    End Sub

'    '''' <summary>
'    '''' http://www.cnblogs.com/bigshuai/archive/2012/06/18/2553808.html
'    '''' </summary>
'    'Private Sub Test()
'    '    Dim data()() = {{-1, -1, 0.0, 2, 0}, {-2, 0R, 0, 1, 1}}.RowIterator.ToArray
'    '    Microsoft.VisualBasic.DataMining.PCA.Data.principalComponentAnalysis(data, 2).Print

'    '    Call Console.WriteLine()
'    '    ' Call New GeneralMatrix({New Double() {-3 / Math.Sqrt(2), -1 / Math.Sqrt(2), 0, 3 / Math.Sqrt(2), -1 / Math.Sqrt(2)}}).Print

'    '    Dim scores As GeneralMatrix = DataMining.PCA.Data.PCANIPALS(data, 2)
'    '    Call scores.Print

'    '    Pause()
'    'End Sub

'    Sub TestPCA_scores()
'        Dim input As GeneralMatrix = File.Load("G:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\scores.csv").AsMatrix(True, True)
'        Dim PCA = DataMining.PCA.PrincipalComponentAnalysis(input, nPC:=2)

'        Call PCA.Print

'        Pause()
'    End Sub
'End Module
