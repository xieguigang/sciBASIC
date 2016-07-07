#Region "Microsoft.VisualBasic::0cb0b5f3ac71dfe32b22c9539085ea71, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\TimeSpan.vb"

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

Imports System.Xml.Serialization


Namespace ComponentModel

    <XmlTypeAttribute("Interval", Namespace:="Microsoft.VisualBasic/Numerical_DataStruct")>
    Public Structure TimeInterval

        <XmlAttribute("dd")> Public Property Days As Integer
        <XmlAttribute("min")> Public Property Minutes As Integer
        <XmlAttribute("hr")> Public Property Hours As Integer
        <XmlAttribute("ss")> Public Property Seconds As Integer
        <XmlAttribute("ms")> Public Property Miliseconds As Integer

        Sub New(TimeSpan As TimeSpan)
            Days = TimeSpan.Days
            Minutes = TimeSpan.Minutes
            Hours = TimeSpan.Hours
            Seconds = TimeSpan.Seconds
            Miliseconds = TimeSpan.Milliseconds
        End Sub

        Public ReadOnly Property TimeSpan As TimeSpan
            Get
                Return New TimeSpan(Days, Hours, Minutes, Seconds, Miliseconds)
            End Get
        End Property

        ''' <summary>
        ''' (dd hh:mm:ss) 输出可以被MySQL数据库所识别的字符串格式
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Format("{0} {1}:{2}:{3}", Days, Hours, Minutes, Seconds)
        End Function

        Public ReadOnly Property Ticks As Long
            Get
                Return TimeSpan.Ticks
            End Get
        End Property

        Public Shared Widening Operator CType(TimeSpan As TimeSpan) As TimeInterval
            Return New TimeInterval(TimeSpan)
        End Operator
    End Structure
End Namespace
