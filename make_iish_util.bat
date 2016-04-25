csc /target:library /out:IISHandler.dll /reference:Microsoft.Web.Administration.dll IISHandler.cs

csc /target:exe /out:mkapp.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\mkapp.cs

csc /target:exe /out:rmapp.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\mkapp.cs

csc /target:exe /out:lsiis.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\lsiis.cs

csc /target:exe /out:mksite.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\mksite.cs

csc /target:exe /out:mkpool.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\mkpool.cs
