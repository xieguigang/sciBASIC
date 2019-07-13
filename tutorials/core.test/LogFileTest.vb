#Region "Microsoft.VisualBasic::b92d41ffb8cc3ca385037b7bcaa47ca6, tutorials\core.test\LogFileTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
