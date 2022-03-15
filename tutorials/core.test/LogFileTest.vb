#Region "Microsoft.VisualBasic::b92d41ffb8cc3ca385037b7bcaa47ca6, sciBASIC#\tutorials\core.test\LogFileTest.vb"

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

    '   Total Lines: 102
    '    Code Lines: 40
    ' Comment Lines: 29
    '   Blank Lines: 33
    '     File Size: 1.83 KB


    ' Module LogFileTest
    ' 
    '     Sub: Main, PrintTest, WriteTes, xmlModelTraceTest
    ' 
    ' Class XmlDataModelTest
    ' 
    '     Properties: ddd, dddddd
    ' 
    ' Class inherittttt
    ' 
    '     Properties: ddddd_sfsdfsd, mybaseClass
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::8fc81cb896b9d27851d6b33e234c6ff9, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module LogFileTest
' 
'     Sub: Main, PrintTest, WriteTes
' 
' 

#End Region

#Region "Microsoft.VisualBasic::32bf033d93b65934e75dc90194c7a96a, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module LogFileTest
' 
'     Sub: Main, PrintTest, WriteTes
' 
' 
' 

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON

Module LogFileTest

    Sub Main()

        Call xmlModelTraceTest()

        WriteTes()
        PrintTest()
    End Sub

    Sub xmlModelTraceTest()
        Dim model As New inherittttt

        Dim xml = model.GetXml

        Call xml.__DEBUG_ECHO

        Dim obj = xml.LoadFromXml(Of inherittttt)

        Call obj.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Const path$ = "./test.log"

    Sub WriteTes()
        Using log As New LogFile(path)
            Call log.WriteLine("123", "test")
        End Using
    End Sub

    Sub PrintTest()

    End Sub
End Module

Public Class XmlDataModelTest : Inherits XmlDataModel

    Public Property ddd As String = Rnd()

    <XmlAttribute>
    Public Property dddddd As Date = Now

End Class

Public Class inherittttt : Inherits XmlDataModelTest

    Public Property ddddd_sfsdfsd As String() = {"dfgdfgdfg", "werwer43444", "898789////"}

    Public Property mybaseClass As New XmlDataModelTest
End Class
