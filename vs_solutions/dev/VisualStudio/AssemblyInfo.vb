Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text
Imports DevAssemblyInfo = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Public Module AssemblyInfoExtensions

    Const assmInfo$ = "My Project/AssemblyInfo.vb"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="vbproj">The file path of the ``*.vbproj`` file.</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetAssemblyInfo(vbproj As String) As DevAssemblyInfo
        Return $"{vbproj.ParentPath}/{assmInfo}".ReadAllText.ParseAssemblyInfo
    End Function

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
        Dim info As New DevAssemblyInfo

        For Each reader As PropertyInfo In GetType(DevAssemblyInfo).GetProperties(PublicProperty)
            With reader
                If .GetIndexParameters.IsNullOrEmpty Then
                    If .PropertyType Is GetType(String) Then
                        Call .SetValue(info, attributes(.Name).Value)
                    ElseIf .PropertyType Is GetType(Boolean) Then
                        Call .SetValue(info, attributes(.Name).Value.ParseBoolean)
                    Else
                        Throw New NotImplementedException(.PropertyType.ToString)
                    End If
                End If
            End With
        Next

        Return info
    End Function
End Module
