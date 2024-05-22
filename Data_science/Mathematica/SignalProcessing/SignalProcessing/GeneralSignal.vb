#Region "Microsoft.VisualBasic::45d621e19eefbc5b75104bdb25be0cad, Data_science\Mathematica\SignalProcessing\SignalProcessing\GeneralSignal.vb"

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

    '   Total Lines: 163
    '    Code Lines: 111 (68.10%)
    ' Comment Lines: 30 (18.40%)
    '    - Xml Docs: 96.67%
    ' 
    '   Blank Lines: 22 (13.50%)
    '     File Size: 5.38 KB


    ' Class GeneralSignal
    ' 
    '     Properties: description, MeasureRange, Measures, measureUnit, meta
    '                 reference, Strength, weight
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: GetText, (+2 Overloads) GetTimeSignals, PopulatePoints, Preprocessing, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing.NDtw.Preprocessing

''' <summary>
''' 
''' </summary>
''' <remarks>
''' a tuple of the signal vector data <see cref="Measures"/> and <see cref="Strength"/>
''' </remarks>
Public Class GeneralSignal : Implements INamedValue, IVector

    ''' <summary>
    ''' usually is the time in unit second.(x axis)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' x axis
    ''' </remarks>
    Public Property Measures As Double()
    ''' <summary>
    ''' the signal strength.(y axis)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' y axis
    ''' </remarks>
    Public Property Strength As Double() Implements IVector.Data

    ''' <summary>
    ''' the unique reference id, or the variable name
    ''' </summary>
    ''' <returns></returns>
    Public Property reference As String Implements INamedValue.Key
    Public Property measureUnit As String
    Public Property description As String
    Public Property meta As Dictionary(Of String, String)
    Public Property weight As Double

    Public ReadOnly Property MeasureRange As DoubleRange
        Get
            If Measures.IsNullOrEmpty Then
                Return {0.0, 0.0}
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

    Sub New()
    End Sub

    Sub New(x As Double(), y As Double())
        Measures = x
        Strength = y
    End Sub

    Sub New(x As Double(), y As Double(), id As String, weight As Double)
        Call Me.New(x, y)

        Me.reference = id
        Me.weight = weight
    End Sub

    Public Overrides Function ToString() As String
        If description.StringEmpty Then
            Return $"{reference}, {Measures.Length} data points"
        Else
            Return description
        End If
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

    Public Iterator Function GetTimeSignals(Of T As ITimeSignal)(activator As Func(Of Double, Double, T)) As IEnumerable(Of T)
        For i As Integer = 0 To _Measures.Length - 1
            Yield activator(_Measures(i), _Strength(i))
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Preprocessing(sig As GeneralSignal, process As IPreprocessor) As GeneralSignal
        Return New GeneralSignal() With {
            .description = sig.description,
            .Measures = process(sig.Measures),
            .measureUnit = sig.measureUnit,
            .meta = sig.meta,
            .reference = sig.reference,
            .Strength = process(sig.Strength),
            .weight = sig.weight
        }
    End Function
End Class
