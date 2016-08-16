Public Module DateParser

    Public ReadOnly Property MonthList As Dictionary(Of String, Integer)

    Sub New()
        MonthList = New Dictionary(Of String, Integer)

        MonthList.Add("January", 1)
        MonthList.Add("Jan", 1)

        MonthList.Add("February", 2)
        MonthList.Add("Feb", 2)

        MonthList.Add("March", 3)
        MonthList.Add("Mar", 3)

        MonthList.Add("April", 4)
        MonthList.Add("Apr", 4)

        MonthList.Add("May", 5)

        MonthList.Add("June", 6)
        MonthList.Add("Jun", 6)

        MonthList.Add("July", 7)
        MonthList.Add("Jul", 7)

        MonthList.Add("August", 8)
        MonthList.Add("Aug", 8)

        MonthList.Add("September", 9)
        MonthList.Add("Sep", 9)

        MonthList.Add("October", 10)
        MonthList.Add("Oct", 10)

        MonthList.Add("November", 11)
        MonthList.Add("Nov", 11)

        MonthList.Add("December", 12)
        MonthList.Add("Dec", 12)
    End Sub

    ''' <summary>
    ''' 从全称或者简称解析出月份的数字
    ''' </summary>
    ''' <param name="mon"></param>
    ''' <returns></returns>
    Public Function GetMonthInteger(mon As String) As Integer
        If Not MonthList.ContainsKey(mon) Then
            Return -1
        Else
            Return MonthList(mon)
        End If
    End Function
End Module
