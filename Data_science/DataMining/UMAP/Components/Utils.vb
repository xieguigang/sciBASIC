#Region "Microsoft.VisualBasic::0ddcac5fc4988da02e99e9be7d60afd5, Data_science\DataMining\UMAP\Components\Utils.vb"

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

    '   Total Lines: 92
    '    Code Lines: 65 (70.65%)
    ' Comment Lines: 12 (13.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (16.30%)
    '     File Size: 3.08 KB


    ' Module Utils
    ' 
    '     Function: Empty, Filled, Range, RejectionSample, ScaleProgressReporter
    ' 
    '     Sub: ShuffleTogether
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.Math

Module Utils

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Function ScaleProgressReporter(progressReporter As RunSlavePipeline.SetProgressEventHandler, start As Double, [end] As Double) As RunSlavePipeline.SetProgressEventHandler
        Return Sub(progress, msg)
                   progressReporter(([end] - start) * progress + start, msg)
               End Sub
    End Function

    ''' <summary>
    ''' Creates an empty array
    ''' </summary>
    Public Function Empty(n As Integer) As Double()
        Return New Double(n - 1) {}
    End Function

    ''' <summary>
    ''' Creates an array filled with index values
    ''' </summary>
    Public Function Range(n As Integer) As Double()
        Return Enumerable.Range(0, n).[Select](Function(i) CDbl(i)).ToArray()
    End Function

    ''' <summary>
    ''' Creates an array filled with a specific value
    ''' </summary>
    Public Function Filled(count As Integer, value As Double) As Double()
        Return Enumerable.Range(0, count).[Select](Function(i) value).ToArray()
    End Function

    ''' <summary>
    ''' Generate nSamples many integers from 0 to poolSize such that no integer is selected twice.The duplication constraint is achieved via rejection sampling.
    ''' </summary>
    Public Function RejectionSample(nSamples As Integer, poolSize As Integer, random As IProvideRandomValues) As Integer()
        Dim result = New Integer(nSamples - 1) {}

        For i = 0 To nSamples - 1
            Dim rejectSample = True

            While rejectSample
                Dim j = random.Next(0, poolSize)
                Dim broken = False

                For k As Integer = 0 To i - 1
                    If j = result(k) Then
                        broken = True
                        Exit For
                    End If
                Next

                If Not broken Then
                    rejectSample = False
                End If

                result(i) = j
            End While
        Next

        Return result
    End Function

    <Extension>
    Friend Sub ShuffleTogether(Of T, T2, T3)(list As List(Of T), other As List(Of T2), weights As List(Of T3), randf As IProvideRandomValues)
        Dim n As Integer = list.Count
        Dim k As Integer
        Dim value As T
        Dim otherValue As T2
        Dim weightsValue As T3

        If other.Count <> n Then
            Throw New Exception()
        End If

        While n > 1
            n -= 1
            k = randf.Next(0, n + 1)
            value = list(k)
            list(k) = list(n)
            list(n) = value
            otherValue = other(k)
            other(k) = other(n)
            other(n) = otherValue
            weightsValue = weights(k)
            weights(k) = weights(n)
            weights(n) = weightsValue
        End While
    End Sub
End Module
