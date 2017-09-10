#Region "Microsoft.VisualBasic::98b7ab5bef228d13108bacac023a6a74, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\PCA\PCA.vb"

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

Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace PCA

    Public Class PCA

        ''' <summary>
        ''' 使用<see cref="CenterNormalize(GeneralMatrix)"/>特征中心化之后的结果矩阵
        ''' </summary>
        ''' <returns></returns>
        Public Property B As GeneralMatrix
        ''' <summary>
        ''' B的协方差矩阵C
        ''' </summary>
        ''' <returns></returns>
        Public Property C As GeneralMatrix

        ''' <summary>
        ''' SVD(<see cref="C"/>)
        ''' </summary>
        ''' <returns></returns>
        Public Property SVD As SingularValueDecomposition

        Public ReadOnly Property S As Vector
            Get
                Return SVD.SingularValues
            End Get
        End Property

        Public ReadOnly Property V As GeneralMatrix
            Get
                Return SVD.V
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return S.ToString
        End Function
    End Class
End Namespace
