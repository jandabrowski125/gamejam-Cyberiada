import std/algorithm
import std/strformat
import std/strutils
import std/sequtils
import std/tables
import std/json

type
  Dictionary = object
    words : seq[string]

const DICT_PATH = "Assets/KnownWords.json"

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
