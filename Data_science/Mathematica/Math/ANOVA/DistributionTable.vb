Friend MustInherit Class DistributionTable

    Shared ReadOnly numerators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 24, 30, 40, 60, 120, 121}
    Shared ReadOnly denominators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 40, 60, 120, 121}

    ReadOnly dist As Double()()
    ReadOnly type As String

    Protected Sub New(type As String)
        Me.type = type
        Me.dist = loadMatrix.ToArray
    End Sub

    Protected MustOverride Function loadMatrix() As IEnumerable(Of Double())

    ' 
    '  The numerator = the number of groups that are being compared ( i.e., group degrees of freedom )
    '  The denomiator = the number of all the observations of all the groups ( i.e., group degrees of freedom )
    '  type = 0.05% or 0.01% test 
    '  
    '  Returns a double... ...if the F score is higher than the returned critial number
    '  then _reject the null hypothesis_
    ' 
    Public Shared Function getCriticalNumber(numerator As Integer, denominator As Integer, type As String) As Double
        Dim n As Integer = getRowIndex(numerator)
        Dim d As Integer = getColIndex(denominator)
        Dim criticalNumber As Double
        Dim row As Double()
        Dim matrix As Double()()

        Static five As New TableFivepercent
        Static one As New TableOnepercent

        ' NOTE: The table is 1 based but array are 0 based so -1 from each of the n and d
        Select Case type
            Case AnovaTest.P_FIVE_PERCENT : matrix = five.dist
            Case AnovaTest.P_ONE_PERCENT : matrix = one.dist
            Case Else
                Throw New NotImplementedException(type)
        End Select

        row = If(n - 1 >= matrix.Length, matrix.Last, matrix(n - 1))
        criticalNumber = row(d - 1)

        Return criticalNumber
    End Function

    Public Overrides Function ToString() As String
        Return $"These F({type}) distribution tables are adapted from: http://www.socr.ucla.edu/applets.dir/f_table.html"
    End Function

    ''' <summary>
    ''' Row = numerator lookup
    ''' </summary>
    ''' <param name="actualNumerator"></param>
    ''' <returns></returns>
    Protected Friend Shared Function getRowIndex(actualNumerator As Integer) As Integer
        Dim chosen = -1

        For i As Integer = 0 To numerators.Length - 1
            If actualNumerator = numerators(i) Then
                chosen = numerators(i)
            ElseIf actualNumerator > numerators(i) Then
                Try
                    chosen = numerators(i + 1)
                Catch __unusedIndexOutOfRangeException1__ As IndexOutOfRangeException
                    ' set to infinity. I.e., the last possible option.
                    chosen = numerators(numerators.Length - 1)
                End Try
            End If
        Next

        Return chosen
    End Function

    ''' <summary>
    ''' Col = denominator lookup
    ''' </summary>
    ''' <param name="actualNumerator"></param>
    ''' <returns></returns>
    Protected Friend Shared Function getColIndex(actualNumerator As Integer) As Integer
        Dim choosen = -1

        For i As Integer = 0 To denominators.Length - 1
            If actualNumerator = denominators(i) Then
                choosen = denominators(i)
            ElseIf actualNumerator > denominators(i) Then
                Try
                    choosen = denominators(i + 1)
                Catch __unusedIndexOutOfRangeException1__ As IndexOutOfRangeException
                    ' set to infinity. I.e., the last possible option.
                    choosen = denominators(denominators.Length - 1)
                End Try
            End If
        Next

        Return choosen
    End Function
End Class