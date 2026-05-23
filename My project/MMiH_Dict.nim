import std/httpclient
import std/algorithm
import std/strformat
import std/strutils
import std/osproc
import std/times
import std/os

# Use following flags to run the script: `nim r -d:ssl -d:release conv.nim`
const ONLINE_CHECK = true # check for words existence from dictionary API
const API          = "https://api.dictionaryapi.dev/api/v2/entries/en/"
const CAP          = 1000 # cap of entries for every session to check
const INFILE       = "words_conv.txt"
const OUTFILE      = "Assets/Words.txt"

var wseq     = newSeq[string]()
var client   = newHttpClient()

proc checkWord(word: string, ix_checked, flength: int): bool =
    if not ONLINE_CHECK: return true
    while true:
      try:
        let resp = get(client, fmt"{API}{word}")
        echo fmt"[{ix_checked}/{flength}] {resp.status}"
        if   resp.status == "429 Too Many Requests": sleep(5000); continue # rechecks after
        elif resp.status == "200 OK": return true
        return false
      except Exception as exc:
        echo exc.msg; sleep(5000); continue # to handle any exceptions occuring

while true: # explicit break by either session system or manually by user
    # session system
    echo fmt"Starting new session..."
    let start    = now()
    let words    = open(INFILE)
    
    let wordslist = words.readAll().split("\n") # all words from INFILE
    let wordslen  = len(wordslist)
    if wordslen == 0: # explicit break by reaching EOF (aka all words got checked and pushed to `out` file)
        close(words)
        break
    let session   = if wordslen >= CAP: wordslist[0..CAP-1]          else: wordslist[0..wordslen-1] # session limit
    let remaining = if wordslen >= CAP: wordslist[CAP-1..wordslen-1] else: @[""]                    # words for next sessions
    
    let wordsout  = open(OUTFILE, fmAppend)
    let seslen    = len(session)
    let remlen    = len(remaining)
    echo fmt"Session length: {seslen}"
    echo fmt"Remaining words for next sessions: {remlen}"

    for ix, word in session.pairs():
        let w = word.replace("\r", "")
        if len(w) > 3 and len(w) < 7:
          if checkWord(w, ix, seslen):
              add(wseq, w)

    block fileUpdates:
        # performs file updates at the end of session, to be used by next session if suitable
        for w in wseq:
            wordsout.write(w & "\n")
        wseq = @[] # resets so that it doesn't use old session words
        
        close(words)
        removeFile(INFILE) # temporary removal, so that we clear the OUTFILE additions

        let words_new = open(INFILE, fmWrite)
        for rem in remaining:
            words_new.write(rem & "\n")
    
        close(words_new)

    block GitCommands: # performs automated git data exchange
        discard execCmd("git pull")
        discard execCmd(fmt"git add {INFILE}")
        discard execCmd(fmt"git add {OUTFILE}")
        discard execCmd("git commit -m \"Automated words' list update\"")
        discard execCmd("git push")
    echo fmt"Session done in: {now()-start}"
    
echo "All sessions done. Press Enter to end."
let u = readLine(stdin)
