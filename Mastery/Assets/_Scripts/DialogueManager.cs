using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public Text nameText;
    public Text dialogueText;

    public Animator animator;

    private Queue<Dialogue> sentences;
    void Start()
    {
        sentences = new Queue<Dialogue>();
    }
    public void StartDialogue(FullConversation conversation)
    {
        animator.SetBool("IsOpen", true);
        nameText.text = conversation.dialogue[0].nameOfSpeaker;
        sentences.Clear();

        foreach (Dialogue sentence in conversation.dialogue) {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }
    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }
        Dialogue sentence = sentences.Dequeue();
        nameText.text = sentence.nameOfSpeaker;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence.sentence));
    }

    IEnumerator TypeSentence(string sentence) {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            yield return null;
        }
    }
    void EndDialogue() {
        animator.SetBool("IsOpen", false);
    }
}
