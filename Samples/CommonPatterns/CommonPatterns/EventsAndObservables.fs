module EventsAndObservables

open System.Reactive.Subjects

type PublishedEvents =
| IncrementRequested

type SubscribedEvents =
| NewValue of int

let eventsObservableOutsideOfEXF = new Subject<PublishedEvents>()
let eventsOriginatingOutsideOfEXF = new Subject<SubscribedEvents>()