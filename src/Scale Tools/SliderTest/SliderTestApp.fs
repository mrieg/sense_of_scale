namespace SliderTest

open Aardvark.Base.Incremental
open Aardvark.UI

module App =
    
    type Action =
        | NumericMessage of Numeric.Action
    
    let update (m : Model) (act : Action) =
        match act with
        | NumericMessage msg -> {m with value = Numeric.update m.value msg}
    
    let view (m : MModel) =
        require Html.semui (
            div [clazz "ui"][
                Numeric.view' [NumericInputType.Slider]   m.value |> UI.map NumericMessage
                Numeric.view' [NumericInputType.InputBox] m.value |> UI.map NumericMessage
            ]
        )
    
    let initial = {value = Numeric.init}

    let threads (m : Model) = ThreadPool.empty

    let app =
        {
            unpersist = Unpersist.instance
            threads = threads
            initial = initial
            view = view
            update = update
        }
    
    let start() = App.start app