#Region "Microsoft.VisualBasic::9c5c87be774f3bb1d96e156924dee57a, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Signals\TestProvider.vb"

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

Namespace Signals

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Inspired by Lee Byron's test data generator.
    ''' </remarks>
    Public Module TestProvider

        ''' <summary>
        ''' Generate signal data for test
        ''' </summary>
        ''' <param name="n%"></param>
        ''' <param name="m%"></param>
        ''' <returns></returns>
        Public Function bumps(n%, m%) As Double()
            Dim a#() = New Double(n - 1) {}
            Dim seed As New Random

            For i As Integer = 0 To m - 1
                Call bump(a, n, seed)
            Next

            Return a
        End Function

        Public Sub bump(a#(), n%, seed As Random)
            Dim x = 1 / (0.1 + seed.NextDouble),
            y = 2 * seed.NextDouble - 0.5,
            Z = 10 / (0.1 + seed.NextDouble)

            For i As Integer = 0 To n - 1
                Dim w = (i / n - y) * Z
                a(i) += x * Math.Exp(-w * w)
            Next
        End Sub
    End Module
End Namespace