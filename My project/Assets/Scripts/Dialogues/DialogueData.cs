using System;
using System.Collections.Generic;

[Serializable]
public class DialogueChoice
{
    public string text;
    public int plus_paragon;
    public int plus_renegade;
    public string follow_up;
}

[Serializable]
public class DialogueNode
{
    public string node_id;
    public string speaker;
    public string text_original;
    public string wordle_solution;
    public string next_node;
    public List<DialogueChoice> choices;
}

[Serializable]
public class DialogueContainer
{
    public List<DialogueNode> nodes;
}

[System.Serializable]
public class EndingChoice {
    public string text;
    public bool end;
}

[System.Serializable]
public class EndingDialogue {
    public string text;
    public List<EndingChoice> choices;
}

[System.Serializable]
public class EndingData {
    public string name;
    public EndingDialogue rejection_dialogue;
    public EndingDialogue acceptance_dialogue;
    public string ending;
    public int paragon_requirements;
    public int overall_paragon_requirement;
}

[System.Serializable]
public class EndingList {
    public List<EndingData> endings;
    public string credits;
    public EndingDialogue unhappy_ending;
}
