#Region "Microsoft.VisualBasic::2288afd9598785195d560ffecb003f84, Data_science\NLP\LDA\LdaInterpreter.vb"

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

    '   Total Lines: 74
    '    Code Lines: 48 (64.86%)
    ' Comment Lines: 12 (16.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (18.92%)
    '     File Size: 2.71 KB


    '     Class LdaInterpreter
    ' 
    '         Function: (+2 Overloads) translate
    ' 
    '         Sub: (+2 Overloads) explain
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace LDA

    ''' <summary>
    ''' @author hankcs
    ''' </summary>
    Public Class LdaInterpreter

        ''' <summary>
        ''' To translate a LDA matrix to readable result </summary>
        ''' <param name="phi"> the LDA model </param>
        ''' <param name="vocabulary"> </param>
        ''' <param name="limit"> limit of max words in a topic </param>
        ''' <returns> a map array </returns>
        Public Shared Function translate(phi As Double()(), vocabulary As Vocabulary, limit As Integer) As Dictionary(Of String, Double)()
            Dim result As Dictionary(Of String, Double)() = New Dictionary(Of String, Double)(phi.Length - 1) {}

            limit = std.Min(limit, phi(0).Length)

            For k As Integer = 0 To phi.Length - 1
                Dim rankMap As New Dictionary(Of Double, String)

                For ii As Integer = 0 To phi(k).Length - 1
                    rankMap(phi(k)(ii)) = vocabulary.getWord(ii)
                Next

                result(k) = rankMap _
                    .OrderByDescending(Function(d) d.Key) _
                    .Take(limit) _
                    .ToDictionary(Function(d) d.Value,
                                  Function(d)
                                      Return d.Key
                                  End Function)
            Next

            Return result
        End Function

        Public Shared Function translate(tp As Double(), phi As Double()(), vocabulary As Vocabulary, limit As Integer) As Dictionary(Of String, Double)
            Dim topicMapArray = translate(phi, vocabulary, limit)
            Dim p As Double = -1.0
            Dim t As Integer = -1

            For k As Integer = 0 To tp.Length - 1
                If tp(k) > p Then
                    p = tp(k)
                    t = k
                End If
            Next

            Return topicMapArray(t)
        End Function

        ''' <summary>
        ''' To print the result in a well formatted form </summary>
        ''' <param name="result"> </param>
        Public Shared Sub explain(result As IDictionary(Of String, Double)())
            Dim i = 0

            For Each topicMap In result
                Console.Write("topic {0:D} :" & vbLf, std.Min(Threading.Interlocked.Increment(i), i - 1))
                explain(topicMap)
                Console.WriteLine()
            Next
        End Sub

        Public Shared Sub explain(topicMap As IDictionary(Of String, Double))
            For Each entry In topicMap
                Console.WriteLine(entry)
            Next
        End Sub
    End Class
End Namespace
