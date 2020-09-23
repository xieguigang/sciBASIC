#Region "Microsoft.VisualBasic::42483e4b7989de00005b7a6109fffbbc, Data_science\Mathematica\SignalProcessing\SignalProcessing\Resampler.vb"

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

    ' Class Resampler
    ' 
    '     Properties: enumerateMeasures
    ' 
    '     Function: CreateSampler, GetIntensity, getPosition
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

    Public Function GetIntensity(x As Double) As Double
        Dim i As Integer = getPosition(x)

        If i = Me.x.Length Then
            Return Me.x(i - 1)
        ElseIf Me.x(i) = x OrElse i = Me.x.Length - 1 Then
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
