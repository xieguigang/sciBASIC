#Region "Microsoft.VisualBasic::a8b092c698432c66e763fa1a8d557a09, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\DateParser.vb"

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
    ''' <param name="mon">大小写不敏感</param>
    ''' <returns></returns>
    Public Function GetMonthInteger(mon As String) As Integer
        If Not MonthList.ContainsKey(mon) Then
            For Each k As String In MonthList.Keys
                If String.Equals(mon, k, StringComparison.OrdinalIgnoreCase) Then
                    Return MonthList(k)
                End If
            Next

            Return -1
        Else
            Return MonthList(mon)
        End If
    End Function

    ''' <summary>
    ''' 00
    ''' </summary>
    ''' <param name="d"></param>
    ''' <returns></returns>
    Public Function FillDateZero(d As Integer) As String
        Return If(d >= 10, d, "0" & d)
    End Function
End Module
