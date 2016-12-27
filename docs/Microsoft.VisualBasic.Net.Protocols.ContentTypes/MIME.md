# MIME
_namespace: [Microsoft.VisualBasic.Net.Protocols.ContentTypes](./index.md)_

MIME stands for "Multipurpose Internet Mail Extensions. It's a way of identifying files on the Internet according to their nature and format. 
 For example, using the ``Content-type`` header value defined in a HTTP response, the browser can open the file with the proper extension/plugin.
 (http://www.freeformatter.com/mime-types-list.html)




### Properties

#### ContentTypes
根据类型来枚举，Key全部都是小写的
#### ExtDict
枚举出所有已知的文件拓展名列表，Key全部都是小写的 (格式: ``.ext``)
#### Unknown
.*（ 二进制流，不知道下载文件类型）
