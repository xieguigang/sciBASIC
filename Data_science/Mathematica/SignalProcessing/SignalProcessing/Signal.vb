﻿#Region "Microsoft.VisualBasic::d7b31422f4511e8088fb2c10c5a17b4b, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/SignalProcessing/SignalProcessing//Signal.vb"

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

    '   Total Lines: 43
    '    Code Lines: 27
    ' Comment Lines: 10
    '   Blank Lines: 6
    '     File Size: 1.39 KB


    ' Class Signal
    ' 
    '     Properties: intensities, times
    ' 
    '     Constructor: (+2 Overloads) Sub New
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

    Sub New(data As IEnumerable(Of ITimeSignal))
        Call MyBase.New(From ti As ITimeSignal In data.SafeQuery Select New TimeSignal(ti))
    End Sub

    Public Shared Operator +(a As Signal, b As Signal) As Signal
        Throw New NotImplementedException
    End Operator
End Class
