#Region "Microsoft.VisualBasic::92ed8a7056654fd5868a2bf770739960, Data_science\Mathematica\Math\Math\Scripting\Factors\FactorVector.vb"

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

    '     Interface IFactorVector
    ' 
    '         Properties: index
    ' 
    '     Class FactorVector
    ' 
    '         Properties: index, Keys
    ' 
    '         Function: AsTable, GetJson, ToString, Vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
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
                              Function(k, i)
                                  Return buffer(i)
                              End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetJson() As String
            Return AsTable.GetJson
        End Function

        Public Overrides Function ToString() As String
            Return index.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace
