Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text
Imports DevAssemblyInfo = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Public Module AssemblyInfo

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
        Dim lines = vb.lTokens _
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
                        Dim name$ = Mid(l, 1, InStr(l, "("))
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
            If reader.GetIndexParameters.IsNullOrEmpty Then
                Call reader.SetValue(info, attributes(reader.Name).Value)
            End If
        Next

        Return info
    End Function
End Module
