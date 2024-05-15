#Region "Microsoft.VisualBasic::3a60e78d2c27791699d7a9d64cb41181, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\struct\TripleDict.vb"

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
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.21 KB


    '     Class TripleDict
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: tripleDict
    ' 
    '         Sub: load
    '         Class CSharpImpl
    ' 
    '             Function: __Assign
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text

Namespace GraphEmbedding.struct

    Public Class TripleDict

        Public pTripleDict As Dictionary(Of String, Boolean) = Nothing

        Public Sub New()
        End Sub

        Public Overridable Function tripleDict() As Dictionary(Of String, Boolean)
            Return pTripleDict
        End Function

        Public Overridable Sub load(fnInput As String)
            pTripleDict = New Dictionary(Of String, Boolean)()
            Dim reader As StreamReader = New StreamReader(New FileStream(fnInput, FileMode.Open, FileAccess.Read), Encoding.UTF8)

            Dim line = ""
            While Not String.ReferenceEquals((CSharpImpl.__Assign(line, reader.ReadLine())), Nothing)
                pTripleDict(line.Trim()) = True
            End While
            reader.Close()
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

End Namespace
