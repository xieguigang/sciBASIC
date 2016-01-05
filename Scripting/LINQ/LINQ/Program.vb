Module Program

    ''' <summary>
    ''' DO_NOTHING
    ''' </summary>
    ''' <remarks></remarks>
    Sub Main()
        Call Console.WriteLine("{0}!{1}", GetType(Program).Assembly.Location, GetType(LINQ.Framework.LQueryFramework).FullName)
    End Sub
End Module
