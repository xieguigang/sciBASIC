Friend MustInherit Class DistributionTable

    Private table_fivepercent As New List(Of Double())()
    Private table_onepercent As New List(Of Double())()
    Private numerators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 24, 30, 40, 60, 120, 121}
    Private denominators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 40, 60, 120, 121}

    Public Sub New()
        setup_fivepercent_table()
        setup_onepercent_table()
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
    Public Overridable Function getCriticalNumber(ByVal numerator As Integer, ByVal denominator As Integer, ByVal type As String) As Double

        Dim n = getRowIndex(numerator)
        Dim d = getColIndex(denominator)

        Dim criticalNumber As Double = -1

        ' NOTE: The table is 1 based but array are 0 based so -1 from each of the n and d
        If type.Equals(AnovaTest.P_FIVE_PERCENT) Then
            Dim row = table_fivepercent(n - 1)
            criticalNumber = row(d - 1)
        ElseIf type.Equals(AnovaTest.P_ONE_PERCENT) Then
            Dim row = table_onepercent(n - 1)
            criticalNumber = row(d - 1)
        End If

        Return criticalNumber
    End Function

    Public Overridable Function about() As String
        Dim msg = "These F distribution tables are adapted from: http://www.socr.ucla.edu/applets.dir/f_table.html"
        Return msg
    End Function

    ' 
    '  Row = numerator lookup
    ' 
    Protected Friend Overridable Function getRowIndex(ByVal actualNumerator As Integer) As Integer
        Dim chosen = -1
        For i = 0 To numerators.Length - 1
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
    ' 
    '  Col = denominator lookup
    ' 
    Protected Friend Overridable Function getColIndex(ByVal actualNumerator As Integer) As Integer
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