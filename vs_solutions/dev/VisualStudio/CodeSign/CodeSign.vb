#Region "Microsoft.VisualBasic::dd2889b664c11a2f7e649fa774042171, Microsoft.VisualBasic.Core\ApplicationServices\VBDev\Signature\CodeSign.vb"

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

    '     Module CodeSign
    ' 
    '         Function: ParseHeaderRegion, SignCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.SecurityString
Imports r = System.Text.RegularExpressions.Regex

Namespace CodeSign

    ''' <summary>
    ''' 使用AES加密进行代码的签名操作
    ''' </summary>
    ''' 
    <HideModuleName>
    Public Module CodeSignHelpers

        Const PhpHeaderRegion$ = "#region ""PHP\\Foundation[:]{2}.*"".+?#endregion"
        Const TypeScriptHeaderRegion$ = "//#region ""Microsoft.TypeScript[:]{2}.*"".+?//#endregion"
        Const ROpenHeaderRegion$ = "#Region ""Microsoft.ROpen[:]{2}.*"".+?#End Region"
        Const VisualBasicHeaderRegion$ = "#Region ""Microsoft.VisualBasic[:]{2}.*"".+?#End Region"

        ReadOnly regionPatterns As New Dictionary(Of Languages, String) From {
            {Languages.PHP, PhpHeaderRegion},
            {Languages.R, ROpenHeaderRegion},
            {Languages.TypeScript, TypeScriptHeaderRegion},
            {Languages.VisualBasic, VisualBasicHeaderRegion}
        }

        ''' <summary>
        ''' 这个函数对代码部分的文本生成md5，然后将md5值字符串使用自己的密匙加密，后将结果写在代码文档的header region之中
        ''' 在验证的时候，只需要取出加密的结果，使用公匙解密，得到md5值，然后再计算代码部分的md5，二者比较一下就能够了解代
        ''' 码是否被修改了
        ''' </summary>
        ''' <param name="code$"></param>
        ''' <param name="key"></param>
        ''' <param name="lang"></param>
        ''' <returns></returns>
        Public Function SignCode(code$, key As AES, Optional lang As Languages = Languages.VisualBasic) As String
            Dim region$ = ParseHeaderRegion(code, lang)
            Dim plainCode$ = code.Trim.Substring(region.Length + 1).Trim
            Dim md5$ = plainCode.MD5
            ' 使用自己的密匙加密md5，得到了待验证的数据
            Dim sign = key.EncryptData(md5)

            Throw New NotImplementedException
        End Function

        Public Function ParseHeaderRegion(code$, lang As Languages) As String
            Return r.Match(code, regionPatterns(lang), RegexICSng).Value
        End Function
    End Module
End Namespace
