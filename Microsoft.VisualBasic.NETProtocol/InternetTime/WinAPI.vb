Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Serialization

Namespace InternetTime

    Public Module WinAPI

        ''' <summary>
        ''' SYSTEMTIME structure used by SetSystemTime
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)> Public Structure SYSTEMTIME
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

        <DllImport("KERNEL32.DLL", EntryPoint:="SetLocalTime", SetLastError:=True, CharSet:=CharSet.Unicode, ExactSpelling:=False, CallingConvention:=CallingConvention.StdCall)>
        Public Function SetLocalTime(ByRef time As SYSTEMTIME) As Int32
        End Function
    End Module
End Namespace