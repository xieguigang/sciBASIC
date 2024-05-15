#Region "Microsoft.VisualBasic::c11ab7c610aae4a79e95336ab65255cc, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\Resampler.vb"

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

    '   Total Lines: 121
    '    Code Lines: 89
    ' Comment Lines: 14
    '   Blank Lines: 18
    '     File Size: 3.61 KB


    ' Class Resampler
    ' 
    '     Properties: enumerateMeasures
    ' 
    '     Function: (+2 Overloads) CreateSampler, GetIntensity, getPosition
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

''' <summary>
''' data signal resampler for continuous signals
''' </summary>
''' <remarks>
''' usually used for align two signal data
''' </remarks>
Public Class Resampler

    Dim x As Double()
    Dim y As Double()
    Dim max_dx As Double = Double.MaxValue

    ''' <summary>
    ''' populate the <see cref="GeneralSignal.Measures"/> of the raw signal
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' example as get the raw time vector from this property
    ''' </remarks>
    Public ReadOnly Property enumerateMeasures As Double()
        Get
            Return x.ToArray
        End Get
    End Property

    Default Public ReadOnly Property GetVector(x As IEnumerable(Of Double)) As Double()
        Get
            If Me.x.Length = 0 Then
                Return x.Select(Function(any) 0.0).ToArray
            Else
                Return x.Select(AddressOf GetIntensity).ToArray
            End If
        End Get
    End Property

    Public Function GetIntensity(x As Double) As Double
        Dim dx As Double = Nothing
        Dim i As Integer = getPosition(x, dx)

        If dx > max_dx Then
            Return 0
        End If

        If i >= Me.x.Length - 1 Then
            Return Me.y(Me.y.Length - 1)
        ElseIf i <= 0 Then
            Return Me.y(0)
        ElseIf std.Abs(Me.x(i) - x) <= 0.000001 Then
            Return Me.y(i)
        ElseIf x < Me.x(i) Then
            Return Me.y(i)
        Else
            Dim x1 = Me.x(i)
            Dim x2 = Me.x(i + 1)
            Dim y1 = Me.y(i)
            Dim y2 = Me.y(i + 1)

            If x2 - x1 > max_dx Then
                ' window too large, returns no signal data
                Return 0
            End If

            If x2 = x1 Then
                Return (y1 + y2) / 2
            Else
                Dim scale As Double = (x - x1) / (x2 - x1)
                Dim dy As Double = (y2 - y1) * scale

                Return y1 + dy
            End If
        End If
    End Function

    Private Function getPosition(x As Double, ByRef dx As Double) As Integer
        For i As Integer = 0 To Me.x.Length - 1
            If Me.x(i) >= x Then
                If i > 0 Then
                    If std.Abs(Me.x(i) - x) > std.Abs(Me.x(i - 1) - x) Then
                        i -= 1
                    End If
                End If

                dx = std.Abs(Me.x(i) - x)
                Return i
            End If
        Next

        dx = x - Me.x(Me.x.Length - 1)

        Return Me.x.Length
    End Function

    Public Shared Function CreateSampler(x As Double(), y As Double(), Optional max_dx As Double = Double.MaxValue) As Resampler
        If x.Length <> y.Length Then
            Throw New ArgumentException($"the size of x should equals to the size of y!")
        End If

        Return New Resampler With {
            .x = x,
            .y = y,
            .max_dx = max_dx
        }
    End Function

    Public Shared Function CreateSampler(raw As GeneralSignal, Optional max_dx As Double = Double.MaxValue) As Resampler
        Dim x = raw.Measures _
            .SeqIterator _
            .OrderBy(Function(xi) xi.value) _
            .ToArray
        Dim y = raw.Strength

        Return New Resampler With {
            .max_dx = max_dx,
            .x = x.Select(Function(xi) xi.value).ToArray,
            .y = x.Select(Function(xi) y(xi.i)).ToArray
        }
    End Function
End Class
