#Region "Microsoft.VisualBasic::1fdc139d8532058f04806c956fdc3aa8, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Vector\NamedVector.vb"

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

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    ''' <summary>
    ''' Factory for <see cref="Dictionary(Of String, Double)"/> to <see cref="Vector"/>
    ''' </summary>
    Public Class NamedVectorFactory

        Public ReadOnly Property Keys As String()

        ReadOnly factors As Factor(Of String)()

        Sub New(factors As IEnumerable(Of String))
            Me.Keys = factors.ToArray
            Me.factors = FactorExtensions.factors(Keys)
        End Sub

        Public Function EmptyVector() As Vector
            Return New Vector(factors.Length - 1)
        End Function

        Public Function AsVector(data As Dictionary(Of String, Double)) As Vector
            Dim vector#() = New Double(factors.Length - 1) {}

            For Each factor As Factor(Of String) In factors
                vector(factor.Value) = data(factor)
            Next

            Return vector.AsVector
        End Function

        Public Function Translate(vector As Vector) As Dictionary(Of String, Double)
            Return factors.ToDictionary(
                Function(factor) factor.FactorValue,
                Function(i) vector(CInt(i.Value)))
        End Function

        Public Overrides Function ToString() As String
            Return factors _
                .Select(Function(factor) factor.FactorValue) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace
