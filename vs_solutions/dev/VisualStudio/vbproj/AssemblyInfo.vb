#Region "Microsoft.VisualBasic::1bebbeb03353bbcf6762f12d5cf2b45d, sciBASIC#\vs_solutions\dev\VisualStudio\vbproj\AssemblyInfo.vb"

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

    '   Total Lines: 77
    '    Code Lines: 58
    ' Comment Lines: 11
    '   Blank Lines: 8
    '     File Size: 3.31 KB


    '     Module AssemblyInfoExtensions
    ' 
    '         Function: GetAssemblyInfo, ParseAssemblyInfo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text
Imports DevAssemblyInfo = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Namespace vbproj

    Public Module AssemblyInfoExtensions

        Const assmInfo$ = "My Project/AssemblyInfo.vb"

        ''' <summary>
        ''' Get assembly info from a vbproject file
        ''' </summary>
        ''' <param name="vbproj">The file path of the ``*.vbproj`` file.</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetAssemblyInfo(vbproj As String) As DevAssemblyInfo
            Return $"{vbproj.ParentPath}/{assmInfo}".ReadAllText.ParseAssemblyInfo
        End Function

        ''' <summary>
        ''' Parse a assembly info vb source file.
        ''' </summary>
        ''' <param name="vb">The VB file content text, not file path.</param>
        ''' <returns></returns>
        <Extension>
        Public Function ParseAssemblyInfo(vb As String) As DevAssemblyInfo
            Dim lines = vb.LineTokens _
                .Select(AddressOf Trim) _
                .Where(Function(l)
                           Return Not l.StringEmpty AndAlso l.First = "<"c AndAlso l.Last = ">"c
                       End Function) _
                .Select(Function(l)
                            Return l.GetStackValue("<", ">") _
                                .GetTagValue(":", trim:=True) _
                                .Value
                        End Function) _
                .ToArray
            Dim attributes = lines _
                .Select(Function(l)
                            Dim name$ = Mid(l, 1, InStr(l, "(") - 1)
                            Dim value$ = l.GetStackValue("(", ")").Trim(ASCII.Quot)

                            Return New NamedValue(Of String) With {
                                .Name = name,
                                .Value = value,
                                .Description = l
                            }
                        End Function) _
                .ToDictionary
            Dim info As New DevAssemblyInfo With {
                .BuiltTime = Now
            }

            For Each reader As PropertyInfo In GetType(DevAssemblyInfo).GetProperties(PublicProperty)
                With reader
                    If attributes.ContainsKey(.Name) AndAlso .GetIndexParameters.IsNullOrEmpty Then
                        If .PropertyType Is GetType(String) Then
                            Call .SetValue(info, attributes(.Name).Value)
                        ElseIf .PropertyType Is GetType(Boolean) Then
                            Call .SetValue(info, attributes(.Name).Value.ParseBoolean)
                        Else
                            Throw New NotImplementedException($"Dim { .Name} As { .PropertyType.ToString} = {attributes(.Name).Value}")
                        End If
                    End If
                End With
            Next

            Return info
        End Function
    End Module
End Namespace
