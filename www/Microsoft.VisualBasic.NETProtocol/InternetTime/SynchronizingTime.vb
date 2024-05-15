#Region "Microsoft.VisualBasic::f134b13d48a2ab1affa6e492f861e229, www\Microsoft.VisualBasic.NETProtocol\InternetTime\SynchronizingTime.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 38
    '    Code Lines: 27
    ' Comment Lines: 3
    '   Blank Lines: 8
    '     File Size: 1.31 KB


    '     Module SetSystemTime
    ' 
    '         Function: __syncTime, [Set], SetSystemTime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
