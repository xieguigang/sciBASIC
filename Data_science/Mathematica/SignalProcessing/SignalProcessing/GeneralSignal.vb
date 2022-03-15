#Region "Microsoft.VisualBasic::9c9b8a659c88d7d40ec26d46f5221950, sciBASIC#\Data_science\Mathematica\SignalProcessing\SignalProcessing\GeneralSignal.vb"

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

    '   Total Lines: 109
    '    Code Lines: 75
    ' Comment Lines: 18
    '   Blank Lines: 16
    '     File Size: 3.60 KB


    ' Class GeneralSignal
    ' 
    '     Properties: description, MeasureRange, Measures, measureUnit, meta
    '                 reference, Strength
    ' 
    '     Function: GetText, GetTimeSignals, PopulatePoints, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq

Public Class GeneralSignal : Implements INamedValue

    ''' <summary>
    ''' usually is the time in unit second.(x axis)
    ''' </summary>
    ''' <returns></returns>
    Public Property Measures As Double()
    ''' <summary>
    ''' the signal strength.(y axis)
    ''' </summary>
    ''' <returns></returns>
    Public Property Strength As Double()

    ''' <summary>
    ''' the unique reference
    ''' </summary>
    ''' <returns></returns>
    Public Property reference As String Implements INamedValue.Key
    Public Property measureUnit As String
    Public Property description As String
    Public Property meta As Dictionary(Of String, String)

    Public ReadOnly Property MeasureRange As DoubleRange
        Get
            If Measures.IsNullOrEmpty Then
                Return {0, 0}
            Else
                Return {Measures.Min, Measures.Max}
            End If
        End Get
    End Property

    ''' <summary>
    ''' take signal subset by a given range of <see cref="Measures"/>
    ''' </summary>
    ''' <param name="min">min of take by <see cref="Measures"/></param>
    ''' <param name="max#">max of take by <see cref="Measures"/></param>
    ''' <returns></returns>
    Default Public ReadOnly Property GetByRange(min#, max#) As GeneralSignal
        Get
            Dim i As Integer = which(Measures.Select(Function(a) a >= min)).FirstOrDefault
            Dim j As Integer = which(Measures.Select(Function(a) a >= max)).FirstOrDefault

            If i = 0 AndAlso j = 0 Then
                i = 0
                j = Measures.Length - 1
            ElseIf j = 0 Then
                j = Measures.Length - 1
            End If

            Return New GeneralSignal With {
                .description = description,
                .measureUnit = measureUnit,
                .meta = New Dictionary(Of String, String)(meta),
                .reference = reference,
                .Measures = Measures.Skip(i).Take(j - i).ToArray,
                .Strength = Strength.Skip(i).Take(j - i).ToArray
            }
        End Get
    End Property

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

    Public Iterator Function GetTimeSignals() As IEnumerable(Of ITimeSignal)
        For i As Integer = 0 To _Measures.Length - 1
            Yield New TimeSignal With {
                .time = _Measures(i),
                .intensity = _Strength(i)
            }
        Next
    End Function
End Class
