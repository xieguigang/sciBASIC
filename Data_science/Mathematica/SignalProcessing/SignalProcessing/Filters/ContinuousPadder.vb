' 
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
    ''' Pads data to left and/or right, starting from the first (last) non-zero cell
    ''' and extending it to the beginning (end) of data. More specifically:
    ''' <para>
    ''' <ul>
    ''' <li>
    ''' Let <tt>l</tt> be the index of the first non-zero element in data (for left
    ''' padding),</li>
    ''' <li>let <tt>r</tt> be the index of the last non-zero element in data (for
    ''' right padding)</li>
    ''' </ul>
    ''' then for every element <tt>e</tt> which index is <tt>i</tt> such that:
    ''' <ul>
    ''' <li>
    ''' <tt>0 </> <l/></tt>, <tt>e</tt> is replaced with element <tt>data[l]</tt>
    ''' (left padding)</li>
    ''' <li>
    ''' <tt>r <i/> <data.length/></tt>, <tt>e</tt> is replaced with element
    ''' <tt>data[r]</tt> (right padding)</li>
    ''' </ul>
    ''' </para>
    ''' Example:
    ''' <para>
    ''' Given data: <tt>[0,0,0,1,2,1,3,1,2,4,0]</tt> result of applying
    ''' ContinuousPadder is: <tt>[1,1,1,1,2,1,3,1,2,4,0]</tt> in case of
    ''' <seealsocref="isPaddingLeft()"/>; <tt>[0,0,0,1,2,1,3,1,2,4,4]</tt> in
    ''' case of <seealsocref="isPaddingRight()"/>;
    ''' </para>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class ContinuousPadder
        Implements Preprocessor

        Private paddingLeftField As Boolean = True

        Private paddingRightField As Boolean = True

        ''' <summary>
        ''' Default construcot. Both left and right padding are turned on
        ''' </summary>
        Public Sub New()

        End Sub

        ''' 
        ''' <paramname="paddingLeft">
        '''            enables or disables left padding </param>
        ''' <paramname="paddingRight">
        '''            enables or disables right padding </param>
        Public Sub New(paddingLeft As Boolean, paddingRight As Boolean)
            paddingLeftField = paddingLeft
            paddingRightField = paddingRight
        End Sub

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            Dim n = data.Length
            If paddingLeftField Then
                Dim l = 0
                ' seek first non-zero cell
                For i = 0 To n - 1
                    If data(i) <> 0 Then
                        l = i
                        Exit For
                    End If
                Next
                Dim y0 = data(l)
                For i = 0 To l - 1
                    data(i) = y0
                Next
            End If
            If paddingRightField Then
                Dim r = 0
                ' seek last non-zero cell
                For i = n - 1 To 0 Step -1
                    If data(i) <> 0 Then
                        r = i
                        Exit For
                    End If
                Next
                Dim ynr = data(r)
                For i = r + 1 To n - 1
                    data(i) = ynr
                Next
            End If
        End Sub

        ''' 
        ''' <returns> {@code paddingLeft} </returns>
        Public Overridable Property PaddingLeft As Boolean
            Get
                Return paddingLeftField
            End Get
            Set(value As Boolean)
                paddingLeftField = value
            End Set
        End Property

        ''' 
        ''' <returns> {@code paddingRight} </returns>
        Public Overridable Property PaddingRight As Boolean
            Get
                Return paddingRightField
            End Get
            Set(value As Boolean)
                paddingRightField = value
            End Set
        End Property



    End Class

End Namespace
