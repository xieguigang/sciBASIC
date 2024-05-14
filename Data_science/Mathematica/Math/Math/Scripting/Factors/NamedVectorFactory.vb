#Region "Microsoft.VisualBasic::17641d6e6576234db8533a6274c72866, Data_science\Mathematica\Math\Math\Scripting\Factors\NamedVectorFactory.vb"

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

    '   Total Lines: 58
    '    Code Lines: 43
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 2.10 KB


    '     Class NamedVectorFactory
    ' 
    '         Properties: Keys
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) AsVector, EmptyVector, ToString, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function EmptyVector() As Vector
            Return New Vector(factors.Length - 1)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVector(data As Dictionary(Of String, Integer)) As Vector
            Return AsVector(data.ToDictionary(Function(k) k.Key, Function(t) CDbl(t.Value)))
        End Function

        Public Function AsVector(data As Dictionary(Of String, Double)) As Vector
            Dim vector#() = New Double(factors.Length - 1) {}

            For Each factor As Factor(Of String) In factors
                vector(CInt(factor.Value)) = data.TryGetValue(factor)
            Next

            Return vector.AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(vector As Vector) As Dictionary(Of String, Double)
            Return factors.ToDictionary(
                Function(factor) factor.FactorValue,
                Function(i)
                    Return vector(CInt(i.Value))
                End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return factors _
                .Select(Function(factor) factor.FactorValue) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace
