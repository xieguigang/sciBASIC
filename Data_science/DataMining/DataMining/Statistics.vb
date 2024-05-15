#Region "Microsoft.VisualBasic::03b505fd9b9ff0e9620f7cdb771288ee, Data_science\DataMining\DataMining\Statistics.vb"

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

    '   Total Lines: 288
    '    Code Lines: 90
    ' Comment Lines: 178
    '   Blank Lines: 20
    '     File Size: 9.50 KB


    ' Module Statistics
    ' 
    '     Function: GetRange, Mean, Median, Mode, (+2 Overloads) StdDev
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math

' AForge Math Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © AForge.NET, 2005-2011
' contacts@aforgenet.com
'

''' <summary>
''' Set of statistics functions.
''' </summary>
''' 
''' <remarks>The class represents collection of simple functions used
''' in statistics.</remarks>
''' 
Public Module Statistics

    ''' <summary>
    ''' Calculate mean value.
    ''' </summary>
    ''' 
    ''' <param name="values">Histogram array.</param>
    ''' 
    ''' <returns>Returns mean value.</returns>
    ''' 
    ''' <remarks><para>The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).</para>
    ''' 
    ''' <para>Sample usage:</para>
    ''' <code>
    ''' // create histogram array
    ''' int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
    ''' // calculate mean value
    ''' double mean = Statistics.Mean( histogram );
    ''' // output it (5.759)
    ''' Console.WriteLine( "mean = " + mean.ToString( "F3" ) );
    ''' </code>
    ''' </remarks>
    ''' 
    Public Function Mean(values As Integer()) As Double
        Dim hits As Integer
        Dim total As Double = 0
        Dim meanVal As Double = 0

        ' for all values
        Dim i As Integer = 0, n As Integer = values.Length
        While i < n
            hits = values(i)
            ' accumulate mean
            meanVal += CDbl(i) * hits
            ' accumalate total
            total += hits
            i += 1
        End While
        Return If((total = 0), 0, meanVal / total)
    End Function

    ''' <summary>
    ''' Calculate standard deviation.
    ''' </summary>
    ''' <param name="values">Histogram array.</param>
    ''' <returns>Returns value of standard deviation.</returns>
    ''' <remarks>
    ''' The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).
    ''' 
    ''' Sample usage:
    ''' 
    ''' ```vbnet
    ''' ' create histogram array
    ''' Dim histogram As Integer() = New Integer() { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 }
    ''' ' calculate standard deviation value
    ''' Dim stdDev = Statistics.StdDev( histogram )
    ''' '' output it (1.999)
    ''' Console.WriteLine( "std.dev. = " &amp; stdDev.ToString( "F3" ) )
    ''' ```
    ''' </remarks>
    Public Function StdDev(values As Integer()) As Double
        Return StdDev(values, Mean(values))
    End Function

    ''' <summary>
    ''' Calculate standard deviation.
    ''' </summary>
    ''' 
    ''' <param name="values">Histogram array.</param>
    ''' <param name="mean">Mean value of the histogram.</param>
    ''' 
    ''' <returns>Returns value of standard deviation.</returns>
    ''' 
    ''' <remarks><para>The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).</para>
    ''' 
    ''' <para>The method is an equevalent to the <see cref="StdDev"/> method,
    ''' but it relieas on the passed mean value, which is previously calculated
    ''' using <see cref="Mean"/> method.</para>
    ''' </remarks>
    ''' 
    Public Function StdDev(values As Integer(), mean As Double) As Double
        Dim stddevVal As Double = 0
        Dim diff As Double
        Dim hits As Integer
        Dim total As Integer = 0

        ' for all values
        Dim i As Integer = 0, n As Integer = values.Length
        While i < n
            hits = values(i)
            diff = CDbl(i) - mean
            ' accumulate std.dev.
            stddevVal += diff * diff * hits
            ' accumalate total
            total += hits
            i += 1
        End While

        Return If((total = 0), 0, std.Sqrt(stddevVal / total))
    End Function

    ''' <summary>
    ''' Calculate median value.
    ''' </summary>
    ''' 
    ''' <param name="values">Histogram array.</param>
    ''' 
    ''' <returns>Returns value of median.</returns>
    ''' 
    ''' <remarks>
    ''' <para>The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).</para>
    ''' 
    ''' <para><note>The median value is calculated accumulating histogram's
    ''' values starting from the <b>left</b> point until the sum reaches 50% of
    ''' histogram's sum.</note></para>
    ''' 
    ''' <para>Sample usage:</para>
    ''' <code>
    ''' // create histogram array
    ''' int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
    ''' // calculate median value
    ''' int median = Statistics.Median( histogram );
    ''' // output it (6)
    ''' Console.WriteLine( "median = " + median );
    ''' </code>
    ''' </remarks>
    ''' 
    Public Function Median(values As Integer()) As Integer
        Dim total As Integer = 0, n As Integer = values.Length

        ' for all values
        For i As Integer = 0 To n - 1
            ' accumalate total
            total += values(i)
        Next

        Dim halfTotal As Integer = total \ 2
        Dim medianVal As Integer = 0, v As Integer = 0

        ' find median value
        While medianVal < n
            v += values(medianVal)
            If v >= halfTotal Then
                Exit While
            End If
            medianVal += 1
        End While

        Return medianVal
    End Function

    ''' <summary>
    ''' Get range around median containing specified percentage of values.
    ''' </summary>
    ''' 
    ''' <param name="values">Histogram array.</param>
    ''' <param name="percent">Values percentage around median.</param>
    ''' 
    ''' <returns>Returns the range which containes specifies percentage
    ''' of values.</returns>
    ''' 
    ''' <remarks>
    ''' <para>The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).</para>
    ''' 
    ''' <para>The method calculates range of stochastic variable, which summary probability
    ''' comprises the specified percentage of histogram's hits.</para>
    ''' 
    ''' <para>Sample usage:</para>
    ''' <code>
    ''' // create histogram array
    ''' int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
    ''' // get 75% range around median
    ''' IntRange range = Statistics.GetRange( histogram, 0.75 );
    ''' // output it ([4, 8])
    ''' Console.WriteLine( "range = [" + range.Min + ", " + range.Max + "]" );
    ''' </code>
    ''' </remarks>
    ''' 
    Public Function GetRange(values As Integer(), percent As Double) As IntRange
        Dim total As Integer = 0, n As Integer = values.Length

        ' for all values
        For i As Integer = 0 To n - 1
            ' accumalate total
            total += values(i)
        Next

        Dim min As Integer, max As Integer, hits As Integer
        Dim h As Integer = CInt(std.Truncate(total * (percent + (1 - percent) / 2)))

        ' get range min value
        min = 0
        hits = total
        While min < n
            hits -= values(min)
            If hits < h Then
                Exit While
            End If
            min += 1
        End While
        ' get range max value
        max = n - 1
        hits = total
        While max >= 0
            hits -= values(max)
            If hits < h Then
                Exit While
            End If
            max -= 1
        End While
        Return New IntRange(min, max)
    End Function

    ''' <summary>
    ''' Calculate mode value.
    ''' </summary>
    ''' 
    ''' <param name="values">Histogram array.</param>
    ''' 
    ''' <returns>Returns mode value of the histogram array.</returns>
    ''' 
    ''' <remarks>
    ''' <para>The input array is treated as histogram, i.e. its
    ''' indexes are treated as values of stochastic function, but
    ''' array values are treated as "probabilities" (total amount of
    ''' hits).</para>
    ''' 
    ''' <para><note>Returns the minimum mode value if the specified histogram is multimodal.</note></para>
    '''
    ''' <para>Sample usage:</para>
    ''' <code>
    ''' // create array
    ''' int[] values = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
    ''' // calculate mode value
    ''' int mode = Statistics.Mode( values );
    ''' // output it (7)
    ''' Console.WriteLine( "mode = " + mode );
    ''' </code>
    ''' </remarks>
    ''' 
    Public Function Mode(values As Integer()) As Integer
        Dim modeVal As Integer = 0, curMax As Integer = 0

        Dim i As Integer = 0, length As Integer = values.Length
        While i < length
            If values(i) > curMax Then
                curMax = values(i)
                modeVal = i
            End If
            i += 1
        End While

        Return modeVal
    End Function
End Module
