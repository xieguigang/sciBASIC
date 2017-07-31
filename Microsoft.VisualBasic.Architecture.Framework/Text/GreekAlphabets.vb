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