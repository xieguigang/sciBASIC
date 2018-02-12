Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development

    Public Module VBCodeSignature

        Const AccessPattern$ = "((Public )|(Private )|(Friend )|(Protected )|(Shadows )|(Shared )|(Overrides )|(Overloads )|(Overridable )|(MustOverrides ))*"
        Const TypePatterns$ = "^\s+" & AccessPattern & "\s*((Class)|(Module)|(Structure)|(Enum)|(Delegate)|(Interface))\s+" & VBLanguage.IdentiferPattern
        Const PropertyPatterns$ = "^\s+" & AccessPattern & "\s*((ReadOnly )|(WriteOnly )|(Default ))*\s*Property\s+" & VBLanguage.IdentiferPattern
        Const MethodPatterns$ = "^\s+" & AccessPattern & "\s*((Sub )|(Function )|(Iterator )|(Operator ))+\s*" & VBLanguage.IdentiferPattern
        Const ClosePatterns$ = "^\s+End\s((Sub)|(Function)|(Class)|(Structure)|(Enum)|(Interface)|(Operator)|(Module))"
        Const CloseTypePatterns$ = "^\s+End\s((Class)|(Structure)|(Enum)|(Interface)|(Module))"
        Const IndentsPattern$ = "^\s+"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function SummaryModules(vb As String) As String
            Return New Pointer(Of String)(vb.lTokens).SummaryInternal(vb)
        End Function

        <Extension>
        Private Function SummaryInternal(vblines As Pointer(Of String), vb$) As String
            Dim line$
            Dim tokens As Value(Of String) = ""
            Dim list As List(Of String)
            Dim type$
            Dim name$
            Dim indents$
            Dim properties As New List(Of NamedValue(Of String))
            Dim methods As New List(Of NamedValue(Of String))
            Dim operators As New List(Of NamedValue(Of String))
            Dim container As New NamedValue(Of String)
            Dim innerModules As New StringBuilder

            Do While Not vblines.EndRead
                line = ++vblines

                If Not (tokens = line.Match(TypePatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    If type = "Enum" Then
                        Dim members = vb _
                            .Match("Enum\s+" & name & ".+?End Enum", RegexICSng) _
                            .lTokens _
                            .Where(Function(s) s.IsPattern("\s+" & VBLanguage.IdentiferPattern & "\s*([=].+?)?\s*")) _
                            .Select(AddressOf Trim) _
                            .Where(Function(s) Not s.StringEmpty) _
                            .ToArray

                        Dim enumType As New StringBuilder

                        enumType.AppendLine(indents & type & " " & name)
                        enumType.AppendLine()
                        enumType.AppendLine(indents & "    " & members.JoinBy(", "))

                        Return enumType.ToString
                    Else
                        If container.IsEmpty Then
                            container = New NamedValue(Of String)(name, type, indents)
                        Else
                            ' 下一层堆栈
                            innerModules.AppendLine((vblines - 1).SummaryInternal(vb))
                        End If
                    End If
                End If
                If Not (tokens = line.Match(PropertyPatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    properties += New NamedValue(Of String)(name, type, indents)
                End If
                If Not (tokens = line.Match(MethodPatterns, RegexICMul)).StringEmpty Then
                    list = tokens.Split(" "c).AsList
                    type = list(-2)
                    name = list(-1)
                    indents = line.Match(IndentsPattern, RegexICMul)

                    If type = "Operator" Then
                        operators += New NamedValue(Of String)(name, type, indents)
                    Else
                        methods += New NamedValue(Of String)(name, type, indents)
                    End If
                End If
                If Not (tokens = line.Match(CloseTypePatterns, RegexICMul)).StringEmpty Then
                    Dim vbType As New StringBuilder
                    Dim members As New List(Of String)

                    vbType.AppendLine(container.Description & container.Value & " " & container.Name)
                    vbType.AppendLine()

                    If Not properties.IsNullOrEmpty Then
                        members += "Properties"
                        members += properties.OrderBy(Function(p) p.Name).Select(Function(p) $"{p.Description}{p.Value} {p.Name}").JoinBy(ASCII.LF)
                        members += ""
                    End If
                    If Not methods.IsNullOrEmpty Then
                        members += "Methods"
                        members += methods.OrderBy(Function(m) m.Value).ThenBy(Function(m) m.Name).Select(Function(m) $"{m.Description}{m.Value} {m.Name}").JoinBy(ASCII.LF)
                        members += ""
                    End If
                    If Not operators.IsNullOrEmpty Then
                        members += "Operators"
                        members += operators.OrderBy(Function(o) o.Name).Select(Function(o) $"{o.Description}{o.Value} {o.Name}").JoinBy(ASCII.LF)
                        members += ""
                    End If

                    vbType.AppendLine(members.JoinBy(ASCII.LF))
                    vbType.AppendLine(innerModules.ToString)

                    Return vbType.ToString
                End If
            Loop

            Throw New NotImplementedException
        End Function
    End Module
End Namespace