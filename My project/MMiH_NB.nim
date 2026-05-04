import MMiH_NarrativeBuilder
import std/tables
import uing

let app = initApp()

proc placeholderMenuProc(_: MenuItem, _: Window) = discard
proc exportAppByMenu(_: MenuItem, _: Window) =
    discard exportApp(app)
# proc dictMenuShow(_: MenuItem, _: Window) =
#     # TODO: closing dictWindow closes also initial window
#     let dictWindow = newWindow("Match Made In Heaven: Dictionary", 300, 300)
#     show dictWindow

# ODDZIELNE WINDOWSY
# główny - dialogue nodes i branche zwizualizowane
#   - dialogue edit (gdy klikniesz na node'a i dasz edit/dasz "nowy node" przy głównym)
#   - dictionary

proc main = 
    block MenuSetup:
        let fileMenu    = newMenu("Files")
        let speakerMenu = newMenu("Manage speakers")
        # let dictMenu    = newMenu("Manage dictionary")
        addItem(fileMenu,    "Export all files", exportAppByMenu)
        addItem(speakerMenu, "Add speaker [unavailable]",      placeholderMenuProc)
        addItem(speakerMenu, "Remove speaker [unavailable]",   placeholderMenuProc)
        # addItem(dictMenu,    "Show dictionary",  dictMenuShow)
        # addItem(dictMenu,    "Add word",         placeholderMenuProc)
        # addItem(dictMenu,    "Remove word",      placeholderMenuProc)
    let speakerGroup  = newGroup("Speakers", true)
    let speakerSelect = newRadioButtons()
    for speakerName in getSpeakers(app):
        speakerSelect.add(speakerName)
  
    let window   = newWindow("Match Made In Heaven: Narrative Editor", 500, 500, hasMenubar=true)
    window.child       = speakerGroup
    speakerGroup.child = speakerSelect

    echo app.dlist["28ec963c-3b86-4b90-9b42-c79f4572aea7"]
  
    show window
    mainLoop()

init()
main()
