#Region "Microsoft.VisualBasic::8049e23e0850cc23293e1563ff3f5c65, Data_science\DataMining\UMAP\Components\Utils.vb"

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

    ' Module Utils
    ' 
    '     Function: Empty, Filled, Max, Mean, Range
    '               RejectionSample
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math

Module Utils

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
    ''' Returns the mean of an array
    ''' </summary>
    Public Function Mean(input As Double()) As Double
        Return input.Sum() / input.Length
    End Function

    ''' <summary>
    ''' Returns the maximum value of an array
    ''' </summary>
    Public Function Max(input As Double()) As Double
        Return input.Max()
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

                For k = 0 To i - 1

                    If j = result(k) Then
                        broken = True
                        Exit For
                    End If
                Next

                If Not broken Then rejectSample = False
                result(i) = j
            End While
        Next

        Return result
    End Function
End Module
