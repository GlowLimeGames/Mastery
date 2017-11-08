using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    public Text nameText;
    public Text dialogueText;

    public GameObject[] faces;
    public Animator animator;

    private Queue<Dialogue> sentences;
    void Start()
    {
        sentences = new Queue<Dialogue>();
        faces = GameObject.FindGameObjectsWithTag("DialogueFace");
        foreach (GameObject face in faces) {
            face.SetActive(false);
        }
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
        foreach (GameObject face in faces)
        {
            face.SetActive(false);
        }
        
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }
        Dialogue sentence = sentences.Dequeue();
        nameText.text = sentence.nameOfSpeaker;
        for (int i = 0; i < faces.Length; i++)
        {
            if (nameText.text.Equals(faces[i].name))
            {
                faces[i].SetActive(true);
            }
        }
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
