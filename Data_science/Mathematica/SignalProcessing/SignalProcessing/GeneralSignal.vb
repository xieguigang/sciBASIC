#Region "Microsoft.VisualBasic::675ebea354e8d24e88a48585eb855bc9, Data_science\Mathematica\SignalProcessing\SignalProcessing\GeneralSignal.vb"

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

' Class GeneralSignal
' 
'     Properties: description, Measures, measureUnit, meta, reference
'                 Strength
' 
'     Function: GetText, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

Public Class GeneralSignal : Implements INamedValue

    Public Property Measures As Double()
    Public Property Strength As Double()

    ''' <summary>
    ''' the unique reference
    ''' </summary>
    ''' <returns></returns>
    Public Property reference As String Implements INamedValue.Key
    Public Property measureUnit As String
    Public Property description As String
    Public Property meta As Dictionary(Of String, String)

    Public Overrides Function ToString() As String
        Return description
    End Function

    Public Function GetText() As String
        Dim sb As New StringBuilder

        For Each line As String In description.LineTokens
            Call sb.AppendLine("# " & line)
        Next

        For Each par In meta
            Call sb.AppendLine($"{par.Key}={par.Value}")
        Next

        Call sb.AppendLine(measureUnit & vbTab & "intensity")

        For i As Integer = 0 To Measures.Length - 1
            Call sb.AppendLine(Measures(i) & vbTab & Strength(i))
        Next

        Return sb.ToString
    End Function

    Public Iterator Function PopulatePoints() As IEnumerable(Of PointF)
        For i As Integer = 0 To _Measures.Length - 1
            Yield New PointF With {
                .X = _Measures(i),
                .Y = _Strength(i)
            }
        Next
    End Function
End Class

''' <summary>
''' data signal resampler for continuous signals
''' </summary>
Public Class Resampler

    Dim raw As GeneralSignal
    Dim x As Double()
    Dim y As Double()

    Public ReadOnly Property enumerateMeasures As Double()
        Get
            Return x.ToArray
        End Get
    End Property

    Public Function GetIntensity(x As Double) As Double
        Dim i As Integer = getPosition(x)

        If Me.x(i) = x OrElse i = Me.x.Length - 1 Then
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
        Dim x = raw.Measures.SeqIterator.OrderBy(Function(xi) xi.value).ToArray
        Dim y = raw.Strength

        Return New Resampler With {
            .raw = raw,
            .x = x.Select(Function(xi) xi.value).ToArray,
            .y = x.Select(Function(xi) y(xi.i)).ToArray
        }
    End Function
End Class