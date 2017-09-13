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
    Public Function ScanTemplates(DIR$, save$) As Dictionary(Of String, Type)
        Dim typeTable As New Dictionary(Of String, Type)

        For Each dll$ In ls - l - r - {"*.dll", "*.exe"} <= DIR
            Dim assm As Assembly = Assembly.LoadFile(dll)
            Dim types = assm.GetTypes _
                .Select(Function(t) (t.GetCustomAttribute(Of TemplateAttribute), t)) _
                .Where(Function(t) Not t.Item1 Is Nothing)

            For Each t In types
                Dim fileName$ = t.Item1.AliasName Or t.Item2.Name.AsDefault
                Dim path$ = $"{save}/{t.Item1.Category}/{fileName}.csv"
                Dim template As IEnumerable(Of Object) = {
                    Activity.ActiveObject(t.Item2)
                }

                Call template.SaveTo(path)
                Call typeTable.Add(t.Item1.Category & "/" & fileName, t.Item2)
            Next
        Next

        Return typeTable
    End Function
End Module
