Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace ApplicationServices.Development

    Public Module VBCodeSignature

        Const AccessPattern$ = "((Public )|(Private )|(Friend )|(Protected )|(Shadows )|(Shared )|(Overrides )|(Overloads )|(Overridable )|(MustOverrides ))*"
        Const TypePatterns$ = "^\s+" & AccessPattern & "\s*((Class)|(Module)|(Structure)|(Enum)|(Delegate)|(Interface))\s+" & VBLanguage.IdentiferPattern
        Const PropertyPatterns$ = "^\s+" & AccessPattern & "\s*((ReadOnly )|(WriteOnly )|(Default ))*\s*Property\s+" & VBLanguage.IdentiferPattern
        Const MethodPatterns$ = "^\s+" & AccessPattern & "\s*((Sub )|(Function )|(Iterator )|(Operator ))+\s*" & VBLanguage.IdentiferPattern
        Const ClosePatterns$ = "^\s+End\s((Sub)|(Function)|(Class)|(Structure)|(Enum)|(Interface)|(Operator)|(Module))"
        Const CloseTypePatterns$ = "^\s+End\s((Class)|(Structure)|(Enum)|(Interface)|(Module))"
        Const IndentsPattern$ = "^\s+"

        <Extension> Public Function SummaryModules(vb As String) As String
            Dim modules As New StringBuilder
            Dim tokens As Value(Of String) = ""
            Dim list As List(Of String)
            Dim type$
            Dim name$
            Dim indents$
            Dim vblines As Pointer(Of String) = vb.lTokens

            For Each line As String In vb.lTokens
                If Not (tokens = line.Match(TypePatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    If Not modules.Length = 0 Then
                        modules.AppendLine()
                    End If

                    modules.AppendLine($"{indents}{type} {name}")
                    modules.AppendLine()

                    If type = "Enum" Then
                        Dim members = vb _
                            .Match("Enum\s+" & name & ".+?End Enum", RegexICSng) _
                            .lTokens _
                            .Where(Function(s) s.IsPattern("\s+" & VBLanguage.IdentiferPattern & "\s*([=].+?)?\s*")) _
                            .Select(AddressOf Trim) _
                            .Where(Function(s) Not s.StringEmpty) _
                            .ToArray

                        modules.AppendLine(indents & "    " & members.JoinBy(", "))
                    End If
                End If
                If Not (tokens = line.Match(PropertyPatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    modules.AppendLine($"{indents}{type} {name}")
                End If
                If Not (tokens = line.Match(MethodPatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    modules.AppendLine($"{indents}{type} {name}")
                End If
            Next

            Return modules.ToString
        End Function

        Private Function SummaryInternal(vblines As Pointer(Of String)) As String

        End Function
    End Module
End Namespace