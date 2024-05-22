#Region "Microsoft.VisualBasic::bb25b87cfb339b0c9a83b9cc696149e0, Data_science\Mathematica\SignalProcessing\SignalProcessing\Signal.vb"

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

    '   Total Lines: 57
    '    Code Lines: 38 (66.67%)
    ' Comment Lines: 10 (17.54%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.79%)
    '     File Size: 2.03 KB


    ' Class Signal
    ' 
    '     Properties: intensities, times
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: Zip
    '     Operators: +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' A vector of the <see cref="TimeSignal"/>
''' </summary>
''' <remarks>
''' a <see cref="TimeSignal"/> point collection
''' </remarks>
Public Class Signal : Inherits Vector(Of TimeSignal)

    Public ReadOnly Property times As Vector
        Get
            Return New Vector(From xi As TimeSignal In Buffer Select xi.time)
        End Get
    End Property

    ''' <summary>
    ''' Get the signal intensity vector of current signal data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property intensities As Vector
        Get
            Return New Vector(From xi As TimeSignal In Buffer Select xi.intensity)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(data As IEnumerable(Of TimeSignal))
        Call MyBase.New(data)
    End Sub

    Sub New(time As IEnumerable(Of Double), value As IEnumerable(Of Double))
        Call MyBase.New(Zip(time.ToArray, value.ToArray))
    End Sub

    Sub New(data As IEnumerable(Of ITimeSignal))
        Call MyBase.New(From ti As ITimeSignal In data.SafeQuery Select New TimeSignal(ti))
    End Sub

    Private Shared Iterator Function Zip(time As Double(), value As Double()) As IEnumerable(Of TimeSignal)
        If time.Length <> value.Length Then
            Throw New InvalidProgramException($"the dimension size of the signal time({time.Length}) mis-matched with the dimension size of signal intensity vector({value.Length})!")
        End If

        For i As Integer = 0 To time.Length - 1
            Yield New TimeSignal(time(i), value(i))
        Next
    End Function

    Public Shared Operator +(a As Signal, b As Signal) As Signal
        Throw New NotImplementedException
    End Operator
End Class
