#Region "Microsoft.VisualBasic::cc8f4283d0f6f33e4f303ad2a7e56c42, mime\application%json\JSONtest\Module1.vb"

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

    '   Total Lines: 64
    '    Code Lines: 49 (76.56%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (23.44%)
    '     File Size: 2.37 KB


    ' Class TestDynamicsObject
    ' 
    '     Properties: str, Tarray, Tarray2
    ' 
    ' Module Module1
    ' 
    '     Sub: deserializeObjectTest, Main, test1
    ' 
    ' Class anyObject
    ' 
    '     Properties: data, name
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class TestDynamicsObject : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property Tarray As Double()
    Public Property str As String
    Public Property Tarray2 As String()
End Class

Module Module1

    Sub Main()
        Call deserializeObjectTest()
    End Sub

    Sub test1()
        Dim aaa = ParseJson("[{a:[1,2,3,4,5,6,7,[{xxoo:[""233333""]}]], b: ""xxxxxooooo""}]")

        Dim t As New TestDynamicsObject With {
            .Tarray = {1, 2, 3, 4, 5, 6, 7, 8},
            .str = "12345" & vbCrLf & "67890",
            .Tarray2 = {
                "xxoo", "1234", "6789", "50"
            }
        }
        t.Add("1234", New NamedValue(Of Integer())("x1", {100, 200, 3}))
        t.Add("2333", New NamedValue(Of Integer())("x2", {-10, 203, 3}))

        Dim json$ = GetExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(t)
        Dim t2 = LoadExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(json)

        Call t.GetJson(maskReadonly:=True).SaveTo("./test_out.json")
        Call json.SaveTo("./test_out2.json")
        Call t2.Tarray.GetJson(maskReadonly:=True).__DEBUG_ECHO
        Call t2.Tarray2.GetJson(maskReadonly:=True).__DEBUG_ECHO

        Pause()
    End Sub

    Sub deserializeObjectTest()
        Dim test1 As String = "{name: 'string_value', data: 'string_value'}"
        Dim test2 As String = "{name: 'string_array', data: ['value1', 'value2', 'value3']}"
        Dim test3 As String = "{name: 'any', data: {name:'nest', data:false}}"
        Dim test4 As String = "{name: 'any', data: {name:'nest', data:{name:'nest_true', data:true}}}"

        Dim o1 = JsonParser.Parse(test1, False).CreateObject(Of anyObject)
        Dim o2 = JsonParser.Parse(test2, False).CreateObject(Of anyObject)
        Dim o3 = JsonParser.Parse(test3, False).CreateObject(Of anyObject)
        Dim o4 = JsonParser.Parse(test4, False).CreateObject(Of anyObject)

        Pause()
    End Sub

End Module

<KnownType(GetType(anyObject))>
Public Class anyObject

    Public Property name As String
    Public Property data As Object

End Class
