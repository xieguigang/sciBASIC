#Region "Microsoft.VisualBasic::a49c9e5c19ca5cedcb7e27beed8739b5, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\MaxPlainCameraView.vb"

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

    '   Total Lines: 170
    '    Code Lines: 106 (62.35%)
    ' Comment Lines: 34 (20.00%)
    '    - Xml Docs: 52.94%
    ' 
    '   Blank Lines: 30 (17.65%)
    '     File Size: 6.71 KB


    '     Class MaxPlainCameraView
    ' 
    '         Function: CalculateCameraPosition, CalculateCentroid, CalculateCovarianceMatrix, CalculateOptimalCameraView, CalculatePlaneNormalViaSVD
    ' 
    '         Sub: SVDecomposition
    ' 
    '     Structure Matrix3x3
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Drawing3D.Math3D

    Public Class MaxPlainCameraView

        ''' <summary>
        ''' 计算最优摄像头视角来垂直俯瞰点云中的主导平面
        ''' </summary>
        ''' <param name="points">三维点云集合</param>
        ''' <param name="distanceScale">摄像头距离缩放因子</param>
        ''' <returns>摄像头位置坐标</returns>
        Public Shared Function CalculateOptimalCameraView(points As IEnumerable(Of Point3D), Optional distanceScale As Double = 2.0) As Point3D
            ' 1. 将点转换为适合矩阵操作的形式
            Dim pointList = points.ToList()
            Dim count = pointList.Count

            ' 2. 计算点云的中心点（质心）
            Dim centroid = CalculateCentroid(pointList)

            ' 3. 构建协方差矩阵
            Dim covarianceMatrix = CalculateCovarianceMatrix(pointList, centroid)

            ' 4. 使用SVD分解计算平面法向量
            Dim normalVector = CalculatePlaneNormalViaSVD(covarianceMatrix)

            ' 5. 计算摄像头位置（沿法向量方向反向移动一定距离）
            Dim cameraPosition = CalculateCameraPosition(centroid, normalVector, pointList, distanceScale)

            Return cameraPosition
        End Function

        ''' <summary>
        ''' 计算点云质心
        ''' </summary>
        Private Shared Function CalculateCentroid(points As List(Of Point3D)) As Point3D
            Dim sumX = 0.0, sumY = 0.0, sumZ = 0.0

            For Each pt In points
                sumX += pt.X
                sumY += pt.Y
                sumZ += pt.Z
            Next

            Dim count = points.Count
            Return New Point3D(sumX / count, sumY / count, sumZ / count)
        End Function

        ''' <summary>
        ''' 计算协方差矩阵
        ''' </summary>
        Private Shared Function CalculateCovarianceMatrix(points As List(Of Point3D), centroid As Point3D) As Matrix3x3
            Dim xx = 0.0, yy = 0.0, zz = 0.0
            Dim xy = 0.0, xz = 0.0, yz = 0.0

            For Each pt In points
                Dim dx = pt.X - centroid.X
                Dim dy = pt.Y - centroid.Y
                Dim dz = pt.Z - centroid.Z

                xx += dx * dx
                yy += dy * dy
                zz += dz * dz
                xy += dx * dy
                xz += dx * dz
                yz += dy * dz
            Next

            Dim count = points.Count
            Return New Matrix3x3(
            xx / count, xy / count, xz / count,
            xy / count, yy / count, yz / count,
            xz / count, yz / count, zz / count)
        End Function

        ''' <summary>
        ''' 通过SVD计算平面法向量
        ''' </summary>
        Private Shared Function CalculatePlaneNormalViaSVD(covarianceMatrix As Matrix3x3) As Point3D
            ' 简化版SVD实现 - 实际应用中可能需要完整的SVD库
            ' 这里使用特征值分解近似求解最小特征值对应的特征向量

            Dim matrixArray = {
            {covarianceMatrix.M11, covarianceMatrix.M12, covarianceMatrix.M13},
            {covarianceMatrix.M21, covarianceMatrix.M22, covarianceMatrix.M23},
            {covarianceMatrix.M31, covarianceMatrix.M32, covarianceMatrix.M33}
        }

            ' 使用雅可比法近似计算特征值和特征向量
            Dim eigenvalues As Double() = {0, 0, 0}
            Dim eigenvectors As Double(,) = {{0, 0, 0}, {0, 0, 0}, {0, 0, 0}}

            SVDecomposition(matrixArray, eigenvalues, eigenvectors)

            ' 找到最小特征值的索引
            Dim minIndex = 0
            If eigenvalues(1) < eigenvalues(minIndex) Then minIndex = 1
            If eigenvalues(2) < eigenvalues(minIndex) Then minIndex = 2

            ' 返回对应的特征向量（平面法向量）
            Return New Point3D(eigenvectors(0, minIndex), eigenvectors(1, minIndex), eigenvectors(2, minIndex))
        End Function

        ''' <summary>
        ''' 计算摄像头位置
        ''' </summary>
        Private Shared Function CalculateCameraPosition(centroid As Point3D, normal As Point3D, points As List(Of Point3D), distanceScale As Double) As Point3D
            ' 计算点云的范围大小
            Dim minX = points.Min(Function(p) p.X)
            Dim maxX = points.Max(Function(p) p.X)
            Dim minY = points.Min(Function(p) p.Y)
            Dim maxY = points.Max(Function(p) p.Y)
            Dim minZ = points.Min(Function(p) p.Z)
            Dim maxZ = points.Max(Function(p) p.Z)

            Dim sizeX = maxX - minX
            Dim sizeY = maxY - minY
            Dim sizeZ = maxZ - minZ

            ' 计算摄像头距离（基于点云大小和缩放因子）
            Dim maxSize = std.Max(std.Max(sizeX, sizeY), sizeZ)
            Dim distance = maxSize * distanceScale

            ' 沿法向量方向移动摄像头到适当位置
            Dim cameraX = centroid.X + normal.X * distance
            Dim cameraY = centroid.Y + normal.Y * distance
            Dim cameraZ = centroid.Z + normal.Z * distance

            Return New Point3D(cameraX, cameraY, cameraZ)
        End Function

        Private Shared Sub SVDecomposition(matrix As Double(,), ByRef eigenvalues As Double(), ByRef eigenvectors As Double(,))
            Dim svd As New SingularValueDecomposition(matrix)

            ' one-dimensional array of singular values
            ' diagonal of S.
            eigenvalues = svd.SingularValues
            ' the right singular vectors
            eigenvectors = svd.V.ArrayPack.ToMatrix
        End Sub
    End Class

    Public Structure Matrix3x3
        Public M11 As Double
        Public M12 As Double
        Public M13 As Double
        Public M21 As Double
        Public M22 As Double
        Public M23 As Double
        Public M31 As Double
        Public M32 As Double
        Public M33 As Double

        Public Sub New(m11 As Double, m12 As Double, m13 As Double,
                   m21 As Double, m22 As Double, m23 As Double,
                   m31 As Double, m32 As Double, m33 As Double)
            Me.M11 = m11
            Me.M12 = m12
            Me.M13 = m13
            Me.M21 = m21
            Me.M22 = m22
            Me.M23 = m23
            Me.M31 = m31
            Me.M32 = m32
            Me.M33 = m33
        End Sub
    End Structure
End Namespace
