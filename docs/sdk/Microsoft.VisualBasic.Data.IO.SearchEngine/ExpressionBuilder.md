# ExpressionBuilder
_namespace: [Microsoft.VisualBasic.Data.IO.SearchEngine](./index.md)_

只是构建出对单个对象的查询计算的表达式，进行整个数据集查询的LIMIT和TOP关键词将不会在这里被处理



### Methods

#### Build
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.ExpressionBuilder.Build(System.String,Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens,System.Boolean,System.Boolean)
```
构建查询表达式的对象模型

|Parameter Name|Remarks|
|--------------|-------|
|query$|-|
|anyDefault|
 If all of the tokens in **`query$`** expression is type @``F:Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens.AnyTerm``, 
 then this parameter will be enable to decided that the relationship between these tokens is 
 @``F:Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens.op_AND`` for all should match or @``F:Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens.op_OR`` for any match?
 (请注意，这个参数值只允许@``F:Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens.op_AND``或者@``F:Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens.op_OR``)
 |
|caseSensitive|计算字符串值的时候是否大小写敏感？|
|allowInStr|是否允许只匹配上部分字符串|



