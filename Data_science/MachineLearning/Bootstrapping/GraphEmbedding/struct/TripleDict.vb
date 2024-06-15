#Region "Microsoft.VisualBasic::f03550035988af09fe31d8bc8a945c4b, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\TripleDict.vb"

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

    '   Total Lines: 30
    '    Code Lines: 21 (70.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (30.00%)
    '     File Size: 847 B


    '     Class TripleDict
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: tripleDict
    ' 
    '         Sub: load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace GraphEmbedding.struct

    Public Class TripleDict

        Public pTripleDict As New Dictionary(Of String, Boolean)

        Public Sub New()
        End Sub

        Public Overridable Function tripleDict() As Dictionary(Of String, Boolean)
            Return pTripleDict
        End Function

        Public Overridable Sub load(fnInput As String)
            Dim reader As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)
            Dim line As value(Of String) = ""

            While Not (line = reader.ReadLine()) Is Nothing
                pTripleDict(line.Trim()) = True
            End While

            reader.Close()
        End Sub
    End Class

End Namespace
