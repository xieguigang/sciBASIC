#Region "Microsoft.VisualBasic::f8318e74705c915ca4cbfeb0789eca83, mime\application%json\Javascript\JsonElement.vb"

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

    '   Total Lines: 50
    '    Code Lines: 27 (54.00%)
    ' Comment Lines: 16 (32.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (14.00%)
    '     File Size: 1.84 KB


    '     Class JsonElement
    ' 
    '         Function: [As], Parse, ParseJSON, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Javascript

    ''' <summary>
    ''' The abstract javascript object model
    ''' </summary>
    Public MustInherit Class JsonElement

        Public Overrides Function ToString() As String
            Return "base::json"
        End Function

        ''' <summary>
        ''' do direct cast to the required json element sub type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of T As JsonElement)() As T
            Return DirectCast(Me, T)
        End Function

        ''' <summary>
        ''' Parse the json string as clr json element
        ''' </summary>
        ''' <param name="jsonStr"></param>
        ''' <param name="default">
        ''' set the default json value returns if the given json string is nothing or ``null`` literal.
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseJSON(jsonStr As String, Optional [default] As JsonElement = Nothing) As JsonElement
            Return If(New JsonParser(jsonStr).OpenJSON, [default])
        End Function

        Public Shared Function Parse(json_str As String) As Task(Of JsonElement)
            Return Task(Of JsonElement).Run(Function() ParseJSON(json_str))
        End Function

        Public Shared Narrowing Operator CType(js As JsonElement) As JavaScriptObject
            If TypeOf js Is JsonObject Then
                Return DirectCast(js, JsonObject).CreateJsObject
            Else
                Return Nothing
            End If
        End Operator
    End Class
End Namespace
