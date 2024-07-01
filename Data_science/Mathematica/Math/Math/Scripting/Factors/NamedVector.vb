#Region "Microsoft.VisualBasic::e002b6eb754d64c2f1d8f4ecb163aa29, Data_science\Mathematica\Math\Math\Scripting\Factors\NamedVector.vb"

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

    '   Total Lines: 66
    '    Code Lines: 48 (72.73%)
    ' Comment Lines: 8 (12.12%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (15.15%)
    '     File Size: 2.38 KB


    '     Class NamedVector
    ' 
    '         Function: GetJson
    ' 
    '         Sub: Add
    ' 
    '         Operators: *
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    ''' <summary>
    ''' 提供字符串映射到具体的数值
    ''' </summary>
    Public Class NamedVector : Inherits FactorVector(Of Double)

        ''' <summary>
        ''' 添加一个映射
        ''' </summary>
        ''' <param name="factor$"></param>
        ''' <param name="value#"></param>
        Public Sub Add(factor$, value#)
            If buffer Is Nothing Then
                buffer = {}
                index = New Dictionary(Of String, Integer)
            End If

            Call index.Add(factor, buffer.Length)
            Call buffer.Add(value)
        End Sub

        Public Overloads Function GetJson(format As String) As String
            Dim table = AsTable() _
                .ToDictionary(Function(key, v) key,
                              Function(key, v)
                                  Return v.ToString(format)
                              End Function)
            Return table.GetJson
        End Function

        Public Shared Widening Operator CType(table As Dictionary(Of String, Double)) As NamedVector
            Dim index As Index(Of String) = table.Keys.Indexing
            Dim vector As Vector = index _
                .Objects _
                .Select(Function(key) table(key)) _
                .AsVector

            Return New NamedVector With {
                .index = index,
                .buffer = vector
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(vector As NamedVector) As Dictionary(Of String, Double)
            Return vector.AsTable
        End Operator

        Public Shared Operator *(A As NamedVector, B As NamedVector) As NamedVector
            Dim names$() = A.Keys.AsSet Or B.Keys
            Dim result = A.Vector(names).AsVector * B.Vector(names).AsVector

            Return New NamedVector With {
                .buffer = result,
                .index = names.Indexing
            }
        End Operator
    End Class
End Namespace
