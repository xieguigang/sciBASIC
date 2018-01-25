Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

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