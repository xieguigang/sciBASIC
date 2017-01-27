# NgenInstaller
_namespace: [Microsoft.VisualBasic.SoftwareToolkits](./index.md)_

Ngen.exe (Native Image Generator)
 
 The Native Image Generator (Ngen.exe) is a tool that improves the performance of managed applications. 
 Ngen.exe creates native images, which are files containing compiled processor-specific machine code, 
 and installs them into the native image cache on the local computer. The runtime can use native images 
 from the cache instead of using the just-in-time (JIT) compiler to compile the original assembly.
 
 Changes To Ngen.exe In the .NET Framework 4
 Ngen.exe now compiles assemblies With full trust, And code access security (CAS) policy Is no longer evaluated.
 Native images that are generated With Ngen.exe can no longer be loaded into applications that are running In Partial trust.
 
 Changes To Ngen.exe In the .NET Framework version 2.0:
 Installing an assembly also installs its dependencies, simplifying the syntax Of Ngen.exe.
 Native images can now be Shared across application domains.
 A New Action, update, re - creates images that have been invalidated.
 Actions can be deferred For execution by a service that uses idle time On the computer To generate And install images.
 Some causes Of image invalidation have been eliminated.

> 
>  1.7 Native代码产生器: NGen.exe
>  
>  随.NET Framework发布的NGen.exe工具可以将IL代码编译成native代码, 当应用程序安装在用户的机器上时. 因为代码是在安装的时候编译的, CLR的JIT编译器不需要在运行时刻编译IL代码
>  这能提高应用程序的性能. NGen.exe工具在下面两个场合很有趣:
>  
>  提高了应用程序的启动速度 运行NGen.exe能提高启动速度, 因为代码已经编译成native代码, 所以在运行时就不需要编译了
>  减少应用程序的工作集 如果你认为一个程序集会被同时载入到多个进程/ Appdomain, 在这个程序集上运行NGen.exe能减少应用程序的工作集, 其原因是NGen.exe工具将IL编译成native代码,
>  然后将输出保存到单独的文件中, 这个文件能同时被内存映射(memory - mapping)到多个进程地址空间中, 允许代码共享, 每个进程 / AppDomain不必为自己拷贝一份代码
>  
>  当一个安装程序调用NGen.exe对一个应用程序或程序集进行编译时, 那个应用程序的所有程序集或者一个特定的程序集会把其IL代码编译成native代码, 一个新的只包含native代码而不含有IL的程序集文件会被NGen.exe创建.
>  这个新的文件被放到名字类似于 C: /Windows/Assembly/NativeImages_v2.0.50727_32的文件夹下面, 这个文件家名字包含了CLR的版本和native代码是否是为x86(32位版本的Windows), x64, 
>  或者Itaninum(64位版本的Windows)编译的信息.
>  
>  现在, 当CLR载入一个程序集文件时, CLR查看对应的NGen native文件是否存在, 如果没发现native文件, CLR JIT对IL代码像通常那样进行编译.
>  然而, 如果对应的native文件存在, CLR将使用native文件中的编译好的代码, 文件中的函数就不需要在运行时刻编译了.
>  
>  在表面上, 这听起来非常好, 听上去就像如果你得到了托管代码的全部优点(垃圾回收, 代码验证, 类型安全, 等等)而不牺牲托管代码的性能(JIT编译), 
>  但是实际情况并不总是那么美好, NGen'd文件有几个潜在的问题:
>  
>  没有知识产权保护 很多人以为可以发布NGen文件而不用发布包含原始IL代码的文件, 从而使他们的知识产权更加保密
>  不幸的是, 这并不可行, 在运行时刻, CLR需要访问程序集的metadata(为某些函数, 例如反射和串行化函数), 这需要发布包含IL和metadata的程序集.
>  此外, 如果由于某种原因, CLR不能使用NGen文件(如下面所描述的), 那么CLR会回到JIT编译, 对程序集的IL代码进行编译, 因此IL代码必须存在.
>  
>  NGen文件可能会过时 当CLR载入NGen文件时, 它会比较以前编译的代码和当前的执行环境的很多特征, 如果任何特征不匹配, NGen文件就不能被使用, JIT编译器进程就要使用. 这是必须被匹配的部分特征列表.
>  
>    程序集模块的版本ID(MVID)
>    被引用的程序集的版本ID
>    处理器类型
>    CLR版本
>    Build类型(release, debug, optimized debug, profiling, 等等)
>  
>  所有链接时的安全性要求都必须在运行时刻被满足才能允许载入.
>  
>  注意有可能以升级的方式运行NGen.exe, 这告诉工具对以前曾经被执行NGen'd的所有的程序集上运行NGen.exe. 当终端用户安装.NET Framework的一个新service pack, 
>  那么service pack的安装程序将会在更新模式下自动运行NGen.exe, 使得NGen文件保持和CLR的版本一致.
>  
>  较差的载入时性能(重定位/绑定): 程序集文件是标准的Windows PE文件, 每个文件包含着一个优先使用的基地址. 很多Windows开发者对围绕基地址和重定位的问题很熟悉, 
>  关于这个主题的更多信息, 可以参考我的书 programming Applications for Microsoft Windows, 4th Edition. 当JIT编译代码时, 不必关心这些问题, 因为正确的内存地址引用会在运行时计算出来.
>  
>  然而, NGen的程序集文件的一些内存地址引用是静态计算的, 当Windows加载一个NGen文件时, 它检查文件是否被载入到优先的基地址上, 如果文件没有载入到优先的基地址, 
>  Windows会重新定位文件, 修改所有内存地址引用. 这是极其耗时的, 因为Windows必须载入整个文件, 并修改文件中的很多字节. 此外, 这个页面文件对应的代码不能跨进程边界共享.
>  
>  因此如果你打算NGen程序集文件, 你应该为你的程序集文件选择好的基地址(通过csc.exe的 / baseaddress命令行开关).当你NGen一个程序集文件时, NGen文件将被赋予一个基地址, 
>  这需要使用一个基于托管程序集基地址的算法. 不幸的是, 微软从没有一个良好的指导来帮助开发者如何赋予基地址. 在64位版本的Windows上, 这还不太会成为问题, 因为地址空间是很充足的, 
>  但是对于一个32位的地址空间, 为每一个程序集选择一个好的基地址几乎是不可能的, 除非你精确地知道什么东西会被载入到进程, 知道那个程序集的大小不会超过后一个版本.
>  
>  较差的执行时性能 当编译代码时, NGen对执行环境做出的假设不会比JIT编译器的多, 这会造成NGen.exe产生较差的代码, 例如, NGen不能优化一些CPU指令, 对静态字段的访问需要简介的操作,
>  因为静态字段实际的地址需要在运行时刻才能知道.NGen到处插入代码来调用类的构造函数, 因为它不知道代码执行的次序, 不知道类的构造憾事是否已经被调用了(见第8章, 类的构造函数).
>  一些NGen应用程序会比JIT编译的代码慢大约5%, 因此, 如果你打算使用NGen来提高应用程序的性能, 你应该对比NGen’d和非NGen’d版本的应用程序, 确定NGen’d版本在实际执行时并不慢. 
>  对于一些应用程序, 减小的工作集大小会提高性能, 因此NGen总体上还是会取胜.
>  
>  因为上面列出的所有问题, 当考虑使用NGen.exe时, 你应该非常小心.对于服务器端的应用程序来说, NGen.exe的用处很小甚至没有意义, 因为只有第一个客户需求经历了性能上的下降, 
>  后面的客户需求都是高速运行的.此外, 对于大多数服务器应用程序, 只需要代码的一个实例, 因此没有工作集方面的利益.
>  
>  对于客户端应用程序, NGen.exe可能对于提高启动速度或者减小工作集有帮助, 如果程序集被多个应用程序同时使用.甚至没有多个应用程序使用一个程序集, NGen一个程序集也会提高工作集.
>  此外, 如果NGen.exe被用于所有的客户端应用程序的程序集, 那么CLR就根本不需要载入JIT编译器, 从而更进一步地降低了工作集.
>  当然, 如果只有一个程序集不是NGen'd或者如果一个程序集的NGen文件不能被使用, JIT编译器就会被载入, 应用程序的工作集将会增加.
>  


### Methods

#### Display
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Display(System.String)
```
Display the state of the native images for an assembly and its dependencies.
 If no argument Is supplied, everything In the native image cache Is displayed.

|Parameter Name|Remarks|
|--------------|-------|
|assemblyName|-|


#### ExecuteQueuedItems
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.ExecuteQueuedItems(Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.PriorityLevels)
```
Execute queued compilation jobs.
 If a priority Is specified, compilation jobs With greater Or equal priority are executed. 
 If no priority Is specified, all queued compilation jobs are executed.

|Parameter Name|Remarks|
|--------------|-------|
|queue|-|


#### Install
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Install
```
将当前目录下的所有的.NET程序都进行安装

#### Queue
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Queue(Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.QueueActions)
```
Pause the native image service, allow the paused service to continue, or query the status of the service.

#### Uninstall
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Uninstall(System.String,Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Scenarios,System.String,System.Boolean)
```
Delete the native images of an assembly and its dependencies from the native image cache.
 To uninstall a single image And its dependencies, use the same command-line arguments that were used to install the image.
 
 Note In the .NET Framework 4, the action uninstall * Is no longer supported.

|Parameter Name|Remarks|
|--------------|-------|
|assemblyName|
 The full display name of the assembly. For example, "myAssembly, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0038abc9deabfle5".
 Only one assembly can be specified per Ngen.exe command line.
 
 * Note You can supply a Partial assembly name, such As myAssembly, For the display And uninstall actions.
 
 The explicit path of the assembly. You can specify a full or relative path.
 If you specify a file name without a path, the assembly must be located In the current directory.
 Only one assembly can be specified per Ngen.exe command line.
 |
|scenarios|-|
|ExeConfig|exePath, Use the configuration of the specified executable assembly.
 Ngen.exe needs to make the same decisions as the loader when binding to dependencies. When a shared component Is loaded at run time, 
 using the Load method, the application's configuration file determines the dependencies that are loaded for the shared component — 
 for example, the version of a dependency that is loaded. The /ExeConfig switch gives Ngen.exe guidance on which dependencies would be loaded at run time.|
|AppBase|directoryPath, When locating dependencies, use the specified directory as the application base.|


#### Update
```csharp
Microsoft.VisualBasic.SoftwareToolkits.NgenInstaller.Update(System.Boolean)
```
Update native images that have become invalid.
 If /queue Is specified, the updates are queued For the native image service. Updates are always scheduled at priority 3, so they run When the computer Is idle.

|Parameter Name|Remarks|
|--------------|-------|
|queue|-|



