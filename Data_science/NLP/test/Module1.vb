#Region "Microsoft.VisualBasic::f024edb30dfd841827a05e91fc603d88, Data_science\NLP\test\Module1.vb"

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

    '   Total Lines: 86
    '    Code Lines: 63 (73.26%)
    ' Comment Lines: 13 (15.12%)
    '    - Xml Docs: 53.85%
    ' 
    '   Blank Lines: 10 (11.63%)
    '     File Size: 4.21 KB


    ' Module Module1
    ' 
    '     Sub: Main111, testtfidf
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.NLP

Module Module1

    Sub testtfidf()
        Dim docs = New String() {"knowledge building needs innovative environments are better at helping their inhabitants explore the adjacent possible", "As a basis for evaluating explanations, creative knowledge building weight of evidence is a poor substitute for the first two criteria listed above.", "A public idea database makes every passing idea visible to everyone else in the organization and do creative work.", "questioning and various disturbances initiate cycles of innovation and creative organization knowledge.", "We need some way to ensure knowledge to spread among environments that any notes that are dropped are dropped."}

        Dim tfIdf As TF_IDF = New TF_IDF(docs, {})
        For i = 0 To tfIdf.docSize - 1
            Console.Write(i + 1.ToString() & vbTab)
            For j = 0 To tfIdf.docSize - 1
                Console.Write(tfIdf.getSimilarity(i, j).ToString() & vbTab)
            Next
            Console.WriteLine()
        Next
    End Sub

    ''' <summary>
    ''' Test program for demonstrating the Stemmer.  It reads text from a
    ''' a list of files, stems each word, and writes the result to standard
    ''' output. Note that the word stemmed is expected to be in lower case:
    ''' forcing lower case must be done outside the Stemmer class.
    ''' Usage: Stemmer file-name file-name ...
    ''' </summary>
    Public Sub Main111(args As String())
        args(0) = "data//stem.txt"
        Dim w = New Char(500) {}
        Dim s As Stemmer = New Stemmer()
        For i As Integer = 0 To args.Length - 1
            Try
                Dim [in] As FileStream = New FileStream(args(i), FileMode.Open, FileAccess.Read)

                Try
                    While True
                        Dim ch = 0 ' @in.Read();
                        If Char.IsLetter(Microsoft.VisualBasic.ChrW(ch)) Then
                            Dim j = 0
                            While True
                                ch = AscW(Char.ToLower(Microsoft.VisualBasic.ChrW(ch)))
                                w(j) = Microsoft.VisualBasic.ChrW(ch)
                                If j < 500 Then
                                    j += 1
                                End If
                                ch = 0 ' @in.Read();
                                If Not Char.IsLetter(Microsoft.VisualBasic.ChrW(ch)) Then
                                    ' to test add(char ch) 
                                    For c = 0 To j - 1
                                        s.add(w(c))
                                    Next

                                    ' or, to test add(char[] w, int j) 
                                    ' s.add(w, j); 

                                    s.stem()
                                    If True Then
                                        Dim u As String

                                        ' and now, to test toString() : 
                                        u = s.ToString()

                                        ' to test getResultBuffer(), getResultLength() : 
                                        ' u = new String(s.getResultBuffer(), 0, s.getResultLength()); 

                                        Console.Write(u)
                                    End If
                                    Exit While
                                End If
                            End While
                        End If
                        If ch < 0 Then
                            Exit While
                        End If
                        Console.Write(Microsoft.VisualBasic.ChrW(ch))
                    End While
                Catch __unusedIOException1__ As IOException
                    Console.WriteLine("error reading " & args(i))
                    Exit For
                End Try
            Catch __unusedFileNotFoundException1__ As FileNotFoundException
                Console.WriteLine("file " & args(i) & " not found")
                Exit For
            End Try
        Next
    End Sub
End Module
