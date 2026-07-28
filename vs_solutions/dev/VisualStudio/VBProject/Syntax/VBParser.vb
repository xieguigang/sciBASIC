Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Syntax

    ''' <summary>
    ''' recursive descent parser for VB.NET source code.
    '''
    ''' <see cref="Parse"/> turns a VB.NET source text string into a symbol
    ''' tree rooted at a synthetic <see cref="TypeContainerSymbol"/> (namespace kind).
    ''' It recognises container types (class/module/structure/enum/interface/
    ''' namespace), their members (function/sub/operator/property and delegate
    ''' declarations) and the local variable symbols (Dim/Static/Const) that
    ''' appear inside a member body. Every clr type reference is stored as a
    ''' <see cref="TypeInfo"/>.
    ''' </summary>
    Public Module VBParser

        ''' <summary>
        ''' parse the given VB.NET source text and return the root symbol
        ''' container (a synthetic namespace). Its <see cref="TypeContainerSymbol.InternalNested"/>
        ''' holds nested types and its <see cref="TypeContainerSymbol.Members"/> holds
        ''' top level members / fields.
        ''' </summary>
        Public Function Parse(source As String) As TypeContainerSymbol
            Dim scanner As New VBScanner()
            Dim stmts As List(Of VBStatement) = scanner.Scan(source)

            Dim root As New NamespaceSymbol()
            root.Name = ""

            Dim i As Integer = 0
            ParseBlock(stmts, i, root, Nothing, Nothing)

            Return root
        End Function

        ' ------------------------------------------------------------------
        ' statement cursor factory
        ' ------------------------------------------------------------------

        Private Function NewCursor(stmt As VBStatement) As StmtParser
            Dim sp As New StmtParser(stmt.Tokens)
            sp.Attributes = New List(Of String)(stmt.Attributes)
            sp.CollectLeading()
            Return sp
        End Function

        ' ------------------------------------------------------------------
        ' block driver
        ' ------------------------------------------------------------------

        Private Sub ParseBlock(stmts As List(Of VBStatement), ByRef i As Integer, container As TypeContainerSymbol, stopKeyword As String, member As CallableMemberSymbol)
            Dim depth As Integer = 0

            While i < stmts.Count
                Dim stmt As VBStatement = stmts(i)

                If stmt.Tokens.Count = 0 Then
                    i += 1
                    Continue While
                End If

                Dim sp As StmtParser = NewCursor(stmt)
                Dim head As String = sp.Current.Text.ToLowerInvariant()

                If head = "end" Then
                    Dim endName As String = If(sp.Pos + 1 < stmt.Tokens.Count, stmt.Tokens(sp.Pos + 1).Text.ToLowerInvariant(), "")
                    If stopKeyword IsNot Nothing AndAlso depth <= 0 AndAlso endName = stopKeyword Then
                        i += 1
                        Return
                    End If
                    depth -= 1
                    i += 1
                    Continue While
                End If

                If head = "next" OrElse head = "loop" Then
                    depth -= 1
                    i += 1
                    Continue While
                End If

                If IsBlockStarter(head) Then
                    depth += 1
                    i += 1
                    Continue While
                End If

                Select Case head
                    Case "class", "module", "structure", "struct", "enum", "interface", "namespace"
                        ParseContainerType(stmt, stmts, i, container)
                    Case "function", "sub", "property", "operator"
                        ParseInvokeMember(stmt, stmts, i, container)
                    Case "delegate"
                        ParseDelegate(stmt, stmts, i, container)
                    Case "dim", "static", "const"
                        If member IsNot Nothing Then
                            DeclareLocals(stmt.Tokens, member)
                        Else
                            ParseField(stmt, container)
                        End If
                        i += 1
                    Case "public", "private", "friend", "protected", "shared", "readonly", "writeonly", "default"
                        If member Is Nothing Then
                            ParseField(stmt, container)
                        End If
                        i += 1
                    Case "inherits", "implements"
                        If member Is Nothing Then
                            ParseContainerClause(stmt, container)
                        End If
                        i += 1
                    Case "imports", "option"
                        ' top level directives : skip (not symbol declarations)
                        i += 1
                    Case Else
                        If member Is Nothing Then
                            ParseField(stmt, container)
                        End If
                        i += 1
                End Select
            End While
        End Sub

        ' ------------------------------------------------------------------
        ' container types
        ' ------------------------------------------------------------------

        Private Sub ParseContainerType(stmt As VBStatement, stmts As List(Of VBStatement), ByRef i As Integer, container As TypeContainerSymbol)
            Dim sp As StmtParser = NewCursor(stmt)
            Dim kw As String = sp.Current.Text.ToLowerInvariant()
            Dim sym As SymbolType = MapContainerSymbol(kw)

            Dim ct As TypeContainerSymbol
            Select Case sym
                Case SymbolType.[Class] : ct = New ClassSymbol()
                Case SymbolType.[Module] : ct = New ModuleSymbol()
                Case SymbolType.[Structure] : ct = New StructureSymbol()
                Case SymbolType.[Enum] : ct = New EnumSymbol()
                Case SymbolType.[Interface] : ct = New InterfaceSymbol()
                Case Else : ct = New NamespaceSymbol()
            End Select
            ct.Modifiers = sp.Modifiers
            ct.Attributes = sp.Attributes
            ct.XmlDoc = stmt.XmlDoc
            ct.Parent = container
            sp.Pos += 1

            If Not sp.Eof Then
                ct.Name = sp.Current.Text
                sp.Pos += 1
            End If

            ' generics: Name (Of T)
            If Not sp.Eof AndAlso sp.Current.Text = "("c AndAlso sp.Pos + 1 < stmt.Tokens.Count AndAlso stmt.Tokens(sp.Pos + 1).Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                ct.GenericTypeArguments = ReadGenericParameters(sp)
            End If

            If sym = SymbolType.[Enum] Then
                If Not sp.Eof AndAlso sp.Current.Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                    sp.Pos += 1
                    CType(ct, EnumSymbol).EnumBaseType = ReadTypeRef(sp)
                End If
            ElseIf sym <> SymbolType.[Namespace] Then
                While Not sp.Eof
                    Dim k As String = sp.Current.Text.ToLowerInvariant()

                    If k = "inherits" Then
                        sp.Pos += 1
                        If ct.InheritsType Is Nothing Then
                            ct.InheritsType = ReadTypeRef(sp)
                        End If
                    ElseIf k = "implements" Then
                        sp.Pos += 1
                        Dim lst As New List(Of TypeInfo)
                        Do
                            lst.Add(ReadTypeRef(sp))
                            If Not sp.Eof AndAlso sp.Current.Text = ","c Then
                                sp.Pos += 1
                            Else
                                Exit Do
                            End If
                        Loop
                        ct.ImplementsInterfaces = lst.ToArray()
                    Else
                        sp.Pos += 1
                    End If
                End While
            End If

            AddToContainer(container, ct)
            i += 1
            Dim stopKw As String = If(kw = "struct", "structure", kw)
            ParseBlock(stmts, i, ct, stopKw, Nothing)
        End Sub

        ' container-level clauses that may appear on their own line inside a
        ' class / structure / interface body.
        Private Sub ParseContainerClause(stmt As VBStatement, container As TypeContainerSymbol)
            Dim sp As StmtParser = NewCursor(stmt)
            Dim k As String = sp.Current.Text.ToLowerInvariant()
            sp.Pos += 1

            If k = "inherits" Then
                If container.InheritsType Is Nothing Then
                    container.InheritsType = ReadTypeRef(sp)
                End If
            ElseIf k = "implements" Then
                Dim lst As New List(Of TypeInfo)
                Do
                    lst.Add(ReadTypeRef(sp))
                    If Not sp.Eof AndAlso sp.Current.Text = ","c Then
                        sp.Pos += 1
                    Else
                        Exit Do
                    End If
                Loop
                container.ImplementsInterfaces = lst.ToArray()
            End If
        End Sub

        ' ------------------------------------------------------------------
        ' members : function / sub / property / operator
        ' ------------------------------------------------------------------

        Private Sub ParseInvokeMember(stmt As VBStatement, stmts As List(Of VBStatement), ByRef i As Integer, container As TypeContainerSymbol)
            Dim sp As StmtParser = NewCursor(stmt)
            Dim kw As String = sp.Current.Text.ToLowerInvariant()

            sp.Pos += 1
            Dim name As String
            If kw = "operator" Then
                name = ReadOperatorName(sp)
            ElseIf Not sp.Eof Then
                name = sp.Current.Text
                sp.Pos += 1
            Else
                name = ""
            End If

            Dim sym As SymbolType = MapMemberSymbol(kw)
            If kw = "sub" AndAlso name.Equals("New", StringComparison.OrdinalIgnoreCase) Then
                sym = SymbolType.[New]
            End If

            Dim inv As CallableMemberSymbol
            If kw = "property" Then
                inv = New PropertySymbol()
            Else
                inv = New MethodSymbol(sym)
            End If
            inv.Modifiers = sp.Modifiers
            inv.Attributes = sp.Attributes
            inv.XmlDoc = stmt.XmlDoc
            inv.Parent = container
            inv.Name = name

            ' generics: Name (Of T) ( params )
            If Not sp.Eof AndAlso sp.Current.Text = "("c AndAlso sp.Pos + 1 < stmt.Tokens.Count AndAlso stmt.Tokens(sp.Pos + 1).Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                inv.GenericTypeArguments = ReadGenericParameters(sp)
            End If

            If Not sp.Eof AndAlso sp.Current.Text = "("c Then
                inv.Parameters = ReadParameters(sp)
            End If

            If Not sp.Eof AndAlso sp.Current.Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                inv.ReturnType = ReadTypeRef(sp)
            End If

            AddToContainer(container, inv)
            i += 1

            If kw = "property" Then
                ' an auto-property has no Get/Set/End Property body
                Dim hasBody As Boolean = False
                Dim k2 As Integer = i
                While k2 < stmts.Count AndAlso stmts(k2).Tokens.Count = 0
                    k2 += 1
                End While
                If k2 < stmts.Count Then
                    Dim sp2 As StmtParser = NewCursor(stmts(k2))
                    Dim h2 As String = sp2.Current.Text.ToLowerInvariant()
                    If h2 = "get" OrElse h2 = "set" Then
                        hasBody = True
                    ElseIf h2 = "end" Then
                        Dim en2 As String = If(sp2.Pos + 1 < stmts(k2).Tokens.Count, stmts(k2).Tokens(sp2.Pos + 1).Text.ToLowerInvariant(), "")
                        If en2 = "property" Then
                            hasBody = True
                        End If
                    End If
                End If
                If hasBody Then
                    ParseBlock(stmts, i, container, "property", inv)
                End If
            Else
                ParseBlock(stmts, i, container, kw, inv)
            End If
        End Sub

        ' ------------------------------------------------------------------
        ' delegate declarations
        ' ------------------------------------------------------------------

        Private Sub ParseDelegate(stmt As VBStatement, stmts As List(Of VBStatement), ByRef i As Integer, container As TypeContainerSymbol)
            Dim sp As StmtParser = NewCursor(stmt)

            If Not sp.Eof AndAlso sp.Current.Text.Equals("delegate", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
            End If

            Dim del As New DelegateSymbol()
            del.Modifiers = sp.Modifiers
            del.Attributes = sp.Attributes
            del.XmlDoc = stmt.XmlDoc
            del.Parent = container

            ' skip the Sub / Function keyword of the delegate
            If Not sp.Eof Then
                sp.Pos += 1
            End If

            If Not sp.Eof Then
                del.Name = sp.Current.Text
                sp.Pos += 1
            End If

            If Not sp.Eof AndAlso sp.Current.Text = "("c AndAlso sp.Pos + 1 < stmt.Tokens.Count AndAlso stmt.Tokens(sp.Pos + 1).Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                del.GenericTypeArguments = ReadGenericParameters(sp)
            End If

            If Not sp.Eof AndAlso sp.Current.Text = "("c Then
                del.Parameters = ReadParameters(sp)
            End If

            If Not sp.Eof AndAlso sp.Current.Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                del.ValueType = ReadTypeRef(sp)
            End If

            AddToContainer(container, del)
            i += 1
        End Sub

        ' ------------------------------------------------------------------
        ' fields and local variables
        ' ------------------------------------------------------------------

        Private Sub ParseField(stmt As VBStatement, container As TypeContainerSymbol)
            Dim sp As StmtParser = NewCursor(stmt)
            Dim rest As List(Of Token) = stmt.Tokens.GetRange(sp.Pos, stmt.Tokens.Count - sp.Pos)
            DeclareFields(rest, container)
        End Sub

        ''' <summary>
        ''' declare type-level fields (Public/Private/Dim X As XX at container scope).
        ''' They are stored in <see cref="TypeContainerSymbol.Members"/>.
        ''' </summary>
        Private Sub DeclareFields(tokens As List(Of Token), parent As TypeContainerSymbol)
            If parent.Members Is Nothing Then
                parent.Members = New Dictionary(Of String, LanguageSymbolType)
            End If

            Dim start As Integer = 0
            If start < tokens.Count AndAlso {"dim", "static", "const"}.Contains(tokens(start).Text.ToLowerInvariant()) Then
                start += 1
            End If

            Dim segs As List(Of List(Of Token)) = SplitTopLevel(tokens.GetRange(start, tokens.Count - start), ","c)
            If segs.Count = 0 Then
                Return
            End If

            ' precompute the own "As" type of every segment
            Dim ownType(segs.Count - 1) As TypeInfo
            For s As Integer = 0 To segs.Count - 1
                Dim seg As List(Of Token) = segs(s)
                If seg.Count >= 3 AndAlso seg(1).Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                    ownType(s) = TypeInfoHelper.TypeRef(CleanType(seg, 2))
                Else
                    ownType(s) = Nothing
                End If
            Next

            For s As Integer = 0 To segs.Count - 1
                Dim seg As List(Of Token) = segs(s)
                If seg.Count = 0 Then
                    Continue For
                End If

                Dim name As String = seg(0).Text
                Dim type As TypeInfo = ownType(s)

                If type Is Nothing Then
                    ' inherit the first following "As" clause
                    For j As Integer = s + 1 To segs.Count - 1
                        If ownType(j) IsNot Nothing Then
                            type = ownType(j)
                            Exit For
                        End If
                    Next
                End If

                If Not parent.Members.ContainsKey(name) Then
                    parent.Members(name) = New VariableSymbol With {
                        .Name = name,
                        .Parent = parent,
                        .ValueType = If(type, TypeInfoHelper.TypeRef("Object"))
                    }
                End If
            Next
        End Sub

        ''' <summary>
        ''' declare local variables (Dim/Static/Const) inside a member body.
        ''' They are stored in <see cref="CallableMemberSymbol.Locals"/> so that
        ''' they are never confused with type-level members.
        ''' </summary>
        Private Sub DeclareLocals(tokens As List(Of Token), member As CallableMemberSymbol)
            If member.Locals Is Nothing Then
                member.Locals = New Dictionary(Of String, VariableSymbol)
            End If

            Dim start As Integer = 0
            If start < tokens.Count AndAlso {"dim", "static", "const"}.Contains(tokens(start).Text.ToLowerInvariant()) Then
                start += 1
            End If

            Dim segs As List(Of List(Of Token)) = SplitTopLevel(tokens.GetRange(start, tokens.Count - start), ","c)
            If segs.Count = 0 Then
                Return
            End If

            ' precompute the own "As" type of every segment
            Dim ownType(segs.Count - 1) As TypeInfo
            For s As Integer = 0 To segs.Count - 1
                Dim seg As List(Of Token) = segs(s)
                If seg.Count >= 3 AndAlso seg(1).Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                    ownType(s) = TypeInfoHelper.TypeRef(CleanType(seg, 2))
                Else
                    ownType(s) = Nothing
                End If
            Next

            For s As Integer = 0 To segs.Count - 1
                Dim seg As List(Of Token) = segs(s)
                If seg.Count = 0 Then
                    Continue For
                End If

                Dim name As String = seg(0).Text
                Dim type As TypeInfo = ownType(s)

                If type Is Nothing Then
                    ' inherit the first following "As" clause
                    For j As Integer = s + 1 To segs.Count - 1
                        If ownType(j) IsNot Nothing Then
                            type = ownType(j)
                            Exit For
                        End If
                    Next
                End If

                If Not member.Locals.ContainsKey(name) Then
                    member.Locals(name) = New VariableSymbol With {
                        .Name = name,
                        .Parent = member,
                        .ValueType = If(type, TypeInfoHelper.TypeRef("Object"))
                    }
                End If
            Next
        End Sub

        ' ------------------------------------------------------------------
        ' low level token helpers
        ' ------------------------------------------------------------------

        Private Sub AddToContainer(container As TypeContainerSymbol, sym As LanguageSymbolType)
            Select Case sym.Type
                Case SymbolType.[Class], SymbolType.[Module], SymbolType.[Structure], SymbolType.[Enum], SymbolType.[Interface], SymbolType.[Namespace]
                    If container.InternalNested Is Nothing Then
                        container.InternalNested = New Dictionary(Of String, LanguageSymbolType)
                    End If
                    container.InternalNested(sym.Name) = sym
                Case Else
                    If container.Members Is Nothing Then
                        container.Members = New Dictionary(Of String, LanguageSymbolType)
                    End If
                    container.Members(sym.Name) = sym
            End Select
        End Sub

        Private Function ReadOperatorName(sp As StmtParser) As String
            If sp.Eof Then
                Return ""
            End If

            Dim tk As Token = sp.Current

            If tk.Text.Equals("CType", StringComparison.OrdinalIgnoreCase) OrElse
               tk.Text.Equals("IsTrue", StringComparison.OrdinalIgnoreCase) OrElse
               tk.Text.Equals("IsFalse", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                Return tk.Text
            End If

            Dim nm As String = tk.Text
            sp.Pos += 1
            Return nm
        End Function

        Private Function ReadTypeRef(sp As StmtParser) As TypeInfo
            If sp.Eof Then
                Return Nothing
            End If

            Dim sb As New StringBuilder()
            sb.Append(sp.Current.Text)
            sp.Pos += 1

            While Not sp.Eof AndAlso sp.Current.Text = "."c
                sb.Append("."c)
                sp.Pos += 1
                If Not sp.Eof Then
                    sb.Append(sp.Current.Text)
                    sp.Pos += 1
                End If
            End While

            ' generic arguments : either "Type (Of ...)" or just "Of ..."
            If Not sp.Eof AndAlso sp.Current.Text = "("c AndAlso
               sp.Pos + 1 < sp.Tokens.Count AndAlso
               sp.Tokens(sp.Pos + 1).Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                sp.Pos += 1
                sb.Append("(Of ")
            ElseIf Not sp.Eof AndAlso sp.Current.Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                sb.Append("(Of ")
            End If

            If sb.ToString().EndsWith("(Of ") Then
                ' we just consumed the "(Of" opener; we are now positioned at
                ' the first type argument, inside one level of parentheses
                ' (the "(" precedes "Of" in VB syntax). Track its paren depth
                ' so we stop at the matching ")".
                Dim depth As Integer = 1

                Do
                    If sp.Eof Then
                        Exit Do
                    End If
                    Dim tk As Token = sp.Current
                    sb.Append(tk.Text)
                    If tk.Text = "("c Then
                        depth += 1
                    ElseIf tk.Text = ")"c Then
                        depth -= 1
                    End If
                    sp.Pos += 1
                Loop While depth > 0
            End If

            Return TypeInfoHelper.TypeRef(sb.ToString().Trim())
        End Function

        Private Function ReadParameters(sp As StmtParser) As Dictionary(Of String, TypeInfo)
            Dim dict As New Dictionary(Of String, TypeInfo)

            If sp.Eof OrElse sp.Current.Text <> "("c Then
                Return dict
            End If

            sp.Pos += 1

            While Not sp.Eof AndAlso sp.Current.Text <> ")"c
                ParseOneParam(sp, dict)

                If Not sp.Eof AndAlso sp.Current.Text = ","c Then
                    sp.Pos += 1
                Else
                    Exit While
                End If
            End While

            If Not sp.Eof AndAlso sp.Current.Text = ")"c Then
                sp.Pos += 1
            End If

            Return dict
        End Function

        Private Sub ParseOneParam(sp As StmtParser, dict As Dictionary(Of String, TypeInfo))
            ' optional parameter modifier (ByVal / ByRef / Optional / ParamArray)
            While Not sp.Eof AndAlso IsParamModifier(sp.Current.Text)
                sp.Pos += 1
            End While

            If sp.Eof OrElse sp.Current.Text = ","c OrElse sp.Current.Text = ")"c Then
                Return
            End If

            Dim name As String = sp.Current.Text
            sp.Pos += 1

            Dim type As TypeInfo = Nothing
            If Not sp.Eof AndAlso sp.Current.Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                sp.Pos += 1
                type = ReadTypeRef(sp)
            End If

            ' skip any trailing default value / array parentheses so we land on
            ' the next parameter or the closing ")"
            While Not sp.Eof AndAlso sp.Current.Text <> ","c AndAlso sp.Current.Text <> ")"c
                sp.Pos += 1
            End While

            If Not dict.ContainsKey(name) Then
                dict(name) = If(type, TypeInfoHelper.TypeRef("Object"))
            End If
        End Sub

        Private Function ReadGenericParameters(sp As StmtParser) As TypeInfo()
            If sp.Eof OrElse Not sp.Current.Text.Equals("Of", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If

            ' the caller has already consumed the opening "(" of "(Of ...)",
            ' so here Current points at the first type parameter name.
            sp.Pos += 1

            Dim names As New List(Of String)

            While Not sp.Eof AndAlso sp.Current.Text <> ")"c
                If sp.Current.Text = ","c Then
                    sp.Pos += 1
                    Continue While
                End If

                If sp.Current.Text.Equals("In", StringComparison.OrdinalIgnoreCase) OrElse
                   sp.Current.Text.Equals("Out", StringComparison.OrdinalIgnoreCase) Then
                    sp.Pos += 1
                    Continue While
                End If

                Dim pname As String = sp.Current.Text
                sp.Pos += 1

                ' skip an optional constraint clause : As <bound>
                If Not sp.Eof AndAlso sp.Current.Text.Equals("As", StringComparison.OrdinalIgnoreCase) Then
                    While Not sp.Eof AndAlso sp.Current.Text <> ","c AndAlso sp.Current.Text <> ")"c
                        sp.Pos += 1
                    End While
                End If

                names.Add(pname)
            End While

            If Not sp.Eof AndAlso sp.Current.Text = ")"c Then
                sp.Pos += 1
            End If

            If names.Count = 0 Then
                Return New TypeInfo() {}
            End If

            Dim arr(names.Count - 1) As TypeInfo
            For k As Integer = 0 To names.Count - 1
                arr(k) = TypeInfoHelper.TypeRef(names(k))
            Next
            Return arr
        End Function

        Private Function CleanType(tokens As List(Of Token), start As Integer) As String
            Dim parts As New List(Of String)
            For k As Integer = start To tokens.Count - 1
                If tokens(k).Text = "="c Then
                    Exit For
                End If
                parts.Add(tokens(k).Text)
            Next

            If parts.Count = 0 Then
                Return "Object"
            End If

            Dim s As String = String.Join(" ", parts)
            s = s.Replace(" (", "(").Replace(" )", ")").Replace("( ", "(").Replace(" )", ")").Replace(" ,", ",")
            Return s.Trim()
        End Function

        Private Function SplitTopLevel(tokens As List(Of Token), sep As String) As List(Of List(Of Token))
            Dim result As New List(Of List(Of Token))
            Dim cur As New List(Of Token)
            Dim depth As Integer = 0

            For Each tk In tokens
                If tk.Text = "("c Then
                    depth += 1
                    cur.Add(tk)
                ElseIf tk.Text = ")"c Then
                    depth -= 1
                    cur.Add(tk)
                ElseIf tk.Text = sep AndAlso depth = 0 Then
                    result.Add(cur)
                    cur = New List(Of Token)
                Else
                    cur.Add(tk)
                End If
            Next

            result.Add(cur)
            Return result
        End Function

        Private Function MapContainerSymbol(kw As String) As SymbolType
            Select Case kw
                Case "class" : Return SymbolType.[Class]
                Case "module" : Return SymbolType.[Module]
                Case "structure", "struct" : Return SymbolType.[Structure]
                Case "enum" : Return SymbolType.[Enum]
                Case "interface" : Return SymbolType.[Interface]
                Case "namespace" : Return SymbolType.[Namespace]
            End Select
            Return SymbolType.[Class]
        End Function

        Private Function MapMemberSymbol(kw As String) As SymbolType
            Select Case kw
                Case "function" : Return SymbolType.[Function]
                Case "sub" : Return SymbolType.[Sub]
                Case "property" : Return SymbolType.[Property]
                Case "operator" : Return SymbolType.[Operator]
            End Select
            Return SymbolType.[Sub]
        End Function

        Private Function IsBlockStarter(kw As String) As Boolean
            ' only control-flow blocks need depth balancing; declarations
            ' (class/function/...) are fully consumed by their own recursive
            ' parser and therefore must not be skipped here.
            Select Case kw
                Case "if", "while", "do", "for", "select", "try", "with", "synclock", "using", "get", "set"
                    Return True
            End Select
            Return False
        End Function

        Private Function IsModifier(kw As String) As Boolean
            Select Case kw
                Case "public", "private", "friend", "protected", "shared", "overloads", "overrides",
                     "overridable", "mustoverride", "notoverridable", "readonly", "writeonly", "default",
                     "partial", "custom", "narrow", "wide", "ansi", "auto", "unicode",
                     "mustinherit", "notinheritable", "shadows"
                    Return True
            End Select
            Return False
        End Function

        Private Function IsParamModifier(kw As String) As Boolean
            Select Case kw.ToLowerInvariant()
                Case "byval", "byref", "optional", "paramarray"
                    Return True
            End Select
            Return False
        End Function

        ' ------------------------------------------------------------------
        ' statement cursor : skips leading attributes and modifiers
        ' ------------------------------------------------------------------

        Private Class StmtParser
            Public Tokens As List(Of Token)
            Public Pos As Integer
            Public Attributes As New List(Of String)
            Public Modifiers As String = ""

            Public Sub New(tk As List(Of Token), Optional p As Integer = 0)
                Tokens = tk
                Pos = p
            End Sub

            Public ReadOnly Property Eof As Boolean
                Get
                    Return Pos >= Tokens.Count
                End Get
            End Property

            Public ReadOnly Property Current As Token
                Get
                    If Eof Then
                        Return New Token With {.Kind = TokenKind.Punctuation, .Text = ""}
                    End If
                    Return Tokens(Pos)
                End Get
            End Property

            Public Sub CollectLeading()
                Do
                    If Not Eof AndAlso Current.Text = "<"c Then
                        Attributes.Add(ReadAttributeBlock())
                    ElseIf Not Eof AndAlso IsModifier(Current.Text.ToLowerInvariant()) Then
                        If Modifiers.Length > 0 Then
                            Modifiers &= " "
                        End If
                        Modifiers &= Current.Text
                        Pos += 1
                    Else
                        Exit Do
                    End If
                Loop
            End Sub

            Public Function ReadAttributeBlock() As String
                ' Current is "<"
                Pos += 1
                Dim sb As New StringBuilder()
                Dim depth As Integer = 0

                While Not Eof
                    Dim tk As Token = Current
                    If tk.Text = "("c Then
                        depth += 1
                        sb.Append(tk.Text)
                        Pos += 1
                    ElseIf tk.Text = ")"c Then
                        depth -= 1
                        sb.Append(tk.Text)
                        Pos += 1
                    ElseIf tk.Text = ">"c AndAlso depth = 0 Then
                        Pos += 1
                        Exit While
                    Else
                        sb.Append(tk.Text)
                        Pos += 1
                    End If
                End While

                Return sb.ToString().Trim()
            End Function
        End Class

    End Module

End Namespace
