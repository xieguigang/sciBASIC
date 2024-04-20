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

Namespace Filters
    ''' <summary>
    ''' De-trends data by setting straight line between the first and the last point
    ''' and subtracting it from data. Having applied filters to data you should
    ''' reverse detrending by using <seealso cref="TrendRemover.retrend(,)"/>
    ''' 
    ''' @author Marcin Rzeźnicki
    ''' 
    ''' </summary>
    Public Class TrendRemover
        Implements Preprocessor

        Public Overridable Sub apply(data As Double()) Implements Preprocessor.apply
            ' de-trend data so to avoid boundary distortion
            ' we will achieve this by setting straight line from end to beginning
            ' and subtracting it from the trend
            Dim n = data.Length
            If n <= 2 Then
                Return
            End If
            Dim y0 = data(0)
            Dim slope = (data(n - 1) - y0) / (n - 1)
            For x As Integer = 0 To n - 1
                data(x) -= slope * x + y0
            Next
        End Sub

        ''' <summary>
        ''' Reverses the effect of <seealso cref="apply()"/> by modifying {@code
        ''' newData}
        ''' </summary>
        ''' <param name="newData">
        '''            processed data </param>
        ''' <param name="data">
        '''            original data </param>
        Public Overridable Sub retrend(newData As Double(), data As Double())
            Dim n = data.Length
            Dim y0 = data(0)
            Dim slope = (data(n - 1) - y0) / (n - 1)
            For x As Integer = 0 To n - 1
                newData(x) += slope * x + y0
            Next
        End Sub

    End Class

End Namespace
