Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem

Namespace Language.UnixBash

    ''' <summary>
    ''' VB.NET language shell syntax supports exports
    ''' </summary>
    Public Module ShellSyntax

#Region "ls -l /*"
        ''' <summary>
        ''' ``ls -l -ext("*.xml") &lt;= DIR``,  The filesystem search proxy
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ls As New Search
        ''' <summary>
        ''' Long name(DIR+fiename), if not only file name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property l As New SearchOpt(SearchOpt.Options.LongName)
        ''' <summary>
        ''' 递归的搜索
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property r As New SearchOpt(SearchOpt.Options.Recursive)
        ''' <summary>
        ''' Searching the directory, if this parameter is not presents, then returns search file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property lsDIR As New SearchOpt(SearchOpt.Options.Directory)

        ''' <summary>
        ''' 可以使用这个来限定文件或者文件夹对象的搜索范围
        ''' </summary>
        ''' <param name="__wildcards">可以为文件拓展或者对文件名的通配符的表达式，假若这个是空的，则会默认搜索所有文件*.*</param>
        ''' <returns></returns>
        Public Function wildcards(ParamArray __wildcards As String()) As SearchOpt
            Dim opt As New SearchOpt(SearchOpt.Options.Ext)

            If __wildcards.IsNullOrEmpty Then
                __wildcards = {"*.*"}
            End If

            For Each s As String In __wildcards
                Call opt.wildcards.Add(s)
            Next

            Return opt
        End Function

        Public Function DIRHandle(res As String) As Integer
            Return FileHandles.OpenHandle(res)
        End Function
#End Region

#Region "rm -rf /*"

        Public ReadOnly Property rm As FileDelete
            Get
                Return New FileDelete
            End Get
        End Property

        Public ReadOnly Property rf As rmOption
        Public ReadOnly Property f As rmOption
#End Region

    End Module
End Namespace