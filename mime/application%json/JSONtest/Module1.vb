#Region "Microsoft.VisualBasic::3df71a28b8cdee7d29d829209ce92fbb, sciBASIC#\mime\application%json\JSONtest\Module1.vb"

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

    '   Total Lines: 37
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.37 KB


    ' Class TestDynamicsObject
    ' 
    '     Properties: str, Tarray, Tarray2
    ' 
    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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

        Call t.GetJson(True).SaveTo("./test_out.json")
        Call json.SaveTo("./test_out2.json")
        Call t2.Tarray.GetJson(maskReadonly:=True).__DEBUG_ECHO
        Call t2.Tarray2.GetJson(maskReadonly:=True).__DEBUG_ECHO

        Pause()
    End Sub

End Module
