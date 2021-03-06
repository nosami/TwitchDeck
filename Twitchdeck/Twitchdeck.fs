﻿// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace Twitchdeck

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open System

module Views =
    let sceneButton name selected =
        let color = if selected then (Color.FromHex "#33B2FF") else Color.Default
        View.Button(
            text = name,
            verticalOptions = LayoutOptions.FillAndExpand,
            backgroundColor = color)

    let noScenes =
        View.ContentPage(content = View.Label(text="There are no scenes defined."))

    let scenes (names: string list) selectedScene =
        View.ContentPage(
            content = View.StackLayout(children = [for name in names -> sceneButton name (name = selectedScene)]))

module App = 
    type Model = {
        SceneNames: string list
        SelectedScene: string
    }

    type Msg =
        | Add of string
        | Remove of string

    let initModel = { SceneNames = ["Scene 1"; "Scene 2"; "Scene 3"]; SelectedScene = "Scene 2" }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | Add name -> model, Cmd.none
        | Remove name -> model, Cmd.none

    let view (model: Model) dispatch =
        if model.SceneNames.Length = 0 then
            Views.noScenes
        else
            Views.scenes model.SceneNames model.SelectedScene
            

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        try
            App.program
#if DEBUG
            |> Program.withConsoleTrace
#endif
            |> Program.runWithDynamicView app
        with
        | ex -> Debug.WriteLine(ex.Message)
                reraise ()

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/tools.html for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/models.html for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


