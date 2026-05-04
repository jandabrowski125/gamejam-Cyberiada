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
    # speaker  : Speaker
    followup : string
    paragon  : int
    reneg    : int
  Dialogue = object
    msg     : string
    wordle  : string # word within `msg`
    next    : string # ID from `dlist` to be forwarded later
    speaker : Speaker
    choices : seq[Choice]
    # # non-exportable data
    # sentlst : seq[string] # `msg` split by dots (for selection of sentences)
    # sentctx : int         # index of selected sentence?

const SPKR_PATH = "Assets/Characters.json"
const DICT_PATH = "Assets/KnownWords.json"
const DIAL_PATH = "Assets/Dialogues/Dialogue.json"

# TEMPORARY
# meant to be later built into simpler seq[string] importer (validator might remain)
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
# TEMPORARY END

proc speakersGet (): seq[Speaker] =
    let df = open(SPKR_PATH)
    let dj = parseJson(df.readAll())
    for act in dj.items():
      let actobj = Speaker(name: act.getStr())
      add(result, actobj)
    close(df)

proc dialogueGet (speakerList: seq[Speaker], dialList: JsonNode): OrderedTable[string, Dialogue] =
    # : dialList - should be result of parsing Dialogue.json and getting its ["nodes"] key (which is meant to be JArray type)
    # echo dialList.kind
    for dialNode in dialList.items():
        var dial = Dialogue(msg     : getOrDefault(dialNode, "text_original").getStr("[ERROR]"),
                            wordle  : getOrDefault(dialNode, "wordle_solution").getStr(""),
                            next    : getOrDefault(dialNode, "next_node").getStr("[ERROR]"), # "" (last node symbol) should still load to not reach default
                            speaker : Speaker(name: getOrDefault(dialNode, "speaker").getStr("[ERROR]")),
                            choices : newSeq[Choice]())
        for chcNode in getOrDefault(dialNode, "choices").items():
            dial.choices.add(Choice(msg      : getOrDefault(chcNode, "text").getStr("[ERROR]"),
                                    followup : getOrDefault(chcNode, "follow_up").getStr("[ERROR]"),
                                    paragon  : getOrDefault(chcNode, "plus_paragon").getInt(0),
                                    reneg    : getOrDefault(chcNode, "plus_renegade").getInt(0)))
        # if dial.msg == "": return ERROR_ID
        # for dial.choices: if chc.msg/followup == "": return ERROR_ID
        # if dial.speaker notin speakerList == "": return ERROR_ID
        # Error object? (aka types: "too long choice seq" MinorError, "empty string" EmptyError, "not parsed correctly" MajorError)
        result[getOrDefault(dialNode, "node_id").getStr()] = dial
   
# === MAIN GAME CONSTRUCTOR === 
type
  App = object
    speakers : seq[Speaker]
    wdict    : seq[string]
    dlist*   : OrderedTable[string, Dialogue]
    # errs     : seq[string] # IDs of dialogues that contain [ERROR] or [EMPTY]
    
proc initApp* (): App =
    result.speakers = speakersGet()
    result.wdict    = dictionaryImport().words
    block dialogues: # unblock it later and make it less verbose on init (migrate all stuff into `dialogueGet`)
        let df = open(DIAL_PATH)
        let dj = parseJson(df.readAll())
        result.dlist = dialogueGet(result.speakers, dj["nodes"])
    
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

proc getDictionary* (a: App): seq[string] =
    return a.wdict
    
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
