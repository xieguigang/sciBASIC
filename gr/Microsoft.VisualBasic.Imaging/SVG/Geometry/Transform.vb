#Region "Microsoft.VisualBasic::e484833245be5d9b2428e81b52b15d30, gr\Microsoft.VisualBasic.Imaging\SVG\Geometry\Transform.vb"

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

    '   Total Lines: 49
    '    Code Lines: 39 (79.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.64 KB


    '     Class Transform
    ' 
    '         Properties: translate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Parse, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVG

    Public Class Transform

        ReadOnly transform As New Dictionary(Of String, String())

        Public ReadOnly Property translate As PointF
            Get
                Dim translate_pars As String() = transform.TryGetValue("translate")

                If translate_pars.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return New PointF(Val(translate_pars(0)), Val(translate_pars(1)))
                End If
            End Get
        End Property

        Friend Sub New(str As String)
            transform = Parse(str) _
                .ToDictionary(Function(a) a.op,
                              Function(a)
                                  Return a.pars
                              End Function)
        End Sub

        Public Overrides Function ToString() As String
            Return transform.GetJson
        End Function

        Private Shared Iterator Function Parse(str As String) As IEnumerable(Of (op$, pars As String()))
            Dim matches = str.Matches($"[a-z]+\s*\({SimpleNumberPattern}(\s+{SimpleNumberPattern})*\)")

            For Each op As String In matches
                Dim op_name = op.Match("[a-z]+")
                Dim pars = op.GetStackValue("(", ")") _
                    .Split _
                    .Select(AddressOf Strings.Trim) _
                    .Where(Function(si) si.Length > 0) _
                    .ToArray

                Yield (Strings.LCase(op_name), pars)
            Next
        End Function
    End Class
End Namespace
