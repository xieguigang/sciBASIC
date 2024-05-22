#Region "Microsoft.VisualBasic::2141ad06ed1e6549c82c62a7c993b1dc, Data_science\Mathematica\Math\DataFrame\Correlation\DistanceMatrix.vb"

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

    '   Total Lines: 82
    '    Code Lines: 59 (71.95%)
    ' Comment Lines: 8 (9.76%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 15 (18.29%)
    '     File Size: 2.79 KB


    ' Class DistanceMatrix
    ' 
    '     Properties: is_dist
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: CreateMatrix, GetQuantile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Quantile

''' <summary>
''' a dissimilarity or similarity structure
''' </summary>
Public Class DistanceMatrix : Inherits DataMatrix

    ''' <summary>
    ''' is correlation matrix or distance matrix
    ''' </summary>
    Public ReadOnly Property is_dist As Boolean = True

    Default Public Overrides Property dist(i%, j%) As Double
        Get
            Dim x As Double = matrix(j)(i)

            ' 这个主要是为了解决from tabular建立矩阵的时候
            ' 丢失自身相关度比较的结果的bug
            If i = j AndAlso x = 0.0 Then
                If Not is_dist Then
                    x = 1
                End If
            End If

            Return x
        End Get
        Set(value As Double)
            matrix(j)(i) = value
        End Set
    End Property

    Sub New(names As IEnumerable(Of String))
        Call MyBase.New(names)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(names As IEnumerable(Of String), matrix As GeneralMatrix, Optional isDistance As Boolean = True)
        Call Me.New(names.Indexing, matrix.ArrayPack, isDistance)
    End Sub

    Sub New(names As Index(Of String), matrix As Double()(), isDistance As Boolean)
        Call MyBase.New(names, matrix)

        Me.is_dist = isDistance
    End Sub

    Public Function GetQuantile(reverse As Boolean) As QuantileEstimationGK
        If reverse Then
            Dim data = matrix.IteratesALL.ToArray
            Dim max = data.Max
            Dim reverse_data = data.Select(Function(x) max - x).ToArray

            Return reverse_data.GKQuantile
        Else
            Return matrix.IteratesALL.GKQuantile
        End If
    End Function

    Public Shared Function CreateMatrix(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of T), isDistance As Boolean) As DistanceMatrix
        Dim matrix As New List(Of Double())
        Dim dataVec As T() = data.SafeQuery.ToArray
        Dim names = dataVec.Keys

        For Each row As T In data
            matrix += names _
                .Select(Function(key)
                            Return row.Properties(key)
                        End Function) _
                .ToArray
        Next

        Return New DistanceMatrix(names, matrix.ToArray, isDistance)
    End Function

End Class
