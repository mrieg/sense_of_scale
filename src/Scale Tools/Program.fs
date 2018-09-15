// ----    run with AARDIUM    ----
(**)
open System

open Aardvark.Base
open Aardvark.Application
open Aardvark.Application.Slim
open Aardvark.UI

open Suave
open Suave.WebPart
open Aardium

[<EntryPoint; STAThread>]
let main argv =
    Ag.initialize()
    Aardvark.Init()
    Aardium.init()

    let useVulkan = false

    let runtime, disposable =
        if useVulkan then
            let app = new Aardvark.Rendering.Vulkan.HeadlessVulkanApplication()
            app.Runtime :> IRuntime, app :> IDisposable
        else
            let app = new OpenGlApplication()
            app.Runtime :> IRuntime, app :> IDisposable
    use __ = disposable
    
    //let app = TestApp.App.app
    let app = ScaleTools.App.app

    let instance = 
        app |> App.start

    WebPart.startServer 4321 [ 
        MutableApp.toWebPart' runtime false instance
        Suave.Files.browseHome
    ]  
    
    Aardium.run {
        url "http://localhost:4321/"
        width 1000
        height 800
        debug true
    }

    0
(**)


// ----    run with CEF    ----
(*
open System
open System.Windows.Forms

open Aardvark.Base
open Aardvark.Application
open Aardvark.Application.WinForms
open Aardvark.UI

open Suave
open Suave.WebPart

[<EntryPoint; STAThread>]
let main argv = 
    
    Xilium.CefGlue.ChromiumUtilities.unpackCef()
    Chromium.init argv

    Ag.initialize()
    Aardvark.Init()

    use app = new OpenGlApplication()
    //let instance = TestApp.App.app |> App.start
    let instance = ScaleTools.App.app |> App.start
    
    WebPart.startServerLocalhost 4321 [ 
        MutableApp.toWebPart' app.Runtime false instance
        Suave.Files.browseHome
    ]  

    use form = new Form()
    form.Width <- 800
    form.Height <- 600

    use ctrl = new AardvarkCefBrowser()
    ctrl.Dock <- DockStyle.Fill
    form.Controls.Add ctrl
    ctrl.StartUrl <- "http://localhost:4321/"
    ctrl.ShowDevTools()
    form.Text <- "Examples"

    Application.Run form

    0
*)