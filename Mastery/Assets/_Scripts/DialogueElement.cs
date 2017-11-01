using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueElement : MonoBehaviour {

    public FullConversation conversation;

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(conversation);

    }
}
