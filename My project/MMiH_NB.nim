import MMiH_NarrativeBuilder
import uing

let app = initApp()

proc placeholderMenuProc(_: MenuItem, _: Window) = discard

proc main = 
    block MenuSetup:
        let speakerMenu = newMenu("Manage speakers")
        speakerMenu.addItem("Add speaker",    placeholderMenuProc)
        speakerMenu.addItem("Remove speaker", placeholderMenuProc)
    let speakerGroup  = newGroup("Speakers", true)
    let speakerSelect = newRadioButtons()
    for speakerName in getSpeakers(app):
        speakerSelect.add(speakerName)
  
    let window   = newWindow("Match Made In Heaven: Narrative Editor", 500, 500, hasMenubar=true)
    window.child       = speakerGroup
    speakerGroup.child = speakerSelect
  
    show window
    mainLoop()

init()
main()
if true: # block by something later
  discard exportApp(app)
