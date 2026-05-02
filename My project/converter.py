import json
import re

keyword_search = r'\[\w*\]'

IN  = "in.json"
OUT = "Assets/Dialogues/Dialogue.json"

def formatMainMessage (txt: str or None) -> str or None:
    if txt is None: return None
    txt = txt.replace("{", "{ ")
    txt = txt.replace("}", " }")
    return purgeHTML(txt)

def purgeHTML (txt: str or None) -> str or None:
    # purges HTML & Wordle brackets
    REPLACEABLE_WORDS = [
        "<p>", "</p>",
        "<i>", "</i>",
        "[", "]"
    ]
    if txt is None: return None # prevents conversion of None
    for WORD in REPLACEABLE_WORDS:
        txt = txt.replace(WORD, "")
    return txt

def purgeHTMLRegEx (txt: re.Match or None) -> str or None:
    if txt is None: return None
    else:
        return purgeHTML(txt.group(0))

class Choice:
    def __init__(self, msg, followup, paragon, renegade):
        self.msg      = msg
        self.followup = followup
        self.par      = paragon
        self.ren      = renegade

    def toDict(self) -> dict:
        return {
            "text":          self.msg,
            "plus_paragon":  self.par,
            "plus_renegade": self.ren,
            "follow_up":     self.followup
        }

class Message:
    def __init__(self, mid, person, msg, wordle, next, choices):
        self.id      = mid
        self.person  = person # 'title'
        self.msg     = msg    # 'content'
        self.wordle  = wordle
        self.next    = next
        self.choices = choices

    def toDict(self) -> dict:
        chs = []
        for ch in self.choices:
            chs.append(ch.toDict())
        return {
            "node_id":         self.id,
            "speaker":         self.person,
            "text_original":   self.msg,
            "wordle_solution": self.wordle,
            "next_node":       self.next,
            "choices":         chs
        }

msgs = [] # : list[Message]

with open(IN, 'r') as f:
    jin = json.load(f)

queue = [jin["startingElement"]] # currently checked var(s), starting from first
elems = jin["elements"]
conns = jin["connections"]

while True: # broken only explicitly
    elem = elems[queue[0]]
    # temp
    msg_choices = []
    msg_next    = ""

    # message manager skips choice msgs and followup msgs, rendering their `== 1` not needing of distinguishing
    if len(elem["outputs"]) == 1:
        msg_next = conns[elem["outputs"][0]]["targetid"] # immediate next message
    elif len(elem["outputs"]) > 1: # choice message type (-> indicates what msg it points towards)
        msg_next = conns[elem["outputs"][0]]["targetid"] # nested next message ID (-> choice)
        msg_next = conns[elems[msg_next]["outputs"][0]]["targetid"] # ...goes one msg deeper, leaping over choice (-> followup)
        msg_next = conns[elems[msg_next]["outputs"][0]]["targetid"] # ...and once more, leaping over followup (-> actual nxt msg)
        for choice in elem["outputs"]:
            msg_fup = elems[conns[elems[conns[choice]["targetid"]]["outputs"][0]]["targetid"]] # followup dict
            tit = purgeHTML(msg_fup["title"]) # title is split to get paragon/renegade stuff
            tits = tit.split(";")
            msg_choices.append(Choice(msg      = purgeHTML(elems[conns[choice]["targetid"]]["content"]),
                                      followup = purgeHTML(msg_fup["content"]),
                                      paragon  = int(tits[1]),
                                      renegade = int(tits[2])))

    msgs.append(Message(mid     = queue[0],
                        person  = purgeHTML(elem["title"]),
                        msg     = formatMainMessage(elem["content"]),
                        wordle  = purgeHTMLRegEx(re.search(keyword_search, elem["content"])),
                        next    = msg_next, # msg_next based on first connection, even with multiple
                        choices = msg_choices))
    queue.pop(0) # removes analysed item
    if msg_next != "":
        queue.append(msg_next)
    if len(queue) == 0 or len(elem["outputs"]) == 0: break # breaks out of loop

msg_dictified = [] # : list[dict]
for msg in msgs:
    msg_dictified.append(msg.toDict())

data = {
  "settings": {
    "game_title": "Alien Wordle Dating Sim",
    "thresholds": {
      "paragon_ending": 50,
      "renegade_ending": 50
    }
  },
  "nodes": msg_dictified
}

with open(OUT,"w") as f:
    json.dump(data, f, indent=2)
