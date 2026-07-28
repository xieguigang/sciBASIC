Public Enum SymbolType
    ' --- Type containers: declarations that can nest types and members ---
    [Namespace] ' - Namespace XXX
    [Class]     ' - Class XXX
    [Module]    ' - Module XXX
    [Structure] ' - Structure XXX
    [Enum]      ' - Enum XXX
    [Interface] ' - Interface XXX
    ' --- Type members: declarations that live inside a type ---
    [New]       ' - Sub New()
    [Function]  ' - Function AAA(x As XX) As XXX
    [Sub]       ' - Sub AAA(x As XX)
    [Operator]  ' - Operator +(x As X, y As Y) As XX
    [Property]  ' - Property X As XX
    [Event]     ' - Event AAA(x As XX)
    [Delegate]  ' - Public Delegate Function AAA(x As XX) As XXX
    ' --- Variables: fields and local variables (Field is intentionally merged) ---
    Variable    ' - Dim X As XX / Public X As XX (field or local variable)
End Enum