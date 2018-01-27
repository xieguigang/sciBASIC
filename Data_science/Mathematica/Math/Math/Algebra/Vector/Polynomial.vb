#Region "Microsoft.VisualBasic::ec5e9a87f5c63e7b065472346bfa6798, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Vector\Polynomial.vb"

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

Namespace LinearAlgebra

    Public Class Polynomial

        Public Property Factors As Double()

        Default Public ReadOnly Property F(x#) As Double
            Get
                Dim ans As Double = 0

                For i As Integer = 0 To Factors.Length - 1
                    ans += Factors(i) * (x ^ i)
                Next

                Return ans
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(format:="F2")
        End Function

        Public Overloads Function ToString(format As String) As String
            Dim items = Factors _
                .Select(Function(a, i)
                            If i = 0 Then
                                Return a.ToString(format)
                            ElseIf i = 1 Then
                                Return $"{a.ToString(format)}*X"
                            Else
                                Return $"{a.ToString(format)}*X^{i}"
                            End If
                        End Function) _
                .ToArray
            Dim Y$ = items.JoinBy(" + ")

            Return Y
        End Function
    End Class
End Namespace
