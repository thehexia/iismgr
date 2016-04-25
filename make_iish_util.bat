csc /target:library /out:IISHandler.dll /reference:Microsoft.Web.Administration.dll IISHandler.cs

csc /target:exe /out:mkapp.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\mkapp.cs

csc /target:exe /out:rmapp.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\rmapp.cs

csc /target:exe /out:lsiis.exe /r:IISHandler.dll /r:NDesk.Options.dll /r:Microsoft.Web.Administration.dll .\Scripts\lsiis.cs
