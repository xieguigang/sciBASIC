#Region "Microsoft.VisualBasic::2c2a9046b1db4922fce76570dd40864c, sciBASIC#\mime\application%json\Javascript\JsonElement.vb"

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

    '   Total Lines: 29
    '    Code Lines: 16
    ' Comment Lines: 8
    '   Blank Lines: 5
    '     File Size: 923 B


    '     Class JsonElement
    ' 
    '         Function: [As], ParseJSON, ToString
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseJSON(jsonStr As String) As JsonElement
            Return New JsonParser().OpenJSON(jsonStr)
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
