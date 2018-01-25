#Region "Microsoft.VisualBasic::6ae2264d923656f03750222be0d4e51d, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Factors\FactorVector.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    Public Interface IFactorVector
        Property index As Dictionary(Of String, Integer)
    End Interface

    ''' <summary>
    ''' 提供和R之中的向量类似的行为：可以用两种方式来访问向量之中的成员，名字或者向量数组的下表
    ''' </summary>
    Public Class FactorVector(Of T) : Inherits GenericVector(Of T)
        Implements IFactorVector

        Public Property index As Dictionary(Of String, Integer) Implements IFactorVector.index

        Default Public Overloads Property Item(name$) As T
            Get
                If Not index.ContainsKey(name) Then
                    Return Nothing
                Else
                    Return buffer(index(name))
                End If
            End Get
            Set(value As T)
                If Not index.ContainsKey(name) Then
                    Call buffer.Add(value)
                    Call index.Add(name, buffer.Length - 1)
                Else
                    buffer(index(name)) = value
                End If
            End Set
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return index.Keys
            End Get
        End Property

        Public Iterator Function Vector(names As IEnumerable(Of String)) As IEnumerable(Of T)
            For Each name As String In names
                Yield Me(name)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsTable() As Dictionary(Of String, T)
            Return index _
                .ToDictionary(Function(k, i) k,
                              Function(k, i) buffer(i))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetJson() As String
            Return AsTable.GetJson
        End Function

        Public Overrides Function ToString() As String
            Return index.Keys.ToArray.GetJson
        End Function
    End Class

    Public Class NamedVector : Inherits FactorVector(Of Double)

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
        Public Shared Narrowing Operator CType(vector As NamedVector) As Dictionary(Of String, Double)
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
