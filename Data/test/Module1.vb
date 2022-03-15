#Region "Microsoft.VisualBasic::f5835f1d37bcd0d77deae4ca8a5c996a, sciBASIC#\Data\test\Module1.vb"

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

    '   Total Lines: 78
    '    Code Lines: 40
    ' Comment Lines: 15
    '   Blank Lines: 23
    '     File Size: 3.15 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.SearchEngine
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    'Function Pair(Of T1, T2)(first As T1, second As T2) As (first As T1, second As T2)
    '    Return (first, second)
    'End Function

    'Public ReadOnly Property Item(index As Integer) As ByRef Integer
    '        Get

    '    End Get
    'End Property

    Sub Main()

        Dim query = "#\d+ AND (X AND obj)".Build(anyDefault:=Tokens.op_OR, allowInStr:=True)

        Dim result As Boolean = query.Match(text:="23333")

        Dim obj As New NamedValue(Of String) With {
            .Name = "Hello world! xieguigang",
            .Value = "23333",
            .Description = "Test Object"
        }
        Dim def As New IObject(GetType(NamedValue(Of String)))

        Dim match As Match = query.Evaluate(def, obj)

        Call match.GetJson.__DEBUG_ECHO.SaveTo("x:\ffff.json")

        Pause()

        '     Dim source As IEnumerable(Of NamedValue(Of String))

        Call "Aedes aegypti strain Liverpool supercont1.301 genomic scaffold, whole genome shotgun sequence".Match("(Aedes OR Aed) AND Aegypti").__DEBUG_ECHO

        Call "Aedes (Diceronyia) furcifer".Match("((Aedes OR Aed) AND furcifer) OR Diceronyia").__DEBUG_ECHO
        Call "Aedes Luteocephalus".Match("(Aedes OR Aed) AND luteocephalus").__DEBUG_ECHO

        Call "Aedes Luteo|cephalus".Match("(Aedes OR Aed) AND luteocephalus").__DEBUG_ECHO
        Call "Aedes Luteo|cephalus".Match("(Aedes OR Aed) AND ~luteocephalus").__DEBUG_ECHO
        Call "Aedes Luteocephalus".Match("(Aedes OR Aed) AND ""luteocephalus""").__DEBUG_ECHO
        Call "A+edes Luteocephalus".Match("(Aedes OR Aed) AND ""luteocephalus""").__DEBUG_ECHO
        Call "A+edes Luteocephalus".Match("(~Aedes OR Aed) AND ""luteocephalus""").__DEBUG_ECHO

        'Dim test As IObject = IObject.FromString("1234")

        'Console.WriteLine("12* AND (NOT ""4"" OR #\d+)".Evaluate(test)) ' T
        'Console.WriteLine("#\d+".Evaluate(test))  ' T
        'Console.WriteLine("""#\d+""".Evaluate(test)) 'F
        'Console.WriteLine("Text:'#\d+'".Evaluate(test))
        'Console.WriteLine("12* AND (NOT ""4"" OR #\d+)".Evaluate(test))
        'Console.WriteLine("12* AND (NOT ""4"" OR #\d+)".Evaluate(test))

        Pause()

        If True And False Or (True And False) Then
            MsgBox(1)
        End If

        Dim exp = ExpressionBuilder.Build("D:\GCModeller\src\runtime\visualbasic_App\Data\query_syntaxTest.txt".ReadAllText)


        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.?ab".WildcardMatch("a.ab"))
        MsgBox("*.*ab".WildcardMatch("a.ab"))
        MsgBox("ab*cc".WildcardMatch("abddddddcc"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))
        MsgBox("*.*".WildcardMatch("a.b"))

        Dim tk = SyntaxParser.Parser("D:\GCModeller\src\runtime\visualbasic_App\Data\query_syntaxTest.txt".ReadAllText)
    End Sub
End Module
