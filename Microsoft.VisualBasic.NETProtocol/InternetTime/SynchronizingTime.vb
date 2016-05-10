Imports System.IO
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.Serialization

Namespace InternetTime

    ''' <summary>
    ''' Synchronizing Time using baidu server
    ''' </summary>
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
End Namespace