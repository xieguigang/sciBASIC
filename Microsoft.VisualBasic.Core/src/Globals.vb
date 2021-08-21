#Region "Microsoft.VisualBasic::066e3fd7c705a01d7797e4fd79b9170f, Microsoft.VisualBasic.Core\src\Globals.vb"

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

' Module Globals
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Language.C

''' <summary>
''' global constants
''' </summary>
Public Module Globals

    ''' <summary>
    ''' The first element in a collection.
    ''' </summary>
    Public Const Scan0 As Integer = 0

    ''' <summary>
    ''' Nothing
    ''' </summary>
    Friend Const null = Nothing

    Public Const void = null

    ''' <summary>
    ''' the start time of the unix time stamp
    ''' </summary>
    Public Const unixEpocUtc As Date = #1970/1/1 0:0:0#

    ''' <summary>
    ''' Diagnostic Messages
    ''' 
    ''' Generate a diagnostic message from its arguments.
    ''' </summary>
    ''' <param name="msg">	
    ''' zero Or more objects which can be coerced to character 
    ''' (And which are pasted together with no separator) Or 
    ''' (for message only) a single condition object.
    ''' </param>
    Public Sub message(ParamArray msg As Object())
        Dim str As String() = msg.SafeQuery.Select(AddressOf any.ToString).ToArray
        Dim strMsg As String = str _
            .Select(Function(s) s.ReplaceMetaChars) _
            .JoinBy("")

        Call strMsg.__INFO_ECHO
    End Sub
End Module
