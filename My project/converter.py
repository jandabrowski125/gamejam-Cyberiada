import json

def purgeHTML (txt: str) -> str:
    return txt.replace("<p>", "").replace("</p>", "")

class Choice:
    def __init__(self, msg, paragon, renegade):
        self.msg = msg
        self.par = paragon
        self.ren = renegade

    def toDict(self) -> dict:
        return {
            "text":          self.msg,
            "plus_paragon":  self.par,
            "plus_renegade": self.ren
        }

class Message:
    def __init__(self, mid, person, msg, next, choices):
        self.id      = mid
        self.person  = person # 'title'
        self.msg     = msg    # 'content'
        self.wordle  = "" # TODO
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

with open('in.json', 'r') as f:
    jin = json.load(f)

queue = [jin["startingElement"]] # currently checked var(s), starting from first
elems = jin["elements"]
conns = jin["connections"]

while True: # broken only explicitly
    elem = elems[queue[0]]
    # temp
    msg_choices = []
    msg_next    = ""

    if len(elem["outputs"]) == 0: break # explicit end
    elif len(elem["outputs"]) == 1:
        msg_next = conns[elem["outputs"][0]]["targetid"] # immediate next message
    elif len(elem["outputs"]) > 1: # choice message type
        msg_next = conns[elem["outputs"][0]]["targetid"] # nested next message ID
        msg_next = conns[elems[msg_next]["outputs"][0]]["targetid"] # ...goes one msg deeper, leaping over choice
        for choice in elem["outputs"]:
            tit = purgeHTML(elems[conns[choice]["targetid"]]["title"]) # title is split later
            tits = tit.split(";")
            msg_choices.append(Choice(msg      = purgeHTML(elems[conns[choice]["targetid"]]["content"]),
                                      paragon  = int(tits[1]),
                                      renegade = int(tits[2])))

    msgs.append(Message(mid     = queue[0],
                        person  = purgeHTML(elem["title"]),
                        msg     = purgeHTML(elem["content"]),
                        next    = msg_next, # msg_next based on first connection, even with multiple
                        choices = msg_choices))
    queue.pop(0) # removes analysed item
    for outp in elem["outputs"]:
        queue.append(conns[outp]["targetid"]) # refill of queue
    if len(queue) == 0: break # breaks out of loop

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

with open("outb.json","w") as f:
    json.dump(data, f, indent=2)