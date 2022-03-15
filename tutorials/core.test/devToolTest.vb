#Region "Microsoft.VisualBasic::f4ee5554c4ef765c4af6e9ba97f47faa, sciBASIC#\tutorials\core.test\devToolTest.vb"

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

    '   Total Lines: 178
    '    Code Lines: 68
    ' Comment Lines: 65
    '   Blank Lines: 45
    '     File Size: 3.67 KB


    ' Module devToolTest
    ' 
    '     Sub: loadTest, Main, parserTest
    '     Class innerTest
    ' 
    '         Properties: (+2 Overloads) AA
    ' 
    '         Function: X, (+2 Overloads) Z
    ' 
    '         Sub: ACC, (+3 Overloads) X1
    ' 
    '         Operators: -, +, <<, <=, >=
    '                    (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
    ' 
    '     Enum innerEnum
    ' 
    '         AAA, BBB, CCC, Get
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::d6e7d85c4444ef86dd581810dc5570d5, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module devToolTest
' 
'     Sub: loadTest, Main, parserTest
' 
'     Class innerTest
' 
'         Properties: (+2 Overloads) AA
' 
'         Function: X, (+2 Overloads) Z
' 
'         Sub: ACC, (+3 Overloads) X1
' 
'         Operators: -, +, <<, <=, >=
'                    (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
' 
' 
'     Enum innerEnum
' 
'         AAA, BBB, CCC, Get
' 
' 

#End Region

#Region "Microsoft.VisualBasic::f5c85a2ca7e9da2e3b03f531838242c5, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module devToolTest
' 
'     Sub: loadTest, Main, parserTest
' 
' 
'     Class innerTest
' 
'         Properties: (+2 Overloads) AA
' 
' 
'         Function: X, (+2 Overloads) Z
' 
' 
'         Sub: ACC, (+3 Overloads) X1
' 
' 
'         Operators: -, +, <<, <=, >=
'                    (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
' 
' 
' 
'     Enum innerEnum
' 
'         AAA, BBB, CCC
' 
' 

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
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
        Dim code = "E:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\ApplicationServices\App.vb".ReadAllText
        Dim list = VBCodeSignature.SummaryModules(code)

        Console.WriteLine(list)

        Pause()
    End Sub

    <Description("AAAAAAAAAA")> Public MustInherit Class innerTest

        Property AA As String
        ReadOnly Property AA(o$) As Integer
            Get

            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)> Function X() As Double
        End Function

        Function Z()

        End Function
        Function Z(o#)

        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)> Sub ACC()

        End Sub

        Sub X1()

        End Sub
        Sub X1(a$)

        End Sub
        Sub X1(o&)

        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsTrue(o As innerTest) As Boolean

        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)> Public Shared Operator IsFalse(o As innerTest) As Boolean

        End Operator
    End Class

    Enum innerEnum
        AAA
        BBB
        CCC
        DDD = 5
    End Enum
End Module
