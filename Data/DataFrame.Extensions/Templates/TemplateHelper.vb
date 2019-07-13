#Region "Microsoft.VisualBasic::3d81155e61152ed021bd0ef6a9827699, Data\DataFrame.Extensions\Templates\TemplateHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module TemplateHelper
    ' 
    '     Function: GetTypesHelperInternal, ScanTemplates
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting

Public Module TemplateHelper

    ''' <summary>
    ''' 扫描目标文件夹之中的所有.NET assembly，然后将<see cref="TemplateAttribute"/>所标记出来的
    ''' 模板类保存到<paramref name="save"/>文件夹之中对应的csv。
    ''' </summary>
    ''' <param name="DIR$"></param>
    ''' <param name="save$"></param>
    ''' <returns></returns>
    Public Function ScanTemplates(DIR$, save$, Optional throwEx As Boolean = True) As Dictionary(Of String, Type)
        Dim typeTable As New Dictionary(Of String, Type)

        For Each dll$ In ls - l - r - {"*.dll", "*.exe"} <= DIR
            Dim assm As Assembly = Assembly.LoadFile(dll)
            Dim types = GetTypesHelperInternal(assm, throwEx) _
                .Select(Function(t)
                            Return (t.GetCustomAttribute(Of TemplateAttribute), t)
                        End Function) _
                .Where(Function(t) Not t.Item1 Is Nothing) _
                .ToArray

            For Each t As (template As TemplateAttribute, type As Type) In types
                With t
                    Dim fileName$ = .template.AliasName Or .type.Name.AsDefault
                    Dim key$ = $"{ .template.Category}/{fileName}"
                    Dim path$ = $"{save}/{key}.csv"
                    Dim template As IEnumerable = {
                        Activity.ActiveObject(.type)
                    }

                    Call template.SaveTable(path, type:= .type)
                    Call typeTable.Add(key, .type)
                End With
            Next
        Next

        Return typeTable
    End Function

    Private Function GetTypesHelperInternal(assembly As Assembly, throwEx As Boolean) As Type()
        Try
            Return EmitReflection.GetTypesHelper(assembly)
        Catch ex As Exception

            ex = New Exception(assembly.ToString, ex)

            If throwEx Then
                Throw
            Else
                Call ex.PrintException
                Call App.LogException(ex)

                Return {}
            End If
        End Try
    End Function
End Module
