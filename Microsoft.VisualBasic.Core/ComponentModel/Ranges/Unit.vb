Namespace ComponentModel.Ranges

    Public Class UnitValue(Of TUnit As {Structure, IComparable(Of TUnit)})

        ''' <summary>
        ''' 分（d） ``10^-1``
        ''' </summary>
        Public Const d = 10 ^ -1
        ''' <summary>
        ''' 厘（c） ``10^-2``
        ''' </summary>
        Public Const c = 10 ^ -2
        ''' <summary>
        ''' 毫（m） ``10^-3``
        ''' </summary>
        Public Const m = 10 ^ -3
        ''' <summary>
        ''' 微（μ） ``10^-6``
        ''' </summary>
        Public Const u = 10 ^ -6
        ''' <summary>
        ''' 纳（n） ``10^-9``
        ''' </summary>
        Public Const n = 10 ^ -9
        ''' <summary>
        ''' 皮（p） ``10^-12``
        ''' </summary>
        Public Const p = 10 ^ -12
        ''' <summary>
        ''' 飞（f） ``10^-15``
        ''' </summary>
        Public Const f = 10 ^ -15
        ''' <summary>
        ''' 阿（a） ``10^-18``
        ''' </summary>
        Public Const a = 10 ^ -18

        Public Property Unit As TUnit
        Public Property Value As Double

        Sub New(value#, unit As TUnit)
            Me.Value = value
            Me.Unit = unit
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Value} ({DirectCast(CObj(Unit), [Enum]).Description})"
        End Function
    End Class
End Namespace