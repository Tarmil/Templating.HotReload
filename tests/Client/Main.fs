module MiniBlazor.Test.Client.Main

open Microsoft.AspNetCore.Blazor.Routing
open Elmish
open MiniBlazor
open MiniBlazor.Html

type Page = Form | Collection

type Item =
    {
        K: int
        V: string
    }

type Model =
    { 
        input: string
        submitted: option<string>
        addKey: int
        revOrder: bool
        items: Map<int, string>
        page: Page
    }

type Message =
    | SetInput of text: string
    | Submit
    | RemoveItem of key: int
    | SetAddKey of key: int
    | SetKeyOf of key: int
    | AddKey
    | ToggleRevOrder
    | SetPage of Page

let InitModel _ =
    {
        input = ""
        submitted = None
        addKey = 4
        revOrder = false
        items = Map [
            0, "it's 0"
            1, "it's 1"
            2, "it's 2"
            3, "it's 3"
        ]
        page = Form
    }

let Update message model =
    match message with
    | SetInput text -> { model with input = text }
    | Submit -> { model with submitted = Some model.input }
    | RemoveItem k -> { model with items = Map.filter (fun k' _ -> k' <> k) model.items }
    | SetAddKey i -> { model with addKey = i }
    | AddKey -> { model with items = Map.add model.addKey (sprintf "it's %i" model.addKey) model.items }
    | SetKeyOf k ->
        match Map.tryFind k model.items with
        | None -> model
        | Some item ->
            let items = model.items |> Map.remove k |> Map.add model.addKey item
            { model with items = items }
    | ToggleRevOrder -> { model with revOrder = not model.revOrder }
    | SetPage p -> { model with page = p }

let ViewForm model dispatch =
    div [] [
        input [attr.value model.input; on.change (fun e -> dispatch (SetInput (unbox e.Value)))]
        input [attr.``type`` "submit"; on.click (fun _ -> dispatch Submit)]
        div [] [text (defaultArg model.submitted "")]
        (match model.submitted with
        | Some s ->
            concat [
                if s.Contains "secret" then
                    yield div [] [text "You typed the secret password!"]

                if s.Contains "super" then
                    yield div [] [text "You typed the super secret password!"]
            ]
        | None -> empty)
    ]

let ViewItem k v dispatch =
    concat [
        li [] [text v]
        li [] [
            input []
            button [on.click (fun _ -> dispatch (SetKeyOf k))] [text "Set key from Add field"]
            button [on.click (fun _ -> dispatch (RemoveItem k))] [text "Remove"]
        ]
    ]

let ViewCollection model dispatch =
    let items =
        if model.revOrder then
            Seq.rev model.items
        else
            model.items :> _
    div [] [
        input [
            attr.``type`` "number"
            attr.value (string model.addKey)
            on.change (fun e -> dispatch (SetAddKey (int (unbox<string> e.Value))))
        ]
        button [on.click (fun _ -> dispatch AddKey)] [text "Add"]
        br []
        button [on.click (fun _ -> dispatch ToggleRevOrder)] [text "Toggle order"]
        ul [] [
            for KeyValue(k, v) in items -> ViewItem k v dispatch
        ]
    ]

let View model dispatch =
    concat [
        style [] [text ".active { background: lightblue; }"]
        p [] [
            comp<NavLink> [attr.href "/"; "Match" => NavLinkMatch.All] [text "Form"]
            text " "
            comp<NavLink> [attr.href "/collection"; "Match" => NavLinkMatch.All] [text "Collection"]
        ]
        (match model.page with
        | Form -> ViewForm model dispatch
        | Collection -> ViewCollection model dispatch)
    ]

type MyApp() =
    inherit ElmishProgramComponent<Model, Message>()

    override this.Program =
        Program.mkSimple InitModel Update View
        |> Program.withConsoleTrace
        |> Program.withRouter {
            getRoute = fun m ->
                match m.page with
                | Form -> ""
                | Collection -> "collection"
            setRoute = fun r ->
                match r with
                | "collection" -> Collection
                | _ -> Form
                |> SetPage
        }
