Public Module Extensions

    ''' <summary>
    ''' Strip out HTML tags while preserving the basic formatting
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    Public Function StripHTMLSafely(source As String) As String
        Try
            Return StripHTMLDirectly(source)
        Catch ex As Exception
            Call App.LogException(New Exception(source, ex))
            Return source
        End Try
    End Function

    ''' <summary>
    ''' Strip out HTML tags while preserving the basic formatting
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks>http://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text</remarks>
    Public Function StripHTMLDirectly(source As String) As String
        Dim result As String

        ' Remove HTML Development formatting
        ' Replace line breaks with space
        ' because browsers inserts space
        result = source.Replace(vbCr, " ")
        ' Replace line breaks with space
        ' because browsers inserts space
        result = result.Replace(vbLf, " ")
        ' Remove step-formatting
        result = result.Replace(vbTab, String.Empty)
        ' Remove repeating spaces because browsers ignore them
        result = System.Text.RegularExpressions.Regex.Replace(result, "( )+", " ")

        ' Remove the header (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*head([^>])*>", "<head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*head( )*>)", "</head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<head>).*(</head>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' remove all scripts (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*script([^>])*>", "<script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*script( )*>)", "</script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        'result = System.Text.RegularExpressions.Regex.Replace(result,
        '         @"(<script>)([^(<script>\.</script>)])*(</script>)",
        '         string.Empty,
        '         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<script>).*(</script>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' remove all styles (prepare first by clearing attributes)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*style([^>])*>", "<style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*style( )*>)", "</style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(<style>).*(</style>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' insert tabs in spaces of <td> tags
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*td([^>])*>", vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' insert line breaks in places of <BR> and <LI> tags
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*br( )*>", vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*li( )*>", vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' insert line paragraphs (double line breaks) in place
        ' if <P>, <DIV> and <TR> tags
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*div([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*tr([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*p([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' Remove remaining tags like <a>, links, images,
        ' comments etc - anything that's enclosed inside < >
        result = System.Text.RegularExpressions.Regex.Replace(result, "<[^>]*>", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' replace special characters:
        result = System.Text.RegularExpressions.Regex.Replace(result, " ", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        result = System.Text.RegularExpressions.Regex.Replace(result, "&bull;", " * ", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&lsaquo;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&rsaquo;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&trade;", "(tm)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&frasl;", "/", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&lt;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&gt;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&copy;", "(c)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "&reg;", "(r)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        ' Remove all others. More can be added, see
        ' http://hotwired.lycos.com/webmonkey/reference/special_characters/
        result = System.Text.RegularExpressions.Regex.Replace(result, "&(.{2,6});", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        ' for testing
        'System.Text.RegularExpressions.Regex.Replace(result,
        '       this.txtRegex.Text,string.Empty,
        '       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        ' make line breaking consistent
        result = result.Replace(vbLf, vbCr)

        ' Remove extra line breaks and tabs:
        ' replace over 2 breaks with 2 and over 4 tabs with 4.
        ' Prepare first to remove any whitespaces in between
        ' the escaped characters and remove redundant tabs in between line breaks
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")( )+(" & vbCr & ")", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbTab & ")( )+(" & vbTab & ")", vbTab & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbTab & ")( )+(" & vbCr & ")", vbTab & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")( )+(" & vbTab & ")", vbCr & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        ' Remove redundant tabs
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+(" & vbCr & ")", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        ' Remove multiple tabs following a line break with just one tab
        result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+", vbCr & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        ' Initial replacement target string for line breaks
        Dim breaks As String = vbCr & vbCr & vbCr
        ' Initial replacement target string for tabs
        Dim tabs As String = vbTab & vbTab & vbTab & vbTab & vbTab
        For index As Integer = 0 To result.Length - 1
            result = result.Replace(breaks, vbCr & vbCr)
            result = result.Replace(tabs, vbTab & vbTab & vbTab & vbTab)
            breaks = breaks & vbCr
            tabs = tabs & vbTab
        Next

        Return result    ' That's it.
    End Function
End Module
