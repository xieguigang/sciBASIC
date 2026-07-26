#Region "Microsoft.VisualBasic::364dd768d4bf151308a6b51ba7f5503f, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\SetOpt\SetOpt.vb"

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
    '     File Size: 1.18 KB


    '     Module SetOpt
    ' 
    '         Function: CreateOpt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.Reflection

    Public Module SetOpt

        Public Function CreateOpt(Of T As {New, Class})(args As CommandLine) As T
            Dim obj As Object = New T
            Dim objVal As Object

            For Each data As KeyValuePair(Of String, PropertyInfo) In DataFramework.Schema(Of T)(
                flag:=PropertyAccess.Writeable,
                nonIndex:=True,
                binds:=PublicProperty
            )
                Dim field As PropertyInfo = data.Value
                Dim opt As OptAttribute = field.GetCustomAttribute(Of OptAttribute)

                If opt Is Nothing Then
                    Continue For
                End If

                Dim val As String = opt(args)

                If val Is Nothing Then
                    Continue For
                Else
                    objVal = Scripting.CTypeDynamic(val, field.PropertyType)
                    field.SetValue(obj, objVal)
                End If
            Next

            Return obj
        End Function

    End Module
End Namespace
