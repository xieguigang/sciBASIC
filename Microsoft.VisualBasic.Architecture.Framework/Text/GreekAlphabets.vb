#Region "Microsoft.VisualBasic::7ce2456a32bc0deaf78d7ef143d540f8, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\GreekAlphabets.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

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

        Public ReadOnly Property Alphabets As New Dictionary(Of String, String) From {
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

        ''' <summary>
        ''' 将字符串文本之中的希腊字母替换为英文单词
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension> Public Function StripGreek(ByRef s As StringBuilder) As StringBuilder
            For Each key In Alphabets.Keys
                Call s.Replace(key, Alphabets(key))
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
    End Module
End Namespace
