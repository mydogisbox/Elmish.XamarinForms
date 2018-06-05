
module MainWindow

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.WPF
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews

open EventsAndObservables
open BackgroundProcessor

module App = 

    type Model = { Count: int; History: DateTime list option }

    let sendIncrementEventOusideEXF () =
        eventsObservableOutsideOfEXF.OnNext(IncrementRequested)
        Cmd.none

    let initModel = { Count = 0; History = None }

    let init () = initModel, Cmd.none

    type Message = 
    | NewValue of CurrentValue:int
    | SendIncrement
    | GetHistory
    | ViewHistory of DateTime list

    let getHistoryCmd =
        async {
            let! historyList = processor.PostAndAsyncReply(fun replyChannel-> BackgroundProcessor.GetHistory replyChannel)
            return ViewHistory historyList
        }
        |> Cmd.ofAsyncMsg

    let update msg model =
        match msg with
        | SendIncrement -> model, sendIncrementEventOusideEXF ()
        | NewValue newValue -> { model with Count = newValue; }, Cmd.none
        | GetHistory -> model, getHistoryCmd
        | ViewHistory list -> { model with History = Some list }, Cmd.none

    let view (model: Model) dispatch =
        Xaml.ContentPage(
            content= Xaml.StackLayout(
                            padding=20.0, verticalOptions=LayoutOptions.StartAndExpand,
                            children= [ 
                                yield Xaml.Button(text="Add", command = (fun () -> dispatch SendIncrement))
                                yield Xaml.Label(text=model.Count.ToString())
                                yield Xaml.Button(text="Show History", command = (fun () -> dispatch GetHistory))
                                yield match model.History with
                                        | Some history -> Xaml.StackLayout(
                                                            children=[for item in history -> Xaml.Label(text = item.ToString())])
                                        | None -> Xaml.Label(text = "history not shown")
                                ]))

open App

let registerApply dispatch =
    eventsOriginatingOutsideOfEXF.Subscribe (fun (SubscribedEvents.NewValue value) -> dispatch (NewValue value)) |> ignore

processor.Start()

type CareOrganizerApp () as app = 

    inherit Application ()
    let program = Program.mkProgram init update view
                    |> Program.withConsoleTrace
                    |> Program.withDynamicView app
                    |> Program.withSubscription (fun _ -> Cmd.ofSub registerApply)
                    |> Program.run
                    |> ignore

type MainWindow() as this =
    inherit FormsApplicationPage()

    do Forms.Init()
       this.LoadApplication(new CareOrganizerApp())

[<STAThread>] 
[<EntryPoint>]
do (new System.Windows.Application()).Run(MainWindow()) |> ignore