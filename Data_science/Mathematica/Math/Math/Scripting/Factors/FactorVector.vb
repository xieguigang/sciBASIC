#Region "Microsoft.VisualBasic::6ae2264d923656f03750222be0d4e51d, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Factors\FactorVector.vb"

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

Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    ''' <summary>
    ''' 提供和R之中的向量类似的行为：可以用两种方式来访问向量之中的成员，名字或者向量数组的下表
    ''' </summary>
    Public Class FactorVector : Inherits GenericVector(Of Object)

        Friend index As Dictionary(Of String, Integer)

        Default Public Overloads Property Item(name$) As Object
            Get
                If Not index.ContainsKey(name) Then
                    Return Nothing
                Else
                    Return buffer(index(name))
                End If
            End Get
            Set(value As Object)
                If Not index.ContainsKey(name) Then
                    Call buffer.Add(value)
                    Call index.Add(name, buffer.Length - 1)
                Else
                    buffer(index(name)) = value
                End If
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return index.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace
