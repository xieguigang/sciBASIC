#Region "Microsoft.VisualBasic::35508495f6a2db5b7c2f116f42921d89, Data_science\MachineLearning\xgboost\util\FVec\FVecTransformer.vb"

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

    '   Total Lines: 61
    '    Code Lines: 19 (31.15%)
    ' Comment Lines: 35 (57.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (11.48%)
    '     File Size: 2.50 KB


    '     Module FVecTransformer
    ' 
    '         Function: (+4 Overloads) fromArray, fromMap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace util

    ''' <summary>
    ''' 构建矩阵所使用的方法
    ''' </summary>
    Public Module FVecTransformer

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">         float values </param>
        ''' <param name="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Single(), treatsZeroAsNA As Boolean) As FVec
            Return New FVecArray.FVecFloatArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">         double values </param>
        ''' <param name="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Double(), treatsZeroAsNA As Boolean) As FVec
            Return New FVecArray.FVecDoubleArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">          float values </param>
        ''' <param name="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Single(), treatsValueAsNA As Single) As FVec
            Return New FVecArray.FVecFloatArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">          double values </param>
        ''' <param name="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Double(), treatsValueAsNA As Double) As FVec
            Return New FVecArray.FVecDoubleArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from map.
        ''' </summary>
        ''' <param name="map"> map containing non-zero values </param>
        ''' <returns> FVec </returns>
        ''' <remarks>
        ''' 构建稀疏矩阵中的一行数据
        ''' </remarks>
        Public Function fromMap(Of T1 As IComparable)(map As IDictionary(Of String, T1)) As FVec
            Return New FVecMapImpl(Of T1)(map)
        End Function
    End Module

End Namespace
