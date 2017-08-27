#Region "Microsoft.VisualBasic::8b473171a3138bd992ffa06af3a07610, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\PCA\Method.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Namespace PCA

    ''' <summary>
    ''' Performs principal component analysis on a set of data and returns the resulting data set. The
    ''' QR algorithm is used to find the eigenvalues and orthonormal eigenvectors of the covariance
    ''' matrix of the data set. The eigenvectors corresponding to the largest eigenvalues are the
    ''' principal components. The data file should be in the same directory as the PCA.class file.
    ''' All numbers should be tab-delimited. The first line of the data should be two numbers: the 
    ''' number of rows R followed by the number of columns C. After that, there should be R lines of 
    ''' C tab-delimited values. The columns would most likely represent the dimensions of measure; the
    ''' rows would each represent a single data point.
    ''' @author	Kushal Ranjan
    ''' @version	051513
    ''' </summary>
    ''' <remarks>
    ''' + https://github.com/kranjan94/Principal-Component-Analysis
    ''' + https://stats.stackexchange.com/questions/222/what-are-principal-component-scores
    ''' </remarks>
    Public Module Method

        '''' <summary>
        '''' Test code. Constructs an arbitrary data table of 5 data points with 3 variables, normalizes
        '''' it, and computes the covariance matrix and its eigenvalues and orthonormal eigenvectors.
        '''' Then determines the two principal components.
        '''' </summary>
        'Public Shared Sub Main(args As String())
        '    Dim data__1 As Double()() = {New Double() {4, 4.2, 3.9, 4.3, 4.1}, New Double() {2, 2.1, 2, 2.1, 2.2}, New Double() {0.6, 0.59, 0.58, 0.62, 0.63}}
        '    Console.WriteLine("Raw data:")
        '    Matrix.print(data__1)
        '    Dim dat As New Data(data__1)
        '    dat.matrixData = dat.matrixData.CenterNormalize
        '    Dim cov As Double()() = dat.covarianceMatrix()
        '    Console.WriteLine("Covariance matrix:")
        '    Matrix.print(cov)
        '    Dim eigen As EigenSet = dat.CovarianceEigenSet
        '    Dim vals As Double()() = {eigen.values}
        '    Console.WriteLine("Eigenvalues:")
        '    Matrix.print(vals)
        '    Console.WriteLine("Corresponding eigenvectors:")
        '    Matrix.print(eigen.vectors)
        '    Console.WriteLine("Two principal components:")
        '    Matrix.print(dat.buildPrincipalComponents(2, eigen))
        '    Console.WriteLine("Principal component transformation:")
        '    Matrix.print(Data.principalComponentAnalysis(data__1, 2))

        '    Dim scores#()() = Data.PCANIPALS(data__1, 3)
        '    Matrix.print(scores)

        '    Pause()
        'End Sub

        '''' <summary>
        '''' PCA implemented using the NIPALS algorithm. The return value is a double[][], where each
        '''' double[] j is an array of the scores of the jth data point corresponding to the desired
        '''' number of principal components. 
        '''' (使用这个函数计算出PCA得分)
        '''' </summary>
        '''' <param name="input">			input raw data array </param>
        '''' <param name="numComponents">	desired number of PCs
        '''' @return				the scores of the data array against the PCS </param>
        'Public Shared Function PCANIPALS(input As Double()(), numComponents As Integer) As Double()()
        '    Dim data As New Data(input)
        '    data.matrixData = data.matrixData.CenterNormalize()
        '    Dim PCA As Double()()() = data.NIPALSAlg(numComponents)
        '    'ORIGINAL LINE: double[][] scores = new double[numComponents][input[0].Length];
        '    Dim scores As Double()() = MAT(Of Double)(numComponents, input(0).Length)
        '    For point As Integer = 0 To scores(0).Length - 1
        '        For comp As Integer = 0 To PCA.Length - 1
        '            scores(comp)(point) = PCA(comp)(0)(point)
        '        Next
        '    Next
        '    Return scores
        'End Function

        '''' <summary>
        '''' Implementation of the non-linear iterative partial least squares algorithm on the data
        '''' matrix for this Data object. The number of PCs returned is specified by the user. </summary>
        '''' <param name="numComponents">	number of principal components desired
        '''' @return				a double[][][] where the ith double[][] contains ti and pi, the scores
        '''' 						and loadings, respectively, of the ith principal component. </param>
        'Friend Overridable Function NIPALSAlg(numComponents As Integer) As Double()()()
        '    Const THRESHOLD As Double = 0.00001
        '    Dim out As Double()()() = New Double(numComponents - 1)()() {}
        '    Dim E As Double()() = Matrix.copy(matrixData.Array)
        '    For i As Integer = 0 To out.Length - 1
        '        Dim eigenOld As Double = 0
        '        Dim eigenNew As Double = 0
        '        Dim p As Double() = New Double(matrixData.Array(0).Length - 1) {}
        '        Dim t As Double() = New Double(matrixData.Array(0).Length - 1) {}
        '        Dim tMatrix As Double()() = {t}
        '        Dim pMatrix As Double()() = {p}
        '        For j As Integer = 0 To t.Length - 1
        '            t(j) = matrixData(i, j)
        '        Next
        '        Do
        '            eigenOld = eigenNew
        '            Dim tMult As Double = 1 / Matrix.dot(t, t)
        '            tMatrix(0) = t
        '            p = Matrix.scale(Matrix.multiply(Matrix.transpose(E), tMatrix), tMult)(0)
        '            p = Matrix.normalize(p)
        '            Dim pMult As Double = 1 / Matrix.dot(p, p)
        '            pMatrix(0) = p
        '            t = Matrix.scale(Matrix.multiply(E, pMatrix), pMult)(0)
        '            eigenNew = Matrix.dot(t, t)
        '        Loop While Math.Abs(eigenOld - eigenNew) > THRESHOLD
        '        tMatrix(0) = t
        '        pMatrix(0) = p
        '        Dim PC As Double()() = {t, p}
        '        '{scores, loadings}
        '        E = Matrix.subtract(E, Matrix.multiply(tMatrix, Matrix.transpose(pMatrix)))
        '        out(i) = PC
        '    Next
        '    Return out
        'End Function

        ''' <summary>
        ''' Performs principal component analysis with a specified number of principal components. 
        ''' (使用这个函数来进行PCA分析)
        ''' </summary>
        ''' <param name="input">input data; each double[] in input is an array of values of a single
        ''' variable for each data point </param>
        ''' <param name="nPC">number of components desired
        ''' @return	the transformed data set</param>
        <Extension> Public Function PrincipalComponentAnalysis(input As GeneralMatrix, nPC As Integer) As GeneralMatrix
            Dim B As GeneralMatrix = input.Transpose.CenterNormalize
            Dim C = B.Covariance
            Dim SVD = C.SVD
            Dim V = SVD.V
            Dim S = SVD.SingularValues
            Dim M = V(Which.IsTrue(S.Top(nPC)))
            Dim PCA = input * M
            Return PCA
        End Function
    End Module
End Namespace

