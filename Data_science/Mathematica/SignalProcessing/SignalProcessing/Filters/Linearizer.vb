﻿' 
'  Copyright [2009] [Marcin Rzeźnicki]
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' 

Namespace mr.go.sgfilter

    ''' <summary>
    ''' Linearizes data by seeking points with relative difference greater than
    ''' <seealsocref="getTruncateRatio()"/> and replacing them with points
    ''' lying on line between the first and the last of such points. Strictly:
    ''' <para>
    ''' let <tt>delta(i)</tt> be function which assigns to an element at index
    ''' <tt>i (data[i])</tt>, for <tt>0
    ''' </> <data.length/></tt>, value of
    ''' <tt>|(data[i] - data[i+1])/data[i]|</tt>. Then for each range <tt>(j,k)</tt>
    ''' of data, such that
    ''' <tt>delta(j) > <seealsocref="getTruncateRatio()"/></tt> and
    ''' <tt>delta(k)
    ''' </> <seealsocref="getTruncateRatio()"/></tt>, <tt>data[x] = ((data[k] -
    ''' data[j])/(k - j)) * (x - k) + data[j])</tt> for <tt>j </> </></tt>.
    ''' </para>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class Linearizer
        Implements Preprocessor

        Private truncateRatioField As Single = 0.5F

        ''' <summary>
        ''' Default constructor. <seealsocref="getTruncateRatio()"/> is 0.5
        ''' </summary>
        Public Sub New()

        End Sub

        ''' 
        ''' <paramname="truncateRatio">
        '''            maximum relative difference of subsequent data points above
        '''            which linearization begins </param>
        ''' <exceptioncref="IllegalArgumentException">
        '''             when {@code truncateRatio} </> </exception>
        Public Sub New(truncateRatio As Single)
            If truncateRatio < 0F Then
                Throw New ArgumentException("truncateRatio < 0")
            End If
            truncateRatioField = truncateRatio
        End Sub

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            Dim n = data.Length - 1
            Dim deltas = computeDeltas(data)
            For i = 0 To n - 1
                If deltas(i) > truncateRatioField Then
                    For k = i + 1 To n - 1
                        If deltas(k) <= truncateRatioField Then
                            linest(data, i, k)
                            i = k - 1
                            GoTo linregContinue
                        End If
                    Next
                    linest(data, i, n)
                    Exit For
                End If
linregContinue:
            Next
linregBreak:
        End Sub

        Protected Friend Overridable Function computeDeltas(data As Double()) As Double()
            Dim n = data.Length
            Dim deltas = New Double(n - 1 - 1) {}
            For i = 0 To n - 1 - 1
                If data(i) = 0 AndAlso data(i + 1) = 0 Then
                    deltas(i) = 0
                Else
                    deltas(i) = Math.Abs(1 - data(i + 1) / data(i))
                End If
            Next
            Return deltas
        End Function

        ''' 
        ''' <returns> {@code truncateRatio} </returns>
        Public Overridable Property TruncateRatio As Single
            Get
                Return truncateRatioField
            End Get
            Set(value As Single)
                If value < 0F Then
                    Throw New ArgumentException("truncateRatio < 0")
                End If
                truncateRatioField = value
            End Set
        End Property

        Protected Friend Overridable Sub linest(data As Double(), x0 As Integer, x1 As Integer)
            If x0 + 1 = x1 Then
                Return
            End If
            Dim slope = (data(x1) - data(x0)) / (x1 - x0)
            Dim y0 = data(x0)
            For x = x0 + 1 To x1 - 1
                data(x) = slope * (x - x0) + y0
            Next
        End Sub


    End Class

End Namespace
