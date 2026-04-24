using System;
using System.Collections.Generic;

[Serializable]
public class DialogueChoice
{
    public string text;
    public int plus_paragon;
    public int plus_renegade;
}

[Serializable]
public class DialogueNode
{
    public string node_id; // Dodajemy ID, żeby łatwo było szukać
    public string speaker;
    public string text_original;
    public string wordle_solution;
    public string next_node;
    public List<DialogueChoice> choices;
}

[Serializable]
public class DialogueContainer
{
    // JsonUtility nie radzi sobie z Dictionary, 
    // więc używamy listy, którą łatwo przeszukać.
    public List<DialogueNode> nodes;
}
