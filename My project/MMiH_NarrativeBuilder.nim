import std/algorithm
import std/strformat
import std/strutils
import std/sequtils
import std/tables
import std/json

type
  Dictionary = object
    words : seq[string]
  Speaker = object
    name* : string
  Choice = object
    msg      : string
    followup : string
    paragon  : int
    reneg    : int
  Dialogue = object
    msg     : string
    wordle  : string # word within `msg`
    next    : string # ID from `dlist` to be forwarded later
    speaker : Speaker
    choices : seq[Choice]

const SPKR_PATH = "Assets/Characters.json"
const DICT_PATH = "Assets/KnownWords.json"
const DIAL_PATH = "Assets/App/Dialogue.json"

proc speakersGet (): seq[Speaker] =
    let df = open(SPKR_PATH)
    let dj = parseJson(df.readAll())
    for act in dj.items():
      let actobj = Speaker(name: act.getStr())
      add(result, actobj)
    close(df)
   
# === MAIN GAME CONSTRUCTOR === 
type
  App = object
    speakers : seq[Speaker]
    dlist    : Table[string, Dialogue]
proc initApp* (): App =
    result.speakers = speakersGet()
    
proc getSpeakers* (a: App): seq[string] =
    # used so that you don't access object; both for GUI usage and JSON export
    for speaker in a.speakers:
      add(result, speaker.name)
      
proc addSpeaker* (a: var App, name: string): bool =
    # returns if succeed to add a speaker
    for speaker in a.speakers:
      if speaker.name == name: return false
    add(a.speakers, Speaker(name: name))
    return true
    
proc exportApp* (a: App): bool =
    # returns if succeed to proceed with all exports
    block SpeakersExport:
        let spf = open(SPKR_PATH, fmWrite)
        spf.write(%getSpeakers(a))
        close(spf)

#======================================================================
proc gameExport (dls: App) =
    var nodes = ""
    for id, dialogue in dls.dlist: # node fills
        nodes.add("""
        REPLACE_ME
        """.dedent())
    let gf = open(DIAL_PATH, fmWrite)
    let outxt = r"""
      {
        "settings": {
          "game_title": "Alien Wordle Dating Sim",
          "thresholds": {
            "paragon_ending": 50,
            "renegade_ending": 50
          }
        },
        "nodes": [
        REPLACE_ME
        ]
      }
      """.dedent()
    gf.write(outxt.replace("REPLACE_ME", nodes))

proc dictionaryAdd (d: var Dictionary, s: string) =
    if s notin d.words:
      add(d.words, s)

proc dictionaryRemove (d: var Dictionary, s: string) =
    let ix = find(d.words, s)
    if ix != -1: # if exists
      delete(d.words, ix)

proc dictionaryValidator (d: var Dictionary) =
    # ensures consistent letters and sorting
    const REPLACEMENTS = {
        "’": "'"
    }.toTable
    for ix, w in d.words.pairs():
      for RE in REPLACEMENTS.keys():
        if RE in w:
          d.words[ix] = replace(w, RE, REPLACEMENTS[RE])
    sort(d.words)

proc dictionaryImport (): Dictionary =
    let df = open(DICT_PATH)
    let dj = parseJson(df.readAll())
    for word_node in dj.items():
      add(result.words, word_node.getStr())
    dictionaryValidator(result)
    close(df)

proc dictionaryExport (d: Dictionary) =
    let df = open(DICT_PATH, fmWrite)
    df.write("[\n")
    for ix, w in d.words.pairs():
      if ix != len(d.words) - 1:
        df.write("  \"" & w & "\",\n")
      else: # if last, omit ','
        df.write("  \"" & w & "\"\n")
    df.write("]\n")
    close(df)

var dd = dictionaryImport()
dictionaryExport(dd)
