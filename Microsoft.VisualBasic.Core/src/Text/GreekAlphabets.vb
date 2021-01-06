#Region "Microsoft.VisualBasic::12a7c6a4b098bc24da9e66f807a11eec, Microsoft.VisualBasic.Core\src\Text\GreekAlphabets.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module GreekAlphabets
    ' 
    '         Properties: alphabets, lower, upper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) AlphabetUnescape, (+2 Overloads) StripGreek, unescapeInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Text

    ''' <summary>
    ''' Processing for the chemical compound name/genome species scientific name.
    ''' </summary>
    Public Module GreekAlphabets

        Const Α$ = "alpha"
        Const Β$ = "beta"
        Const Γ$ = "gamma"
        Const Δ$ = "delta"
        Const Ε$ = "epsilon"
        Const Ζ$ = "zeta"
        Const Η$ = "eta"
        Const Θ$ = "theta"
        Const Ι$ = "iota"
        Const Κ$ = "kappa"
        Const Λ$ = "lambda"
        Const Μ$ = "mu"
        Const Ν$ = "nu"
        Const Ξ$ = "xi"
        Const Ο$ = "omicron"
        Const Π$ = "pi"
        Const Ρ$ = "rho"
        Const Σ$ = "sigma"
        Const Τ$ = "tau"
        Const Υ$ = "upsilon"
        Const Φ$ = "phi"
        Const Χ$ = "chi"
        Const Ψ$ = "psi"
        Const Ω$ = "omega"

        Public ReadOnly Property alphabets As Dictionary(Of String, String)
        Public ReadOnly Property upper As Dictionary(Of String, String)
        Public ReadOnly Property lower As Dictionary(Of String, String)

        Sub New()
            alphabets = New Dictionary(Of String, String) From {
               {"α", GreekAlphabets.Α}, {"Α", GreekAlphabets.Α},
               {"β", GreekAlphabets.Β}, {"Β", GreekAlphabets.Β},
               {"γ", GreekAlphabets.Γ}, {"Γ", GreekAlphabets.Γ},
               {"δ", GreekAlphabets.Δ}, {"Δ", GreekAlphabets.Δ},
               {"ε", GreekAlphabets.Ε}, {"Ε", GreekAlphabets.Ε},
               {"ζ", GreekAlphabets.Ζ}, {"Ζ", GreekAlphabets.Ζ},
               {"η", GreekAlphabets.Η}, {"Η", GreekAlphabets.Η},
               {"θ", GreekAlphabets.Θ}, {"Θ", GreekAlphabets.Θ},
               {"ι", GreekAlphabets.Ι}, {"Ι", GreekAlphabets.Ι},
               {"κ", GreekAlphabets.Κ}, {"Κ", GreekAlphabets.Κ},
               {"λ", GreekAlphabets.Λ}, {"Λ", GreekAlphabets.Λ},
               {"μ", GreekAlphabets.Μ}, {"Μ", GreekAlphabets.Μ},
               {"ν", GreekAlphabets.Ν}, {"Ν", GreekAlphabets.Ν},
               {"ξ", GreekAlphabets.Ξ}, {"Ξ", GreekAlphabets.Ξ},
               {"ο", GreekAlphabets.Ο}, {"Ο", GreekAlphabets.Ο},
               {"π", GreekAlphabets.Π}, {"Π", GreekAlphabets.Π},
               {"ρ", GreekAlphabets.Ρ}, {"Ρ", GreekAlphabets.Ρ},
               {"σ", GreekAlphabets.Σ}, {"Σ", GreekAlphabets.Σ},
               {"τ", GreekAlphabets.Τ}, {"Τ", GreekAlphabets.Τ},
               {"υ", GreekAlphabets.Υ}, {"Υ", GreekAlphabets.Υ},
               {"φ", GreekAlphabets.Φ}, {"Φ", GreekAlphabets.Φ},
               {"χ", GreekAlphabets.Χ}, {"Χ", GreekAlphabets.Χ},
               {"ψ", GreekAlphabets.Ψ}, {"Ψ", GreekAlphabets.Ψ},
               {"ω", GreekAlphabets.Ω}, {"Ω", GreekAlphabets.Ω}
            }

            _upper = alphabets.Subset({
                "Α", "Β", "Γ", "Δ", "Ε", "Ζ",
                "Η", "Θ", "Ι", "Κ", "Λ", "Μ",
                "Ν", "Ξ", "Ο", "Π", "Ρ", "Σ",
                "Τ", "Υ", "Φ", "Χ", "Ψ", "Ω"
            })
            _lower = alphabets.Subset(alphabets.Keys.AsSet - upper.Keys)
        End Sub

        ''' <summary>
        ''' 将字符串文本之中的希腊字母替换为英文单词
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension> Public Function StripGreek(ByRef s As StringBuilder) As StringBuilder
            For Each key In alphabets.Keys
                Call s.Replace(key, alphabets(key))
            Next
            Return s
        End Function

        ''' <summary>
        ''' 将字符串文本之中的希腊字母替换为英文单词
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <returns></returns>
        <Extension> Public Function StripGreek(s$) As String
            Return New StringBuilder(s).StripGreek.ToString
        End Function

        Const contactSymbols = "[\-\(\)&;\s ,\.:\|\[\]\+\*]"
        Const escapePattern$ = contactSymbols & "[a-z]{2,10}" & contactSymbols

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <param name="removesContacts"></param>
        ''' <param name="upperCase"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AlphabetUnescape(s$, Optional removesContacts As Boolean = False, Optional upperCase As Boolean = False) As String
            Return s.unescapeInternal(escapePattern, upperCase, removesContacts)
        End Function

        <Extension>
        Private Function unescapeInternal(s$, escapePattern$, upperCase As Boolean, removesContacts As Boolean) As String
            ' 如果直接匹配替换的话，可能会将单词之中的一部分给错误的替换掉，
            ' 所以在这里假设希腊字母是在连接符周围的
            Dim matches = s.Matches(escapePattern, RegexICSng)
            Dim sb As New StringBuilder(s)
            Dim alphabets = (upper Or lower.When(Not upperCase)).ReverseMaps

            For Each match As String In matches
                Dim term$ = Mid(match, 2, Length:=match.Length - 2)
                Dim greek$ = alphabets.TryGetValue(term.ToLower)

                If Not greek Is Nothing Then
                    If Not removesContacts Then
                        match = term
                    End If

                    Call sb.Replace(match, greek)
                End If
            Next

            Return sb.ToString
        End Function

#If NET_48 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AlphabetUnescape(s$, contacts As (left As Char, right As Char), Optional upperCase As Boolean = False) As String
            Return s.unescapeInternal($"[{contacts.left}][a-z]{{2,10}}[{contacts.right}]", upperCase, True)
        End Function

#End If
    End Module
End Namespace
