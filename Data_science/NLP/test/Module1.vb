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
