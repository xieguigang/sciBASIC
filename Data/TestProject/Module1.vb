Module Module1
    Sub Main()

        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.?ab".WildcardMatch("a.ab"))
        MsgBox("*.*ab".WildcardMatch("a.ab"))
        MsgBox("ab*cc".WildcardMatch("abddddddcc"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))

        Dim tk = Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Parser("D:\GCModeller\src\runtime\visualbasic_App\Data\query_syntaxTest.txt".ReadAllText)
    End Sub
End Module
