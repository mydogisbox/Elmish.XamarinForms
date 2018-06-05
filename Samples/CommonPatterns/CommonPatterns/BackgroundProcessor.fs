module BackgroundProcessor

open System
open EventsAndObservables

let mutable incrementHistory: DateTime list = []

type ProcessorMessages =
| Increment
| GetHistory of AsyncReplyChannel<DateTime list>

let processor = new MailboxProcessor<ProcessorMessages>(fun inbox-> 
        let rec messageLoop () = async {
                let! msg = inbox.Receive()

                match msg with
                | Increment -> 
                    incrementHistory <- DateTime.Now::incrementHistory
                    eventsOriginatingOutsideOfEXF.OnNext(NewValue incrementHistory.Length)

                | GetHistory replyChannel -> 
                    replyChannel.Reply incrementHistory

                do! messageLoop ()
            }
        messageLoop ())

eventsObservableOutsideOfEXF.Subscribe(fun msg -> match msg with 
                                                  | PublishedEvents.IncrementRequested -> processor.Post Increment ) |> ignore