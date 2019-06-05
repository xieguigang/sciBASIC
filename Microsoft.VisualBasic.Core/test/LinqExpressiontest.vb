Module LinqExpressiontest

    Sub Main()

        Dim any = <exec <%= From x In 100.SeqRandom Select x + 100 %>/>

        Pause()
    End Sub
End Module
