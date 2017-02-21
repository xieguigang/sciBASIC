#Region "Microsoft.VisualBasic::6e0efb7564a4efe0e91a3885b4bf259c, ..\sciBASIC#\Data_science\Mathematical\Plots\g\AxisScalling.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language

Namespace Graphic.Axis

    Public Module AxisScalling

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="max#"></param>
        ''' <param name="parts%"></param>
        ''' <param name="min#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + 0-10
        ''' + 0-100
        ''' + 0-1000
        ''' + 0-1E30
        ''' + 0-1E-30
        ''' + 0-1
        ''' + 0-0.1
        ''' </remarks>
        Public Function GetAxisValues(max#, Optional parts% = 10, Optional min# = 0R, Optional t# = 5 / 6, Optional decimal% = -1) As Double()
            Dim vmax# = __max(max, min)
            Dim vmin#

            If min < 0 Then
                vmin = -__max(Math.Abs(min), 0)
            Else
                vmin = If(min < t * max, 0, min - (max - min) / 20)
            End If

            Dim d = __fix(vmax, True) - __fix(vmin, False)
            Dim p = Math.Round(Math.Log10(d), 0)
            Dim tick# = 2 * ((10 ^ p) / parts)
            Dim out As List(Of Double) = GetAxisByTick(vmax, tick, vmin)

            If out(-2) > max Then
                Call out.RemoveLast
            End If

            If 0 <= [decimal] Then
                out = New List(Of Double)(out.Select(Function(x) Math.Round(x, [decimal])))
            End If

            Return out.ToArray
        End Function

        Private Function __max(max#, min#) As Double
            Return max + (max - min) / 20
        End Function

        Private Function __fix(ByRef n#, enlarge As Boolean) As Double
            If n = 0R Then
                Return n
            End If

            'If enlarge Then
            '    If n > 0 Then
            '        n += 1
            '    End If
            'Else
            '    If n < 0 Then
            '        n -= 1
            '    End If
            'End If

            Dim p% = Math.Round(Math.Log10(Math.Abs(n)), 0)
            Dim d = 10 ^ (p - 1)
            Dim v#
            Dim s = Math.Sign(n)

            If Not enlarge Then
                p = 10 ^ (p - 1)
            Else
                p = 10 ^ p
            End If

            For i As Double = 0 To 10 Step 0.5
                v = s * p + s * i * d

                If enlarge Then
                    If n <= v Then
                        n = v
                        Exit For
                    End If
                Else
                    If (n > 0 AndAlso n <= v) OrElse (n < 0 AndAlso n > v) Then
                        n = v

                        'If enlarge Then
                        '    ' 由于v已经是比原先的数要大的值，所以在这里可以直接跳过了
                        '    Exit For
                        'End If

                        n -= 0.5 * d

                        Exit For
                    End If
                End If
            Next

            Return n
        End Function

        <Extension>
        Public Function GetAxisValues(range As DoubleRange, Optional parts% = 10) As Double()
            Return GetAxisValues(range.Max, parts, range.Min)
        End Function

        <Extension>
        Public Function GetAxisByTick(range As DoubleRange, tick#) As Double()
            Return GetAxisByTick(range.Max, tick, range.Min).ToArray
        End Function

        Public Function GetAxisByTick(max#, tick#, Optional min# = 0R) As List(Of Double)
            Dim l As New List(Of Double)
            Dim i# = min

            Do While i < max
                l.Add(i)
                i += tick
            Loop

            l.Add(i)

            Return l
        End Function
    End Module
End Namespace