Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.Serialization

<StructLayout(LayoutKind.Sequential)>
Public Structure SYSTEMTIME
    Public Year As Short
    Public Month As Short
    Public DayOfWeek As Short
    Public Day As Short
    Public Hour As Short
    Public Minute As Short
    Public Second As Short
    Public Miliseconds As Short

    Sub New(d As Date)
        Year = d.Year()
        Month = d.Month()
        Day = d.Day
        Hour = d.Hour
        ' 这个函数使用的是0时区的时间,对于我们用+8时区的,时间要自己算一下.如要设12点，则为12-8   
        Minute = d.Minute
        Second = d.Second
        Miliseconds = d.Millisecond
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure

Module SetSystemTime

    <DllImport("kernel32.dll", CharSet:=CharSet.Ansi)>
    Public Function SetSystemTime(ByRef time As SYSTEMTIME) As Boolean
    End Function

    Const TimeServices As String = "http://open.baidu.com/special/time/"

    Private Function __syncTime() As Date
        Dim web As String = TimeServices.GET
        Dim WebTimes As String = Regex.Match(web, "[0-9]{13}", RegexOptions.Singleline).Value '获取源码中时间信息  
        Dim WebTime As Date = #1/1/1970 8:00:00 AM# '定义初始时间  

        WebTime = DateAdd(DateInterval.Second, WebTimes / 1000, WebTime) '换算成现在的时间  

        Return WebTime
    End Function

    Public Function [Set]() As Boolean
        Dim serverTime As Date = __syncTime().AddHours(-20)
        Dim t As New SYSTEMTIME(serverTime)
        Return SetSystemTime(t)
    End Function
End Module
