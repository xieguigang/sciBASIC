#Region "Microsoft.VisualBasic::17718f49fb756469c74ff02ddccfecbf, Data_science\MachineLearning\VAE\GMM\EMGaussianMixtureModel\GaussianMixtureComponent.vb"

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

    '   Total Lines: 50
    '    Code Lines: 40 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.00%)
    '     File Size: 2.38 KB


    '     Class GaussianMixtureComponent
    ' 
    '         Properties: CovMatrix, Mean, Position, Weight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: componentPDFandProb, multiVariateGaussianPDF, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports RealMatrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Class GaussianMixtureComponent

        Const PI As Double = std.PI
        Const e As Double = std.E

        Public ReadOnly Property Position As Integer
        Public Overridable ReadOnly Property Mean As RealMatrix
        Public Overridable ReadOnly Property CovMatrix As RealMatrix
        Public Overridable ReadOnly Property Weight As Double

        Public Sub New(position As Integer, mean As RealMatrix, covMatrix As RealMatrix, weight As Double)
            Me.Position = position
            Me.Mean = mean
            Me.CovMatrix = covMatrix
            Me.Weight = weight
        End Sub

        Public Sub New(position As Integer, mean As Double(), variance As Double()(), weight As Double)
            Me.New(position, New RealMatrix(mean), New RealMatrix(variance), weight)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function componentPDFandProb(x As Double()) As Double
            Return _Weight * multiVariateGaussianPDF(x, _Mean, _CovMatrix)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Position}] weight: {Weight}"
        End Function

        Public Shared Function multiVariateGaussianPDF(x As Double(), means As RealMatrix, CovMatrix As RealMatrix) As Double
            Dim d = x.Length
            Dim xMatrix As RealMatrix = New RealMatrix(x)
            Dim xMinusMeansMatrix = xMatrix - means
            Dim lu As LUDecomposition = New LUDecomposition(CovMatrix)
            Dim CovInverse As RealMatrix = CType(CType(lu.Solve(CType(RealMatrix.Identity(lu.Pivot.Length), RealMatrix)), RealMatrix).Inverse(), RealMatrix)
            Dim eExponent As Double = -0.5 * (CType((CType(xMinusMeansMatrix.Transpose(), RealMatrix) * CovInverse), RealMatrix) * xMinusMeansMatrix)(0, 0)
            Dim CovMatrixDeterminant As Double = (New LUDecomposition(CovMatrix)).Determinant()

            Return 1 / (std.Pow(2 * PI, d / 2) * std.Sqrt(CovMatrixDeterminant)) * std.Pow(e, eExponent)
        End Function
    End Class
End Namespace
