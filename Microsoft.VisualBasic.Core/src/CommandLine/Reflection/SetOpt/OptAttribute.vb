#Region "Microsoft.VisualBasic::3d38791efb0a6c87a9262be06e2182e0, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\SetOpt\OptAttribute.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29 (76.32%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (23.68%)
    '     File Size: 1.10 KB


    '     Class OptAttribute
    ' 
    '         Properties: Names
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Property)>
    Public Class OptAttribute : Inherits Attribute

        Public ReadOnly Property Names As String()

        Default Public ReadOnly Property Value(args As CommandLine) As String
            Get
                If Names.IsNullOrEmpty Then
                    Return Nothing
                End If

                For Each name As String In Names
                    If args.ContainsParameter(name) Then
                        Return args(name)
                    End If
                    If args.BoolFlags.IndexOf(name) > -1 Then
                        Return "True"
                    End If
                Next

                Return Nothing
            End Get
        End Property

        Sub New(ParamArray names As String())
            Me.Names = names
        End Sub

        Public Overrides Function ToString() As String
            Return Names.GetJson
        End Function

    End Class
End Namespace
