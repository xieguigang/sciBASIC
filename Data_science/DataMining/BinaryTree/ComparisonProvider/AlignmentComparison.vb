#Region "Microsoft.VisualBasic::13031076e28d4842280b3073479610e8, Data_science\DataMining\BinaryTree\ComparisonProvider\AlignmentComparison.vb"

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

    '   Total Lines: 95
    '    Code Lines: 77
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 3.58 KB


    ' Class AlignmentComparison
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: FromMatrix, GetObject, GetSimilarity
    ' 
    ' Enum CompareMethods
    ' 
    '     RelativeDistance, SpectrumDotProduct
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class AlignmentComparison : Inherits ComparisonProvider

    ReadOnly dataIndex As Dictionary(Of String, Double())
    ReadOnly compare As CompareMethods = CompareMethods.SpectrumDotProduct

    Sub New(dataset As NamedValue(Of Dictionary(Of String, Double))(),
            equals As Double,
            gt As Double,
            Optional method As CompareMethods = CompareMethods.SpectrumDotProduct)

        Call MyBase.New(equals, gt)

        Dim names As String() = dataset _
            .Select(Function(a) a.Value.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        dataIndex = dataset _
            .ToDictionary(Function(d) d.Name,
                          Function(d)
                              Return names _
                                  .Select(Function(col) d.Value.TryGetValue(col)) _
                                  .ToArray
                          End Function)
        compare = method
    End Sub

    Sub New(dataset As IEnumerable(Of NamedCollection(Of Double)),
            equals As Double,
            gt As Double,
            Optional method As CompareMethods = CompareMethods.SpectrumDotProduct)

        Call MyBase.New(equals, gt)

        dataIndex = dataset _
            .ToDictionary(Function(d) d.name,
                          Function(d)
                              Return d.value
                          End Function)
        compare = method
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub New(m As Dictionary(Of String, Double()),
                    eq As Double,
                    gt As Double,
                    Optional method As CompareMethods = CompareMethods.SpectrumDotProduct)

        Call MyBase.New(eq, gt)

        dataIndex = m
        compare = method
    End Sub

    Public Overrides Function GetSimilarity(x As String, y As String) As Double
        Select Case compare
            Case CompareMethods.SpectrumDotProduct
                Dim xvec As New Vector(dataIndex(x))
                Dim yvec As New Vector(dataIndex(y))

                Return SSM(xvec, yvec)
            Case CompareMethods.RelativeDistance
                Dim xvec As Double() = SIMD.Divide.f64_op_divide_f64_scalar(dataIndex(x), dataIndex(x).Max)
                Dim yvec As Double() = SIMD.Divide.f64_op_divide_f64_scalar(dataIndex(y), dataIndex(y).Max)

                Return xvec.EuclideanDistance(yvec)
            Case Else
                Throw New NotImplementedException(compare.Description)
        End Select
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function FromMatrix(Of T As {INamedValue, IVector})(m As IEnumerable(Of T), eq As Double, gt As Double) As AlignmentComparison
        Return New AlignmentComparison(m.ToDictionary(Function(a) a.Key, Function(a) a.Data), eq, gt)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetObject(id As String) As Object
        Return dataIndex(id)
    End Function
End Class

Public Enum CompareMethods
    SpectrumDotProduct
    RelativeDistance
End Enum
