Imports System.Threading
Imports std = System.Math

' 
' 	
' 	   Porter stemmer in Java. The original paper is in
' 	
' 	       Porter, 1980, An algorithm for suffix stripping, Program, Vol. 14,
' 	       no. 3, pp 130-137,
' 	
' 	   See also http://www.tartarus.org/~martin/PorterStemmer
' 	
' 	   History:
' 	
' 	   Release 1
' 	
' 	   Bug 1 (reported by Gonzalo Parra 16/10/99) fixed as marked below.
' 	   The words 'aed', 'eed', 'oed' leave k at 'a' for step 3, and b[k-1]
' 	   is then out outside the bounds of b.
' 	
' 	   Release 2
' 	
' 	   Similarly,
' 	
' 	   Bug 2 (reported by Steve Dyrdahl 22/2/00) fixed as marked below.
' 	   'ion' by itself leaves j = -1 in the test for 'ion' in step 5, and
' 	   b[j] is then outside the bounds of b.
' 	
' 	   Release 3
' 	
' 	   Considerably revised 4/9/00 in the light of many helpful suggestions
' 	   from Brian Goetz of Quiotix Corporation (brian@quiotix.com).
' 	
' 	   Release 4
' 	
' 	

''' <summary>
''' Stemmer, implementing the Porter Stemming Algorithm
'''  
''' The Stemmer class transforms a word into its root form.  The input
''' word can be provided a character at time (by calling add()), or at once
''' by calling one of the various stem(something) methods.
''' 
''' > https://github.com/meefen/npl-lab/tree/master
''' </summary>
''' <remarks>
''' The Porter stemming algorithm is a process for removing the commoner morphological and inflexional endings 
''' from words in English. Its main use is as part of a term normalization process that is usually done when 
''' setting up Information Retrieval systems.
''' 
''' **History and Development:**
''' - The algorithm was developed by Martin Porter in 1980.
''' - It has become one of the most popular stemming algorithms in English, largely due to its simplicity and effectiveness.
''' 
''' **How it Works:**
''' - The Porter stemmer works by applying a series of rules that remove or replace word endings. These rules are 
'''   applied in a specific order, and they are designed to handle different inflections and suffixes.
''' - The rules are divided into steps, and each step contains a set of conditions and transformations. If a word meets a certain condition, a specific transformation is applied.
''' - The algorithm uses a series of suffix replacement rules to reduce words to their root form. For example, "running" might be reduced to "run" by removing the "-ing" suffix.
''' **Steps in the Algorithm:**
''' 1. **Plural Reduction:** Removes plural suffixes, like "-s" or "-es".
''' 2. **Past Tense Reduction:** Changes past tense endings, such as "-ed".
''' 3. **Adjective to Adverb Reduction:** Handles suffixes that convert adjectives to adverbs, like "-ly".
''' 4. **Progressive Reduction:** Deals with progressive tense endings, such as "-ing".
''' 5. **Suffix Reduction:** Applies a series of more complex rules to handle various other suffixes.
''' **Advantages:**
''' - **Simplicity:** The algorithm is relatively simple to implement and understand.
''' - **Effectiveness:** It works well for a wide range of English words, reducing them to their base or root form.
''' - **Broad Applicability:** It is used in various applications, including search engines, text analysis, and natural language processing.
''' **Disadvantages:**
''' - **Over-Stemming:** Sometimes it reduces words too aggressively, leading to over-stemming. For example, "university" 
'''   might be reduced to "univers", which is not a valid English word.
''' - **Under-Stemming:** It may fail to reduce related words to the same stem. For example, "meeting" and "meet" might not be reduced to the same stem.
''' - **Language Specific:** It is designed specifically for English and may not work well for other languages.
''' **Applications:**
''' - **Information Retrieval:** Improves the accuracy of search engines by reducing words to their base form, allowing for more effective matching of queries to documents.
''' - **Text Mining:** Helps in analyzing large collections of text by reducing the dimensionality of the data.
''' - **Natural Language Processing (NLP):** Used in various NLP tasks, such as text classification, clustering, and summarization.
''' **Extensions and Variants:**
''' - Over the years, several extensions and variants of the Porter stemmer have been developed to address its limitations 
'''   and improve its performance. These include the Snowball stemmer and the Porter2 stemmer.
''' In summary, the Porter stemming algorithm is a foundational technique in text processing that helps in normalizing words 
''' to their base form, thereby enhancing the efficiency and effectiveness of various text-based applications. Despite its
''' limitations, it remains a widely used and influential algorithm in the field of computational linguistics and information 
''' retrieval.
''' </remarks>
Public Class Stemmer
    Private b As Char()
    Private i, i_end, j, k As Integer
    Private Const INC As Integer = 50
    ' unit of size whereby b is increased 
    Public Sub New()
        b = New Char(49) {}
        i = 0
        i_end = 0
    End Sub

    ''' <summary>
    ''' Add a character to the word being stemmed.  When you are finished
    ''' adding characters, you can call stem(void) to stem the word.
    ''' </summary>

    Public Overridable Sub add(ch As Char)
        If i = b.Length Then
            Dim new_b = New Char(i + INC - 1) {}
            For c = 0 To i - 1
                new_b(c) = b(c)
            Next
            b = new_b
        End If
        b(std.Min(Interlocked.Increment(i), i - 1)) = ch
    End Sub


    ''' <summary>
    ''' Adds wLen characters to the word being stemmed contained in a portion
    ''' of a char[] array. This is like repeated calls of add(char ch), but
    ''' faster.
    ''' </summary>

    Public Overridable Sub add(w As Char(), wLen As Integer)
        If i + wLen >= b.Length Then
            Dim new_b = New Char(i + wLen + INC - 1) {}
            For c = 0 To i - 1
                new_b(c) = b(c)
            Next
            b = new_b
        End If
        For c = 0 To wLen - 1
            b(std.Min(Interlocked.Increment(i), i - 1)) = w(c)
        Next
    End Sub

    ''' <summary>
    ''' After a word has been stemmed, it can be retrieved by toString(),
    ''' or a reference to the internal buffer can be retrieved by getResultBuffer
    ''' and getResultLength (which is generally more efficient.)
    ''' </summary>
    Public Overrides Function ToString() As String
        Return New String(b, 0, i_end)
    End Function

    ''' <summary>
    ''' Returns the length of the word resulting from the stemming process.
    ''' </summary>
    Public Overridable ReadOnly Property ResultLength As Integer
        Get
            Return i_end
        End Get
    End Property

    ''' <summary>
    ''' Returns a reference to a character buffer containing the results of
    ''' the stemming process.  You also need to consult getResultLength()
    ''' to determine the length of the result.
    ''' </summary>
    Public Overridable ReadOnly Property ResultBuffer As Char()
        Get
            Return b
        End Get
    End Property

    ' cons(i) is true <=> b[i] is a consonant. 

    Private Function cons(i As Integer) As Boolean
        Select Case b(i)
            Case "a"c, "e"c, "i"c, "o"c, "u"c
                Return False
            Case "y"c
                Return If(i = 0, True, Not cons(i - 1))
            Case Else
                Return True
        End Select
    End Function

    ' m() measures the number of consonant sequences between 0 and j. if c is
    ' a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
    ' presence,
    ' 
    ' <c><v>       gives 0
    ' <c>vc<v>     gives 1
    ' <c>vcvc<v>   gives 2
    ' <c>vcvcvc<v> gives 3
    ' ....
    ' 

    Private Function m() As Integer
        Dim n = 0
        Dim i = 0
        While True
            If i > j Then
                Return n
            End If
            If Not cons(i) Then
                Exit While
            End If
            i += 1
        End While
        i += 1
        While True
            While True
                If i > j Then
                    Return n
                End If
                If cons(i) Then
                    Exit While
                End If
                i += 1
            End While
            i += 1
            n += 1
            While True
                If i > j Then
                    Return n
                End If
                If Not cons(i) Then
                    Exit While
                End If
                i += 1
            End While
            i += 1
        End While

        Throw New InvalidCastException
    End Function

    ' vowelinstem() is true <=> 0,...j contains a vowel 

    Private Function vowelinstem() As Boolean
        Dim i As Integer
        For i = 0 To j
            If Not cons(i) Then
                Return True
            End If
        Next
        Return False
    End Function

    ' doublec(j) is true <=> j,(j-1) contain a double consonant. 

    Private Function doublec(j As Integer) As Boolean
        If j < 1 Then
            Return False
        End If
        If b(j) <> b(j - 1) Then
            Return False
        End If
        Return cons(j)
    End Function

    ' cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
    ' and also if the second c is not w,x or y. this is used when trying to
    ' restore an e at the end of a short word. e.g.
    ' 
    ' cav(e), lov(e), hop(e), crim(e), but
    ' snow, box, tray.
    ' 
    ' 

    Private Function cvc(i As Integer) As Boolean
        If i < 2 OrElse Not cons(i) OrElse cons(i - 1) OrElse Not cons(i - 2) Then
            Return False
        End If
        If True Then
            Dim ch As Char = b(i)
            If ch = "w"c OrElse ch = "x"c OrElse ch = "y"c Then
                Return False
            End If
        End If
        Return True
    End Function

    Private Function ends(s As String) As Boolean
        Dim l = s.Length
        Dim o = k - l + 1
        If o < 0 Then
            Return False
        End If
        For i = 0 To l - 1
            If b(o + i) <> s(i) Then
                Return False
            End If
        Next
        j = k - l
        Return True
    End Function

    ' setto(s) sets (j+1),...k to the characters in the string s, readjusting
    ' k. 

    Private Sub setto(s As String)
        Dim l = s.Length
        Dim o = j + 1
        For i = 0 To l - 1
            b(o + i) = s(i)
        Next
        k = j + l
    End Sub

    ' r(s) is used further down. 

    Private Sub r(s As String)
        If m() > 0 Then
            setto(s)
        End If
    End Sub

    ' step1() gets rid of plurals and -ed or -ing. e.g.
    ' 
    ' caresses  ->  caress
    ' ponies    ->  poni
    ' ties      ->  ti
    ' caress    ->  caress
    ' cats      ->  cat
    ' 
    ' feed      ->  feed
    ' agreed    ->  agree
    ' disabled  ->  disable
    ' 
    ' matting   ->  mat
    ' mating    ->  mate
    ' meeting   ->  meet
    ' milling   ->  mill
    ' messing   ->  mess
    ' 
    ' meetings  ->  meet
    ' 
    ' 

    Private Sub step1()
        If b(k) = "s"c Then
            If ends("sses") Then
                k -= 2
            Else
                If ends("ies") Then
                    setto("i")
                Else
                    If b(k - 1) <> "s"c Then
                        k -= 1
                    End If
                End If
            End If
        End If
        If ends("eed") Then
            If m() > 0 Then
                k -= 1
            End If
        Else
            If (ends("ed") OrElse ends("ing")) AndAlso vowelinstem() Then
                k = j
                If ends("at") Then
                    setto("ate")
                Else
                    If ends("bl") Then
                        setto("ble")
                    Else
                        If ends("iz") Then
                            setto("ize")
                        Else
                            If doublec(k) Then
                                k -= 1
                                If True Then
                                    Dim ch As Char = b(k)
                                    If ch = "l"c OrElse ch = "s"c OrElse ch = "z"c Then
                                        k += 1
                                    End If
                                End If
                            ElseIf m() = 1 AndAlso cvc(k) Then
                                setto("e")
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Sub

    ' step2() turns terminal y to i when there is another vowel in the stem. 

    Private Sub step2()
        If ends("y") AndAlso vowelinstem() Then
            b(k) = "i"c
        End If
    End Sub

    ' step3() maps double suffices to single ones. so -ization ( = -ize plus
    ' -ation) maps to -ize etc. note that the string before the suffix must give
    ' m() > 0. 

    Private Sub step3()
        If k = 0 Then
            Return
        End If
        Select Case b(k - 1)
            Case "a"c
                If ends("ational") Then
                    r("ate")
                    Exit Select
                End If
                If ends("tional") Then
                    r("tion")
                    Exit Select
                End If
            Case "c"c
                If ends("enci") Then
                    r("ence")
                    Exit Select
                End If
                If ends("anci") Then
                    r("ance")
                    Exit Select
                End If
            Case "e"c
                If ends("izer") Then
                    r("ize")
                    Exit Select
                End If
            Case "l"c
                If ends("bli") Then
                    r("ble")
                    Exit Select
                End If
                If ends("alli") Then
                    r("al")
                    Exit Select
                End If
                If ends("entli") Then
                    r("ent")
                    Exit Select
                End If
                If ends("eli") Then
                    r("e")
                    Exit Select
                End If
                If ends("ousli") Then
                    r("ous")
                    Exit Select
                End If
            Case "o"c
                If ends("ization") Then
                    r("ize")
                    Exit Select
                End If
                If ends("ation") Then
                    r("ate")
                    Exit Select
                End If
                If ends("ator") Then
                    r("ate")
                    Exit Select
                End If
            Case "s"c
                If ends("alism") Then
                    r("al")
                    Exit Select
                End If
                If ends("iveness") Then
                    r("ive")
                    Exit Select
                End If
                If ends("fulness") Then
                    r("ful")
                    Exit Select
                End If
                If ends("ousness") Then
                    r("ous")
                    Exit Select
                End If
            Case "t"c
                If ends("aliti") Then
                    r("al")
                    Exit Select
                End If
                If ends("iviti") Then
                    r("ive")
                    Exit Select
                End If
                If ends("biliti") Then
                    r("ble")
                    Exit Select
                End If
            Case "g"c
                If ends("logi") Then
                    r("log")
                    Exit Select
                End If
        End Select
    End Sub

    ' step4() deals with -ic-, -full, -ness etc. similar strategy to step3. 

    Private Sub step4()
        Select Case b(k)
            Case "e"c
                If ends("icate") Then
                    r("ic")
                    Exit Select
                End If
                If ends("ative") Then
                    r("")
                    Exit Select
                End If
                If ends("alize") Then
                    r("al")
                    Exit Select
                End If
            Case "i"c
                If ends("iciti") Then
                    r("ic")
                    Exit Select
                End If
            Case "l"c
                If ends("ical") Then
                    r("ic")
                    Exit Select
                End If
                If ends("ful") Then
                    r("")
                    Exit Select
                End If
            Case "s"c
                If ends("ness") Then
                    r("")
                    Exit Select
                End If
        End Select
    End Sub

    ' step5() takes off -ant, -ence etc., in context <c>vcvc<v>. 

    Private Sub step5()
        If k = 0 Then
            Return
        End If
        Select Case b(k - 1)
            Case "a"c
                If ends("al") Then
                    Exit Select
                End If
                Return
            Case "c"c
                If ends("ance") Then
                    Exit Select
                End If
                If ends("ence") Then
                    Exit Select
                End If
                Return
            Case "e"c
                If ends("er") Then
                    Exit Select
                End If
                Return
            Case "i"c
                If ends("ic") Then
                    Exit Select
                End If
                Return
            Case "l"c
                If ends("able") Then
                    Exit Select
                End If
                If ends("ible") Then
                    Exit Select
                End If
                Return
            Case "n"c
                If ends("ant") Then
                    Exit Select
                End If
                If ends("ement") Then
                    Exit Select
                End If
                If ends("ment") Then
                    Exit Select
                End If
                ' element etc. not stripped before the m 
                If ends("ent") Then
                    Exit Select
                End If
                Return
            Case "o"c
                If ends("ion") AndAlso j >= 0 AndAlso (b(j) = "s"c OrElse b(j) = "t"c) Then
                    Exit Select
                End If
                ' j >= 0 fixes Bug 2 
                If ends("ou") Then
                    Exit Select
                End If
                Return
                ' takes care of -ous 
            Case "s"c
                If ends("ism") Then
                    Exit Select
                End If
                Return
            Case "t"c
                If ends("ate") Then
                    Exit Select
                End If
                If ends("iti") Then
                    Exit Select
                End If
                Return
            Case "u"c
                If ends("ous") Then
                    Exit Select
                End If
                Return
            Case "v"c
                If ends("ive") Then
                    Exit Select
                End If
                Return
            Case "z"c
                If ends("ize") Then
                    Exit Select
                End If
                Return
            Case Else
                Return
        End Select
        If m() > 1 Then
            k = j
        End If
    End Sub

    ' step6() removes a final -e if m() > 1. 

    Private Sub step6()
        j = k
        If b(k) = "e"c Then
            Dim a As Integer = m()
            If a > 1 OrElse a = 1 AndAlso Not cvc(k - 1) Then
                k -= 1
            End If
        End If
        If b(k) = "l"c AndAlso doublec(k) AndAlso m() > 1 Then
            k -= 1
        End If
    End Sub

    ''' <summary>
    ''' Stem the word placed into the Stemmer buffer through calls to add().
    ''' Returns true if the stemming process resulted in a word different
    ''' from the input.  You can retrieve the result with
    ''' getResultLength()/getResultBuffer() or toString().
    ''' </summary>
    Public Overridable Sub stem()
        k = i - 1
        If k > 1 Then
            step1()
            step2()
            step3()
            step4()
            step5()
            step6()
        End If
        i_end = k + 1
        i = 0
    End Sub
End Class
