Namespace Logical

    Public Enum Tokens
        UNDEFINE
        ''' <summary>
        ''' And Or Not Xor Nor Nand
        ''' </summary>
        [Operator]
        ''' <summary>
        ''' &lt;&lt;, &lt;, &lt;=, >, =>, >>, ~=, =, &lt;>
        ''' </summary>
        Comparer
        ''' <summary>
        ''' Space or VbTab
        ''' </summary>
        WhiteSpace
    End Enum

    Public Module TokenIcer

        Public ReadOnly Property Tokens As IReadOnlyDictionary(Of String, Tokens) =
            New Dictionary(Of String, Tokens) From {
 _
            {"And", Logical.Tokens.Operator},
            {"Or", Logical.Tokens.Operator},
            {"Not", Logical.Tokens.Operator},
            {"Xor", Logical.Tokens.Operator},
            {"Nor", Logical.Tokens.Operator},
            {"Nand", Logical.Tokens.Operator},
            {"<<", Logical.Tokens.Operator},
            {"<", Logical.Tokens.Operator},
            {"<=", Logical.Tokens.Operator},
            {">", Logical.Tokens.Operator},
            {"=>", Logical.Tokens.Operator},
            {">>", Logical.Tokens.Operator},
            {"~=", Logical.Tokens.Operator},
            {"=", Logical.Tokens.Operator},
            {"<>", Logical.Tokens.Operator},
            {vbTab, Logical.Tokens.WhiteSpace},
            {" ", Logical.Tokens.WhiteSpace}
        }


    End Module
End Namespace