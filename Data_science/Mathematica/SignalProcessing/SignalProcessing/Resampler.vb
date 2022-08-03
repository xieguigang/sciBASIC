#Region "Microsoft.VisualBasic::f5c4848179aa59575e050f23c3ce185b, sciBASIC#\Data_science\Mathematica\SignalProcessing\SignalProcessing\Resampler.vb"

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

    '   Total Lines: 78
    '    Code Lines: 58
    ' Comment Lines: 7
    '   Blank Lines: 13
    '     File Size: 2.27 KB


    ' Class Resampler
    ' 
    '     Properties: enumerateMeasures
    ' 
    '     Function: (+2 Overloads) CreateSampler, GetIntensity, getPosition
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

''' <summary>
''' data signal resampler for continuous signals
''' </summary>
Public Class Resampler

    Dim x As Double()
    Dim y As Double()

    ''' <summary>
    ''' populate the <see cref="GeneralSignal.Measures"/> of the raw signal
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property enumerateMeasures As Double()
        Get
            Return x.ToArray
        End Get
    End Property

    Default Public ReadOnly Property GetVector(x As IEnumerable(Of Double)) As Double()
        Get
            Return x.Select(AddressOf GetIntensity).ToArray
        End Get
    End Property

    Public Function GetIntensity(x As Double) As Double
        Dim i As Integer = getPosition(x)

        If i >= Me.x.Length - 1 Then
            Return Me.y(Me.y.Length - 1)
        ElseIf i <= 0 Then
            Return Me.y(0)
        ElseIf Me.x(i) = x Then
            Return Me.y(i)
        Else
            Dim x1 = Me.x(i)
            Dim x2 = Me.x(i + 1)
            Dim y1 = Me.y(i)
            Dim y2 = Me.y(i + 1)
            Dim scale As Double = (x - x1) / (x2 - x1)
            Dim dy As Double = (y2 - y1) * scale

            Return y1 + dy
        End If
    End Function

    Private Function getPosition(x As Double) As Integer
        For i As Integer = 0 To Me.x.Length - 1
            If Me.x(i) >= x Then
                Return i
            End If
        Next

        Return Me.x.Length
    End Function

    Public Shared Function CreateSampler(x As Double(), y As Double()) As Resampler
        If x.Length <> y.Length Then
            Throw New ArgumentException($"the size of x should equals to the size of y!")
        End If

        Return New Resampler With {.x = x, .y = y}
    End Function

    Public Shared Function CreateSampler(raw As GeneralSignal) As Resampler
        Dim x = raw.Measures _
            .SeqIterator _
            .OrderBy(Function(xi) xi.value) _
            .ToArray
        Dim y = raw.Strength

        Return New Resampler With {
            .x = x.Select(Function(xi) xi.value).ToArray,
            .y = x.Select(Function(xi) y(xi.i)).ToArray
        }
    End Function
End Class
