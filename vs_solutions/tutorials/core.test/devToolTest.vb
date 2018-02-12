#Region "Microsoft.VisualBasic::8f47ed2a3a056cae65bbc3b18f55c97e, ..\sciBASIC#\vs_solutions\tutorials\core.test\devToolTest.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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

Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Public Module devToolTest

    Sub Main()
        Call parserTest()
        Call loadTest()
    End Sub

    Sub loadTest()
        Dim assembly = New ProjectSpace(True)

        assembly.ImportFromXmlDocFile("E:\GCModeller\GCModeller\bin\Microsoft.VisualBasic.Framework_v47_dotnet_8da45dcd8060cc9a.xml")

        Pause()
    End Sub

    Sub parserTest()
        Dim code = "E:\repo\xDoc\Yilia\runtime\sciBASIC#\Microsoft.VisualBasic.Core\CommandLine\CommandLine.vb".ReadAllText
        Dim list = VBCodeSignature.SummaryModules(code)

        Console.WriteLine(list)

        Pause()
    End Sub

    Public MustInherit Class innerTest

        Property AA As String
        ReadOnly Property AA(o$) As Integer
            Get

            End Get
        End Property

        Function X() As Double
        End Function

        Function Z()

        End Function
        Function Z(o#)

        End Function

        Sub ACC()

        End Sub

        Sub X1()

        End Sub
        Sub X1(a$)

        End Sub
        Sub X1(o&)

        End Sub

        Public Shared Operator +(o As innerTest) As innerTest

        End Operator
        Public Shared Operator -(o As innerTest) As innerTest

        End Operator
        Public Shared Operator <<(o As innerTest, i%) As Double

        End Operator

        Public Shared Operator <=(o As innerTest, i%) As Double

        End Operator
        Public Shared Operator >=(o As innerTest, i%) As Double

        End Operator

        Public Shared Operator IsTrue(o As innerTest) As Boolean

        End Operator
        Public Shared Operator IsFalse(o As innerTest) As Boolean

        End Operator
    End Class

    Enum innerEnum
        AAA
        BBB
        CCC
        DDD = 5
    End Enum
End Module

