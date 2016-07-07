#Region "Microsoft.VisualBasic::a9ebb3b448dd70adab705aa563303ae8, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.NETProtocol\InternetTime\WinAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
