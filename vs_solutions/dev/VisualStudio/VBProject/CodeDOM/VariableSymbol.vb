Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM


    ''' <summary>
    ''' a variable symbol. Per design, class/structure fields AND local method variables
    ''' share this single type (Field is intentionally not split out). Use <see cref="LanguageSymbolType.Parent"/>
    ''' to tell whether it is a field (Parent is a TypeContainerSymbol) or a local (Parent is a MemberSymbol).
    ''' </summary>
    Public Class VariableSymbol : Inherits MemberSymbol

        Public Property ValueType As TypeInfo

        Public Overrides ReadOnly Property Type As SymbolType = SymbolType.Variable

    End Class
End Namespace