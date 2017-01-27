# PS1
_namespace: [Microsoft.VisualBasic.Language.UnixBash](./index.md)_

PS (Prompt Sign)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.#ctor(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|ps1|-|

> 
>  Example as:
>  
>  ```vbnet
>  ' Fedora 12
>  PS1='[\u@\h \w \A #\#]\$ '
>  ```
>  

#### A
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.A
```
``\A`` 显示时间, 24小时格式: ``HH:MM``

#### d
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.d
```
``\d`` 代表日期, 格式为``Weekday Month Date``, 例如``Mon Aug 1``

#### Fedora12
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.Fedora12
```
Fedora 12 PS1 variable of the bash shell: ``[\u@\h \w \A #\#]\$ ``

#### n
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.n
```
``\#`` 下达的第几个指令。

#### r
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.r
```
``\$`` 提示字符，如果是``root``时，提示字符为``#``，否则就是``$``。

#### T
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.T
```
``\T`` 显示时间, 12 小时的时间格式!

#### tl
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.tl
```
``\t`` 显示时间, 为 24 小时格式, 如: ``HH:MM:SS``

#### u
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.u
```
``\u`` 目前使用者的账号名称

#### v
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.v
```
``\v`` ``BASH``的版本信息

#### W
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.W
```
``\W`` 利用``@``M:Microsoft.VisualBasic.ProgramPathSearchTool.BaseName(System.String)````取得工作目录名称，所以仅会列出最后一个目录名。

#### wl
```csharp
Microsoft.VisualBasic.Language.UnixBash.PS1.wl
```
``\w`` 完整的工作目录名称。家目录会以``~``取代


### Properties

#### H
``\H`` 完整的主机名称
#### hl
``\h`` 仅取主机名称的第一个名字
