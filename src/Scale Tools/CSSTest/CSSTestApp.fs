namespace CSSTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI

module Html =
    
    let textInputColorable (str : IMod<string>) (textColor : C4b) changeAction =
        Incremental.div
            (AttributeMap.ofList[])
                (
                    alist {
                        let! t = str
                        let col = sprintf "color:%s" (Html.ofC4b textColor)
                        yield
                            Incremental.input
                                (AttributeMap.ofList[
                                    style col
                                    attribute "type" "text"
                                    attribute "value" t
                                    onChange changeAction
                                ])
                    }
                )

module App =
    
    type Action =
        | SetString of string
    
    let update (m : Model) (act : Action) =
        match act with
        | SetString str -> {m with str = str}
    
    let testCss = {kind = Stylesheet; name = "css-teststyle"; url = "css-teststyle.css"}

    let view (m : MModel) =
        require Html.semui (
            body [][
                div [clazz "ui"; style "background:#121212; width:30%; height:100%"][
                    Html.table [
                        Html.row "" [
                            Html.SemUi.textBox m.str SetString
                        ]
                        Html.row "" [
                            Html.textInputColorable m.str C4b.Black SetString
                        ]
                    ]
                ]
            ]
        )
    
    let initial = {str = "Test"}

    let threads (m : Model) = ThreadPool.empty

    let app =
        {
            unpersist = Unpersist.instance
            threads = threads
            initial = initial
            update = update
            view = view
        }
    
    let start() = App.start app